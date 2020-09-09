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
using System.Threading.Tasks;
using Dawn;
using Xunit;
using Xunit.Abstractions;

namespace HEAL.Bricks.XTests {

  [Trait("Category", "Integration")]
  public class PackageManagerIntegrationTests : IClassFixture<RandomTempDirectoryFixture> {
    private readonly ITestOutputHelper output;
    private readonly RandomTempDirectoryFixture tempDir;
    private string TestDir { get; }
    private Settings Settings { get; }

    public PackageManagerIntegrationTests(RandomTempDirectoryFixture tempDir, ITestOutputHelper output) {
      this.output = output;
      this.tempDir = tempDir;

      TestDir = tempDir.CreateRandomSubdirectory();
      string packagesPath = Path.Combine(TestDir, "packages");
      Directory.CreateDirectory(packagesPath);
      string packagesCachePath = Path.Combine(TestDir, "packages_cache");
      Directory.CreateDirectory(packagesCachePath);

      Settings = new Settings {
        PackagesPath = packagesPath,
        PackagesCachePath = packagesCachePath
      };
    }

    #region SearchRemotePackagesAsync
    [Theory]
    [InlineData("PackageId:TestPackage.ListedStable", new string[] { "TestPackage.ListedStable" })]
    public async Task SearchRemotePackagesAsync_WithSearchString_ReturnsFoundPackages(string searchString, string[] expectedPackageNames) {
      IPackageManager pm = PackageManager.Create(Settings);

      IEnumerable<string> result = (await pm.SearchRemotePackagesAsync(searchString, 0, 10)).Select(x => x.Package.Id);

      Assert.Equal(expectedPackageNames.OrderBy(x => x), result.OrderBy(x => x));
    }
    #endregion

    #region GetRemotePackageAsync, GetRemotePackagesAsync
    [Theory]
    [InlineData("TestPackage.ListedStable", "2.0.2")]
    public async Task GetRemotePackageAsync_WithPackageAndVersion_ReturnsPackage(string packageId, string version) {
      IPackageManager pm = PackageManager.Create(Settings);

      RemotePackageInfo result = await pm.GetRemotePackageAsync(packageId, version);

      Assert.Equal(packageId, result?.Id);
      Assert.Equal(version, result?.Version.ToString());
    }
    [Theory]
    [InlineData("TestPackage.ListedStable", true, new string[] { "1.0.0", "2.0.0", "2.0.2", "2.0.6" })]
    [InlineData("TestPackage.AlwaysPrerelease", true, new string[] { "5.0.0-beta" })]
    [InlineData("TestPackage.AlwaysPrerelease", false, new string[] { })]
    public async Task GetRemotePackagesAsync_WithPackage_ReturnsPackages(string packageId, bool includePreReleases, string[] expectedVersions) {
      IPackageManager pm = PackageManager.Create(Settings);

      IEnumerable<RemotePackageInfo> result = await pm.GetRemotePackagesAsync(packageId, includePreReleases);

      Assert.True(result.Select(x => x.Id).All(x => x == packageId));
      Assert.Equal(expectedVersions.OrderBy(x => x), result.Select(x => x.Version.ToString()).OrderBy(x => x));
    }
    #endregion

