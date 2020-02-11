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
using NuGetPackageDependency = NuGet.Packaging.Core.PackageDependency;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  internal class NuGetConnector {
    private ILogger logger = NullLogger.Instance;

    public ISettings Settings { get; }
    public NuGetFramework CurrentFramework { get; private set; } = GetCurrentFramework();
    public IEnumerable<SourceRepository> Repositories { get; }

    public NuGetConnector(ISettings settings) {
      if (settings == null) throw new ArgumentNullException(nameof(settings));

      Settings = settings;
      Repositories = settings.Repositories.Select(x => CreateSourceRepository(x)).ToArray();
    }

    public void EnableLogging(LogLevel minLevel = LogLevel.Information) {
      logger = new NuGetLogger(minLevel);
    }
    public void DisableLogging() {
      logger = NullLogger.Instance;
    }
    public void ClearLog() {
      (logger as NuGetLogger)?.Clear();
    }
    public string[] GetLog() {
      return (logger as NuGetLogger)?.GetLog() ?? Array.Empty<string>();
    }

    #region GetPackageAsync, GetPackagesAsync, SearchPackagesAsync
    public async Task<IPackageSearchMetadata> GetPackageAsync(PackageIdentity identity, CancellationToken cancellationToken) {
      if (identity == null) throw new ArgumentNullException(nameof(identity));
      if (string.IsNullOrEmpty(identity.Id)) throw new ArgumentException($"{nameof(identity)}.Id is null or empty.", nameof(identity));
      if (!identity.HasVersion) throw new ArgumentException($"{nameof(identity)} has no version.", nameof(identity));

      return (await GetPackagesAsync(Enumerable.Repeat(identity, 1), cancellationToken)).SingleOrDefault();
    }

    public async Task<IEnumerable<IPackageSearchMetadata>> GetPackagesAsync(IEnumerable<PackageIdentity> identities, CancellationToken cancellationToken) {
      if (identities == null) throw new ArgumentNullException(nameof(identities));
      if (identities.Any(x => x == null)) throw new ArgumentException($"{nameof(identities)} contains null elements.", nameof(identities));
      if (identities.Any(x => string.IsNullOrEmpty(x.Id))) throw new ArgumentException($"{nameof(identities)} contains elements whose Id is null or empty.", nameof(identities));
      if (identities.Any(x => !x.HasVersion)) throw new ArgumentException($"{nameof(identities)} contains elements which have no version.", nameof(identities));

      List<IPackageSearchMetadata> packages = new List<IPackageSearchMetadata>();
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        foreach (PackageIdentity identity in identities) {
          foreach (SourceRepository sourceRepository in Repositories) {
            PackageMetadataResource packageMetadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>(cancellationToken);
            IPackageSearchMetadata package = await packageMetadataResource.GetMetadataAsync(identity, cacheContext, logger, cancellationToken);
            if (package != null) {
              packages.Add(package);
              break;
            }
          }
        }
      }
      return packages.Distinct(PackageSearchMetadataComparer.Default).OrderBy(x => x, PackageSearchMetadataComparer.Default).ToArray();
    }
    public async Task<IEnumerable<IPackageSearchMetadata>> GetPackagesAsync(string packageId, bool includePreReleases, CancellationToken cancellationToken) {
      if (packageId == null) throw new ArgumentNullException(nameof(packageId));
      if (packageId == "") throw new ArgumentException($"{nameof(packageId)} is empty.", nameof(packageId));

      return await GetPackagesAsync(Enumerable.Repeat(packageId, 1), includePreReleases, cancellationToken);
    }
    public async Task<IEnumerable<IPackageSearchMetadata>> GetPackagesAsync(IEnumerable<string> packageIds, bool includePreReleases, CancellationToken cancellationToken) {
      if (packageIds == null) throw new ArgumentNullException(nameof(packageIds));
      if (packageIds.Any(x => string.IsNullOrEmpty(x))) throw new ArgumentException($"{nameof(packageIds)} contains elements which are null or empty.", nameof(packageIds));

      List<IPackageSearchMetadata> packages = new List<IPackageSearchMetadata>();
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        foreach (string packageId in packageIds) {
          foreach (SourceRepository sourceRepository in Repositories) {
            PackageMetadataResource packageMetadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>(cancellationToken);
            packages.AddRange(await packageMetadataResource.GetMetadataAsync(packageId, includePreReleases, false, cacheContext, logger, cancellationToken));
          }
        }
      }
      return packages.Distinct(PackageSearchMetadataComparer.Default).OrderBy(x => x, PackageSearchMetadataComparer.Default).ToArray();
    }

    public async Task<IEnumerable<(string Repository, IPackageSearchMetadata Package)>> SearchPackagesAsync(string searchString, bool includePreReleases, int skip, int take, CancellationToken cancellationToken) {
      if (searchString == null) throw new ArgumentNullException(nameof(searchString));

      List<(string Repository, IPackageSearchMetadata Package)> packages = new List<(string Repository, IPackageSearchMetadata Package)>();
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        foreach (SourceRepository sourceRepository in Repositories) {
          PackageSearchResource packageSearchResource = await sourceRepository.GetResourceAsync<PackageSearchResource>(cancellationToken);
          SearchFilter filter = new SearchFilter(includePreReleases);
          IEnumerable<IPackageSearchMetadata> searchResult = await packageSearchResource.SearchAsync(searchString, filter, skip, take, logger, cancellationToken);
          packages.AddRange(searchResult.Select(x => (Repository: sourceRepository.PackageSource.Source, Package: x)));
        }
      }
      return packages.OrderBy(x => x.Package, PackageSearchMetadataComparer.Default).ToArray();
    }
    #endregion

    #region GetPackageDependenciesAsync
    public async Task<IEnumerable<SourcePackageDependencyInfo>> GetPackageDependenciesAsync(PackageIdentity identity, bool getDependenciesRecursively, CancellationToken cancellationToken) {
      if (identity == null) throw new ArgumentNullException(nameof(identity));
      if (string.IsNullOrEmpty(identity.Id)) throw new ArgumentException($"{nameof(identity)}.Id is null or empty.", nameof(identity));
      if (!identity.HasVersion) throw new ArgumentException($"{nameof(identity)} has no version.", nameof(identity));

      return await GetPackageDependenciesAsync(Enumerable.Repeat(identity, 1), getDependenciesRecursively, cancellationToken);
    }
    public async Task<IEnumerable<SourcePackageDependencyInfo>> GetPackageDependenciesAsync(IEnumerable<PackageIdentity> identities, bool getDependenciesRecursively, CancellationToken cancellationToken) {
      if (identities == null) throw new ArgumentNullException(nameof(identities));
      if (identities.Any(x => x == null)) throw new ArgumentException($"{nameof(identities)} contains null elements.", nameof(identities));
      if (identities.Any(x => string.IsNullOrEmpty(x.Id))) throw new ArgumentException($"{nameof(identities)} contains elements whose Id is null or empty.", nameof(identities));
      if (identities.Any(x => !x.HasVersion)) throw new ArgumentException($"{nameof(identities)} contains elements which have no version.", nameof(identities));

      HashSet<SourcePackageDependencyInfo> foundDependencies = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        foreach (PackageIdentity identity in identities) {
          if (foundDependencies.Contains(identity)) continue;

          bool found = false;
          foreach (SourceRepository sourceRepository in Repositories) {
            DependencyInfoResource dependencyInfoResource = await sourceRepository.GetResourceAsync<DependencyInfoResource>(cancellationToken);
            SourcePackageDependencyInfo dependency = await dependencyInfoResource.ResolvePackage(identity, CurrentFramework, cacheContext, logger, cancellationToken);
            if (dependency != null) {
              foundDependencies.Add(dependency);
              if (getDependenciesRecursively)
                await GetPackageDependenciesAsync(dependency.Dependencies, getDependenciesRecursively, foundDependencies, cacheContext, cancellationToken);
              found = true;
              break;
            }
          }
          if (!found) throw new InvalidOperationException($"Dependencies of package {identity.ToString()} not found.");
        }
      }
      return foundDependencies.ToArray();
    }
    public async Task<IEnumerable<SourcePackageDependencyInfo>> GetPackageDependenciesAsync(NuGetPackageDependency dependency, bool getDependenciesRecursively, CancellationToken cancellationToken) {
      if (dependency == null) throw new ArgumentNullException(nameof(dependency));

      return await GetPackageDependenciesAsync(Enumerable.Repeat(dependency, 1), getDependenciesRecursively, cancellationToken);
    }
    public async Task<IEnumerable<SourcePackageDependencyInfo>> GetPackageDependenciesAsync(IEnumerable<NuGetPackageDependency> dependencies, bool getDependenciesRecursively, CancellationToken cancellationToken) {
      if (dependencies == null) throw new ArgumentNullException(nameof(dependencies));
      if (dependencies.Any(x => x == null)) throw new ArgumentException($"{nameof(dependencies)} contains null elements.", nameof(dependencies));

      HashSet<SourcePackageDependencyInfo> foundDependencies = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        await GetPackageDependenciesAsync(dependencies, getDependenciesRecursively, foundDependencies, cacheContext, cancellationToken);
      }
      return foundDependencies.ToArray();
    }
    private async Task GetPackageDependenciesAsync(IEnumerable<NuGetPackageDependency> dependencies, bool getDependenciesRecursively, HashSet<SourcePackageDependencyInfo> foundDependencies, SourceCacheContext cacheContext, CancellationToken cancellationToken) {
      if (dependencies == null) throw new ArgumentNullException(nameof(dependencies));
      if (dependencies.Any(x => x == null)) throw new ArgumentException($"{nameof(dependencies)} contains null elements.", nameof(dependencies));
      if (foundDependencies == null) throw new ArgumentNullException(nameof(foundDependencies));
      if (cacheContext == null) throw new ArgumentNullException(nameof(cacheContext));

      foreach (NuGetPackageDependency dependency in dependencies) {
        HashSet<SourcePackageDependencyInfo> satisfyingPackages = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
        foreach (SourceRepository sourceRepository in Repositories) {
          // find all satisfying packages
          DependencyInfoResource dependencyInfoResource = await sourceRepository.GetResourceAsync<DependencyInfoResource>(cancellationToken);
          satisfyingPackages.AddRange((await dependencyInfoResource.ResolvePackages(dependency.Id, CurrentFramework, cacheContext, logger, cancellationToken)).Where(x => dependency.VersionRange.Satisfies(x.Version)));
        }
        if (satisfyingPackages.Count() == 0) throw new InvalidOperationException($"No packages found which satisfy dependency {dependencies.ToString()}.");

        foreach (SourcePackageDependencyInfo package in satisfyingPackages) {
          if (!foundDependencies.Contains(package)) {
            foundDependencies.Add(package);
            if (getDependenciesRecursively)
              await GetPackageDependenciesAsync(package.Dependencies, getDependenciesRecursively, foundDependencies, cacheContext, cancellationToken);
          }
        }
      }
    }
    #endregion

    #region ResolveDependencies
    public IEnumerable<SourcePackageDependencyInfo> ResolveDependencies(IEnumerable<string> additionalPackages, IEnumerable<PackageIdentity> existingPackages, IEnumerable<SourcePackageDependencyInfo> availablePackages, CancellationToken cancellationToken, out bool resolveSucceeded) {
      if ((additionalPackages == null) && (existingPackages == null)) throw new ArgumentNullException(nameof(additionalPackages) + ", " + nameof(existingPackages));
      if (additionalPackages == null) additionalPackages = Enumerable.Empty<string>();
      if (additionalPackages.Any(x => string.IsNullOrEmpty(x))) throw new ArgumentException($"{nameof(additionalPackages)} contains elements which are null or empty.", nameof(additionalPackages));
      if (existingPackages == null) existingPackages = Enumerable.Empty<PackageIdentity>();
      if (existingPackages.Any(x => x == null)) throw new ArgumentException($"{nameof(existingPackages)} contains elements which are null.", nameof(existingPackages));
      if (existingPackages.Any(x => string.IsNullOrEmpty(x.Id))) throw new ArgumentException($"{nameof(existingPackages)} contains elements whose Id is null or empty.", nameof(existingPackages));
      if (existingPackages.Any(x => !x.HasVersion)) throw new ArgumentException($"{nameof(existingPackages)} contains elements which have no version.", nameof(existingPackages));
      if (existingPackages.GroupBy(x => x.Id).Any(x => x.Count() > 1)) throw new ArgumentException($"{nameof(existingPackages)} contains elements which have the same Id.", nameof(existingPackages));
      if (availablePackages == null) throw new ArgumentNullException(nameof(availablePackages));
      if (availablePackages.Any(x => x == null)) throw new ArgumentException($"{nameof(availablePackages)} contains elements which are null.", nameof(availablePackages));

      PackageResolverContext context = new PackageResolverContext(DependencyBehavior.Highest,
                                                                  additionalPackages,
                                                                  existingPackages.Select(x => x.Id),
                                                                  Enumerable.Empty<PackageReference>(),
                                                                  existingPackages,
                                                                  availablePackages,
                                                                  Repositories.Select(x => x.PackageSource),
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
      return resolvedDependencies;
    }
    #endregion

    #region GetInstalledPackages, GetPackageDownloaderAsync, InstallPackageAsync
    public IEnumerable<PackageFolderReader> GetInstalledPackages() {
      return Directory.GetDirectories(Settings.PackagesPath).Select(x => new PackageFolderReader(x));
    }

    public async Task<IPackageDownloader> GetPackageDownloaderAsync(PackageIdentity identity, CancellationToken cancellationToken) {
      if (identity == null) throw new ArgumentNullException(nameof(identity));
      if (string.IsNullOrEmpty(identity.Id)) throw new ArgumentException($"{nameof(identity)}.Id is null or empty.", nameof(identity));
      if (!identity.HasVersion) throw new ArgumentException($"{nameof(identity)} has no version.", nameof(identity));

      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        foreach (SourceRepository sourceRepository in Repositories) {
          FindPackageByIdResource findPackageByIdResource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
          IPackageDownloader downloader = await findPackageByIdResource.GetPackageDownloaderAsync(identity, cacheContext, logger, cancellationToken);
          if (downloader != null) return downloader;
        }
      }
      return null;
    }

    public async Task InstallPackageAsync(SourcePackageDependencyInfo package, CancellationToken cancellationToken) {
      if (package == null) throw new ArgumentNullException(nameof(package));
      if (string.IsNullOrEmpty(package.Id)) throw new ArgumentException($"{nameof(package)}.Id is null or empty.", nameof(package));
      if (!package.HasVersion) throw new ArgumentException($"{nameof(package)} has no version.", nameof(package));
      if (package.Source == null) throw new ArgumentException($"{nameof(package)}.Source is null.", nameof(package));

      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        DownloadResource downloadResource = await package.Source.GetResourceAsync<DownloadResource>(cancellationToken);
        PackageDownloadContext downloadContext = new PackageDownloadContext(cacheContext, Settings.PackagesCachePath, cacheContext.DirectDownload);
        using (DownloadResourceResult downloadResult = await downloadResource.GetDownloadResourceResultAsync(package, downloadContext, Settings.PackagesCachePath, logger, cancellationToken)) {
          if (downloadResult.Status == DownloadResourceResultStatus.NotFound) throw new ArgumentException($"{nameof(package)} at {nameof(package)}.Source not found.", nameof(package));

          PackagePathResolver packagePathResolver = new PackagePathResolver(Settings.PackagesPath);
          PackageExtractionContext packageExtractionContext = new PackageExtractionContext(PackageSaveMode.Defaultv3, XmlDocFileSaveMode.Skip, null, logger);
          await PackageExtractor.ExtractPackageAsync(downloadResult.PackageSource, downloadResult.PackageStream, packagePathResolver, packageExtractionContext, cancellationToken);
        }
      }
    }
    #endregion

    #region Helpers
    private static NuGetFramework GetCurrentFramework() {
      string frameworkName = Assembly.GetEntryAssembly().GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
      return frameworkName != null ? NuGetFramework.ParseFrameworkName(frameworkName, DefaultFrameworkNameProvider.Instance) : NuGetFramework.AnyFramework;
    }
    internal void SetFrameworkForUnitTests(string frameworkName) {
      // only used for unit tests to correct the current NuGet framework manually
      // Explanation: In unit tests, TargetFrameworkAttribute.FrameworkName of the entry assembly (testhost.dll) returns
      // ".NETCoreApp,Version=v1.0". Consequently, GetCurrentFramework returns .NET Core 1.0 as the current .NET framework
      // for NuGet packages. As HEAL.Bricks is a .NET Standard 2.0 library and applications using HEAL.Bricks therefore
      // have to be at least .NET Core 2.0 or .NET Framework 4.6.1, the detected NuGet framework is wrong and has to be
      // corrected. Otherwise, dependency resultion does not work correctly, as NuGet looks for dependencies of .NET Core 1.0.
      CurrentFramework = NuGetFramework.ParseFrameworkName(frameworkName, DefaultFrameworkNameProvider.Instance);
    }
    private static SourceRepository CreateSourceRepository(string packageSource) {
      return Repository.CreateSource(Repository.Provider.GetCoreV3(), packageSource);
    }
    private static SourceCacheContext CreateSourceCacheContext() {
      return new SourceCacheContext();
    }
    private static SourceCacheContext CreateNoSourceCacheContext() {
      return new SourceCacheContext() { NoCache = true, DirectDownload = true };
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

    #region NuGetLogger
    private class NuGetLogger : ILogger {
      private readonly List<string> log = new List<string>();
      private readonly LogLevel minLevel;

      public NuGetLogger() {
        minLevel = LogLevel.Verbose;
      }
      public NuGetLogger(LogLevel minLevel) {
        this.minLevel = minLevel;
      }

      public string[] GetLog() {
        return log.ToArray();
      }
      public void Clear() {
        log.Clear();
      }

      public void Log(LogLevel level, string data) {
        if (level >= minLevel)
          log.Add($"{level.ToString()}: {data}");
      }
      public void Log(ILogMessage message) {
        if (message.Level >= minLevel)
          log.Add($"[{message.Time.ToString()}] {message.Level.ToString()}: {message.FormatWithCode()}");
      }
      public Task LogAsync(LogLevel level, string data) {
        return Task.Run(() => Log(level, data));
      }
      public Task LogAsync(ILogMessage message) {
        return Task.Run(() => Log(message));
      }
      public void LogDebug(string data) {
        Log(LogLevel.Debug, data);
      }
      public void LogError(string data) {
        Log(LogLevel.Error, data);
      }
      public void LogInformation(string data) {
        Log(LogLevel.Information, data);
      }
      public void LogInformationSummary(string data) {
        Log(LogLevel.Information, $"[Summary] {data}");
      }
      public void LogMinimal(string data) {
        Log(LogLevel.Minimal, data);
      }
      public void LogVerbose(string data) {
        Log(LogLevel.Verbose, data);
      }
      public void LogWarning(string data) {
        Log(LogLevel.Warning, data);
      }
    }
    #endregion
    #endregion
  }
}
