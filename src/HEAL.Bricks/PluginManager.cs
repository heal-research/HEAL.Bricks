#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using FluentValidation;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  public sealed class PluginManager : IPluginManager {
    public static IPluginManager Create(ISettings settings) {
      Guard.Argument(settings, nameof(settings)).NotNull();
      return new PluginManager(settings);
    }

    private readonly INuGetConnector nuGetConnector;
    private ILogger logger = NuGetLogger.NoLogger;

    public ISettings Settings { get; }
    public IEnumerable<LocalPackageInfo> InstalledPackages { get; private set; } = Enumerable.Empty<LocalPackageInfo>();
    public PluginManagerStatus Status { get; private set; } = PluginManagerStatus.Uninitialized;

    private PluginManager(ISettings settings) {
      Settings = settings;
      nuGetConnector = new NuGetConnector(settings.Repositories, logger);
      Initialize();
    }
    internal PluginManager(ISettings settings, INuGetConnector nuGetConnector, bool initialize = true) {
      // only used for unit tests, if a mocked NuGetConnector has to be used
      Settings = settings;
      this.nuGetConnector = nuGetConnector;
      if (initialize) Initialize();
    }

    private void Initialize() {
      IEnumerable<PackageFolderReader> packageReaders = Enumerable.Empty<PackageFolderReader>();
      try {
        packageReaders = nuGetConnector.GetInstalledPackages(Settings.PackagesPath);
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
      IEnumerable<PackageIdentity> installedPackages = InstalledPackages.Select(x => x.nuspecReader.GetIdentity());
      IEnumerable<SourcePackageDependencyInfo> allDependencies = await nuGetConnector.GetPackageDependenciesAsync(installedPackages, true, cancellationToken);
      IEnumerable<LocalPackageInfo> latestVersionOfInstalledPlugins = InstalledPackages.Where(x => x.IsPlugin).GroupBy(x => x.Id).Select(x => x.OrderByDescending(y => y, PackageInfoIdentityComparer.Default).First());
      IEnumerable<PackageIdentity> installedPlugins = latestVersionOfInstalledPlugins.Select(x => x.nuspecReader.GetIdentity());

      IEnumerable<SourcePackageDependencyInfo> resolvedDependencies = nuGetConnector.ResolveDependencies(Enumerable.Empty<string>(), installedPlugins, allDependencies, cancellationToken, out bool resolveSucceeded);
      if (!resolveSucceeded) throw new InvalidOperationException("Dependency resolution failed.");

      IEnumerable<SourcePackageDependencyInfo> missingDependencies = resolvedDependencies.Where(x => !InstalledPackages.Any(y => x.Equals(y.nuspecReader.GetIdentity())));
      IEnumerable<IPackageSearchMetadata> packageMetadata = await nuGetConnector.GetPackagesAsync(missingDependencies, cancellationToken);
      return packageMetadata.Zip(missingDependencies, (x, y) => new RemotePackageInfo(x, y)).ToArray();
    }
    public async Task InstallMissingDependenciesAsync(CancellationToken cancellationToken = default) {
      await InstallRemotePackagesAsync(await GetMissingDependenciesAsync(cancellationToken), false, cancellationToken);
    }

    public async Task<IEnumerable<(string Repository, RemotePackageInfo Package)>> SearchRemotePackagesAsync(string searchString, int skip, int take, bool includePreReleases = false, CancellationToken cancellationToken = default) {
      Guard.Argument(searchString, nameof(searchString)).NotNull();

      IEnumerable<(string Repository, IPackageSearchMetadata Package)> packages = await nuGetConnector.SearchPackagesAsync(searchString, includePreReleases, skip, take, cancellationToken);
      IEnumerable<SourcePackageDependencyInfo> dependencyInfos = await nuGetConnector.GetPackageDependenciesAsync(packages.Select(x => x.Package.Identity), false, cancellationToken);
      return packages.Zip(dependencyInfos, (x, y) => (x.Repository, Package: new RemotePackageInfo(x.Package, y))).ToArray();
    }
    public async Task<RemotePackageInfo> GetRemotePackageAsync(string packageId, string version, CancellationToken cancellationToken = default) {
      Guard.Argument(packageId, nameof(packageId)).NotNull().NotEmpty();
      Guard.Argument(version, nameof(version)).NotNull().NotEmpty().ValidNuGetVersionString();

      PackageIdentity identity = new PackageIdentity(packageId, NuGetVersion.Parse(version));
      IPackageSearchMetadata package = await nuGetConnector.GetPackageAsync(identity, cancellationToken);
      if (package == null) return null;

      SourcePackageDependencyInfo dependencyInfo = (await nuGetConnector.GetPackageDependenciesAsync(package.Identity, false, cancellationToken)).Single();
      return new RemotePackageInfo(package, dependencyInfo);
    }
    public async Task<IEnumerable<RemotePackageInfo>> GetRemotePackagesAsync(string packageId, bool includePreReleases = false, CancellationToken cancellationToken = default) {
      Guard.Argument(packageId, nameof(packageId)).NotNull().NotEmpty();

      IEnumerable<IPackageSearchMetadata> packages = await nuGetConnector.GetPackagesAsync(packageId, includePreReleases, cancellationToken);
      IEnumerable<SourcePackageDependencyInfo> dependencyInfos = await nuGetConnector.GetPackageDependenciesAsync(packages.Select(x => x.Identity), false, cancellationToken);
      return packages.Zip(dependencyInfos, (x, y) => new RemotePackageInfo(x, y)).ToArray();
    }

    public async Task InstallRemotePackageAsync(RemotePackageInfo package, bool installMissingDependencies = true, CancellationToken cancellationToken = default) {
      Guard.Argument(package, nameof(package)).NotNull();

      await InstallRemotePackagesAsync(Enumerable.Repeat(package, 1), installMissingDependencies, cancellationToken);
    }
    public async Task InstallRemotePackagesAsync(IEnumerable<RemotePackageInfo> packages, bool installMissingDependencies = true, CancellationToken cancellationToken = default) {
      Guard.Argument(packages, nameof(packages)).NotNull().NotEmpty().DoesNotContainNull();

      if (packages.Count() > 0) {
        await nuGetConnector.InstallPackagesAsync(packages.Select(x => x.sourcePackageDependencyInfo), Settings.PackagesPath, Settings.PackagesCachePath, cancellationToken);
        Initialize();
        if (installMissingDependencies) await InstallMissingDependenciesAsync(cancellationToken);
      }
    }

    public void RemoveInstalledPackage(LocalPackageInfo package) {
      Guard.Argument(package, nameof(package)).NotNull().Member(p => p.PackagePath, s => s.NotNull().NotEmpty());

      RemoveInstalledPackages(Enumerable.Repeat(package, 1));
    }
    public void RemoveInstalledPackages(IEnumerable<LocalPackageInfo> packages) {
      Guard.Argument(packages, nameof(packages)).NotNull().DoesNotContainNull().Require(packages.All(x => !string.IsNullOrEmpty(x.PackagePath)));

      if (packages.Count() > 0) {
        foreach (LocalPackageInfo package in packages) {
          try {
            Directory.Delete(package.PackagePath, true);
          }
          catch (DirectoryNotFoundException) { }
        }
        Initialize();
      }
    }

    public async Task<RemotePackageInfo> GetPackageUpdateAsync(LocalPackageInfo package, bool includePreReleases = false, CancellationToken cancellationToken = default) {
      Guard.Argument(package, nameof(package)).NotNull();

      return (await GetPackageUpdatesAsync(Enumerable.Repeat(package, 1), includePreReleases, cancellationToken)).SingleOrDefault();
    }
    public async Task<IEnumerable<RemotePackageInfo>> GetPackageUpdatesAsync(IEnumerable<LocalPackageInfo> packages, bool includePreReleases = false, CancellationToken cancellationToken = default) {
      Guard.Argument(packages, nameof(packages)).NotNull().DoesNotContainNull();

      List<PackageIdentity> updates = new List<PackageIdentity>();
      IEnumerable<(string PackageId, NuGetVersion Version)> latestVersions = await nuGetConnector.GetLatestVersionsAsync(packages.Select(x => x.Id), includePreReleases, cancellationToken);
      foreach (PackageIdentity package in packages.Select(x => x.packageIdentity)) {
        (string PackageId, NuGetVersion Version) latestVersion = latestVersions.Where(x => (x.PackageId == package.Id) && (x.Version.CompareTo(package.Version) > 0)).SingleOrDefault();
        if (latestVersion.Version != null) {
          updates.Add(new PackageIdentity(latestVersion.PackageId, latestVersion.Version));
        }
      }

      IEnumerable<IPackageSearchMetadata> latestPackages = await nuGetConnector.GetPackagesAsync(updates, cancellationToken);
      IEnumerable<SourcePackageDependencyInfo> dependencyInfos = await nuGetConnector.GetPackageDependenciesAsync(latestPackages.Select(x => x.Identity), false, cancellationToken);
      return latestPackages.Zip(dependencyInfos, (x, y) => new RemotePackageInfo(x, y)).ToArray();
    }
    public async Task<IEnumerable<RemotePackageInfo>> GetPackageUpdatesAsync(bool includePreReleases = false, CancellationToken cancellationToken = default) {
      return await GetPackageUpdatesAsync(InstalledPackages, includePreReleases, cancellationToken);
    }

    public async Task InstallPackageUpdatesAsync(bool installMissingDependencies = true, bool includePreReleases = false, CancellationToken cancellationToken = default) {
      await InstallRemotePackagesAsync(await GetPackageUpdatesAsync(includePreReleases, cancellationToken), installMissingDependencies, cancellationToken);
    }

    public void LoadPackageAssemblies(LocalPackageInfo package) {
      Guard.Argument(package, nameof(package)).NotNull().Member(p => p.Status, s => s.NotEqual(PackageStatus.Unknown)
                                                                                     .NotEqual(PackageStatus.DependenciesMissing)
                                                                                     .NotEqual(PackageStatus.IndirectDependenciesMissing)
                                                                                     .NotEqual(PackageStatus.IncompatibleFramework));

      if (package.Status != PackageStatus.Loaded) {
        foreach (string assemblyPath in package.ReferenceItems) {
          AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
        }
        package.Status = PackageStatus.Loaded;
      }
    }
    public void LoadPackageAssemblies() {
      foreach (LocalPackageInfo package in InstalledPackages.Where(x => x.Status == PackageStatus.OK)) {
        LoadPackageAssemblies(package);
      }
    }

    #region Logging
    public bool LoggingEnabled => logger != NuGetLogger.NoLogger;
    public void EnableLogging(bool logDebugInfo = false) {
      logger = new NuGetLogger(logDebugInfo ? LogLevel.Debug : LogLevel.Information);
    }
    public void DisableLogging() {
      logger = NuGetLogger.NoLogger;
    }
    public string[] GetLog() {
      return (logger as NuGetLogger)?.GetLog() ?? Array.Empty<string>();
    }
    public void ClearLog() {
      (logger as NuGetLogger)?.Clear();
    }
    #endregion

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
              // all packages which fulfill the dependency are known and at least one of these packages is OK
              package.Status = PackageStatus.OK;
            }
          }
          packageStatusChanged = packageStatusChanged || package.Status != PackageStatus.Unknown;
        }
      } while (packageStatusChanged);
    }

    private static PluginManagerStatus GetPluginManagerStatus(IEnumerable<LocalPackageInfo> packages) {
      IEnumerable<LocalPackageInfo> plugins = packages.Where(x => x.IsPlugin);
      if (plugins.All(x => (x.Status == PackageStatus.OK) || (x.Status == PackageStatus.Outdated))) {
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
