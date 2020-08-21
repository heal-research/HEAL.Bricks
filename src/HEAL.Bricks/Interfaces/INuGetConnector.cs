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
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGetPackageDependency = NuGet.Packaging.Core.PackageDependency;

namespace HEAL.Bricks {
  internal interface INuGetConnector {
    NuGetFramework CurrentFramework { get; }


    IEnumerable<LocalPackageInfo> GetLocalPackages(string packagesPath, string bricksPackageTag);
    Task<RemotePackageInfo> GetRemotePackageAsync(string packageId, string version, CancellationToken ct);
    Task<IEnumerable<RemotePackageInfo>> GetRemotePackagesAsync(string packageId, bool includePreReleases, CancellationToken ct);
    Task<IEnumerable<(string Repository, RemotePackageInfo Package)>> SearchRemotePackagesAsync(string searchString, int skip, int take, bool includePreReleases, CancellationToken ct);
    Task InstallRemotePackagesAsync(IEnumerable<RemotePackageInfo> packages, string packagesPath, string packagesCachePath, CancellationToken ct);
    void RemoveLocalPackages(IEnumerable<LocalPackageInfo> packages);
    Task<IEnumerable<RemotePackageInfo>> GetMissingDependenciesAsync(IEnumerable<LocalPackageInfo> packages, CancellationToken ct);
    Task<IEnumerable<RemotePackageInfo>> GetPackageUpdatesAsync(IEnumerable<LocalPackageInfo> packages, bool includePreReleases, CancellationToken ct);


    Task<IPackageSearchMetadata> GetPackageAsync(PackageIdentity identity, CancellationToken ct);
    Task<IEnumerable<IPackageSearchMetadata>> GetPackagesAsync(IEnumerable<PackageIdentity> identities, CancellationToken ct);
    Task<IEnumerable<IPackageSearchMetadata>> GetPackagesAsync(string packageId, bool includePreReleases, CancellationToken ct);
    Task<IEnumerable<IPackageSearchMetadata>> GetPackagesAsync(IEnumerable<string> packageIds, bool includePreReleases, CancellationToken ct);
    Task<IEnumerable<(string Repository, IPackageSearchMetadata Package)>> SearchPackagesAsync(string searchString, bool includePreReleases, int skip, int take, CancellationToken ct);

    Task<IEnumerable<SourcePackageDependencyInfo>> GetPackageDependenciesAsync(PackageIdentity identity, bool getDependenciesRecursively, CancellationToken ct);
    Task<IEnumerable<SourcePackageDependencyInfo>> GetPackageDependenciesAsync(IEnumerable<PackageIdentity> identities, bool getDependenciesRecursively, CancellationToken ct);
    Task<IEnumerable<SourcePackageDependencyInfo>> GetPackageDependenciesAsync(NuGetPackageDependency dependency, bool getDependenciesRecursively, CancellationToken ct);
    Task<IEnumerable<SourcePackageDependencyInfo>> GetPackageDependenciesAsync(IEnumerable<NuGetPackageDependency> dependencies, bool getDependenciesRecursively, CancellationToken ct);

    IEnumerable<SourcePackageDependencyInfo> ResolveDependencies(IEnumerable<string> additionalPackages, IEnumerable<PackageIdentity> existingPackages, IEnumerable<SourcePackageDependencyInfo> availablePackages, CancellationToken cancellationToken, out bool resolveSucceeded);

    IEnumerable<PackageFolderReader> GetInstalledPackages(string packagesPath);
    Task<IPackageDownloader> GetPackageDownloaderAsync(PackageIdentity identity, CancellationToken cancellationToken);
    Task InstallPackageAsync(SourcePackageDependencyInfo package, string packagesPath, string packagesCachePath, CancellationToken cancellationToken);
    Task InstallPackagesAsync(IEnumerable<SourcePackageDependencyInfo> packages, string packagesPath, string packagesCachePath, CancellationToken cancellationToken);

    Task<NuGetVersion> GetLatestVersionAsync(string packageId, bool includePreReleases, CancellationToken cancellationToken);
    Task<IEnumerable<(string PackageId, NuGetVersion Version)>> GetLatestVersionsAsync(IEnumerable<string> packageIds, bool includePreReleases, CancellationToken cancellationToken);
  }
}
