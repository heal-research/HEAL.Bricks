#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  public interface IPackageManager {
    ISettings Settings { get; }
    IEnumerable<LocalPackageInfo> InstalledPackages { get; }
    PackageManagerStatus Status { get; }

    Task<IEnumerable<(string Repository, RemotePackageInfo Package)>> SearchRemotePackagesAsync(string searchString, int skip, int take, bool includePreReleases = false, CancellationToken cancellationToken = default);
    Task<RemotePackageInfo> GetRemotePackageAsync(string packageId, string version, CancellationToken cancellationToken = default);
    Task<IEnumerable<RemotePackageInfo>> GetRemotePackagesAsync(string packageId, bool includePreReleases = false, CancellationToken cancellationToken = default);
    
    Task InstallRemotePackageAsync(RemotePackageInfo package, bool installMissingDependencies = true, CancellationToken cancellationToken = default);
    Task InstallRemotePackagesAsync(IEnumerable<RemotePackageInfo> packages, bool installMissingDependencies = true, CancellationToken cancellationToken = default);
    
    void RemoveInstalledPackage(LocalPackageInfo package);
    void RemoveInstalledPackages(IEnumerable<LocalPackageInfo> packages);

    Task<IEnumerable<RemotePackageInfo>> GetMissingDependenciesAsync(CancellationToken cancellationToken = default);
    Task InstallMissingDependenciesAsync(CancellationToken cancellationToken = default);

    Task<RemotePackageInfo> GetPackageUpdateAsync(LocalPackageInfo package, bool includePreReleases = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<RemotePackageInfo>> GetPackageUpdatesAsync(IEnumerable<LocalPackageInfo> packages, bool includePreReleases = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<RemotePackageInfo>> GetPackageUpdatesAsync(bool includePreReleases = false, CancellationToken cancellationToken = default);
    
    Task InstallPackageUpdatesAsync(bool installMissingDependencies = true, bool includePreReleases = false, CancellationToken cancellationToken = default);

    void LoadPackageAssemblies(LocalPackageInfo package);
    void LoadPackageAssemblies();

    bool LoggingEnabled { get; }
    void EnableLogging(bool logDebugInfo = false);
    void DisableLogging();
    string[] GetLog();
    void ClearLog();
  }
}