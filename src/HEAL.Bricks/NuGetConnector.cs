#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using NuGetPackageDependency = NuGet.Packaging.Core.PackageDependency;

namespace HEAL.Bricks {
  internal class NuGetConnector : INuGetConnector {
    private IEnumerable<SourceRepository> repositories = Enumerable.Empty<SourceRepository>();
    private ILogger logger = NuGetLogger.NoLogger;

    public NuGetFramework CurrentFramework { get; private set; } = GetFrameworkFromEntryAssembly();

    private NuGetConnector() { }
    public NuGetConnector(IEnumerable<string> repositories, ILogger logger) {
      this.repositories = repositories.Select(x => CreateSourceRepository(x)).ToArray();
      this.logger = logger;
    }

    public static NuGetConnector CreateForTests(string frameworkName, IEnumerable<string> packageSources, ILogger logger) {
      // only used for tests to create a specifically initialized NuGetConnector
      return CreateForTests(frameworkName, packageSources.Select(ps => CreateSourceRepository(ps)).ToArray(), logger);
    }
    public static NuGetConnector CreateForTests(string frameworkName, IEnumerable<SourceRepository> repositories, ILogger logger) {
      // only used for tests to create a specifically initialized NuGetConnector
      // frameworkName: In unit tests, TargetFrameworkAttribute.FrameworkName of the entry assembly (testhost.dll) returns
      // ".NETCoreApp,Version=v1.0" (MSTest) or ".NETCoreApp,Version=v1.0" (xUnit). Consequently, GetFrameworkFromEntryAssembly
      // returns .NET Core 1.0 or .NET Core 2.1 as the current .NET framework for NuGet packages. As HEAL.Bricks is a .NET
      // Standard 2.0 library and applications using HEAL.Bricks therefore have to be at least .NET Core 2.0 or
      // .NET Framework 4.6.1, the detected NuGet framework is wrong and has to be defined explicitly. Otherwise, dependency
      // resolution does not work correctly, as NuGet looks for dependencies of .NET Core 1.0 or .NET Core 2.1.
      // repositories: used to initialize NuGetConnector with mocked instances of SourceRepository.
      return new NuGetConnector {
        repositories = repositories,
        logger = logger,
        CurrentFramework = GetFrameworkFromName(frameworkName)
      };
    }

