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
  internal class NuGetConnector {
    public string AppDirectory { get; }
    public NuGetFramework CurrentFramework { get; }
    public SourceRepository LocalRepository { get; }
    public IEnumerable<SourceRepository> RemoteRepositories { get; }
    public IEnumerable<SourceRepository> AllRepositories {
      get {
        return RemoteRepositories.Prepend(LocalRepository);
      }
    }

    public NuGetConnector(params string[] remoteRepositories) {
      AppDirectory = GetAppDirectory();
      CurrentFramework = GetCurrentFramework();
      LocalRepository = CreateSourceRepository(AppDirectory);
      RemoteRepositories = remoteRepositories.Select(x => CreateSourceRepository(x)).ToArray();
    }

    public async Task<IPackageSearchMetadata> GetPackageAsync(PackageIdentity identity, IEnumerable<SourceRepository> sourceRepositories, CancellationToken cancellationToken) {
      IEnumerable<IPackageSearchMetadata> packages = Enumerable.Empty<IPackageSearchMetadata>();
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        foreach (SourceRepository sourceRepository in sourceRepositories) {
          PackageMetadataResource packageMetadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>(cancellationToken);
          IPackageSearchMetadata package = await packageMetadataResource.GetMetadataAsync(identity, cacheContext, NullLogger.Instance, cancellationToken);
          if (package != null) return package;
        }
      }
      return null;
    }

    public async Task<IEnumerable<IPackageSearchMetadata>> GetPackagesAsync(string packageId, IEnumerable<SourceRepository> sourceRepositories, bool includePreReleases, CancellationToken cancellationToken) {
      IEnumerable<IPackageSearchMetadata> packages = Enumerable.Empty<IPackageSearchMetadata>();
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        foreach (SourceRepository sourceRepository in sourceRepositories) {
          PackageMetadataResource packageMetadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>(cancellationToken);
          packages = packages.Concat(await packageMetadataResource.GetMetadataAsync(packageId, includePreReleases, false, cacheContext, NullLogger.Instance, cancellationToken));
        }
      }
      return packages.Distinct(PackageSearchMetadataEqualityComparer.Default);
    }

    public async Task<IEnumerable<IPackageSearchMetadata>> SearchPackagesAsync(string searchString, IEnumerable<SourceRepository> sourceRepositories, bool includePreReleases, CancellationToken cancellationToken) {
      IEnumerable<IPackageSearchMetadata> packages = Enumerable.Empty<IPackageSearchMetadata>();
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        foreach (SourceRepository sourceRepository in sourceRepositories) {
          PackageSearchResource packageSearchResource = await sourceRepository.GetResourceAsync<PackageSearchResource>(cancellationToken);
          SearchFilter filter = new SearchFilter(includePreReleases);
          packages = packages.Concat(await packageSearchResource.SearchAsync(searchString, filter, 0, int.MaxValue, NullLogger.Instance, cancellationToken));
        }
      }
      return packages.Distinct(PackageSearchMetadataEqualityComparer.Default);
    }

    public async Task<IEnumerable<SourcePackageDependencyInfo>> GetPackageDependenciesAsync(PackageIdentity identity, IEnumerable<SourceRepository> sourceRepositories, bool resolveDependenciesRecursively, CancellationToken cancellationToken) {
      HashSet<SourcePackageDependencyInfo> resolvedDependencies = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {

        async Task ResolvePackageDependenciesAsync(PackageIdentity id) {
          if (resolvedDependencies.Contains(id)) return;
          foreach (SourceRepository sourceRepository in sourceRepositories) {
            DependencyInfoResource dependencyInfoResource = await sourceRepository.GetResourceAsync<DependencyInfoResource>(cancellationToken);
            SourcePackageDependencyInfo dependencies = await dependencyInfoResource.ResolvePackage(id, CurrentFramework, cacheContext, NullLogger.Instance, cancellationToken);
            if (dependencies != null) {
              resolvedDependencies.Add(dependencies);
              if (resolveDependenciesRecursively) {
                foreach (PackageDependency dependency in dependencies.Dependencies) {
                  await ResolvePackageDependenciesAsync(new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion));
                }
              }
              return;
            }
          }
        };

        await ResolvePackageDependenciesAsync(identity);
      }
      return resolvedDependencies;
    }

    public async Task<IPackageDownloader> GetPackageDownloaderAsync(PackageIdentity identity, IEnumerable<SourceRepository> sourceRepositories, CancellationToken cancellationToken) {
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        foreach (SourceRepository sourceRepository in sourceRepositories) {
          FindPackageByIdResource findPackageByIdResource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
          IPackageDownloader downloader = await findPackageByIdResource.GetPackageDownloaderAsync(identity, cacheContext, NullLogger.Instance, cancellationToken);
          if (downloader != null) return downloader;
        }
      }
      return null;
    }

    private string GetAppDirectory() {
      return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
    }
    private NuGetFramework GetCurrentFramework() {
      string frameworkName = Assembly.GetEntryAssembly().GetCustomAttribute<TargetFrameworkAttribute>().FrameworkName;
      return NuGetFramework.ParseFrameworkName(frameworkName, DefaultFrameworkNameProvider.Instance);
    }
    private SourceRepository CreateSourceRepository(string packageSource) {
      return new SourceRepository(new PackageSource(packageSource), Repository.Provider.GetCoreV3());
    }
    private SourceCacheContext CreateSourceCacheContext() {
      return new SourceCacheContext() { NoCache = true };
    }

    private class PackageSearchMetadataEqualityComparer : IEqualityComparer<IPackageSearchMetadata> {
      public static PackageSearchMetadataEqualityComparer Default => new PackageSearchMetadataEqualityComparer();
      private readonly PackageIdentityComparer identityComparer = PackageIdentityComparer.Default;
      public bool Equals(IPackageSearchMetadata x, IPackageSearchMetadata y) {
        return identityComparer.Equals(x.Identity, y.Identity);
      }
      public int GetHashCode(IPackageSearchMetadata obj) {
        return identityComparer.GetHashCode(obj.Identity);
      }
    }
  }
}
