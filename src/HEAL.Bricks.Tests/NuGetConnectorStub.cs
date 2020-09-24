#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace HEAL.Bricks.Tests {
  public class NuGetConnectorStub : INuGetConnector {
    private readonly List<LocalPackageInfo> localPackages = new List<LocalPackageInfo>();
    private readonly List<RemotePackageInfo> remotePackages = new List<RemotePackageInfo>();

    public NuGetConnectorStub() { }
    public NuGetConnectorStub(IEnumerable<LocalPackageInfo> localPackages) : this(localPackages, Enumerable.Empty<RemotePackageInfo>()) { }
    public NuGetConnectorStub(IEnumerable<RemotePackageInfo> remotePackages) : this(Enumerable.Empty<LocalPackageInfo>(), remotePackages) { }
    public NuGetConnectorStub(IEnumerable<LocalPackageInfo> localPackages, IEnumerable<RemotePackageInfo> remotePackages) {
      this.localPackages.AddRange(localPackages);
      this.remotePackages.AddRange(remotePackages);
    }

    public IEnumerable<LocalPackageInfo> GetLocalPackages(string packagesPath) {
      return localPackages;
    }

    public Task<RemotePackageInfo> GetRemotePackageAsync(string packageId, string version, CancellationToken ct) {
      return Task.FromResult(remotePackages.Where(x => (x.Id == packageId) && (x.Version.ToString() == version)).SingleOrDefault());
    }

    public Task<IEnumerable<RemotePackageInfo>> GetRemotePackagesAsync(string packageId, bool includePreReleases, CancellationToken ct) {
      return Task.FromResult(includePreReleases ? remotePackages.Where(x => (x.Id == packageId)) : remotePackages.Where(x => (x.Id == packageId) && !x.Version.IsPrerelease));
    }

    public Task<IEnumerable<(string Repository, RemotePackageInfo Package)>> SearchRemotePackagesAsync(string searchString, int skip, int take, bool includePreReleases, CancellationToken ct) {
      return Task.FromResult(remotePackages.Where(x => (x.Id.Contains(searchString))).Select(x => (Repository: x.Source, Package: x)));
    }

    public Task InstallRemotePackagesAsync(IEnumerable<RemotePackageInfo> packages, string packagesPath, string packagesCachePath, CancellationToken ct) {
      foreach (var package in packages)
        localPackages.Add(LocalPackageInfo.CreateForTestsFromRemotePackageInfo(package));
      return Task.CompletedTask;
    }

    public void RemoveLocalPackages(IEnumerable<LocalPackageInfo> packages, string packagesPath) {
      bool ok = true;
      foreach (var package in packages) {
        ok = ok && localPackages.Remove(package);
      }
      if (!ok) throw new DirectoryNotFoundException("Package not found");
    }

    public Task<IEnumerable<RemotePackageInfo>> GetMissingDependenciesAsync(IEnumerable<LocalPackageInfo> packages, CancellationToken ct) {
      List<PackageInfo> pkgs = new List<PackageInfo>(packages);
      List<RemotePackageInfo> missing = new List<RemotePackageInfo>();
      bool newMissingFound;
      do {
        newMissingFound = false;
        foreach (var package in pkgs) {
          foreach (var dependency in package.Dependencies) {
            if (!pkgs.Any(x => (x.Id == dependency.Id) && dependency.VersionRange.Satiesfies(x.Version))) {
              PackageVersion bestVersion = dependency.VersionRange.FindBestMatch(remotePackages.Where(x => x.Id == dependency.Id).Select(x => x.Version));
              if (bestVersion != null) {
                missing.Add(remotePackages.Where(x => (x.Id == dependency.Id) && (x.Version.Equals(bestVersion))).Single());
                newMissingFound = true;
              }
            }
          }
        }
        foreach (var m in missing) {
          if (!pkgs.Contains(m)) pkgs.Add(m);
        }
      } while (newMissingFound);
      return Task.FromResult(missing.AsEnumerable());
    }

    public Task<IEnumerable<RemotePackageInfo>> GetPackageUpdatesAsync(IEnumerable<LocalPackageInfo> packages, bool includePreReleases, CancellationToken ct) {
      List<RemotePackageInfo> updates = new List<RemotePackageInfo>();
      foreach (var package in packages) {
        RemotePackageInfo update = remotePackages.Where(x => (x.Id == package.Id) && (x.Version.CompareTo(package.Version) > 0) && (includePreReleases || !x.Version.IsPrerelease))
                                                 .OrderByDescending(x => x.Version).FirstOrDefault();
        if (update != null) updates.Add(update);
      }
      return Task.FromResult(updates.AsEnumerable());
    }

    #region Unsupported Members
    public NuGetFramework CurrentFramework => throw new NotImplementedException();

    public IEnumerable<PackageFolderReader> GetInstalledPackages(string packagesPath) {
      throw new NotImplementedException();
    }
    public Task<NuGetVersion> GetLatestVersionAsync(string packageId, bool includePreReleases, CancellationToken cancellationToken) {
      throw new NotImplementedException();
    }
    public Task<IEnumerable<(string PackageId, NuGetVersion Version)>> GetLatestVersionsAsync(IEnumerable<string> packageIds, bool includePreReleases, CancellationToken cancellationToken) {
      throw new NotImplementedException();
    }
    public Task<IPackageSearchMetadata> GetPackageAsync(PackageIdentity identity, CancellationToken ct) {
      throw new NotImplementedException();
    }
    public Task<IEnumerable<SourcePackageDependencyInfo>> GetPackageDependenciesAsync(PackageIdentity identity, bool getDependenciesRecursively, CancellationToken ct) {
      throw new NotImplementedException();
    }
    public Task<IEnumerable<SourcePackageDependencyInfo>> GetPackageDependenciesAsync(IEnumerable<PackageIdentity> identities, bool getDependenciesRecursively, CancellationToken ct) {
      throw new NotImplementedException();
    }
    public Task<IEnumerable<SourcePackageDependencyInfo>> GetPackageDependenciesAsync(NuGet.Packaging.Core.PackageDependency dependency, bool getDependenciesRecursively, CancellationToken ct) {
      throw new NotImplementedException();
    }
    public Task<IEnumerable<SourcePackageDependencyInfo>> GetPackageDependenciesAsync(IEnumerable<NuGet.Packaging.Core.PackageDependency> dependencies, bool getDependenciesRecursively, CancellationToken ct) {
      throw new NotImplementedException();
    }
    public Task<IPackageDownloader> GetPackageDownloaderAsync(PackageIdentity identity, CancellationToken cancellationToken) {
      throw new NotImplementedException();
    }
    public Task<IEnumerable<IPackageSearchMetadata>> GetPackagesAsync(IEnumerable<PackageIdentity> identities, CancellationToken ct) {
      throw new NotImplementedException();
    }
    public Task<IEnumerable<IPackageSearchMetadata>> GetPackagesAsync(string packageId, bool includePreReleases, CancellationToken ct) {
      throw new NotImplementedException();
    }
    public Task<IEnumerable<IPackageSearchMetadata>> GetPackagesAsync(IEnumerable<string> packageIds, bool includePreReleases, CancellationToken ct) {
      throw new NotImplementedException();
    }
    public Task InstallPackageAsync(SourcePackageDependencyInfo package, string packagesPath, string packagesCachePath, CancellationToken cancellationToken) {
      throw new NotImplementedException();
    }
    public Task InstallPackagesAsync(IEnumerable<SourcePackageDependencyInfo> packages, string packagesPath, string packagesCachePath, CancellationToken cancellationToken) {
      throw new NotImplementedException();
    }
    public IEnumerable<SourcePackageDependencyInfo> ResolveDependencies(IEnumerable<string> additionalPackages, IEnumerable<PackageIdentity> existingPackages, IEnumerable<SourcePackageDependencyInfo> availablePackages, CancellationToken cancellationToken, out bool resolveSucceeded) {
      throw new NotImplementedException();
    }
    public Task<IEnumerable<(string Repository, IPackageSearchMetadata Package)>> SearchPackagesAsync(string searchString, bool includePreReleases, int skip, int take, CancellationToken ct) {
      throw new NotImplementedException();
    }
    #endregion
  }
}