    #region INuGetConnector Methods
    public virtual IEnumerable<LocalPackageInfo> GetLocalPackages(string packagesPath) {
      IEnumerable<PackageFolderReader> packageReaders = Enumerable.Empty<PackageFolderReader>();
      try {
        packageReaders = GetInstalledPackages(packagesPath);
        return packageReaders.Select(x => new LocalPackageInfo(x, CurrentFramework)).ToArray();
      }
      finally {
        foreach (PackageFolderReader packageReader in packageReaders) packageReader.Dispose();
      }
    }
    public virtual async Task<RemotePackageInfo> GetRemotePackageAsync(string packageId, string version, CancellationToken ct) {
      PackageIdentity identity = new PackageIdentity(packageId, NuGetVersion.Parse(version));
      IPackageSearchMetadata package = await GetPackageAsync(identity, ct);
      if (package == null) return null;

      SourcePackageDependencyInfo dependencyInfo = (await GetPackageDependenciesAsync(package.Identity, false, ct)).Single();
      return new RemotePackageInfo(package, dependencyInfo);
    }
    public virtual async Task<IEnumerable<RemotePackageInfo>> GetRemotePackagesAsync(string packageId, bool includePreReleases, CancellationToken ct) {
      IEnumerable<IPackageSearchMetadata> packages = await GetPackagesAsync(packageId, includePreReleases, ct);
      IEnumerable<SourcePackageDependencyInfo> dependencyInfos = await GetPackageDependenciesAsync(packages.Select(x => x.Identity), false, ct);
      return packages.Zip(dependencyInfos, (x, y) => new RemotePackageInfo(x, y)).ToArray();
    }
    public virtual async Task<IEnumerable<(string Repository, RemotePackageInfo Package)>> SearchRemotePackagesAsync(string searchString, int skip, int take, bool includePreReleases, CancellationToken ct) {
      IEnumerable<(string Repository, IPackageSearchMetadata Package)> packages = await SearchPackagesAsync(searchString, includePreReleases, skip, take, ct);
      IEnumerable<SourcePackageDependencyInfo> dependencyInfos = await GetPackageDependenciesAsync(packages.Select(x => x.Package.Identity), false, ct);
      return packages.Zip(dependencyInfos, (x, y) => (x.Repository, Package: new RemotePackageInfo(x.Package, y))).ToArray();
    }
    public virtual async Task InstallRemotePackagesAsync(IEnumerable<RemotePackageInfo> packages, string packagesPath, string packagesCachePath, CancellationToken ct) {
      await InstallPackagesAsync(packages.Select(x => x.sourcePackageDependencyInfo), packagesPath, packagesCachePath, ct);
    }
    public virtual void RemoveLocalPackages(IEnumerable<LocalPackageInfo> packages, string packagesPath) {
      foreach (LocalPackageInfo package in packages) {
        Directory.Delete(Path.Combine(packagesPath, package.PackagePath), true);
      }
    }
    public virtual async Task<IEnumerable<RemotePackageInfo>> GetMissingDependenciesAsync(IEnumerable<LocalPackageInfo> packages, CancellationToken ct) {
      IEnumerable<PackageIdentity> localPackages = packages.Select(x => x.packageIdentity);
      IEnumerable<SourcePackageDependencyInfo> allDependencies = await GetPackageDependenciesAsync(localPackages, true, ct);
      IEnumerable<LocalPackageInfo> latestVersionOfInstalledPackages = packages.GroupBy(x => x.Id).Select(x => x.OrderByDescending(y => y, PackageInfoIdentityComparer.Default).First());
      IEnumerable<PackageIdentity> installedPackages = latestVersionOfInstalledPackages.Select(x => x.packageIdentity);

      IEnumerable<SourcePackageDependencyInfo> resolvedDependencies = ResolveDependencies(Enumerable.Empty<string>(), installedPackages, allDependencies, ct, out bool resolveSucceeded);
      if (!resolveSucceeded) throw new InvalidOperationException("Dependency resolution failed.");

      IEnumerable<SourcePackageDependencyInfo> missingDependencies = resolvedDependencies.Where(x => !packages.Any(y => x.Equals(y.packageIdentity)));
      IEnumerable<IPackageSearchMetadata> packageMetadata = await GetPackagesAsync(missingDependencies, ct);
      packageMetadata = packageMetadata.OrderBy(x => x, PackageSearchMetadataComparer.Default);
      missingDependencies = missingDependencies.OrderBy(x => x, PackageIdentityComparer.Default);
      return packageMetadata.Zip(missingDependencies, (x, y) => new RemotePackageInfo(x, y)).ToArray();
    }
    public virtual async Task<IEnumerable<RemotePackageInfo>> GetPackageUpdatesAsync(IEnumerable<LocalPackageInfo> packages, bool includePreReleases, CancellationToken ct) {
      List<PackageIdentity> updates = new List<PackageIdentity>();
      IEnumerable<(string PackageId, NuGetVersion Version)> latestVersions = await GetLatestVersionsAsync(packages.Select(x => x.Id), includePreReleases, ct);
      foreach (PackageIdentity package in packages.Select(x => x.packageIdentity)) {
        (string PackageId, NuGetVersion Version) latestVersion = latestVersions.Where(x => (x.PackageId == package.Id) && (x.Version.CompareTo(package.Version) > 0)).SingleOrDefault();
        if (latestVersion.Version != null) {
          updates.Add(new PackageIdentity(latestVersion.PackageId, latestVersion.Version));
        }
      }

      IEnumerable<IPackageSearchMetadata> latestPackages = await GetPackagesAsync(updates, ct);
      IEnumerable<SourcePackageDependencyInfo> dependencyInfos = await GetPackageDependenciesAsync(latestPackages.Select(x => x.Identity), false, ct);
      return latestPackages.Zip(dependencyInfos, (x, y) => new RemotePackageInfo(x, y)).ToArray();
    }
    #endregion

