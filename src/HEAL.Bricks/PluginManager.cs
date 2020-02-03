#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
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
    public IEnumerable<PackageInfo> InstalledPackages { get; private set; } = Enumerable.Empty<PackageInfo>();
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
        PackageInfo[] installedPackages = packageReaders.Select(x => new PackageInfo(x, nuGetConnector.CurrentFramework, Settings.PluginTag)).ToArray();
        UpdatePackageAndDependencyStatus(installedPackages);
        Status = GetPluginManagerStatus(installedPackages);
        InstalledPackages = installedPackages;
      }
      finally {
        foreach (PackageFolderReader packageReader in packageReaders) packageReader.Dispose();
      }
    }
    public async Task<IEnumerable<RemotePackageInfo>> ResolveMissingDependenciesAsync(CancellationToken cancellationToken = default) {
      if (Status == PluginManagerStatus.Uninitialized) Initialize();

      IEnumerable<PackageIdentity> installedPackages = InstalledPackages.Select(x => x.nuspecReader.GetIdentity());
      IEnumerable<PackageIdentity> installedPlugins = InstalledPackages.Where(x => x.IsPlugin).Select(x => x.nuspecReader.GetIdentity());
      IEnumerable<SourcePackageDependencyInfo> allDependencies = await nuGetConnector.GetPackageDependenciesAsync(installedPackages, true, cancellationToken);
      IEnumerable<SourcePackageDependencyInfo> resolvedDependencies = nuGetConnector.ResolveDependencies(installedPlugins.Select(x => x.Id), allDependencies, cancellationToken, out bool resolveSucceeded);
      if (!resolveSucceeded) throw new InvalidOperationException("Dependency resolution failed.");

      IEnumerable<SourcePackageDependencyInfo> missingDependencies = resolvedDependencies.Where(x => !InstalledPackages.Any(y => x.Equals(y.nuspecReader.GetIdentity())));
      return missingDependencies.Select(x => new RemotePackageInfo(x));
    }
    public async Task InstallRemotePackageAsync(RemotePackageInfo package, CancellationToken cancellationToken = default) {
      if (package == null) throw new ArgumentNullException(nameof(package));

      await nuGetConnector.InstallPackageAsync(package.sourcePackageDependencyInfo, cancellationToken);
    }

    #region Helpers
    private static void UpdatePackageAndDependencyStatus(IEnumerable<PackageInfo> packages) {
      foreach (PackageInfo package in packages) {
        foreach (PackageDependency dependency in package.Dependencies) {
          dependency.Status = packages.Any(x => (x.Id == dependency.Id) && dependency.VersionRange.Satiesfies(x.Version)) ? PackageDependencyStatus.OK : PackageDependencyStatus.Missing;
        }
        if (package.Status == PackageStatus.Unknown) {
          if (package.Dependencies.Count() == 0) {
            package.Status = PackageStatus.OK;
          } else if (package.Dependencies.Any(x => x.Status == PackageDependencyStatus.Missing)) {
            package.Status = PackageStatus.DependenciesMissing;
          } else {
            // in this case we do not know, if dependencies of dependencies might be missing; so status is still unknown
            package.Status = PackageStatus.Unknown;
          }
        }
      }
      bool packageStatusChanged;
      do {
        packageStatusChanged = false;
        foreach (PackageInfo package in packages.Where(x => x.Status == PackageStatus.Unknown)) {
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

    private static PluginManagerStatus GetPluginManagerStatus(IEnumerable<PackageInfo> packages) {
      IEnumerable<PackageInfo> plugins = packages.Where(x => x.IsPlugin);
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
