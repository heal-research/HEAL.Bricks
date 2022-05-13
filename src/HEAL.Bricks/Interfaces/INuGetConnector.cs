#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Frameworks;

namespace HEAL.Bricks {
  internal interface INuGetConnector {
    NuGetFramework CurrentFramework { get; }

    IEnumerable<LocalPackageInfo> GetLocalPackages(string packagesPath);

    Task<RemotePackageInfo?> GetRemotePackageAsync(string packageId, string version, CancellationToken ct);

    Task<IEnumerable<RemotePackageInfo>> GetRemotePackagesAsync(string packageId, bool includePreReleases, CancellationToken ct);

    Task<IEnumerable<(string Repository, RemotePackageInfo Package)>> SearchRemotePackagesAsync(string searchString, int skip, int take, bool includePreReleases, CancellationToken ct);

    Task InstallRemotePackagesAsync(IEnumerable<RemotePackageInfo> packages, string packagesPath, string packagesCachePath, CancellationToken ct);

    void RemoveLocalPackages(IEnumerable<LocalPackageInfo> packages, string packagesPath);

    Task<IEnumerable<RemotePackageInfo>> GetMissingDependenciesAsync(IEnumerable<LocalPackageInfo> packages, CancellationToken ct);

    Task<IEnumerable<RemotePackageInfo>> GetPackageUpdatesAsync(IEnumerable<LocalPackageInfo> packages, bool includePreReleases, CancellationToken ct);
  }
}
