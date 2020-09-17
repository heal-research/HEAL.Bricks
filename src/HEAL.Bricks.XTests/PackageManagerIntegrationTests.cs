#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
    [InlineData("TestPackage.ListedStable",     true,  new string[] { "1.0.0", "2.0.0", "2.0.2", "2.0.6" })]
    [InlineData("TestPackage.AlwaysPrerelease", true,  new string[] { "5.0.0-beta" })]
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
    [InlineData(Constants.netCoreApp31FrameworkName,    "TestPackage.Depends.SupportingMultipleFrameworks", "1.2.0", false,  new[] { "TestPackage.Depends.SupportingMultipleFrameworks" },                                             new[] { "1.2.0" },          PackageManagerStatus.InvalidPackages)]
    [InlineData(Constants.netFramework45FrameworkName,  "TestPackage.Depends.SupportingMultipleFrameworks", "1.2.0", true,   new[] { "TestPackage.Depends.SupportingMultipleFrameworks", "TestPackage.SupportingMultipleFrameworks" }, new[] { "1.2.0", "1.0.0" }, PackageManagerStatus.OK)]
    [InlineData(Constants.netCoreApp31FrameworkName,    "TestPackage.Depends.SupportingMultipleFrameworks", "1.2.0", true,   new[] { "TestPackage.Depends.SupportingMultipleFrameworks", "TestPackage.SupportingMultipleFrameworks" }, new[] { "1.2.0", "1.0.0" }, PackageManagerStatus.InvalidPackages)]
    [InlineData(Constants.netFramework472FrameworkName, "TestPackage.SupportingMultipleFrameworks",         "1.2.0", true,   new[] { "TestPackage.SupportingMultipleFrameworks" },                                                     new[] { "1.2.0" },          PackageManagerStatus.OK)]
    [InlineData(Constants.netFramework35FrameworkName,  "TestPackage.SupportingMultipleFrameworks",         "1.2.0", true,   new[] { "TestPackage.SupportingMultipleFrameworks" },                                                     new[] { "1.2.0" },          PackageManagerStatus.InvalidPackages)]
    [InlineData(Constants.netCoreApp31FrameworkName,    "TestPackage.SupportingMultipleFrameworks",         "1.2.0", true,   new[] { "TestPackage.SupportingMultipleFrameworks" },                                                     new[] { "1.2.0" },          PackageManagerStatus.InvalidPackages)]
    public async Task InstallRemotePackageAsync_WithPackage(string currentFramework, string packageId, string version, bool installMissingDependencies, string[] expectedPackageNames, string[] expectedVersions, PackageManagerStatus expectedStatus) {
      INuGetConnector nuGetConnector = NuGetConnector.CreateForTests(currentFramework, Settings.Repositories, new XunitLogger(output));
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

    #region RemoveInstalledPackage
    [Theory]
    [InlineData("TestPackage.ListedStable", "2.0.2")]
    public async Task RemoveInstalledPackage_WithInstalledPackage(string packageId, string version) {
      IPackageManager pm = PackageManager.Create(Settings);
      RemotePackageInfo remotePackage = await pm.GetRemotePackageAsync(packageId, version);
      await pm.InstallRemotePackageAsync(remotePackage, installMissingDependencies: true);
      LocalPackageInfo packageToRemove = pm.InstalledPackages.Single();

      pm.RemoveInstalledPackage(packageToRemove);

      Assert.Empty(pm.InstalledPackages);
      Assert.False(Directory.EnumerateFileSystemEntries(Settings.PackagesPath).Any());
    }
    #endregion

    #region InstallPackageUpdatesAsync
    [Theory]
    [InlineData("TestPackage.ListedStable", "1.0.0", true,  true,  "2.0.6")]
    public async Task InstallPackageUpdatesAsync_WhenUpdatesArePending(string packageId, string version, bool installMissingDependencies, bool includePreReleases, string expectedVersion) {
      IPackageManager pm = PackageManager.Create(Settings);
      RemotePackageInfo remotePackage = await pm.GetRemotePackageAsync(packageId, version);
      await pm.InstallRemotePackageAsync(remotePackage, installMissingDependencies: false);

      await pm.InstallPackageUpdatesAsync(installMissingDependencies: installMissingDependencies, includePreReleases: includePreReleases);

      WriteInstalledPackagesToOutput(pm);
      Assert.Equal(expectedVersion, pm.InstalledPackages.Where(x => x.Id == packageId).OrderByDescending(x => x.Version).First().Version.ToString());
    }
    #endregion

    #region GetPackageLoadInfos
    [Theory(Skip = "WIP")]
    [InlineData(Constants.netCoreApp31FrameworkName,    "SimSharp", "3.3.2", new[] { "SimSharp.dll" })]
    [InlineData(Constants.netFramework472FrameworkName, "SimSharp", "3.3.2", new[] { "SimSharp.dll" })]
    [InlineData(Constants.netFramework35FrameworkName,  "SimSharp", "3.3.2", new string[0])]
    public async Task GetPackageLoadInfos_ReturnsPackageLoadInfos(string currentFramework, string packageId, string version, string[] expectedAssemblies) {
      INuGetConnector nuGetConnector = NuGetConnector.CreateForTests(currentFramework, Settings.Repositories, new XunitLogger(output));
      IPackageManager pm = PackageManager.CreateForTests(Settings, nuGetConnector);
      RemotePackageInfo remotePackage = await pm.GetRemotePackageAsync(packageId, version);
      await pm.InstallRemotePackageAsync(remotePackage, installMissingDependencies: false);

      IEnumerable<PackageLoadInfo> result = pm.GetPackageLoadInfos();

      if (expectedAssemblies.Length == 0) {
        Assert.Empty(result);
      } else {
        Assert.Collection(result, x => {
          Assert.All(x.AssemblyPaths, y => Path.IsPathRooted(y));
          Assert.Equal(expectedAssemblies.OrderBy(y => y), x.AssemblyPaths.Select(y => Path.GetFileName(y)).OrderBy(y => y));
        });
      }
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
