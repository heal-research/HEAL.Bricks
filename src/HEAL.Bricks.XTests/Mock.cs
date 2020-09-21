#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGetPackageDependency = NuGet.Packaging.Core.PackageDependency;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGet.Configuration;

namespace HEAL.Bricks.XTests {
  public static class Mock {
    public static Mock<T> Of<T>() where T : class {
      return new Mock<T>();
    }

    #region INuGetConnector
    internal static INuGetConnector EmptyNuGetConnector() {
      Mock<INuGetConnector> mock = new Mock<INuGetConnector>();
      mock.Setup(m => m.GetLocalPackages(It.IsAny<string>())).Returns(Enumerable.Empty<LocalPackageInfo>());
      return mock.Object;
    }

    internal static Mock<INuGetConnector> GetLocalPackages(this Mock<INuGetConnector> mock, string packagesPathToVerify, params LocalPackageInfo[] packages) {
      mock.Setup(m => m.GetLocalPackages(It.Is<string>(s => s == packagesPathToVerify))).Returns(packages);
      return mock;
    }
    internal static Mock<INuGetConnector> GetLocalPackages(this Mock<INuGetConnector> mock, params LocalPackageInfo[] packages) {
      mock.Setup(m => m.GetLocalPackages(It.IsAny<string>())).Returns(packages);
      return mock;
    }
    internal static Mock<INuGetConnector> InstallRemotePackagesAsync(this Mock<INuGetConnector> mock) {
      mock.Setup(m => m.InstallRemotePackagesAsync(It.IsAny<IEnumerable<RemotePackageInfo>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
      return mock;
    }
    #endregion

    #region SourceRepository, PackageMetadataResource, DependencyInfoResource, PackageSearchResource
    public static SourceRepository CreateSourceRepositoryMock(string packageSource, params (PackageIdentity Id, NuGetPackageDependency[] Dependencies)[] packages) {
      var metadata = packages.Select(x => CreatePackageSearchMetadata(x.Id)).ToArray();
      var dependencies = packages.Select(x => CreateSourcePackageDependenyInfo(x.Id, x.Dependencies)).ToArray();
      var repository = Mock.Of<SourceRepository>()
                           .PackageSource(packageSource)
                           .GetResourceAsync(Mock.Of<PackageMetadataResource>().GetMetadataAsync(metadata))
                           .GetResourceAsync(Mock.Of<DependencyInfoResource>().ResolvePackage(dependencies))
                           .GetResourceAsync(Mock.Of<PackageSearchResource>().SearchAsync(metadata));
      return repository.Object;
    }

    public static Mock<SourceRepository> GetResourceAsync<T>(this Mock<SourceRepository> mock, Mock<T> resource) where T : class, INuGetResource {
      mock.Setup(x => x.GetResourceAsync<T>(It.IsAny<CancellationToken>())).Returns(Task.FromResult(resource.Object));
      return mock;
    }
    public static Mock<SourceRepository> PackageSource(this Mock<SourceRepository> mock, string packageSource) {
      mock.Setup(x => x.PackageSource).Returns(new PackageSource(packageSource));
      return mock;
    }

    public static Mock<PackageMetadataResource> GetMetadataAsync(this Mock<PackageMetadataResource> mock, params IPackageSearchMetadata[] packages) {
      mock.Setup(x => x.GetMetadataAsync(It.IsAny<PackageIdentity>(), It.IsAny<SourceCacheContext>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()))
          .Returns<PackageIdentity, SourceCacheContext, ILogger, CancellationToken>((id, cache, logger, token) => {
            IPackageSearchMetadata package = packages.Where(p => p.Identity.Equals(id)).FirstOrDefault();
            if (package == null) {
              logger.Log(LogLevel.Warning, $"Package '{id}' not found.");
            } else {
              logger.Log(LogLevel.Information, $"Returned IPackageSearchMetadata of package '{id}'.");
            }
            return Task.FromResult(package);
          });
      mock.Setup(x => x.GetMetadataAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<SourceCacheContext>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()))
          .Returns<string, bool, bool, SourceCacheContext, ILogger, CancellationToken>((id, includePreReleases, includeUnlisted, cache, logger, token) => {
            IEnumerable<IPackageSearchMetadata> matches = packages.Where(p => p.Identity.Id.Equals(id));
            if (!includePreReleases) {
              matches = matches.Where(x => !x.Identity.Version.IsPrerelease);
            }
            if (matches.Count() == 0) {
              logger.Log(LogLevel.Warning, $"Package '{id}' not found.");
            }
            else {
              logger.Log(LogLevel.Information, $"Returned IPackageSearchMetadata of package '{id}'.");
            }
            return Task.FromResult(matches);
          });
      return mock;
    }
    public static Mock<DependencyInfoResource> ResolvePackage(this Mock<DependencyInfoResource> mock, params SourcePackageDependencyInfo[] dependencies) {
      mock.Setup(x => x.ResolvePackage(It.IsAny<PackageIdentity>(), It.IsAny<NuGetFramework>(), It.IsAny<SourceCacheContext>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()))
          .Returns<PackageIdentity, NuGetFramework, SourceCacheContext, ILogger, CancellationToken>((id, cache, framework, logger, token) => {
            SourcePackageDependencyInfo dependency = dependencies.Where(d => (d.Id == id.Id) && (d.Version.Equals(id.Version))).FirstOrDefault();
            if (dependency == null) {
              logger.Log(LogLevel.Warning, $"Dependencies of package '{id}' not found.");
            } else {
              logger.Log(LogLevel.Information, $"Returned SourcePackageDependencyInfo of package '{id}'.");
            }
            return Task.FromResult(dependency);
          });
      return mock;
    }
    public static Mock<PackageSearchResource> SearchAsync(this Mock<PackageSearchResource> mock, params IPackageSearchMetadata[] packages) {
      mock.Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<SearchFilter>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()))
          .Returns<string, SearchFilter, int, int, ILogger, CancellationToken>((searchTerm, filters, skip, take, logger, token) => {
            IEnumerable<IPackageSearchMetadata> matches = packages.Where(p => p.Identity.Id.Contains(searchTerm));
            if (!filters.IncludePrerelease) {
              matches = matches.Where(x => !x.Identity.Version.IsPrerelease);
            }
            if (matches.Count() == 0) {
              logger.Log(LogLevel.Warning, $"Search for '{searchTerm}' returned no packages.");
            }
            else {
              logger.Log(LogLevel.Information, $"Search for '{searchTerm}' returned {matches.Count()} packages.");
            }
            matches = matches.Skip(skip).Take(take);
            return Task.FromResult(matches);
          });
      return mock;
    }
    #endregion

    #region Helpers
    public static PackageIdentity CreatePackageIdentity(string id, string version) {
      return new PackageIdentity(id, new NuGetVersion(version));
    }
    public static NuGetPackageDependency CreateDependency(string id, string minVersion) {
      return new NuGetPackageDependency(id, new VersionRange(new NuGetVersion(minVersion)));
    }
    public static (PackageIdentity Id, NuGetPackageDependency[] Dependencies) CreatePackage(string id, string version, params (string Dependency, string MinVersion)[] dependencies) {
      return (CreatePackageIdentity(id, version), dependencies.Select(x => CreateDependency(x.Dependency, x.MinVersion)).ToArray());
    }
    private static IPackageSearchMetadata CreatePackageSearchMetadata(PackageIdentity id) {
      return PackageSearchMetadataBuilder.FromIdentity(id).Build();
    }
    private static SourcePackageDependencyInfo CreateSourcePackageDependenyInfo(PackageIdentity id, IEnumerable<NuGetPackageDependency> dependencies, SourceRepository repository = null) {
      return new SourcePackageDependencyInfo(id.Id, id.Version, dependencies, false, repository);
    }
    #endregion
  }
}
