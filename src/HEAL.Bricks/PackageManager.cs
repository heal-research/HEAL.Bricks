#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using NuGet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  public sealed class PackageManager : IPackageManager {
    public static IPackageManager Create(BricksOptions options) {
      Guard.Argument(options, nameof(options)).NotNull().Member(s => s.Repositories, x => x.NotNull().NotEmpty().DoesNotContainNull())
                                                        .Member(s => s.PackagesPath, x => x.NotNull().NotEmpty().NotWhiteSpace().AbsolutePath())
                                                        .Member(s => s.PackagesCachePath, x => x.NotNull().NotEmpty().NotWhiteSpace().AbsolutePath());
      return new PackageManager(options);
    }
    internal static IPackageManager CreateForTests(BricksOptions options, INuGetConnector nuGetConnector) {
      return new PackageManager(options, nuGetConnector);
    }

    private readonly INuGetConnector nuGetConnector;
    private ILogger logger = NuGetLogger.NoLogger;

    public BricksOptions Options { get; }
    public IEnumerable<LocalPackageInfo> InstalledPackages { get; private set; } = Enumerable.Empty<LocalPackageInfo>();
    public PackageManagerStatus Status { get; private set; } = PackageManagerStatus.Undefined;

    private PackageManager(BricksOptions options) {
      Options = options;
      nuGetConnector = new NuGetConnector(options.Repositories, logger);
      Initialize();
    }
    private PackageManager(BricksOptions options, INuGetConnector nuGetConnector) {
      // only used for unit tests, if a mocked NuGetConnector has to be used
      Options = options;
      this.nuGetConnector = nuGetConnector;
      Initialize();
    }

    private void Initialize() {
      if (!Directory.Exists(Options.PackagesPath)) Directory.CreateDirectory(Options.PackagesPath);
      if (!Directory.Exists(Options.PackagesCachePath)) Directory.CreateDirectory(Options.PackagesCachePath);

      IEnumerable<LocalPackageInfo> installedPackages = nuGetConnector.GetLocalPackages(Options.PackagesPath).ToArray();
      SetPackageAndDependencyStatus(installedPackages);
      Status = GetPackageManagerStatus(installedPackages);
      InstalledPackages = installedPackages;
    }

    public async Task<IEnumerable<(string Repository, RemotePackageInfo Package)>> SearchRemotePackagesAsync(string searchString, int skip, int take, bool includePreReleases = false, CancellationToken cancellationToken = default) {
      Guard.Argument(searchString, nameof(searchString)).NotNull();

      return await nuGetConnector.SearchRemotePackagesAsync(searchString, skip, take, includePreReleases, cancellationToken);
    }
    public async Task<RemotePackageInfo?> GetRemotePackageAsync(string packageId, string version, CancellationToken cancellationToken = default) {
      Guard.Argument(packageId, nameof(packageId)).NotNull().NotEmpty();
      Guard.Argument(version, nameof(version)).NotNull().NotEmpty().ValidNuGetVersionString();

      return await nuGetConnector.GetRemotePackageAsync(packageId, version, cancellationToken);
    }
    public async Task<IEnumerable<RemotePackageInfo>> GetRemotePackagesAsync(string packageId, bool includePreReleases = false, CancellationToken cancellationToken = default) {
      Guard.Argument(packageId, nameof(packageId)).NotNull().NotEmpty();

      return await nuGetConnector.GetRemotePackagesAsync(packageId, includePreReleases, cancellationToken);
    }

    public async Task InstallRemotePackageAsync(RemotePackageInfo package, bool installMissingDependencies = true, CancellationToken cancellationToken = default) {
      Guard.Argument(package, nameof(package)).NotNull();

      await InstallRemotePackagesAsync(Enumerable.Repeat(package, 1), installMissingDependencies, cancellationToken);
    }
    public async Task InstallRemotePackagesAsync(IEnumerable<RemotePackageInfo> packages, bool installMissingDependencies = true, CancellationToken cancellationToken = default) {
      Guard.Argument(packages, nameof(packages)).NotNull().NotEmpty().DoesNotContainNull();

      await nuGetConnector.InstallRemotePackagesAsync(packages, Options.PackagesPath, Options.PackagesCachePath, cancellationToken);
      Initialize();
      if (installMissingDependencies) await InstallMissingDependenciesAsync(cancellationToken);
    }

    public void RemoveInstalledPackage(LocalPackageInfo package) {
      Guard.Argument(package, nameof(package)).NotNull().Member(p => p.PackagePath, s => s.NotNull().NotEmpty().RelativePath());

      RemoveInstalledPackages(Enumerable.Repeat(package, 1));
    }
    public void RemoveInstalledPackages(IEnumerable<LocalPackageInfo> packages) {
      Guard.Argument(packages, nameof(packages)).NotNull().NotEmpty().DoesNotContainNull().Require(packages.All(x => !string.IsNullOrEmpty(x.PackagePath) && !Path.IsPathRooted(x.PackagePath)));

      try {
        nuGetConnector.RemoveLocalPackages(packages, Options.PackagesPath);
      }
      catch (DirectoryNotFoundException e) {
        throw new InvalidOperationException("Package not found.", e);
      }
      finally {
        Initialize();
      }
    }

    public async Task<IEnumerable<RemotePackageInfo>> GetMissingDependenciesAsync(CancellationToken cancellationToken = default) {
      IEnumerable<LocalPackageInfo> installedPackages = InstalledPackages.Where(x => x.Status == PackageStatus.DependenciesMissing);
      return await nuGetConnector.GetMissingDependenciesAsync(installedPackages, cancellationToken);
    }
    public async Task InstallMissingDependenciesAsync(CancellationToken cancellationToken = default) {
      IEnumerable<RemotePackageInfo> dependencies = await GetMissingDependenciesAsync(cancellationToken);
      if (dependencies.Any()) {
        await InstallRemotePackagesAsync(dependencies, false, cancellationToken);
      }
    }

    public async Task<RemotePackageInfo?> GetPackageUpdateAsync(LocalPackageInfo package, bool includePreReleases = false, CancellationToken cancellationToken = default) {
      Guard.Argument(package, nameof(package)).NotNull();

      return (await GetPackageUpdatesAsync(Enumerable.Repeat(package, 1), includePreReleases, cancellationToken)).SingleOrDefault();
    }
    public async Task<IEnumerable<RemotePackageInfo>> GetPackageUpdatesAsync(IEnumerable<LocalPackageInfo> packages, bool includePreReleases = false, CancellationToken cancellationToken = default) {
      Guard.Argument(packages, nameof(packages)).NotNull().DoesNotContainNull();

      return await nuGetConnector.GetPackageUpdatesAsync(packages, includePreReleases, cancellationToken);
    }
    public async Task<IEnumerable<RemotePackageInfo>> GetPackageUpdatesAsync(bool includePreReleases = false, CancellationToken cancellationToken = default) {
      IEnumerable<LocalPackageInfo> installedPackages = InstalledPackages.Where(x => (x.Status == PackageStatus.OK) ||
                                                                                     (x.Status == PackageStatus.DependenciesMissing) ||
                                                                                     (x.Status == PackageStatus.IndirectDependenciesMissing));
      return await GetPackageUpdatesAsync(installedPackages, includePreReleases, cancellationToken);
    }
    public async Task InstallPackageUpdatesAsync(bool installMissingDependencies = true, bool includePreReleases = false, CancellationToken cancellationToken = default) {
      IEnumerable<RemotePackageInfo> updates = await GetPackageUpdatesAsync(includePreReleases, cancellationToken);
      if (updates.Any()) {
        await InstallRemotePackagesAsync(updates, installMissingDependencies, cancellationToken);
      }
    }

    public PackageLoadInfo GetPackageLoadInfo(LocalPackageInfo package) {
      Guard.Argument(package, nameof(package)).NotNull().Member(p => p.Status, s => s.Equal(PackageStatus.OK));

      return new PackageLoadInfo(package, Options.PackagesPath, Options.AppPath);
    }
    public IEnumerable<PackageLoadInfo> GetPackageLoadInfos() {
      return InstalledPackages.Where(x => x.Status == PackageStatus.OK).Select(x => GetPackageLoadInfo(x)).ToArray();
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
    private static void SetPackageAndDependencyStatus(IEnumerable<LocalPackageInfo> packages) {
      foreach (LocalPackageInfo package in packages) {
        package.Status = PackageStatus.Undefined; 
        foreach (PackageDependency dependency in package.Dependencies) {
          dependency.Status = packages.Any(x =>
            (x.Id == dependency.Id) && (x.Status != PackageStatus.IncompatibleFramework) && dependency.VersionRange.Satiesfies(x.Version)
          ) ? PackageDependencyStatus.OK : PackageDependencyStatus.Missing;
        }
      }
      
      foreach (var group in packages.GroupBy(x => x.Id)) {
        foreach (LocalPackageInfo package in group.OrderByDescending(x => x.Version).Skip(1)) {
          package.Status = PackageStatus.Outdated;
        }
      }

      foreach (LocalPackageInfo package in packages.Where(x => x.Status == PackageStatus.Undefined)) {
        if (!package.Dependencies.Any()) {
          package.Status = PackageStatus.OK;
        } else if (package.Dependencies.Any(x => x.Status == PackageDependencyStatus.Missing)) {
          package.Status = PackageStatus.DependenciesMissing;
        } else {
          // we do not know yet, if dependencies of dependencies might be missing; so status remains unknown
        }
      }

      bool packageStatusChanged;
      do {
        packageStatusChanged = false;
        foreach (LocalPackageInfo package in packages.Where(x => x.Status == PackageStatus.Undefined)) {
          foreach (PackageDependency dependency in package.Dependencies.Where(x => x.Status == PackageDependencyStatus.OK)) {
            var dependencyPackages = packages.Where(x => (x.Id == dependency.Id) && dependency.VersionRange.Satiesfies(x.Version));
            if (dependencyPackages.Any(x => x.Status == PackageStatus.Undefined)) {
              // some of the packages which fulfill the dependency are still unknown, so we need to reset the package status to unknown
              package.Status = PackageStatus.Undefined;
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
          packageStatusChanged = packageStatusChanged || package.Status != PackageStatus.Undefined;
        }
      } while (packageStatusChanged);
    }

    private static PackageManagerStatus GetPackageManagerStatus(IEnumerable<LocalPackageInfo> packages) {
      if (packages.All(x => (x.Status == PackageStatus.OK) || (x.Status == PackageStatus.Outdated))) {
        return PackageManagerStatus.OK;
      } else if (packages.Any(x => x.Status == PackageStatus.Undefined)) {
        return PackageManagerStatus.Undefined;
      } else if (packages.Any(x => x.Status == PackageStatus.DependenciesMissing || x.Status == PackageStatus.IndirectDependenciesMissing || x.Status == PackageStatus.IncompatibleFramework)) {
        return PackageManagerStatus.InvalidPackages;
      } else {
        return PackageManagerStatus.Undefined;
      }
    }
    #endregion
  }
}
