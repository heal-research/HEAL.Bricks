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

    public string AppDirectory => GetAppDirectory();
    public NuGetFramework CurrentFramework { get; private set; } = GetCurrentFramework();
    public IEnumerable<SourceRepository> LocalRepositories { get; }
    public IEnumerable<SourceRepository> RemoteRepositories { get; }
    public IEnumerable<SourceRepository> AllRepositories {
      get {
        return LocalRepositories.Concat(RemoteRepositories);
      }
    }

    //TODO: Add method to filter packages for those which support the current framework

    public NuGetConnector(IEnumerable<string> localRepositoriesRelativePaths, IEnumerable<string> remoteRepositories) {
      LocalRepositories = localRepositoriesRelativePaths.Select(x => Path.Combine(AppDirectory, x)).Select(y => CreateSourceRepository(y)).ToArray();
      RemoteRepositories = remoteRepositories.Select(x => CreateSourceRepository(x)).ToArray();
    }
    public NuGetConnector(params string[] remoteRepositories) : this(Enumerable.Repeat(GetAppDirectory(), 1), remoteRepositories) { }

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

    public async Task<IPackageSearchMetadata> GetPackageAsync(PackageIdentity identity, IEnumerable<SourceRepository> sourceRepositories, CancellationToken cancellationToken) {
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        foreach (SourceRepository sourceRepository in sourceRepositories) {
          PackageMetadataResource packageMetadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>(cancellationToken);
          IPackageSearchMetadata package = await packageMetadataResource.GetMetadataAsync(identity, cacheContext, logger, cancellationToken);
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
          packages = packages.Concat(await packageMetadataResource.GetMetadataAsync(packageId, includePreReleases, false, cacheContext, logger, cancellationToken));
        }
      }
      return packages.Distinct(PackageSearchMetadataEqualityComparer.Default);
    }

    public async Task<IEnumerable<IPackageSearchMetadata>> GetLocalPackagesAsync(bool includePreReleases, CancellationToken cancellationToken) {
      IEnumerable<IPackageSearchMetadata> latestPackages = Enumerable.Empty<IPackageSearchMetadata>();
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        foreach (SourceRepository sourceRepository in LocalRepositories) {
          PackageSearchResource packageSearchResource = await sourceRepository.GetResourceAsync<PackageSearchResource>(cancellationToken);
          SearchFilter filter = new SearchFilter(includePreReleases);
          latestPackages = latestPackages.Concat(await packageSearchResource.SearchAsync("", filter, 0, int.MaxValue, logger, cancellationToken));
        }
      }
      latestPackages = latestPackages.Distinct(PackageSearchMetadataEqualityComparer.Default);

      IEnumerable<IPackageSearchMetadata> allPackages = Enumerable.Empty<IPackageSearchMetadata>();
      foreach (IPackageSearchMetadata latestPackage in latestPackages) {
        allPackages = allPackages.Concat(await GetPackagesAsync(latestPackage.Identity.Id, LocalRepositories, includePreReleases, cancellationToken));
      }
      return allPackages.Distinct(PackageSearchMetadataEqualityComparer.Default);
    }

    public async Task<IEnumerable<IPackageSearchMetadata>> SearchRemotePackagesAsync(string searchString, bool includePreReleases, CancellationToken cancellationToken) {
      IEnumerable<IPackageSearchMetadata> packages = Enumerable.Empty<IPackageSearchMetadata>();
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        foreach (SourceRepository sourceRepository in RemoteRepositories) {
          PackageSearchResource packageSearchResource = await sourceRepository.GetResourceAsync<PackageSearchResource>(cancellationToken);
          SearchFilter filter = new SearchFilter(includePreReleases);
          packages = packages.Concat(await packageSearchResource.SearchAsync(searchString, filter, 0, int.MaxValue, logger, cancellationToken));
        }
      }
      return packages.Distinct(PackageSearchMetadataEqualityComparer.Default);
    }

    public async Task<IEnumerable<SourcePackageDependencyInfo>> GetPackageDependenciesAsync(PackageIdentity identity, IEnumerable<SourceRepository> sourceRepositories, bool resolveDependenciesRecursively, CancellationToken cancellationToken) {
      return await GetPackageDependenciesAsync(Enumerable.Repeat(identity, 1), sourceRepositories, resolveDependenciesRecursively, cancellationToken);
    }
    public async Task<IEnumerable<SourcePackageDependencyInfo>> GetPackageDependenciesAsync(IEnumerable<PackageIdentity> identities, IEnumerable<SourceRepository> sourceRepositories, bool resolveDependenciesRecursively, CancellationToken cancellationToken) {
      HashSet<SourcePackageDependencyInfo> resolvedDependencies = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {

        async Task ResolvePackageDependenciesAsync(PackageIdentity id) {
          if (resolvedDependencies.Contains(id)) return;
          foreach (SourceRepository sourceRepository in sourceRepositories) {
            DependencyInfoResource dependencyInfoResource = await sourceRepository.GetResourceAsync<DependencyInfoResource>(cancellationToken);
            SourcePackageDependencyInfo dependencies = await dependencyInfoResource.ResolvePackage(id, CurrentFramework, cacheContext, logger, cancellationToken);
            if (dependencies != null) {
              resolvedDependencies.Add(dependencies);
              if (resolveDependenciesRecursively) {
                foreach (NuGet.Packaging.Core.PackageDependency dependency in dependencies.Dependencies) {
                  await ResolvePackageDependenciesAsync(new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion));
                }
              }
              return;
            }
          }
        };

        foreach (PackageIdentity identity in identities) {
          await ResolvePackageDependenciesAsync(identity);
        }
      }
      return resolvedDependencies;
    }

    public async Task<IPackageDownloader> GetPackageDownloaderAsync(PackageIdentity identity, CancellationToken cancellationToken) {
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        foreach (SourceRepository sourceRepository in RemoteRepositories) {
          FindPackageByIdResource findPackageByIdResource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
          IPackageDownloader downloader = await findPackageByIdResource.GetPackageDownloaderAsync(identity, cacheContext, logger, cancellationToken);
          if (downloader != null) return downloader;
        }
      }
      return null;
    }

    public async Task<DownloadResourceResult> DownloadPackageAsync(SourcePackageDependencyInfo package, CancellationToken cancellationToken) {
      using (SourceCacheContext cacheContext = CreateSourceCacheContext()) {
        DownloadResource downloadResource = await package.Source.GetResourceAsync<DownloadResource>(cancellationToken);
        string packagesFolder = Path.Combine(GetAppDirectory(), "packages");
        return await downloadResource.GetDownloadResourceResultAsync(package, new PackageDownloadContext(cacheContext), packagesFolder, logger, cancellationToken);
      }
    }

    public async Task InstallPackageAsync(DownloadResourceResult package, CancellationToken cancellationToken) {
      var packagePathResolver = new PackagePathResolver(GetAppDirectory());
      var packageExtractionContext = new PackageExtractionContext(PackageSaveMode.Defaultv3, XmlDocFileSaveMode.Skip, null, logger);
      await PackageExtractor.ExtractPackageAsync(package.PackageSource, package.PackageStream, packagePathResolver, packageExtractionContext, cancellationToken);
    }

    public IEnumerable<SourcePackageDependencyInfo> ResolveDependencies(IEnumerable<string> requiredIds, IEnumerable<SourcePackageDependencyInfo> availablePackages, CancellationToken cancellationToken, out bool resolveSucceeded) {
      PackageResolverContext context = new PackageResolverContext(DependencyBehavior.Highest,
                                                                  requiredIds,
                                                                  Enumerable.Empty<string>(),
                                                                  Enumerable.Empty<PackageReference>(),
                                                                  Enumerable.Empty<PackageIdentity>(),
                                                                  availablePackages,
                                                                  AllRepositories.Select(x => x.PackageSource),
                                                                  logger);
      PackageResolver resolver = new PackageResolver();
      IEnumerable<PackageIdentity> resolvedIdentities = Enumerable.Empty<PackageIdentity>();
      try {
        resolvedIdentities = resolver.Resolve(context, cancellationToken);
      }
      catch (NuGetResolverConstraintException) {
        resolveSucceeded = false;
        return Enumerable.Empty<SourcePackageDependencyInfo>();
      }
      IEnumerable<SourcePackageDependencyInfo> resolvedDependencies = resolvedIdentities.Select(i => availablePackages.Single(x => PackageIdentityComparer.Default.Equals(i, x)));
      resolveSucceeded = true;
      return resolvedDependencies;
    }

    private static string GetAppDirectory() {
      return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
    }
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
      return new SourceCacheContext() { NoCache = true };
    }

    #region PackageSearchMetadataEqualityComparer
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
  }
}
