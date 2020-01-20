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
    public static IPluginManager Create(string pluginTag, params string[] remoteRepositories) {
      return new PluginManager(pluginTag, remoteRepositories);
    }

    private readonly NuGetConnector nuGetConnector;
    public IEnumerable<string> RemoteRepositories {
      get { return nuGetConnector?.RemoteRepositories.Select(x => x.PackageSource.Source) ?? Enumerable.Empty<string>(); }
    }
    public string PluginTag { get; } = "HEALBricksPlugin";
    public IEnumerable<PackageInfo> Packages { get; private set; } = Enumerable.Empty<PackageInfo>();
    public IEnumerable<PackageInfo> Plugins => Packages.Where(x => x.IsPlugin);
    public PluginManagerStatus Status { get; private set; } = PluginManagerStatus.Uninitialized;

    private PluginManager(string pluginTag) {
      PluginTag = pluginTag ?? "";
    }
    private PluginManager(string pluginTag, params string[] remoteRepositories) : this(pluginTag) {
      nuGetConnector = new NuGetConnector(remoteRepositories);
    }
    internal PluginManager(string pluginTag, NuGetConnector nuGetConnector) : this(pluginTag) {
      // only used for unit tests, if a specially initialized NuGetConnector is required
      this.nuGetConnector = nuGetConnector ?? throw new ArgumentNullException(nameof(nuGetConnector));
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default) {
      (bool ResolveSucceeded, HashSet<PackageInfo> Packages) readLocalPackagesResult = await ReadLocalPackages(nuGetConnector, PluginTag, cancellationToken);
      UpdatePackageAndDependencyStatus(readLocalPackagesResult.Packages);
      PluginManagerStatus status = readLocalPackagesResult.ResolveSucceeded ? GetPluginManagerStatus(readLocalPackagesResult.Packages) : PluginManagerStatus.ResolveFailed;
      Packages = readLocalPackagesResult.Packages;
      Status = status;
    }
    public async Task ResolveMissingDependenciesAsync(CancellationToken cancellationToken = default) {
      if (Status == PluginManagerStatus.Uninitialized) await InitializeAsync();
      
      IEnumerable<IPackageSearchMetadata> localPackages = Packages.Select(x => x.nuGetPackageMetadata);
      IEnumerable<SourcePackageDependencyInfo> allDependencies = await nuGetConnector.GetPackageDependenciesAsync(localPackages.Select(x => x.Identity), nuGetConnector.AllRepositories, true, cancellationToken);
      IEnumerable<SourcePackageDependencyInfo> resolvedDependencies = nuGetConnector.ResolveDependencies(localPackages.Where(x => x.Tags.Contains(PluginTag)).Select(x => x.Identity.Id), allDependencies, cancellationToken, out bool resolveSucceeded);
      IEnumerable<SourcePackageDependencyInfo> dependencies = resolveSucceeded ? resolvedDependencies : allDependencies;

      HashSet<PackageInfo> packages = new HashSet<PackageInfo>(Packages, PackageInfoIdentityComparer.Default);
      foreach (SourcePackageDependencyInfo dependency in dependencies.Where(x => !localPackages.Any(y => y.Identity.Id == x.Id && y.Identity.Version == x.Version))) {
        IPackageSearchMetadata packageMetadata = await nuGetConnector.GetPackageAsync(new PackageIdentity(dependency.Id, dependency.Version), nuGetConnector.AllRepositories, cancellationToken);
        packages.Add(new PackageInfo(packageMetadata, dependency.Dependencies, PluginTag) { Status = PackageStatus.LocalMissing });
      }
      UpdatePackageAndDependencyStatus(packages);
      PluginManagerStatus status = resolveSucceeded ? GetPluginManagerStatus(Packages) : PluginManagerStatus.ResolveFailed;
      Packages = packages;
      Status = status;
    }
    public async Task DownloadMissingDependenciesAsync(CancellationToken cancellationToken = default) {
      if (Status == PluginManagerStatus.Uninitialized) await InitializeAsync();

      foreach (PackageInfo missingPackage in Packages.Where(x => x.Status == PackageStatus.LocalMissing)) {
        using (IPackageDownloader downloader = await nuGetConnector.GetPackageDownloaderAsync(missingPackage.nuGetPackageMetadata.Identity, cancellationToken)) {
          string path = Path.Combine(nuGetConnector.LocalRepositories.First().PackageSource.Source, missingPackage.Id + "." + missingPackage.Version.ToString() + ".nupkg");
          bool ok = await downloader.CopyNupkgFileToAsync(path, cancellationToken);
          if (!ok) throw new InvalidOperationException($"Download of package {missingPackage.ToString()} failed.");
        }
      }
    }

    public async Task<bool> DownloadPackageAsync(PackageInfo package, string targetFolder, CancellationToken cancellationToken = default) {
      string path = Path.Combine(targetFolder, package.Id + "." + package.Version.ToString() + ".nupkg");
      using (IPackageDownloader downloader = await nuGetConnector.GetPackageDownloaderAsync(package.nuGetPackageMetadata.Identity, cancellationToken)) {
        return await downloader.CopyNupkgFileToAsync(path, cancellationToken);
      }
    }

    #region Static Helpers
    private static async Task<(bool ResolveSucceeded, HashSet<PackageInfo> Packages)> ReadLocalPackages(NuGetConnector nuGetConnector, string pluginTag, CancellationToken cancellationToken) {
      IEnumerable<IPackageSearchMetadata> localPackages = await nuGetConnector.GetLocalPackagesAsync(true, cancellationToken);
      IEnumerable<SourcePackageDependencyInfo> localDependencies = await nuGetConnector.GetPackageDependenciesAsync(localPackages.Select(x => x.Identity), nuGetConnector.LocalRepositories, true, cancellationToken);
      IEnumerable<SourcePackageDependencyInfo> resolvedDependencies = nuGetConnector.ResolveDependencies(localPackages.Where(x => x.Tags.Contains(pluginTag)).Select(x => x.Identity.Id), localDependencies, cancellationToken, out bool resolveSucceeded);
      IEnumerable<SourcePackageDependencyInfo> dependencies = resolveSucceeded ? resolvedDependencies : localDependencies;

      HashSet<PackageInfo> packages = new HashSet<PackageInfo>(PackageInfoIdentityComparer.Default);
      foreach (SourcePackageDependencyInfo package in dependencies) {
        packages.Add(new PackageInfo(localPackages.Single(x => x.Identity.Id == package.Id && x.Identity.Version == package.Version), package.Dependencies, pluginTag));
      }
      return (resolveSucceeded, packages);
    }

    private static void UpdatePackageAndDependencyStatus(IEnumerable<PackageInfo> packages) {
      foreach (PackageInfo package in packages) {
        foreach (PackageDependency dependency in package.Dependencies) {
          dependency.Status = packages.Any(x => (x.Id == dependency.Id) && dependency.VersionRange.Satiesfies(x.Version) && x.Status != PackageStatus.LocalMissing) ? PackageDependencyStatus.OK : PackageDependencyStatus.LocalMissing;
        }
        if (package.Status != PackageStatus.LocalMissing) {
          if (package.Dependencies.Count() == 0) {
            package.Status = PackageStatus.OK;
          } else if (package.Dependencies.Any(x => x.Status == PackageDependencyStatus.LocalMissing)) {
            package.Status = PackageStatus.DependenciesMissing;
          } else {
            package.Status = PackageStatus.Unknown;
          }
        }
      }
      bool packageStatusChanged;
      do {
        packageStatusChanged = false;
        foreach (PackageInfo package in packages.Where(x => x.Status == PackageStatus.Unknown)) {
          foreach (PackageDependency dependency in package.Dependencies.Where(x => x.Status == PackageDependencyStatus.OK)) {
            var dependencyPackages = packages.Where(x => (x.Id == dependency.Id) && dependency.VersionRange.Satiesfies(x.Version));
            if (dependencyPackages.Any(x => x.Status == PackageStatus.Unknown)) {
              package.Status = PackageStatus.Unknown;
              break;
            } else if (dependencyPackages.All(x => x.Status == PackageStatus.DependenciesMissing || x.Status == PackageStatus.IndirectDependenciesMissing || x.Status == PackageStatus.LocalMissing)) {
              package.Status = PackageStatus.IndirectDependenciesMissing;
              break;
            } else if (dependencyPackages.Any(x => x.Status == PackageStatus.OK)) {
              package.Status = PackageStatus.OK;
            }
          }
          packageStatusChanged = packageStatusChanged || package.Status != PackageStatus.Unknown;
        }
      } while (packageStatusChanged);
    }

    private static PluginManagerStatus GetPluginManagerStatus(IEnumerable<PackageInfo> packages) {
      IEnumerable<PackageInfo> plugins = packages.Where(x => x.IsPlugin);
      if (plugins.All(x => x.Status == PackageStatus.OK)) {
        return PluginManagerStatus.OK;
      } else if (plugins.Any(x => x.Status == PackageStatus.Unknown)) {
        return PluginManagerStatus.Unknown;
      } else if (plugins.Any(x => x.Status == PackageStatus.DependenciesMissing || x.Status == PackageStatus.IndirectDependenciesMissing || x.Status == PackageStatus.LocalMissing)) {
        return PluginManagerStatus.DependenciesMissing;
      } else {
        return PluginManagerStatus.Unknown;
      }
    }
    #endregion
  }
}
