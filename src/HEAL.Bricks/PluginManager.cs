#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  public sealed class PluginManager : IPluginManager {
    public static IPluginManager Create(ISettings settings) {
      if (settings == null) throw new ArgumentNullException(nameof(settings));

      return new PluginManager(settings);
    }

    private readonly NuGetConnector nuGetConnector;
    public ISettings Settings { get; }
    public IEnumerable<LocalPackageInfo> InstalledPackages { get; private set; } = Enumerable.Empty<LocalPackageInfo>();
    public PluginManagerStatus Status { get; private set; } = PluginManagerStatus.Uninitialized;

    private PluginManager(ISettings settings) {
      Settings = settings;
      nuGetConnector = new NuGetConnector(settings);
    }
    internal PluginManager(NuGetConnector nuGetConnector) {
      // only used for unit tests, if a specially initialized NuGetConnector is required
      this.nuGetConnector = nuGetConnector ?? throw new ArgumentNullException(nameof(nuGetConnector));
      Settings = nuGetConnector?.Settings;
    }

    public void Initialize() {
      IEnumerable<PackageFolderReader> packageReaders = Enumerable.Empty<PackageFolderReader>();
      try {
        packageReaders = nuGetConnector.GetInstalledPackages();
        LocalPackageInfo[] installedPackages = packageReaders.Select(x => new LocalPackageInfo(x, nuGetConnector.CurrentFramework, Settings.PluginTag)).ToArray();
        UpdatePackageAndDependencyStatus(installedPackages);
        Status = GetPluginManagerStatus(installedPackages);
        InstalledPackages = installedPackages;
      }
      finally {
        foreach (PackageFolderReader packageReader in packageReaders) packageReader.Dispose();
      }
    }

    public async Task<IEnumerable<RemotePackageInfo>> GetMissingDependenciesAsync(CancellationToken cancellationToken = default) {
      if (Status == PluginManagerStatus.Uninitialized) Initialize();

      IEnumerable<PackageIdentity> installedPackages = InstalledPackages.Select(x => x.nuspecReader.GetIdentity());
      IEnumerable<SourcePackageDependencyInfo> allDependencies = await nuGetConnector.GetPackageDependenciesAsync(installedPackages, true, cancellationToken);
      IEnumerable<LocalPackageInfo> latestVersionOfInstalledPlugins = InstalledPackages.Where(x => x.IsPlugin).GroupBy(x => x.Id).Select(x => x.OrderByDescending(y => y, PackageInfoIdentityComparer.Default).First());
      IEnumerable<PackageIdentity> installedPlugins = latestVersionOfInstalledPlugins.Select(x => x.nuspecReader.GetIdentity());

      IEnumerable<SourcePackageDependencyInfo> resolvedDependencies = nuGetConnector.ResolveDependencies(Enumerable.Empty<string>(), installedPlugins, allDependencies, cancellationToken, out bool resolveSucceeded);
      if (!resolveSucceeded) throw new InvalidOperationException("Dependency resolution failed.");

      IEnumerable<SourcePackageDependencyInfo> missingDependencies = resolvedDependencies.Where(x => !InstalledPackages.Any(y => x.Equals(y.nuspecReader.GetIdentity())));
      IEnumerable<IPackageSearchMetadata> packageMetadata = await nuGetConnector.GetPackagesAsync(missingDependencies, cancellationToken);
      return packageMetadata.Zip(missingDependencies, (x, y) => new RemotePackageInfo(x, y));
    }
    public async Task InstallMissingDependenciesAsync(CancellationToken cancellationToken = default) {
      if (Status == PluginManagerStatus.Uninitialized) Initialize();

      IEnumerable<RemotePackageInfo> missingPackages = (await GetMissingDependenciesAsync(cancellationToken)).ToArray();
      if (missingPackages.Count() > 0) {
        foreach (RemotePackageInfo missingPackage in missingPackages) {
          await nuGetConnector.InstallPackageAsync(missingPackage.sourcePackageDependencyInfo, cancellationToken);
        }
        Initialize();
      }
    }

    public async Task<IEnumerable<(string Repository, RemotePackageInfo Package)>> SearchRemotePackagesAsync(string searchString, bool includePreReleases, int skip, int take, CancellationToken cancellationToken = default) {
      if (searchString == null) throw new ArgumentNullException(nameof(searchString));

      IEnumerable<(string Repository, IPackageSearchMetadata Package)> packages = await nuGetConnector.SearchPackagesAsync(searchString, includePreReleases, skip, take, cancellationToken);
      IEnumerable<SourcePackageDependencyInfo> dependencyInfos = await nuGetConnector.GetPackageDependenciesAsync(packages.Select(x => x.Package.Identity), false, cancellationToken);
      return packages.Zip(dependencyInfos, (x, y) => (x.Repository, Package: new RemotePackageInfo(x.Package, y)));
    }

    public async Task<RemotePackageInfo> GetRemotePackageAsync(string packageId, string version, CancellationToken cancellationToken = default) {
      if (packageId == null) throw new ArgumentNullException(nameof(packageId));
      if (packageId == "") throw new ArgumentException($"{nameof(packageId)} is empty.", nameof(packageId));
      if (version == null) throw new ArgumentNullException(nameof(version));
      if (version == "") throw new ArgumentException($"{nameof(version)} is empty.", nameof(version));
      if (!NuGetVersion.TryParse(version, out NuGetVersion nuGetVersion)) throw new ArgumentException($"{nameof(version)} is invalid.", nameof(version));

      PackageIdentity identity = new PackageIdentity(packageId, nuGetVersion);
      IPackageSearchMetadata package = await nuGetConnector.GetPackageAsync(identity, cancellationToken);
      if (package == null) return null;

      SourcePackageDependencyInfo dependencyInfo = (await nuGetConnector.GetPackageDependenciesAsync(package.Identity, false, cancellationToken)).Single();
      return new RemotePackageInfo(package, dependencyInfo);
    }
    public async Task<IEnumerable<RemotePackageInfo>> GetRemotePackagesAsync(string packageId, bool includePreReleases, CancellationToken cancellationToken = default) {
      if (packageId == null) throw new ArgumentNullException(nameof(packageId));
      if (packageId == "") throw new ArgumentException($"{nameof(packageId)} is empty.", nameof(packageId));

      IEnumerable<IPackageSearchMetadata> packages = await nuGetConnector.GetPackagesAsync(packageId, includePreReleases, cancellationToken);
      IEnumerable<SourcePackageDependencyInfo> dependencyInfos = await nuGetConnector.GetPackageDependenciesAsync(packages.Select(x => x.Identity), false, cancellationToken);
      return packages.Zip(dependencyInfos, (x, y) => new RemotePackageInfo(x, y));
    }

    public async Task InstallRemotePackageAsync(RemotePackageInfo package, bool installMissingDependencies, CancellationToken cancellationToken = default) {
      if (package == null) throw new ArgumentNullException(nameof(package));

      await nuGetConnector.InstallPackageAsync(package.sourcePackageDependencyInfo, cancellationToken);
      Initialize();
      if (installMissingDependencies) await InstallMissingDependenciesAsync(cancellationToken);
    }

    public void RemoveInstalledPackage(LocalPackageInfo package) {
      if (package == null) throw new ArgumentNullException(nameof(package));
      if (string.IsNullOrEmpty(package.Path)) throw new ArgumentException($"{nameof(package)}.Path is null or empty.", nameof(package));

      Directory.Delete(package.Path, true);
      Initialize();
    }

    public async Task<RemotePackageInfo> GetPackageUpdateAsync(LocalPackageInfo package, bool includePreReleases = false, CancellationToken cancellationToken = default) {
      if (package == null) throw new ArgumentNullException(nameof(package));

      if (Status == PluginManagerStatus.Uninitialized) Initialize();

      NuGetVersion latestVersion = await nuGetConnector.GetLatestVersionAsync(package.Id, includePreReleases, cancellationToken);
      if ((latestVersion != null) && (latestVersion.CompareTo(package.Version.nuGetVersion) > 0)) {
        IPackageSearchMetadata latestPackage = await nuGetConnector.GetPackageAsync(new PackageIdentity(package.Id, latestVersion), cancellationToken);
        SourcePackageDependencyInfo dependencyInfo = (await nuGetConnector.GetPackageDependenciesAsync(latestPackage.Identity, false, cancellationToken)).Single();
        return new RemotePackageInfo(latestPackage, dependencyInfo);
      }
      return null;
    }
    public async Task<IEnumerable<RemotePackageInfo>> GetPackageUpdatesAsync(bool includePreReleases = false, CancellationToken cancellationToken = default) {
      if (Status == PluginManagerStatus.Uninitialized) Initialize();

      List<PackageIdentity> updates = new List<PackageIdentity>();
      IEnumerable<PackageIdentity> installedPackages = InstalledPackages.Select(x => x.nuspecReader.GetIdentity());
      IEnumerable<(string PackageId, NuGetVersion Version)> latestVersions = await nuGetConnector.GetLatestVersionsAsync(installedPackages.Select(x => x.Id), includePreReleases, cancellationToken);
      foreach (PackageIdentity installedPackage in installedPackages) {
        (string PackageId, NuGetVersion Version) latestVersion = latestVersions.Where(x => (x.PackageId == installedPackage.Id) && (x.Version.CompareTo(installedPackage.Version) > 0)).SingleOrDefault();
        if (latestVersion.Version != null) {
          updates.Add(new PackageIdentity(latestVersion.PackageId, new NuGetVersion(latestVersion.Version)));
        }
      }

      IEnumerable<IPackageSearchMetadata> packages = await nuGetConnector.GetPackagesAsync(updates, cancellationToken);
      IEnumerable<SourcePackageDependencyInfo> dependencyInfos = await nuGetConnector.GetPackageDependenciesAsync(packages.Select(x => x.Identity), false, cancellationToken);
      return packages.Zip(dependencyInfos, (x, y) => new RemotePackageInfo(x, y));
    }

    public async Task InstallPackageUpdatesAsync(bool includePreReleases = false, CancellationToken cancellationToken = default) {
      if (Status == PluginManagerStatus.Uninitialized) Initialize();

      IEnumerable<RemotePackageInfo> packageUpdates = (await GetPackageUpdatesAsync(includePreReleases, cancellationToken)).ToArray();
      if (packageUpdates.Count() > 0) {
        foreach (RemotePackageInfo packageUpdate in packageUpdates) {
          await nuGetConnector.InstallPackageAsync(packageUpdate.sourcePackageDependencyInfo, cancellationToken);
        }
        await InstallMissingDependenciesAsync();
      }
    }

    #region Helpers
    private static void UpdatePackageAndDependencyStatus(IEnumerable<LocalPackageInfo> packages) {
      foreach (var group in packages.GroupBy(x => x.Id)) {
        foreach (LocalPackageInfo package in group.OrderByDescending(x => x.Version).Skip(1)) {
          package.Status = PackageStatus.Outdated;
        }
      }
      foreach (LocalPackageInfo package in packages) {
        foreach (PackageDependency dependency in package.Dependencies) {
          dependency.Status = packages.Any(x => (x.Id == dependency.Id) && dependency.VersionRange.Satiesfies(x.Version)) ? PackageDependencyStatus.OK : PackageDependencyStatus.Missing;
        }
        if (package.Status == PackageStatus.Unknown) {
          if (package.Dependencies.Count() == 0) {
            package.Status = PackageStatus.OK;
          } else if (package.Dependencies.Any(x => x.Status == PackageDependencyStatus.Missing)) {
            package.Status = PackageStatus.DependenciesMissing;
          } else {
            // in this case we do not know, if dependencies of dependencies might be missing; so status remains unknown
            package.Status = PackageStatus.Unknown;
          }
        }
      }
      bool packageStatusChanged;
      do {
        packageStatusChanged = false;
        foreach (LocalPackageInfo package in packages.Where(x => x.Status == PackageStatus.Unknown)) {
          foreach (PackageDependency dependency in package.Dependencies.Where(x => x.Status == PackageDependencyStatus.OK)) {
            var dependencyPackages = packages.Where(x => (x.Id == dependency.Id) && dependency.VersionRange.Satiesfies(x.Version));
            if (dependencyPackages.Any(x => x.Status == PackageStatus.Unknown)) {
              // some of the packages which fulfill the dependency are still unknown, so we need to reset the package status to unknown
              package.Status = PackageStatus.Unknown;
              break;
            } else if (dependencyPackages.All(x => x.Status == PackageStatus.DependenciesMissing || x.Status == PackageStatus.IndirectDependenciesMissing || x.Status == PackageStatus.IncompatibleFramework)) {
              // all packages which fulfill the dependency are either missing or incompatible
              package.Status = PackageStatus.IndirectDependenciesMissing;
              break;
            } else if (dependencyPackages.Any(x => x.Status == PackageStatus.OK)) {
              // all packages which fulfill the dependency are known and at least one of these packages is ok
              package.Status = PackageStatus.OK;
            }
          }
          packageStatusChanged = packageStatusChanged || package.Status != PackageStatus.Unknown;
        }
      } while (packageStatusChanged);
    }

    private static PluginManagerStatus GetPluginManagerStatus(IEnumerable<LocalPackageInfo> packages) {
      IEnumerable<LocalPackageInfo> plugins = packages.Where(x => x.IsPlugin);
      if (plugins.All(x => x.Status == PackageStatus.OK)) {
        return PluginManagerStatus.OK;
      } else if (plugins.Any(x => x.Status == PackageStatus.Unknown)) {
        return PluginManagerStatus.Unknown;
      } else if (plugins.Any(x => x.Status == PackageStatus.DependenciesMissing || x.Status == PackageStatus.IndirectDependenciesMissing || x.Status == PackageStatus.IncompatibleFramework)) {
        return PluginManagerStatus.InvalidPlugins;
      } else {
        return PluginManagerStatus.Unknown;
      }
    }
    #endregion
  }
}