    #region InstallRemotePackageAsync
    [Theory]
    [InlineData("TestPackage.Depends.SupportingMultipleFrameworks", "1.2.0", false,  new[] { "TestPackage.Depends.SupportingMultipleFrameworks" }, new[] { "1.2.0" }, PackageManagerStatus.InvalidPackages)]
    [InlineData("TestPackage.Depends.SupportingMultipleFrameworks", "1.2.0", true, new[] { "TestPackage.Depends.SupportingMultipleFrameworks", "TestPackage.SupportingMultipleFrameworks" }, new[] { "1.2.0", "1.2.0" }, PackageManagerStatus.OK)]
    public async Task InstallRemotePackageAsync_WithPackage(string packageId, string version, bool installMissingDependencies, string[] expectedPackageNames, string[] expectedVersions, PackageManagerStatus expectedStatus) {
      INuGetConnector nuGetConnector = NuGetConnector.CreateForTests(Constants.netCoreApp31FrameworkName, Settings.Repositories, new XunitLogger(output));
      IPackageManager pm = PackageManager.CreateForTests(Settings, nuGetConnector);
      RemotePackageInfo packageToInstall = await pm.GetRemotePackageAsync(packageId, version);
      var expectedPackages = expectedPackageNames.Zip(expectedVersions).OrderBy(x => x.First);

      await pm.InstallRemotePackageAsync(packageToInstall, installMissingDependencies: installMissingDependencies);

      WriteInstalledPackagesToOutput(pm);
      Assert.Equal(expectedPackages.Select(x => x.First), pm.InstalledPackages.OrderBy(x => x).Select(x => x.Id));
      Assert.Equal(expectedPackages.Select(x => x.Second), pm.InstalledPackages.OrderBy(x => x).Select(x => x.Version.ToString()));
      Assert.Equal(expectedStatus, pm.Status);
    }
    #endregion

    #region RemoveInstalledPackage, RemoveInstalledPackages
    [Theory]
    [InlineData("a", PackageManagerStatus.InvalidPackages)]
    [InlineData("c", PackageManagerStatus.OK)]
    public void RemoveInstalledPackage_WithInstalledPackage(string packageNameToRemove, PackageManagerStatus expectedStatus) {
      LocalPackageInfo[] localPackages = new[] {
        LocalPackageInfo.CreateForTests("a", "1.0.0", packagePath: "packagePath"),
        LocalPackageInfo.CreateForTests("b", "1.0.0", new[] { PackageDependency.CreateForTests("a", "1.0.0") }, packagePath: "packagePath"),
        LocalPackageInfo.CreateForTests("c", "1.0.0", new[] { PackageDependency.CreateForTests("b", "1.0.0") }, packagePath: "packagePath")
      };
      INuGetConnector nuGetConnectorStub = new NuGetConnectorStub(localPackages);
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, nuGetConnectorStub);
      LocalPackageInfo packageToRemove = localPackages.Where(x => x.Id == packageNameToRemove).Single();

      pm.RemoveInstalledPackage(packageToRemove);

      Assert.DoesNotContain(packageToRemove, pm.InstalledPackages);
      Assert.Equal(expectedStatus, pm.Status);
    }
    [Theory]
    [InlineData(new[] { "a", "c" }, PackageManagerStatus.InvalidPackages)]
    [InlineData(new[] { "b", "c" }, PackageManagerStatus.OK)]
    public void RemoveInstalledPackages_WithInstalledPackages(string[] packageNamesToRemove, PackageManagerStatus expectedStatus) {
      LocalPackageInfo[] localPackages = new[] {
        LocalPackageInfo.CreateForTests("a", "1.0.0", packagePath: "packagePath"),
        LocalPackageInfo.CreateForTests("b", "1.0.0", new[] { PackageDependency.CreateForTests("a", "1.0.0") }, packagePath: "packagePath"),
        LocalPackageInfo.CreateForTests("c", "1.0.0", new[] { PackageDependency.CreateForTests("b", "1.0.0") }, packagePath: "packagePath")
      };
      INuGetConnector nuGetConnectorStub = new NuGetConnectorStub(localPackages);
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, nuGetConnectorStub);
      LocalPackageInfo[] packagesToRemove = localPackages.Where(x => packageNamesToRemove.Contains(x.Id)).ToArray();

      pm.RemoveInstalledPackages(packagesToRemove);

