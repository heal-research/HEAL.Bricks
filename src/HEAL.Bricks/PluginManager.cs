#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
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
    public static IPluginManager Create(params string[] remoteRepositories) {
      return new PluginManager(remoteRepositories);
    }

    private readonly NuGetConnector nuGetConnector;
    public IEnumerable<string> RemoteRepositories { get; }


    public PluginManager(params string[] remoteRepositories) {
      RemoteRepositories = remoteRepositories;
      nuGetConnector = new NuGetConnector(remoteRepositories);
    }

    private class PackageMetadata {
      public IPackageSearchMetadata Metadata { get; }
      public SourcePackageDependencyInfo Dependencies { get; }
      public PackageMetadata(IPackageSearchMetadata metadata, SourcePackageDependencyInfo dependencies) {
        this.Metadata = metadata;
        this.Dependencies = dependencies;
      }
    }

    public async Task<IEnumerable<IPluginInfo>> GetLocalPluginsAsync(string pluginTag, CancellationToken cancellationToken = default) {
      IEnumerable<IPluginInfo> plugins = Enumerable.Empty<IPluginInfo>();
      IEnumerable<IPackageSearchMetadata> packages = await nuGetConnector.GetLocalPackagesAsync(true, cancellationToken);
      foreach (IPackageSearchMetadata package in packages.Where(x => string.IsNullOrEmpty(pluginTag) || x.Tags.Contains(pluginTag))) {
        IEnumerable<SourcePackageDependencyInfo> dependencies = await nuGetConnector.GetPackageDependenciesAsync(package.Identity, nuGetConnector.LocalRepositories, false, cancellationToken);
        plugins = plugins.Append(new PluginInfo(package, dependencies.Single()));
      }
      return plugins;
    }

    public async Task<IEnumerable<IPluginInfo>> GetRemotePluginsAsync(string searchString, bool includePreReleases = true, bool allVersions = true, CancellationToken cancellationToken = default) {
      IEnumerable<IPluginInfo> plugins = Enumerable.Empty<IPluginInfo>();
      IEnumerable<IPackageSearchMetadata> packages = await nuGetConnector.SearchRemotePackagesAsync(searchString, includePreReleases, cancellationToken);
      foreach (IPackageSearchMetadata package in packages) {
        IEnumerable<SourcePackageDependencyInfo> dependencies = await nuGetConnector.GetPackageDependenciesAsync(package.Identity, nuGetConnector.RemoteRepositories, false, cancellationToken);
        plugins = plugins.Append(new PluginInfo(package, dependencies.Single()));
      }
      return plugins;
    }

    public async Task<IEnumerable<IPluginInfo>> GetLocalPluginDependenciesAsync(string pluginTag, CancellationToken cancellationToken = default) {
      string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      SourceRepository sourceRepository = new SourceRepository(new PackageSource(appDir), Repository.Provider.GetCoreV3());
      ListResource listResource = await sourceRepository.GetResourceAsync<ListResource>();
      IEnumeratorAsync<IPackageSearchMetadata> enumerator = (await listResource.ListAsync("", true, true, false, NullLogger.Instance, cancellationToken)).GetEnumeratorAsync();
      HashSet<SourcePackageDependencyInfo> dependencies = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
      using (SourceCacheContext cacheContext = new SourceCacheContext()) {
        while (await enumerator.MoveNextAsync()) {
          IPackageSearchMetadata package = enumerator.Current;
          if (string.IsNullOrEmpty(pluginTag) || package.Tags.Contains(pluginTag)) {
            await GetPackageDependenciesAsync(package.Identity, nuGetConnector.CurrentFramework, cacheContext, appDir, dependencies, cancellationToken);
            await GetPackageDependenciesAsync(package.Identity, nuGetConnector.CurrentFramework, cacheContext, "https://api.nuget.org/v3/index.json", dependencies, cancellationToken);
          }
        }
      }
      return dependencies.Select(x => new PluginInfo(x.Id, x.Version.ToString()));
    }

    internal async Task GetPackageDependenciesAsync(PackageIdentity package, NuGetFramework framework, SourceCacheContext cacheContext, string packageSource, ISet<SourcePackageDependencyInfo> packages, CancellationToken cancellationToken) {
      if (packages.Contains(package)) return;

      SourceRepository sourceRepository = new SourceRepository(new PackageSource(packageSource), Repository.Provider.GetCoreV3());
      var dependencyInfoResource = await sourceRepository.GetResourceAsync<DependencyInfoResource>();
      SourcePackageDependencyInfo dependencyInfo = await dependencyInfoResource.ResolvePackage(package, framework, cacheContext, NullLogger.Instance, cancellationToken);
      if (dependencyInfo == null) return;

      packages.Add(dependencyInfo);
      foreach (var dependency in dependencyInfo.Dependencies) {
        await GetPackageDependenciesAsync(new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion), framework, cacheContext, packageSource, packages, cancellationToken);
      }
    }

    private async Task<IEnumerable<PackageMetadata>> GetPackageDependenciesAsync(PackageIdentity package, NuGetFramework framework, SourceCacheContext cacheContext, IEnumerable<SourceRepository> sourceRepositories, bool resolveDependencies, CancellationToken cancellationToken) {
      foreach (SourceRepository sourceRepository in sourceRepositories) {
        PackageMetadataResource packageMetadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>();
        IPackageSearchMetadata metadata = await packageMetadataResource.GetMetadataAsync(package, cacheContext, NullLogger.Instance, cancellationToken);
        if (metadata != null) {
          return await GetPackageDependenciesAsync(metadata, framework, cacheContext, sourceRepositories, resolveDependencies, cancellationToken);
        }
      }
      return Enumerable.Empty<PackageMetadata>();
    }

    private async Task<IEnumerable<PackageMetadata>> GetPackageDependenciesAsync(IPackageSearchMetadata package, NuGetFramework framework, SourceCacheContext cacheContext, IEnumerable<SourceRepository> sourceRepositories, bool resolveDependencies, CancellationToken cancellationToken) {
      foreach (SourceRepository sourceRepository in sourceRepositories) {
        IEnumerable<PackageMetadata> packages = Enumerable.Empty<PackageMetadata>();
        DependencyInfoResource dependencyInfoResource = await sourceRepository.GetResourceAsync<DependencyInfoResource>();
        SourcePackageDependencyInfo dependencies = await dependencyInfoResource.ResolvePackage(package.Identity, framework, cacheContext, NullLogger.Instance, cancellationToken);
        if (dependencies != null) {
          packages = packages.Append(new PackageMetadata(package, dependencies));
          if (resolveDependencies) {
            foreach (PackageDependency dependency in dependencies.Dependencies) {
              packages = packages.Concat(await GetPackageDependenciesAsync(new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion), framework, cacheContext, sourceRepositories, resolveDependencies, cancellationToken));
            }
          }
        }
        return packages;
      }
      return Enumerable.Empty<PackageMetadata>();
    }

    public async Task<bool> DownloadPluginAsync(IPluginInfo pluginInfo, string targetFolder, string packageSource = "https://api.nuget.org/v3/index.json", CancellationToken cancellationToken = default) {
      PackageIdentity identity = new PackageIdentity(pluginInfo.Name, NuGetVersion.Parse(pluginInfo.Version));
      SourceRepository sourceRepository = new SourceRepository(new PackageSource(packageSource), Repository.Provider.GetCoreV3());
      DownloadResource downloadResource = await sourceRepository.GetResourceAsync<DownloadResource>();
      using (SourceCacheContext sourceCacheContext = new SourceCacheContext { NoCache = true }) {
        PackageDownloadContext downloadContext = new PackageDownloadContext(sourceCacheContext, targetFolder, true);
        using (DownloadResourceResult result = await downloadResource.GetDownloadResourceResultAsync(identity, downloadContext, "", NullLogger.Instance, cancellationToken)) {

          if ((result.Status == DownloadResourceResultStatus.Cancelled) || (result.Status == DownloadResourceResultStatus.NotFound))
            return false;

          using (FileStream targetFile = new FileStream(Path.Combine(targetFolder, $"{pluginInfo.Name}.{pluginInfo.Version}.nupkg"), FileMode.Create)) {
            await result.PackageStream.CopyToAsync(targetFile, 81920, cancellationToken);
            targetFile.Close();
          }
        }
      }
      return true;
    }
  }
}
