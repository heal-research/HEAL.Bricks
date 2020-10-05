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
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace HEAL.Bricks.Tests {
  [Trait("Category", "Unit")]
  public class PackageManagerUnitTests {
    private readonly ITestOutputHelper output;

    public PackageManagerUnitTests(ITestOutputHelper output) {
      this.output = output;
    }

    #region Create
    [Fact]
    public void Create_WithNullParameter_ThrowsArgumentNullException() {
      var e = Assert.Throws<ArgumentNullException>(() => PackageManager.Create(null));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Fact]
    public void Create_WithSettingsWhereRepositoriesIsEmpty_ThrowsArgumentException() {
      Settings settings = new Settings();
      settings.Repositories.Clear();

      var e = Assert.Throws<ArgumentException>(() => PackageManager.Create(settings));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithSettingsWhereRepositoriesContainsNullOrEmpty_ThrowsArgumentException(string repository) {
      Settings settings = new Settings();
      settings.Repositories.Add((repository, string.Empty, string.Empty));

      var e = Assert.Throws<ArgumentException>(() => PackageManager.Create(settings));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    #endregion

    #region CreateForTests
    [Fact]
    public void CreateForTests_ReturnsInstanceWhereStatusIsOK() {
      ISettings settings = new Settings() {
        PackagesPath = Path.GetTempPath()
      };
      LocalPackageInfo[] packages = new[] {
        LocalPackageInfo.CreateForTests("a", "1.0.0")
      };
      Mock<INuGetConnector> nuGetConnectorMock = Mock.Of<INuGetConnector>().GetLocalPackages(settings.PackagesPath, packages);

      IPackageManager pm = PackageManager.CreateForTests(settings, nuGetConnectorMock.Object);

      WriteInstalledPackagesToOutput(pm);
      nuGetConnectorMock.VerifyAll();
      Assert.NotNull(pm);
      Assert.NotNull(pm.Settings);
      Assert.Equal(packages.OrderBy(x => x), pm.InstalledPackages.OrderBy(x => x));
      Assert.True(pm.InstalledPackages.All(x => x.Status == PackageStatus.OK));
      Assert.Equal(PackageManagerStatus.OK, pm.Status);
    }
    [Fact]
    public void CreateForTests_WithoutPackages_ReturnsInstanceWhereStatusIsOK() {
      INuGetConnector nuGetConnector = new NuGetConnectorStub();

      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, nuGetConnector);

      Assert.Empty(pm.InstalledPackages);
      Assert.Equal(PackageManagerStatus.OK, pm.Status);
    }
    [Fact]
    public void CreateForTests_WithSinglePackage_ReturnsInstanceWhereStatusIsOK() {
      LocalPackageInfo[] localPackages = new[] {
        LocalPackageInfo.CreateForTests("a", "1.0.0")
      };
      INuGetConnector nuGetConnector = new NuGetConnectorStub(localPackages);

      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, nuGetConnector);

      WriteInstalledPackagesToOutput(pm);
      Assert.Equal(localPackages.OrderBy(x => x), pm.InstalledPackages.OrderBy(x => x));
      Assert.True(pm.InstalledPackages.All(x => x.Status == PackageStatus.OK));
      Assert.Equal(PackageManagerStatus.OK, pm.Status);
    }
    [Fact]
    public void CreateForTests_WithDependencies_ReturnsInstanceWhereStatusIsOK() {
      LocalPackageInfo[] localPackages = new[] {
        LocalPackageInfo.CreateForTests("a", "2.0.0"),
        LocalPackageInfo.CreateForTests("b", "1.0.0", new[] { PackageDependency.CreateForTests("a", "1.0.0") }),
      };
      INuGetConnector nuGetConnector = new NuGetConnectorStub(localPackages);

      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, nuGetConnector);

      WriteInstalledPackagesToOutput(pm);
      Assert.Equal(localPackages.OrderBy(x => x), pm.InstalledPackages.OrderBy(x => x));
      Assert.True(pm.InstalledPackages.All(x => x.Status == PackageStatus.OK));
      Assert.Equal(PackageDependencyStatus.OK, pm.InstalledPackages.Where(x => x.Id == "b").Single().Dependencies.First().Status);
      Assert.Equal(PackageManagerStatus.OK, pm.Status);
    }
    [Fact]
    public void CreateForTests_WithOutdatedPackages_ReturnsInstanceWhereStatusIsOK() {
      LocalPackageInfo[] localPackages = new[] {
        LocalPackageInfo.CreateForTests("a", "1.0.0"),
        LocalPackageInfo.CreateForTests("a", "2.0.0-alpha.1"),
        LocalPackageInfo.CreateForTests("a", "2.0.0")
      };
      INuGetConnector nuGetConnector = new NuGetConnectorStub(localPackages);

      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, nuGetConnector);

      WriteInstalledPackagesToOutput(pm);
      Assert.Equal(localPackages.OrderBy(x => x), pm.InstalledPackages.OrderBy(x => x));
      Assert.Equal(PackageStatus.OK, pm.InstalledPackages.Where(x => x.Version.ToString() == "2.0.0").Single().Status);
      Assert.Equal(PackageStatus.Outdated, pm.InstalledPackages.Where(x => x.Version.ToString() == "2.0.0-alpha.1").Single().Status);
      Assert.Equal(PackageStatus.Outdated, pm.InstalledPackages.Where(x => x.Version.ToString() == "1.0.0").Single().Status);
      Assert.Equal(PackageManagerStatus.OK, pm.Status);
    }
    [Fact]
    public void CreateForTests_WithMissingDependencies_ReturnsInstanceWhereStatusIsInvalidPackages() {
      LocalPackageInfo[] localPackages = new[] {
        LocalPackageInfo.CreateForTests("a", "2.0.0"),
        LocalPackageInfo.CreateForTests("b", "1.0.0", new[] { PackageDependency.CreateForTests("a", "2.1.0") }),
        LocalPackageInfo.CreateForTests("c", "1.0.0", new[] { PackageDependency.CreateForTests("x", "1.0.0") }),
        LocalPackageInfo.CreateForTests("d", "1.0.0", new[] { PackageDependency.CreateForTests("c", "0.9.0") }),
      };
      INuGetConnector nuGetConnector = new NuGetConnectorStub(localPackages);

      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, nuGetConnector);

      WriteInstalledPackagesToOutput(pm);
      Assert.Equal(localPackages.OrderBy(x => x), pm.InstalledPackages.OrderBy(x => x));
      Assert.Equal(PackageStatus.OK, pm.InstalledPackages.Where(x => x.Id == "a").Single().Status);
      Assert.Equal(PackageStatus.DependenciesMissing, pm.InstalledPackages.Where(x => x.Id == "b").Single().Status);
      Assert.Equal(PackageDependencyStatus.Missing, pm.InstalledPackages.Where(x => x.Id == "b").Single().Dependencies.First().Status);
      Assert.Equal(PackageStatus.DependenciesMissing, pm.InstalledPackages.Where(x => x.Id == "c").Single().Status);
      Assert.Equal(PackageDependencyStatus.Missing, pm.InstalledPackages.Where(x => x.Id == "c").Single().Dependencies.First().Status);
      Assert.Equal(PackageStatus.IndirectDependenciesMissing, pm.InstalledPackages.Where(x => x.Id == "d").Single().Status);
      Assert.Equal(PackageDependencyStatus.OK, pm.InstalledPackages.Where(x => x.Id == "d").Single().Dependencies.First().Status);
      Assert.Equal(PackageManagerStatus.InvalidPackages, pm.Status);
    }
    [Fact]
    public void CreateForTests_WithIncompatiblePackages_ReturnsInstanceWhereStatusIsInvalidPackages() {
      LocalPackageInfo[] localPackages = new[] {
        LocalPackageInfo.CreateForTests("a", "2.0.0", frameworkNotSupported: true),
        LocalPackageInfo.CreateForTests("b", "1.0.0", new[] { PackageDependency.CreateForTests("a", "1.0.0") }),
      };
      INuGetConnector nuGetConnector = new NuGetConnectorStub(localPackages);

      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, nuGetConnector);

      WriteInstalledPackagesToOutput(pm);
      Assert.Equal(localPackages.OrderBy(x => x), pm.InstalledPackages.OrderBy(x => x));
      Assert.Equal(PackageStatus.IncompatibleFramework, pm.InstalledPackages.Where(x => x.Id == "a").Single().Status);
      Assert.Equal(PackageStatus.DependenciesMissing, pm.InstalledPackages.Where(x => x.Id == "b").Single().Status);
      Assert.Equal(PackageDependencyStatus.Missing, pm.InstalledPackages.Where(x => x.Id == "b").Single().Dependencies.First().Status);
      Assert.Equal(PackageManagerStatus.InvalidPackages, pm.Status);
    }
    #endregion

    #region SearchRemotePackagesAsync
    [Theory]
    [InlineData("", new string[] { "a", "ab", "abc", "b", "c" })]
    [InlineData("c", new string[] { "abc", "c" })]
    [InlineData("x", new string[] { })]
    public async Task SearchRemotePackagesAsync_WithSearchString_ReturnsFoundPackages(string searchString, string[] expectedPackageNames) {
      RemotePackageInfo[] remotePackages = new[] {
        RemotePackageInfo.CreateForTests("a", "1.0.0"),
        RemotePackageInfo.CreateForTests("ab", "1.0.0"),
        RemotePackageInfo.CreateForTests("abc", "1.0.0"),
        RemotePackageInfo.CreateForTests("b", "1.0.0"),
        RemotePackageInfo.CreateForTests("c", "1.0.0")
      };
      INuGetConnector nuGetConnectorStub = new NuGetConnectorStub(remotePackages);
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, nuGetConnectorStub);

      IEnumerable<string> result = (await pm.SearchRemotePackagesAsync(searchString, 0, 10)).Select(x => x.Package.Id);

      Assert.Equal(expectedPackageNames.OrderBy(x => x), result.OrderBy(x => x));
    }
    [Fact]
    public async Task SearchRemotePackagesAsync_WithNullParameter_ThrowsArgumentNullException() {
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, new NuGetConnectorStub());

      var e = await Assert.ThrowsAsync<ArgumentNullException>(() => pm.SearchRemotePackagesAsync(null, 10, 5));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    #endregion

    #region GetRemotePackageAsync, GetRemotePackagesAsync
    [Theory]
    [InlineData("b", "1.0.0", "b", "1.0.0")]
    [InlineData("x", "1.0.0", null, null)]
    [InlineData("c", "2.0.0", null, null)]
    public async Task GetRemotePackageAsync_WithPackageAndVersion_ReturnsPackageOrNull(string packageId, string version, string expectedPackageId, string expectedVersion) {
      RemotePackageInfo[] remotePackages = new[] {
        RemotePackageInfo.CreateForTests("a", "1.0.0"),
        RemotePackageInfo.CreateForTests("b", "1.0.0"),
        RemotePackageInfo.CreateForTests("c", "1.0.0")
      };
      INuGetConnector nuGetConnectorStub = new NuGetConnectorStub(remotePackages);
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, nuGetConnectorStub);

      RemotePackageInfo result = await pm.GetRemotePackageAsync(packageId, version);

      Assert.Equal(expectedPackageId, result?.Id);
      Assert.Equal(expectedVersion, result?.Version.ToString());
    }
    [Theory]
    [InlineData(null, null)]
    [InlineData(null, "1.0.0")]
    [InlineData("a", null)]
    public async Task GetRemotePackageAsync_WithNullParameter_ThrowsArgumentNullException(string packageId, string version) {
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, new NuGetConnectorStub());

      var e = await Assert.ThrowsAsync<ArgumentNullException>(() => pm.GetRemotePackageAsync(packageId, version));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData("", "")]
    [InlineData("", "invalid")]
    [InlineData("", "1.0.0")]
    [InlineData("a", "")]
    [InlineData("a", "invalid")]
    public async Task GetRemotePackageAsync_WithEmptyOrInvalidParameter_ThrowsArgumentException(string packageId, string version) {
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, new NuGetConnectorStub());

      var e = await Assert.ThrowsAsync<ArgumentException>(() => pm.GetRemotePackageAsync(packageId, version));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData("a", true, "a", new string[] { "1.0.0", "2.0.0-alpha.1", "2.0.0" })]
    [InlineData("a", false, "a", new string[] { "1.0.0", "2.0.0" })]
    [InlineData("x", false, null, null)]
    public async Task GetRemotePackagesAsync_WithPackage_ReturnsPackages(string packageId, bool includePreReleases, string expectedPackageId, string[] expectedVersions) {
      RemotePackageInfo[] remotePackages = new[] {
        RemotePackageInfo.CreateForTests("a", "1.0.0"),
        RemotePackageInfo.CreateForTests("a", "2.0.0-alpha.1"),
        RemotePackageInfo.CreateForTests("a", "2.0.0"),
        RemotePackageInfo.CreateForTests("b", "1.0.0"),
      };
      INuGetConnector nuGetConnectorStub = new NuGetConnectorStub(remotePackages);
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, nuGetConnectorStub);

      IEnumerable<RemotePackageInfo> result = await pm.GetRemotePackagesAsync(packageId, includePreReleases);

      if (expectedPackageId == null) {
        Assert.Empty(result);
      } else {
        Assert.True(result.Select(x => x.Id).All(x => x == expectedPackageId));
        Assert.Equal(expectedVersions.OrderBy(x => x), result.Select(x => x.Version.ToString()).OrderBy(x => x));
      }
    }
    [Fact]
    public async Task GetRemotePackagesAsync_WithNullParameter_ThrowsArgumentNullException() {
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, new NuGetConnectorStub());

      var e = await Assert.ThrowsAsync<ArgumentNullException>(() => pm.GetRemotePackagesAsync(null));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Fact]
    public async Task GetRemotePackagesAsync_WithEmptyParameter_ThrowsArgumentException() {
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, new NuGetConnectorStub());

      var e = await Assert.ThrowsAsync<ArgumentException>(() => pm.GetRemotePackagesAsync(""));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    #endregion

    #region InstallRemotePackageAsync, InstallRemotePackagesAsync
    [Theory]
    [InlineData("d", true,  new[] { "a", "b", "c", "d" }, new[] { "1.0.0", "1.1.0", "1.0.0", "1.0.0" }, PackageManagerStatus.OK)]
    [InlineData("d", false, new[] { "d"                }, new[] { "1.0.0"                            }, PackageManagerStatus.InvalidPackages)]
    public async Task InstallRemotePackageAsync_WithPackage(string packageNameToInstall, bool installMissingDependencies, string[] expectedPackageNames, string[] expectedVersions, PackageManagerStatus expectedStatus) {
      RemotePackageInfo[] remotePackages = new[] {
        RemotePackageInfo.CreateForTests("a", "1.0.0"),
        RemotePackageInfo.CreateForTests("b", "1.0.0", new[] { PackageDependency.CreateForTests("a", "1.0.0") }),
        RemotePackageInfo.CreateForTests("b", "1.1.0", new[] { PackageDependency.CreateForTests("a", "1.0.0") }),
        RemotePackageInfo.CreateForTests("b", "2.0.0", new[] { PackageDependency.CreateForTests("a", "1.0.0") }),
        RemotePackageInfo.CreateForTests("c", "1.0.0", new[] { PackageDependency.CreateForTests("b", "1.0.1") }),
        RemotePackageInfo.CreateForTests("d", "1.0.0", new[] { PackageDependency.CreateForTests("c", "1.0.0") })
      };
      RemotePackageInfo packageToInstall = remotePackages.Where(x => x.Id == packageNameToInstall).Single();
      LocalPackageInfo[] expectedPackages = expectedPackageNames.Select((n, i) => LocalPackageInfo.CreateForTests(n, expectedVersions[i])).ToArray();
      INuGetConnector nuGetConnectorStub = new NuGetConnectorStub(remotePackages);
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, nuGetConnectorStub);

      await pm.InstallRemotePackageAsync(packageToInstall, installMissingDependencies: installMissingDependencies);

      WriteInstalledPackagesToOutput(pm);
      Assert.Equal(expectedPackages.OrderBy(x => x), pm.InstalledPackages.OrderBy(x => x));
      Assert.Equal(expectedStatus, pm.Status);
    }
    [Fact]
    public async Task InstallRemotePackageAsync_WithNullParameter_ThrowsArgumentNullException() {
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, new NuGetConnectorStub());

      var e = await Assert.ThrowsAsync<ArgumentNullException>(() => pm.InstallRemotePackageAsync(null));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData(new[] { "c", "d" }, true,  new[] { "a", "b", "c", "d" }, new[] { "1.0.0", "1.0.0", "1.0.0", "1.0.0" }, PackageManagerStatus.OK)]
    [InlineData(new[] { "c", "d" }, false, new[] { "c", "d"           }, new[] { "1.0.0", "1.0.0"                   }, PackageManagerStatus.InvalidPackages)]
    public async Task InstallRemotePackagesAsync_WithPackages(string[] packageNamesToInstall, bool installMissingDependencies, string[] expectedPackageNames, string[] expectedVersions, PackageManagerStatus expectedStatus) {
      RemotePackageInfo[] remotePackages = new[] {
        RemotePackageInfo.CreateForTests("a", "1.0.0"),
        RemotePackageInfo.CreateForTests("b", "1.0.0", new[] { PackageDependency.CreateForTests("a", "1.0.0") }),
        RemotePackageInfo.CreateForTests("c", "1.0.0", new[] { PackageDependency.CreateForTests("b", "1.0.0") }),
        RemotePackageInfo.CreateForTests("d", "1.0.0", new[] { PackageDependency.CreateForTests("c", "1.0.0") })
      };
      INuGetConnector nuGetConnectorStub = new NuGetConnectorStub(remotePackages);
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, nuGetConnectorStub);
      IEnumerable<RemotePackageInfo> packagesToInstall = remotePackages.Where(x => packageNamesToInstall.Contains(x.Id));
      LocalPackageInfo[] expectedPackages = expectedPackageNames.Select((n, i) => LocalPackageInfo.CreateForTests(n, expectedVersions[i])).ToArray();

      await pm.InstallRemotePackagesAsync(packagesToInstall, installMissingDependencies: installMissingDependencies);

      WriteInstalledPackagesToOutput(pm);
      Assert.Equal(expectedPackages.OrderBy(x => x), pm.InstalledPackages.OrderBy(x => x));
      Assert.Equal(expectedStatus, pm.Status);
    }
    [Fact]
    public async Task InstallRemotePackagesAsync_WithNullParameter_ThrowsArgumentNullException() {
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, new NuGetConnectorStub());

      var e = await Assert.ThrowsAsync<ArgumentNullException>(() => pm.InstallRemotePackagesAsync(null));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Fact]
    public async Task InstallRemotePackagesAsync_WithEmptyParameter_ThrowsArgumentException() {
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, new NuGetConnectorStub());

      var e = await Assert.ThrowsAsync<ArgumentException>(() => pm.InstallRemotePackagesAsync(Enumerable.Empty<RemotePackageInfo>()));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Fact]
    public async Task InstallRemotePackagesAsync_WithParameterContainsNull_ThrowsArgumentException() {
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, new NuGetConnectorStub());

      var e = await Assert.ThrowsAsync<ArgumentException>(() => pm.InstallRemotePackagesAsync(new RemotePackageInfo[] { null }));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
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
    [Fact]
    public void RemoveInstalledPackage_WithNotInstalledPackage_ThrowsInvalidOperationException() {
      LocalPackageInfo[] localPackages = new[] {
        LocalPackageInfo.CreateForTests("a", "1.0.0", packagePath: "packagePath"),
        LocalPackageInfo.CreateForTests("b", "1.0.0", new[] { PackageDependency.CreateForTests("a", "1.0.0") }, packagePath: "packagePath"),
        LocalPackageInfo.CreateForTests("c", "1.0.0", new[] { PackageDependency.CreateForTests("b", "1.0.0") }, packagePath: "packagePath")
      };
      LocalPackageInfo packageToRemove = LocalPackageInfo.CreateForTests("x", "1.0.0", packagePath: "packagePath");
      INuGetConnector nuGetConnectorStub = new NuGetConnectorStub(localPackages);
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, nuGetConnectorStub);

      var e = Assert.Throws<InvalidOperationException>(() => pm.RemoveInstalledPackage(packageToRemove));
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    [Fact]
    public void RemoveInstalledPackage_WithNullParameter_ThrowsArgumentNullException() {
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, new NuGetConnectorStub());

      var e = Assert.Throws<ArgumentNullException>(() => pm.RemoveInstalledPackage(null));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("rootedPath")]
    public void RemoveInstalledPackage_WithParameterWherePackagePathIsInvalid_ThrowsArgumentException(string packagePath) {
      if (!string.IsNullOrEmpty(packagePath)) {
        packagePath = (Constants.Platform == Platform.Windows ? "C:/" : "/") + packagePath;
      }
      LocalPackageInfo package = LocalPackageInfo.CreateForTests("a", "1.0.0", packagePath: packagePath);
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, new NuGetConnectorStub());

      var e = Assert.Throws<ArgumentException>(() => pm.RemoveInstalledPackage(package));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
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
    [Fact]
    public void RemoveInstalledPackages_WithNotInstalledPackages_ThrowsInvalidOperationException() {
      LocalPackageInfo[] localPackages = new[] {
        LocalPackageInfo.CreateForTests("a", "1.0.0", packagePath: "packagePath"),
        LocalPackageInfo.CreateForTests("b", "1.0.0", new[] { PackageDependency.CreateForTests("a", "1.0.0") }, packagePath: "packagePath"),
        LocalPackageInfo.CreateForTests("c", "1.0.0", new[] { PackageDependency.CreateForTests("b", "1.0.0") }, packagePath: "packagePath")
      };
      LocalPackageInfo[] packagesToRemove = new[] {
        localPackages[2],
        LocalPackageInfo.CreateForTests("x", "1.0.0", packagePath: "packagePath")
      };
      INuGetConnector nuGetConnectorStub = new NuGetConnectorStub(localPackages);
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, nuGetConnectorStub);

      var e = Assert.Throws<InvalidOperationException>(() => pm.RemoveInstalledPackages(packagesToRemove));
      Assert.False(string.IsNullOrEmpty(e.Message));
      foreach (LocalPackageInfo packageToRemove in packagesToRemove) {
        Assert.DoesNotContain(packageToRemove, pm.InstalledPackages);
      }
    }
    [Fact]
    public void RemoveInstalledPackages_WithNullParameter_ThrowsArgumentNullException() {
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, new NuGetConnectorStub());

      var e = Assert.Throws<ArgumentNullException>(() => pm.RemoveInstalledPackages(null));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Fact]
    public void RemoveInstalledPackages_WithEmptyParameter_ThrowsArgumentException() {
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, new NuGetConnectorStub());

      var e = Assert.Throws<ArgumentException>(() => pm.RemoveInstalledPackages(new LocalPackageInfo[0]));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Fact]
    public void RemoveInstalledPackages_WithParameterContainsNull_ThrowsArgumentException() {
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, new NuGetConnectorStub());

      var e = Assert.Throws<ArgumentException>(() => pm.RemoveInstalledPackages(new LocalPackageInfo[] { null }));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("rootedPath")]
    public void RemoveInstalledPackages_WithParameterWherePackagePathIsInvalid_ThrowsArgumentException(string packagePath) {
      if (!string.IsNullOrEmpty(packagePath)) {
        packagePath = (Constants.Platform == Platform.Windows ? "C:/" : "/") + packagePath;
      }
      LocalPackageInfo[] packages = new[] { LocalPackageInfo.CreateForTests("a", "1.0.0", packagePath: packagePath) };
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, new NuGetConnectorStub());

      var e = Assert.Throws<ArgumentException>(() => pm.RemoveInstalledPackages(packages));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
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
    public void GetPackageLoadInfo_WithNullParameter_ThrowsArgumentNullException() {
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, new NuGetConnectorStub());

      var e = Assert.Throws<ArgumentNullException>(() => pm.GetPackageLoadInfo(null));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData(PackageStatus.DependenciesMissing)]
    [InlineData(PackageStatus.IncompatibleFramework)]
    [InlineData(PackageStatus.IndirectDependenciesMissing)]
    [InlineData(PackageStatus.Outdated)]
    [InlineData(PackageStatus.Undefined)]
    public void GetPackageLoadInfo_WithPackageWhereStatusIsNotOK_ThrowsArgumentException(PackageStatus packageStatus) {
      LocalPackageInfo packageToLoad = LocalPackageInfo.CreateForTests("a", "1.0.0");
      packageToLoad.Status = packageStatus;
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, new NuGetConnectorStub());

      var e = Assert.Throws<ArgumentException>(() => pm.GetPackageLoadInfo(packageToLoad));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
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
    [Fact]
    public void GetPackageLoadInfos_WhenNoPackagesAvailable_ReturnsEmpty() {
      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, new NuGetConnectorStub());

      IEnumerable<PackageLoadInfo> result = pm.GetPackageLoadInfos();

      Assert.Empty(result);
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