    #region GetPackageAsync, GetPackagesAsync, SearchPackagesAsync
    public virtual async Task<IPackageSearchMetadata> GetPackageAsync(PackageIdentity identity,
                                                                      CancellationToken ct) {
      return (await GetPackagesAsync(Enumerable.Repeat(identity, 1), ct)).SingleOrDefault();
    }

    public virtual async Task<IEnumerable<IPackageSearchMetadata>> GetPackagesAsync(IEnumerable<PackageIdentity> identities,
                                                                                    CancellationToken ct) {
      List<IPackageSearchMetadata> packages = new List<IPackageSearchMetadata>();
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        foreach (PackageIdentity identity in identities) {
          foreach (SourceRepository sourceRepository in repositories) {
            PackageMetadataResource packageMetadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>(ct);
            IPackageSearchMetadata package = await packageMetadataResource.GetMetadataAsync(identity, cacheContext, logger, ct);
            if (package != null) {
              packages.Add(package);
              break;
            }
          }
        }
      }
      return packages.Distinct(PackageSearchMetadataComparer.Default).OrderBy(x => x, PackageSearchMetadataComparer.Default).ToArray();
    }
    public virtual async Task<IEnumerable<IPackageSearchMetadata>> GetPackagesAsync(string packageId,
                                                                                    bool includePreReleases,
                                                                                    CancellationToken ct) {
      return await GetPackagesAsync(Enumerable.Repeat(packageId, 1), includePreReleases, ct);
    }
    public virtual async Task<IEnumerable<IPackageSearchMetadata>> GetPackagesAsync(IEnumerable<string> packageIds,
                                                                                    bool includePreReleases,
                                                                                    CancellationToken ct) {
      List<IPackageSearchMetadata> packages = new List<IPackageSearchMetadata>();
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        foreach (string packageId in packageIds) {
          foreach (SourceRepository sourceRepository in repositories) {
            PackageMetadataResource packageMetadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>(ct);
            packages.AddRange(await packageMetadataResource.GetMetadataAsync(packageId, includePreReleases, false, cacheContext, logger, ct));
          }
        }
      }
      return packages.Distinct(PackageSearchMetadataComparer.Default).OrderBy(x => x, PackageSearchMetadataComparer.Default).ToArray();
    }

    public virtual async Task<IEnumerable<(string Repository, IPackageSearchMetadata Package)>> SearchPackagesAsync(string searchString,
                                                                                                                    bool includePreReleases,
                                                                                                                    int skip,
                                                                                                                    int take,
                                                                                                                    CancellationToken ct) {
      List<(string Repository, IPackageSearchMetadata Package)> packages = new List<(string Repository, IPackageSearchMetadata Package)>();
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        foreach (SourceRepository sourceRepository in repositories) {
          PackageSearchResource packageSearchResource = await sourceRepository.GetResourceAsync<PackageSearchResource>(ct);
          SearchFilter filter = new SearchFilter(includePreReleases);
          IEnumerable<IPackageSearchMetadata> searchResult = await packageSearchResource.SearchAsync(searchString, filter, skip, take, logger, ct);
          packages.AddRange(searchResult.Select(x => (Repository: sourceRepository.PackageSource.Source, Package: x)));
        }
      }
      return packages.OrderBy(x => x.Package, PackageSearchMetadataComparer.Default).ToArray();
    }
    #endregion

    #region GetPackageDependenciesAsync
    public virtual async Task<IEnumerable<SourcePackageDependencyInfo>> GetPackageDependenciesAsync(PackageIdentity identity,
                                                                                                    bool getDependenciesRecursively,
                                                                                                    CancellationToken ct) {
      return await GetPackageDependenciesAsync(Enumerable.Repeat(identity, 1), getDependenciesRecursively, ct);
    }
    public virtual async Task<IEnumerable<SourcePackageDependencyInfo>> GetPackageDependenciesAsync(IEnumerable<PackageIdentity> identities,
                                                                                                    bool getDependenciesRecursively,
                                                                                                    CancellationToken ct) {
      HashSet<SourcePackageDependencyInfo> foundDependencies = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        foreach (PackageIdentity identity in identities) {
          if (foundDependencies.Contains(identity)) continue;

          bool found = false;
          foreach (SourceRepository sourceRepository in repositories) {
            DependencyInfoResource dependencyInfoResource = await sourceRepository.GetResourceAsync<DependencyInfoResource>(ct);
            SourcePackageDependencyInfo dependency = await dependencyInfoResource.ResolvePackage(identity, CurrentFramework, cacheContext, logger, ct);
            if (dependency != null) {
              foundDependencies.Add(dependency);
              if (getDependenciesRecursively)
                await GetPackageDependenciesAsync(dependency.Dependencies, getDependenciesRecursively, foundDependencies, cacheContext, ct);
              found = true;
              break;
            }
          }
          if (!found) throw new InvalidOperationException($"Dependencies of package {identity} not found.");
        }
      }
      return foundDependencies.ToArray();
    }
    public virtual async Task<IEnumerable<SourcePackageDependencyInfo>> GetPackageDependenciesAsync(NuGetPackageDependency dependency,
                                                                                                    bool getDependenciesRecursively,
                                                                                                    CancellationToken ct) {
      return await GetPackageDependenciesAsync(Enumerable.Repeat(dependency, 1), getDependenciesRecursively, ct);
    }
    public virtual async Task<IEnumerable<SourcePackageDependencyInfo>> GetPackageDependenciesAsync(IEnumerable<NuGetPackageDependency> dependencies,
                                                                                                    bool getDependenciesRecursively,
                                                                                                    CancellationToken ct) {
      HashSet<SourcePackageDependencyInfo> foundDependencies = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        await GetPackageDependenciesAsync(dependencies, getDependenciesRecursively, foundDependencies, cacheContext, ct);
      }
      return foundDependencies.ToArray();
    }
    private async Task GetPackageDependenciesAsync(IEnumerable<NuGetPackageDependency> dependencies,
                                                   bool getDependenciesRecursively,
                                                   HashSet<SourcePackageDependencyInfo> foundDependencies,
                                                   SourceCacheContext cacheContext,
                                                   CancellationToken ct) {
      foreach (NuGetPackageDependency dependency in dependencies) {
        HashSet<SourcePackageDependencyInfo> satisfyingPackages = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
        foreach (SourceRepository sourceRepository in repositories) {
          // find all satisfying packages
          DependencyInfoResource dependencyInfoResource = await sourceRepository.GetResourceAsync<DependencyInfoResource>(ct);
          satisfyingPackages.AddRange((await dependencyInfoResource.ResolvePackages(dependency.Id, CurrentFramework, cacheContext, logger, ct)).Where(x => dependency.VersionRange.Satisfies(x.Version)));
        }
        if (satisfyingPackages.Count() == 0) throw new InvalidOperationException($"No packages found which satisfy dependency {dependency}.");

        foreach (SourcePackageDependencyInfo package in satisfyingPackages) {
          if (!foundDependencies.Contains(package)) {
            foundDependencies.Add(package);
            if (getDependenciesRecursively)
              await GetPackageDependenciesAsync(package.Dependencies, getDependenciesRecursively, foundDependencies, cacheContext, ct);
          }
        }
      }
    }
    #endregion

    #region ResolveDependencies
    public virtual IEnumerable<SourcePackageDependencyInfo> ResolveDependencies(IEnumerable<string> additionalPackages,
                                                                                IEnumerable<PackageIdentity> existingPackages,
                                                                                IEnumerable<SourcePackageDependencyInfo> availablePackages,
                                                                                CancellationToken cancellationToken,
                                                                                out bool resolveSucceeded) {
      if (additionalPackages == null) additionalPackages = Enumerable.Empty<string>();
      if (existingPackages == null) existingPackages = Enumerable.Empty<PackageIdentity>();

      if ((additionalPackages.Count() == 0) && (existingPackages.Count() == 0)) {
        resolveSucceeded = true;
        return Enumerable.Empty<SourcePackageDependencyInfo>();
      }
      if (availablePackages.Count() == 0) {
        resolveSucceeded = false;
        return Enumerable.Empty<SourcePackageDependencyInfo>();
      }

      PackageResolverContext context = new PackageResolverContext(DependencyBehavior.Lowest,
                                                                  additionalPackages,
                                                                  existingPackages.Select(x => x.Id),
                                                                  Enumerable.Empty<PackageReference>(),
                                                                  existingPackages,
                                                                  availablePackages,
                                                                  repositories.Select(x => x.PackageSource),
                                                                  logger);
      PackageResolver resolver = new PackageResolver();
      IEnumerable<PackageIdentity> resolvedIdentities = Enumerable.Empty<PackageIdentity>();
      try {
        resolvedIdentities = resolver.Resolve(context, cancellationToken);
      }
      catch (NuGetResolverException) {
        resolveSucceeded = false;
        return Enumerable.Empty<SourcePackageDependencyInfo>();
      }
      IEnumerable<SourcePackageDependencyInfo> resolvedDependencies = resolvedIdentities.Select(i => availablePackages.Single(x => PackageIdentityComparer.Default.Equals(i, x)));
      resolveSucceeded = true;
      return resolvedDependencies.ToArray();
    }
    #endregion

    #region GetInstalledPackages, GetPackageDownloaderAsync, InstallPackageAsync, InstallPackagesAsync
    public virtual IEnumerable<PackageFolderReader> GetInstalledPackages(string packagesPath) {
      return Directory.GetDirectories(packagesPath).Select(x => new PackageFolderReader(x));
    }

    public virtual async Task<IPackageDownloader> GetPackageDownloaderAsync(PackageIdentity identity,
                                                                            CancellationToken cancellationToken) {
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        foreach (SourceRepository sourceRepository in repositories) {
          FindPackageByIdResource findPackageByIdResource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
          IPackageDownloader downloader = await findPackageByIdResource.GetPackageDownloaderAsync(identity, cacheContext, logger, cancellationToken);
          if (downloader != null) return downloader;
        }
      }
      return null;
    }

    public virtual async Task InstallPackageAsync(SourcePackageDependencyInfo package,
                                                  string packagesPath,
                                                  string packagesCachePath,
                                                  CancellationToken cancellationToken) {
      await InstallPackagesAsync(Enumerable.Repeat(package, 1), packagesPath, packagesCachePath, cancellationToken);
    }
    public virtual async Task InstallPackagesAsync(IEnumerable<SourcePackageDependencyInfo> packages,
                                                   string packagesPath,
                                                   string packagesCachePath,
                                                   CancellationToken cancellationToken) {
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        foreach (SourcePackageDependencyInfo package in packages) {
          DownloadResource downloadResource = await package.Source.GetResourceAsync<DownloadResource>(cancellationToken);
          PackageDownloadContext downloadContext = new PackageDownloadContext(cacheContext, packagesCachePath, cacheContext.DirectDownload);
          using (DownloadResourceResult downloadResult = await downloadResource.GetDownloadResourceResultAsync(package, downloadContext, packagesCachePath, logger, cancellationToken)) {
            if (downloadResult.Status == DownloadResourceResultStatus.NotFound) throw new InvalidOperationException($"{package} at package source {package.Source.PackageSource} not found.");

            PackagePathResolver packagePathResolver = new PackagePathResolver(packagesPath);
            PackageExtractionContext packageExtractionContext = new PackageExtractionContext(PackageSaveMode.Defaultv3, XmlDocFileSaveMode.Skip, null, logger);
            await PackageExtractor.ExtractPackageAsync(downloadResult.PackageSource, downloadResult.PackageStream, packagePathResolver, packageExtractionContext, cancellationToken);
          }
        }
      }
    }
    #endregion

    #region GetLatestVersionAsync, GetLatestVersionsAsync
    public virtual async Task<NuGetVersion> GetLatestVersionAsync(string packageId,
                                                                  bool includePreReleases,
                                                                  CancellationToken cancellationToken) {
      return (await GetLatestVersionsAsync(Enumerable.Repeat(packageId, 1), includePreReleases, cancellationToken)).SingleOrDefault().Version;
    }
    public virtual async Task<IEnumerable<(string PackageId, NuGetVersion Version)>> GetLatestVersionsAsync(IEnumerable<string> packageIds,
                                                                                                            bool includePreReleases,
                                                                                                            CancellationToken cancellationToken) {
      List<KeyValuePair<string, NuGetVersion>> versions = new List<KeyValuePair<string, NuGetVersion>>();
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        foreach (SourceRepository sourceRepository in repositories) {
          MetadataResource metadataResource = await sourceRepository.GetResourceAsync<MetadataResource>(cancellationToken);
          versions.AddRange(await metadataResource.GetLatestVersions(packageIds, includePreReleases, false, cacheContext, logger, cancellationToken));
        }
      }
      return versions.Where(x => x.Value != null)
                     .GroupBy(x => x.Key)
                     .OrderBy(x => x.Key)
                     .Select(x => (PackageId: x.Key, Version: x.OrderByDescending(y => y.Value, VersionComparer.Default).First().Value))
                     .ToArray();
    }
    #endregion

    #region Helpers
    private static NuGetFramework GetFrameworkFromEntryAssembly() {
      string frameworkName = Assembly.GetEntryAssembly().GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
      return frameworkName != null ? NuGetFramework.ParseFrameworkName(frameworkName, DefaultFrameworkNameProvider.Instance) : NuGetFramework.AnyFramework;
    }
    private static NuGetFramework GetFrameworkFromName(string frameworkName) {
      return NuGetFramework.ParseFrameworkName(frameworkName, DefaultFrameworkNameProvider.Instance);
    }

    private static SourceRepository CreateSourceRepository(string packageSource) {
      return Repository.CreateSource(Repository.Provider.GetCoreV3(), packageSource);
    }

    private static SourceCacheContext CreateSourceCacheContext(bool useCache = true) {
      return useCache ? new SourceCacheContext() : new SourceCacheContext() { NoCache = true, DirectDownload = true };
    }

    #region PackageSearchMetadataEqualityComparer
    private class PackageSearchMetadataComparer : IEqualityComparer<IPackageSearchMetadata>, IComparer<IPackageSearchMetadata> {
      public static PackageSearchMetadataComparer Default => new PackageSearchMetadataComparer();
      private readonly PackageIdentityComparer identityComparer = PackageIdentityComparer.Default;
      public bool Equals(IPackageSearchMetadata x, IPackageSearchMetadata y) {
        return identityComparer.Equals(x.Identity, y.Identity);
      }
      public int GetHashCode(IPackageSearchMetadata obj) {
        return identityComparer.GetHashCode(obj.Identity);
      }
      public int Compare(IPackageSearchMetadata x, IPackageSearchMetadata y) {
        return identityComparer.Compare(x.Identity, y.Identity);
      }
    }
    #endregion
    #endregion
  }
}
