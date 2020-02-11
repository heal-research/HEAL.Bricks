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
  public interface IPluginManager {
    ISettings Settings { get; }
    IEnumerable<LocalPackageInfo> InstalledPackages { get; }
    PluginManagerStatus Status { get; }

    void Initialize();
    Task<IEnumerable<RemotePackageInfo>> GetMissingDependenciesAsync(CancellationToken cancellationToken = default);
    Task InstallMissingDependenciesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<(string Repository, RemotePackageInfo Package)>> SearchRemotePackagesAsync(string searchString, bool includePreReleases, int skip, int take, CancellationToken cancellationToken = default);
    Task<RemotePackageInfo> GetRemotePackageAsync(string packageId, string version, CancellationToken cancellationToken = default);
    Task<IEnumerable<RemotePackageInfo>> GetRemotePackagesAsync(string packageId, bool includePreReleases, CancellationToken cancellationToken = default);
    Task InstallRemotePackageAsync(RemotePackageInfo package, bool installMissingDependencies, CancellationToken cancellationToken = default);
    void RemoveInstalledPackage(LocalPackageInfo package);
  }
}