      foreach (LocalPackageInfo packageToRemove in packagesToRemove) {
        Assert.DoesNotContain(packageToRemove, pm.InstalledPackages);
      }
      Assert.Equal(expectedStatus, pm.Status);
    }
    #endregion

    #region GetMissingDependenciesAsync, InstallMissingDependenciesAsync
    [Fact]
    public async Task GetMissingDependenciesAsync_WhenDependenciesAreMissing_ReturnsMissingDependencies() {
      LocalPackageInfo[] localPackages = new[] {
        LocalPackageInfo.CreateForTests("d", "1.0.0", new[] { PackageDependency.CreateForTests("c", "1.0.0") })
      };
      RemotePackageInfo[] remotePackages = new[] {
        RemotePackageInfo.CreateForTests("a", "1.0.0"),
        RemotePackageInfo.CreateForTests("b", "1.0.0", new[] { PackageDependency.CreateForTests("a", "1.0.0") }),
        RemotePackageInfo.CreateForTests("c", "1.0.0", new[] { PackageDependency.CreateForTests("b", "1.0.0") }),
        RemotePackageInfo.CreateForTests("d", "1.0.0", new[] { PackageDependency.CreateForTests("c", "1.0.0") })
      };
      INuGetConnector nuGetConnectorStub = new NuGetConnectorStub(localPackages, remotePackages);
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, nuGetConnectorStub);
      RemotePackageInfo[] expectedDependencies = new[] {
        RemotePackageInfo.CreateForTests("a", "1.0.0"),
        RemotePackageInfo.CreateForTests("b", "1.0.0"),
        RemotePackageInfo.CreateForTests("c", "1.0.0")
      };

      IEnumerable<RemotePackageInfo> result = await pm.GetMissingDependenciesAsync();

      Assert.Equal(expectedDependencies.OrderBy(x => x), result.OrderBy(x => x));
    }
    [Fact]
    public async Task GetMissingDependenciesAsync_WhenNoDependenciesAreMissing_ReturnsEmpty() {
      LocalPackageInfo[] localPackages = new[] {
        LocalPackageInfo.CreateForTests("a", "1.0.0"),
        LocalPackageInfo.CreateForTests("b", "1.0.0", new[] { PackageDependency.CreateForTests("a", "1.0.0") }),
        LocalPackageInfo.CreateForTests("c", "1.0.0", new[] { PackageDependency.CreateForTests("b", "1.0.0") }),
        LocalPackageInfo.CreateForTests("d", "1.0.0", new[] { PackageDependency.CreateForTests("c", "1.0.0") })
      };
      RemotePackageInfo[] remotePackages = new[] {
        RemotePackageInfo.CreateForTests("a", "1.0.0"),
        RemotePackageInfo.CreateForTests("b", "1.0.0", new[] { PackageDependency.CreateForTests("a", "1.0.0") }),
        RemotePackageInfo.CreateForTests("c", "1.0.0", new[] { PackageDependency.CreateForTests("b", "1.0.0") }),
        RemotePackageInfo.CreateForTests("d", "1.0.0", new[] { PackageDependency.CreateForTests("c", "1.0.0") })
      };
      INuGetConnector nuGetConnectorStub = new NuGetConnectorStub(localPackages, remotePackages);
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, nuGetConnectorStub);

      IEnumerable<RemotePackageInfo> result = await pm.GetMissingDependenciesAsync();

      Assert.Empty(result);
    }
    [Fact]
    public async Task InstallMissingDependenciesAsync_WhenDependenciesAreMissing() {
      LocalPackageInfo[] localPackages = new[] {
        LocalPackageInfo.CreateForTests("d", "1.0.0", new[] { PackageDependency.CreateForTests("c", "1.0.0") })
      };
      RemotePackageInfo[] remotePackages = new[] {
        RemotePackageInfo.CreateForTests("a", "1.0.0"),
        RemotePackageInfo.CreateForTests("b", "1.0.0", new[] { PackageDependency.CreateForTests("a", "1.0.0") }),
        RemotePackageInfo.CreateForTests("c", "1.0.0", new[] { PackageDependency.CreateForTests("b", "1.0.0") }),
        RemotePackageInfo.CreateForTests("d", "1.0.0", new[] { PackageDependency.CreateForTests("c", "1.0.0") })
      };
      INuGetConnector nuGetConnectorStub = new NuGetConnectorStub(localPackages, remotePackages);
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, nuGetConnectorStub);
      LocalPackageInfo[] expectedPackages = new[] {
        LocalPackageInfo.CreateForTests("a", "1.0.0"),
        LocalPackageInfo.CreateForTests("b", "1.0.0"),
        LocalPackageInfo.CreateForTests("c", "1.0.0"),
        LocalPackageInfo.CreateForTests("d", "1.0.0")
      };

      await pm.InstallMissingDependenciesAsync();

      Assert.Equal(expectedPackages.OrderBy(x => x), pm.InstalledPackages.OrderBy(x => x));
      Assert.Equal(PackageManagerStatus.OK, pm.Status);
    }
    [Fact]
    public async Task InstallMissingDependenciesAsync_WhenNoDependenciesAreMissing() {
      LocalPackageInfo[] localPackages = new[] {
        LocalPackageInfo.CreateForTests("a", "1.0.0"),
        LocalPackageInfo.CreateForTests("b", "1.0.0", new[] { PackageDependency.CreateForTests("a", "1.0.0") }),
        LocalPackageInfo.CreateForTests("c", "1.0.0", new[] { PackageDependency.CreateForTests("b", "1.0.0") }),
        LocalPackageInfo.CreateForTests("d", "1.0.0", new[] { PackageDependency.CreateForTests("c", "1.0.0") })
      };
      RemotePackageInfo[] remotePackages = new[] {
        RemotePackageInfo.CreateForTests("a", "1.0.0"),
        RemotePackageInfo.CreateForTests("b", "1.0.0", new[] { PackageDependency.CreateForTests("a", "1.0.0") }),
        RemotePackageInfo.CreateForTests("c", "1.0.0", new[] { PackageDependency.CreateForTests("b", "1.0.0") }),
        RemotePackageInfo.CreateForTests("d", "1.0.0", new[] { PackageDependency.CreateForTests("c", "1.0.0") })
      };
      INuGetConnector nuGetConnectorStub = new NuGetConnectorStub(localPackages, remotePackages);
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, nuGetConnectorStub);

      await pm.InstallMissingDependenciesAsync();

      Assert.Equal(localPackages.OrderBy(x => x), pm.InstalledPackages.OrderBy(x => x));
      Assert.Equal(PackageManagerStatus.OK, pm.Status);
    }
    #endregion

    #region GetPackageUpdateAsync, GetPackageUpdatesAsync, InstallPackageUpdatesAsync
    [Theory]
    [InlineData("a", "1.0.0", true, "a", "2.1.0-alpha.1")]
    [InlineData("a", "1.0.0", false, "a", "2.0.0")]
    [InlineData("a", "2.1.0-alpha.1", true, null, null)]
    [InlineData("a", "2.1.0-alpha.1", false, null, null)]
    [InlineData("a", "2.0.0", false, null, null)]
    [InlineData("b", "1.0.0", true, null, null)]
    [InlineData("b", "1.0.0", false, null, null)]
    public async Task GetPackageUpdateAsync_WithPackage_ReturnsLatestUpdateOrNull(string packageToUpdate, string versionToUpdate, bool includePreReleases, string expectedPackage, string expectedVersion) {
      RemotePackageInfo[] remotePackages = new[] {
        RemotePackageInfo.CreateForTests("a", "1.0.0"),
        RemotePackageInfo.CreateForTests("a", "1.1.0-alpha.1"),
        RemotePackageInfo.CreateForTests("a", "1.1.0"),
        RemotePackageInfo.CreateForTests("a", "2.0.0-alpha.1"),
        RemotePackageInfo.CreateForTests("a", "2.0.0"),
        RemotePackageInfo.CreateForTests("a", "2.1.0-alpha.1")
      };
      LocalPackageInfo localPackage = LocalPackageInfo.CreateForTests(packageToUpdate, versionToUpdate);
      INuGetConnector nuGetConnectorStub = new NuGetConnectorStub(remotePackages);
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, nuGetConnectorStub);

      RemotePackageInfo result = await pm.GetPackageUpdateAsync(localPackage, includePreReleases: includePreReleases);

      if (expectedPackage == null) {
        Assert.Null(result);
      } else {
        Assert.Equal(expectedPackage, result.Id);
        Assert.Equal(expectedVersion, result.Version.ToString());
      }
    }
    [Theory]
    [InlineData(true, new[] { "a", "b" }, new string[] { "2.1.0-alpha.1", "2.0.0" })]
    [InlineData(false, new[] { "a", "b" }, new string[] { "2.0.0", "2.0.0" })]
    public async Task GetPackageUpdatesAsync_WhenUpdatesArePending_ReturnsLatestUpdates(bool includePreReleases, string[] expectedPackageNames, string[] expectedVersions) {
      LocalPackageInfo[] localPackages = new[] {
        LocalPackageInfo.CreateForTests("a", "1.0.0"),
        LocalPackageInfo.CreateForTests("b", "1.0.0"),
        LocalPackageInfo.CreateForTests("c", "2.0.0")
      };
      RemotePackageInfo[] remotePackages = new[] {
        RemotePackageInfo.CreateForTests("a", "1.0.0"),
        RemotePackageInfo.CreateForTests("a", "2.0.0"),
        RemotePackageInfo.CreateForTests("a", "2.1.0-alpha.1"),
        RemotePackageInfo.CreateForTests("b", "1.0.0"),
        RemotePackageInfo.CreateForTests("b", "1.1.0-alpha.1"),
        RemotePackageInfo.CreateForTests("b", "2.0.0"),
        RemotePackageInfo.CreateForTests("c", "2.0.0")
      };
      RemotePackageInfo[] expectedPackages = expectedPackageNames.Select((n, i) => RemotePackageInfo.CreateForTests(n, expectedVersions[i])).ToArray();
      INuGetConnector nuGetConnectorStub = new NuGetConnectorStub(localPackages, remotePackages);
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, nuGetConnectorStub);

      IEnumerable<RemotePackageInfo> result = await pm.GetPackageUpdatesAsync(includePreReleases: includePreReleases);

      Assert.Equal(expectedPackages.OrderBy(x => x.Id), result.OrderBy(x => x.Id));
    }
    [Theory]
    [InlineData(true,  true,  new[] { "a", "b", "c", "x", "y", "z" }, new[] { "2.1.0-alpha.1", "2.0.0", "2.0.0", "2.0.0", "2.0.0", "2.0.0" }, PackageManagerStatus.OK)]
    [InlineData(true,  false, new[] { "a", "b", "c", "x", "y", "z" }, new[] { "2.0.0",         "2.0.0", "2.0.0", "2.0.0", "2.0.0", "2.0.0" }, PackageManagerStatus.OK)]
    [InlineData(false, true,  new[] { "a", "b", "c"                }, new[] { "2.1.0-alpha.1", "2.0.0", "2.0.0"                            }, PackageManagerStatus.InvalidPackages)]
    [InlineData(false, false, new[] { "a", "b", "c"                }, new[] { "2.0.0"        , "2.0.0", "2.0.0"                            }, PackageManagerStatus.InvalidPackages)]
    public async Task InstallPackageUpdatesAsync_WhenUpdatesArePending(bool installMissingDependencies, bool includePreReleases, string[] expectedPackageNames, string[] expectedVersions, PackageManagerStatus expectedStatus) {
      LocalPackageInfo[] localPackages = new[] {
        LocalPackageInfo.CreateForTests("a", "1.0.0"),
        LocalPackageInfo.CreateForTests("b", "1.0.0"),
        LocalPackageInfo.CreateForTests("c", "2.0.0", new[] { PackageDependency.CreateForTests("y", "1.0.0") })
      };
      RemotePackageInfo[] remotePackages = new[] {
        RemotePackageInfo.CreateForTests("a", "1.0.0"),
        RemotePackageInfo.CreateForTests("a", "2.0.0"),
        RemotePackageInfo.CreateForTests("a", "2.1.0-alpha.1"),
        RemotePackageInfo.CreateForTests("b", "1.0.0"),
        RemotePackageInfo.CreateForTests("b", "1.1.0-alpha.1"),
        RemotePackageInfo.CreateForTests("b", "2.0.0", new[] { PackageDependency.CreateForTests("x", "1.0.0") }),
        RemotePackageInfo.CreateForTests("c", "2.0.0", new[] { PackageDependency.CreateForTests("y", "1.0.0") }),
        RemotePackageInfo.CreateForTests("x", "2.0.0", new[] { PackageDependency.CreateForTests("z", "1.0.0") }),
        RemotePackageInfo.CreateForTests("y", "2.0.0"),
        RemotePackageInfo.CreateForTests("z", "2.0.0")
      };
      LocalPackageInfo[] expectedPackages = expectedPackageNames.Select((n, i) => LocalPackageInfo.CreateForTests(n, expectedVersions[i])).ToArray();
      INuGetConnector nuGetConnectorStub = new NuGetConnectorStub(localPackages, remotePackages);
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, nuGetConnectorStub);

      await pm.InstallPackageUpdatesAsync(installMissingDependencies: installMissingDependencies, includePreReleases: includePreReleases);

      WriteInstalledPackagesToOutput(pm);
      Assert.Equal(expectedPackages.OrderBy(x => x.Id), pm.InstalledPackages.Where(x => x.Status != PackageStatus.Outdated).OrderBy(x => x.Id));
      Assert.Equal(expectedStatus, pm.Status);
    }
    #endregion

    #region GetPackageLoadInfo, GetPackageLoadInfos
    [Fact]
    public void GetPackageLoadInfo_WithPackage_ReturnsPackageLoadInfo() {
      LocalPackageInfo[] localPackages = new[] {
        LocalPackageInfo.CreateForTests("a", "1.0.0", referenceItems: new[] { "path/to/assemblyA1", "path/to/assemblyA1" })
      };
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, new NuGetConnectorStub(localPackages));

      PackageLoadInfo result = pm.GetPackageLoadInfo(localPackages[0]);

      Assert.Equal(localPackages[0].Id, result.Id);
      Assert.Equal(localPackages[0].Version.ToString(), result.Version);
      Assert.Equal(localPackages[0].ReferenceItems, result.AssemblyPaths);
    }
    [Fact]
    public void GetPackageLoadInfos_ReturnsPackageLoadInfos() {
      LocalPackageInfo[] localPackages = new[] {
        LocalPackageInfo.CreateForTests("a", "1.0.0", referenceItems: new[] { "path/to/assemblyA1", "path/to/assemblyA2" }),
        LocalPackageInfo.CreateForTests("b", "1.0.0", referenceItems: new[] { "path/to/assemblyB1", "path/to/assemblyB2" }),
        LocalPackageInfo.CreateForTests("c", "1.0.0", referenceItems: new[] { "path/to/assemblyC1", "path/to/assemblyC2" }, frameworkNotSupported: true)
      };
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, new NuGetConnectorStub(localPackages));

      IEnumerable<PackageLoadInfo> result = pm.GetPackageLoadInfos();

      Assert.Collection(result.OrderBy(x => x.Id),
        first => {
          Assert.Equal(localPackages[0].Id, first.Id);
          Assert.Equal(localPackages[0].Version.ToString(), first.Version);
          Assert.Equal(localPackages[0].ReferenceItems, first.AssemblyPaths);
        },
        second => {
          Assert.Equal(localPackages[1].Id, second.Id);
          Assert.Equal(localPackages[1].Version.ToString(), second.Version);
          Assert.Equal(localPackages[1].ReferenceItems, second.AssemblyPaths);
        }
      );
    }
    #endregion

    #region Helpers
    private void WriteInstalledPackagesToOutput(IPackageManager pm) {
      output.WriteLine("PackageManager.InstalledPackages:");
      foreach (var p in pm.InstalledPackages) {
        output.WriteLine(p.ToStringWithDependencies());
      }
    }
    #endregion
  }
}
