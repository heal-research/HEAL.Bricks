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
    public static IPluginManager Create(params string[] remoteRepositories) {
      return new PluginManager(remoteRepositories);
    }

    private readonly NuGetConnector nuGetConnector;
    public IEnumerable<string> RemoteRepositories {
      get { return nuGetConnector?.RemoteRepositories.Select(x => x.PackageSource.Source) ?? Enumerable.Empty<string>(); }
    }
    public IEnumerable<PackageInfo> Packages { get; private set; }

    private PluginManager() {
      Packages = Enumerable.Empty<PackageInfo>();
    }
    private PluginManager(params string[] remoteRepositories) : this() {
      nuGetConnector = new NuGetConnector(remoteRepositories);
    }
    internal PluginManager(NuGetConnector nuGetConnector) : this() {
      this.nuGetConnector = nuGetConnector;
    }

    public async Task InitializeAsync(string pluginTag, CancellationToken cancellationToken = default) {
      List<PackageInfo> packages = new List<PackageInfo>();
      foreach (IPackageSearchMetadata localPackage in await nuGetConnector.GetLocalPackagesAsync(true, cancellationToken)) {
        SourcePackageDependencyInfo dependencies = (await nuGetConnector.GetPackageDependenciesAsync(localPackage.Identity, nuGetConnector.LocalRepositories, false, cancellationToken)).Single();
        packages.Add(new PackageInfo(localPackage, dependencies.Dependencies, pluginTag));
      }
//      IEnumerable<SourcePackageDependencyInfo> resolvedDependencies = nuGetConnector.ResolveDependencies(localPackages.Select(x => x.Identity.Id), allDependencies, cancellationToken, out bool resolveFailed);
      
      foreach (PackageInfo package in packages) {
        foreach (PackageDependency dependency in package.Dependencies) {
          dependency.Status = packages.Any(x => (x.Id == dependency.Id) && dependency.VersionRange.Satiesfies(x.Version)) ? PackageDependencyStatus.OK : PackageDependencyStatus.LocalMissing;
        }
        package.Status = package.Dependencies.Any(x => x.Status == PackageDependencyStatus.LocalMissing) ? PackageStatus.DependenciesMissing : PackageStatus.OK;
      }
      bool packageStatusChanged;
      do {
        packageStatusChanged = false;
        foreach (PackageInfo package in packages.Where(x => x.Status == PackageStatus.OK)) {
          foreach (PackageDependency dependency in package.Dependencies.Where(x => x.Status == PackageDependencyStatus.OK)) {
            if (packages.Where(x => (x.Id == dependency.Id) && dependency.VersionRange.Satiesfies(x.Version)).All(x => x.Status == PackageStatus.DependenciesMissing || x.Status == PackageStatus.IndirectDependenciesMissing)) {
              package.Status = PackageStatus.IndirectDependenciesMissing;
              packageStatusChanged = true;
            }
          }
        }
      } while (packageStatusChanged);
      Packages = packages;
    }

    public async Task<bool> DownloadPackageAsync(PackageInfo package, string targetFolder, CancellationToken cancellationToken = default) {
      string path = Path.Combine(targetFolder, package.Id + "." + package.Version.ToString() + ".nupkg");
      using (IPackageDownloader downloader = await nuGetConnector.GetPackageDownloaderAsync(package.nuGetPackageMetadata.Identity, cancellationToken)) {
        return await downloader.CopyNupkgFileToAsync(path, cancellationToken);
      }
    }
  }
}
