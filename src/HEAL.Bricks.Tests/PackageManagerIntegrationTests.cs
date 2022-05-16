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
using Xunit;
using Xunit.Abstractions;

namespace HEAL.Bricks.Tests {

  [Trait("Category", "Integration")]
  public class PackageManagerIntegrationTests : IClassFixture<RandomTempDirectoryFixture> {
    private readonly ITestOutputHelper output;
    private readonly RandomTempDirectoryFixture tempDir;
    private string TestDir { get; }
    private BricksOptions Options { get; }

    public PackageManagerIntegrationTests(RandomTempDirectoryFixture tempDir, ITestOutputHelper output) {
      this.output = output;
      this.tempDir = tempDir;

      TestDir = tempDir.CreateRandomSubdirectory();
      string packagesPath = Path.Combine(TestDir, "packages");
      Directory.CreateDirectory(packagesPath);
      string packagesCachePath = Path.Combine(TestDir, "packages_cache");
      Directory.CreateDirectory(packagesCachePath);

      Options = new BricksOptions {
        PackagesPath = packagesPath,
        PackagesCachePath = packagesCachePath
      };
      Options.Repositories.Add(BricksOptions.PublicNuGetRepository);
    }

    #region SearchRemotePackagesAsync
    [Theory]
    [InlineData("PackageId:TestPackage.ListedStable", new string[] { "TestPackage.ListedStable" })]
    public async Task SearchRemotePackagesAsync_WithSearchString_ReturnsFoundPackages(string searchString, string[] expectedPackageNames) {
      PackageManager pm = new(Options);

      IEnumerable<string> result = (await pm.SearchRemotePackagesAsync(searchString, 0, 10)).Select(x => x.Package.Id);

      Assert.Equal(expectedPackageNames.OrderBy(x => x), result.OrderBy(x => x));
    }
    #endregion

    #region GetRemotePackageAsync, GetRemotePackagesAsync
    [Theory]
    [InlineData("TestPackage.ListedStable", "2.0.2")]
    public async Task GetRemotePackageAsync_WithPackageAndVersion_ReturnsPackage(string packageId, string version) {
      PackageManager pm = new(Options);

      RemotePackageInfo? result = await pm.GetRemotePackageAsync(packageId, version);

      Assert.Equal(packageId, result?.Id);
      Assert.Equal(version, result?.Version.ToString());
    }
    [Theory]
    [InlineData("TestPackage.ListedStable",     true,  new string[] { "1.0.0", "2.0.0", "2.0.2", "2.0.6" })]
    [InlineData("TestPackage.AlwaysPrerelease", true,  new string[] { "5.0.0-beta" })]
    [InlineData("TestPackage.AlwaysPrerelease", false, new string[] { })]
    public async Task GetRemotePackagesAsync_WithPackage_ReturnsPackages(string packageId, bool includePreReleases, string[] expectedVersions) {
      PackageManager pm = new(Options);

      IEnumerable<RemotePackageInfo> result = await pm.GetRemotePackagesAsync(packageId, includePreReleases);

      Assert.True(result.Select(x => x.Id).All(x => x == packageId));
      Assert.Equal(expectedVersions.OrderBy(x => x), result.Select(x => x.Version.ToString()).OrderBy(x => x));
    }
    #endregion

    #region InstallRemotePackageAsync
    [Theory]
    [InlineData(Constants.netCoreApp60FrameworkName, "TestPackage.Depends.SupportingMultipleFrameworks", "1.2.0", false,  new[] { "TestPackage.Depends.SupportingMultipleFrameworks" },                                             new[] { "1.2.0" },          PackageManagerStatus.InvalidPackages)]
    [InlineData(Constants.netCoreApp60FrameworkName, "TestPackage.Depends.SupportingMultipleFrameworks", "1.2.0", true,   new[] { "TestPackage.Depends.SupportingMultipleFrameworks", "TestPackage.SupportingMultipleFrameworks" }, new[] { "1.2.0", "1.0.0" }, PackageManagerStatus.InvalidPackages)]
    [InlineData(Constants.netCoreApp60FrameworkName, "TestPackage.SupportingMultipleFrameworks",         "1.2.0", true,   new[] { "TestPackage.SupportingMultipleFrameworks" },                                                     new[] { "1.2.0" },          PackageManagerStatus.InvalidPackages)]
    public async Task InstallRemotePackageAsync_WithPackage(string currentFramework, string packageId, string version, bool installMissingDependencies, string[] expectedPackageNames, string[] expectedVersions, PackageManagerStatus expectedStatus) {
      NuGetConnector nuGetConnector = new(currentFramework, Options.Repositories, new XunitLogger(output));
      PackageManager pm = new(Options, nuGetConnector);
      RemotePackageInfo packageToInstall = await pm.GetRemotePackageAsync(packageId, version) ?? throw new InvalidOperationException("Cannot resolve remote package to install");
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
      PackageManager pm = new(Options);
      RemotePackageInfo remotePackage = await pm.GetRemotePackageAsync(packageId, version) ?? throw new InvalidOperationException("Cannot resolve remote package to install");
      await pm.InstallRemotePackageAsync(remotePackage, installMissingDependencies: true);
      LocalPackageInfo packageToRemove = pm.InstalledPackages.Single();

      pm.RemoveInstalledPackage(packageToRemove);

      Assert.Empty(pm.InstalledPackages);
      Assert.False(Directory.EnumerateFileSystemEntries(Options.PackagesPath).Any());
    }
    #endregion

    #region InstallPackageUpdatesAsync
    [Theory]
    [InlineData("SimSharp", "3.4.0", true, true, "3.4.1")]
    [InlineData("HEAL.Attic", "1.6.0", true, true, "1.7.0")]
    public async Task InstallPackageUpdatesAsync_WhenUpdatesArePending(string packageId, string version, bool installMissingDependencies, bool includePreReleases, string expectedVersion) {
      NuGetConnector nuGetConnector = new(Constants.netCoreApp60FrameworkName, Options.Repositories, new XunitLogger(output));
      PackageManager pm = new(Options, nuGetConnector);
      RemotePackageInfo remotePackage = await pm.GetRemotePackageAsync(packageId, version) ?? throw new InvalidOperationException("Cannot resolve remote package to install");
      await pm.InstallRemotePackageAsync(remotePackage, installMissingDependencies: false);

      await pm.InstallPackageUpdatesAsync(installMissingDependencies: installMissingDependencies, includePreReleases: includePreReleases);

      WriteInstalledPackagesToOutput(pm);
      Assert.Equal(expectedVersion, pm.InstalledPackages.Where(x => x.Id == packageId).OrderByDescending(x => x.Version).First().Version.ToString());
    }
    #endregion

    #region GetPackageLoadInfos
    [Theory]
    [InlineData(Constants.netCoreApp60FrameworkName, "SimSharp", "3.3.2", new[] { "SimSharp.dll" })]
    [InlineData(Constants.netCoreApp60FrameworkName, "Dawn.Guard", "1.12.0", new[] { "Dawn.Guard.dll" })]
    [InlineData(Constants.netCoreApp60FrameworkName, "System.Drawing.Common", "6.0.0", new[] { "System.Drawing.Common.dll", "Microsoft.Win32.SystemEvents.dll" })]
    public async Task GetPackageLoadInfos_ReturnsPackageLoadInfos(string currentFramework, string packageId, string version, string[] expectedAssemblies) {
      NuGetConnector nuGetConnector = new(currentFramework, Options.Repositories, new XunitLogger(output));
      PackageManager pm = new(Options, nuGetConnector);
      RemotePackageInfo remotePackage = await pm.GetRemotePackageAsync(packageId, version) ?? throw new InvalidOperationException("Cannot resolve remote package to install");
      await pm.InstallRemotePackageAsync(remotePackage, installMissingDependencies: true);

      IEnumerable<PackageLoadInfo> result = pm.GetPackageLoadInfos();

      if (expectedAssemblies.Length == 0) {
        Assert.Empty(result);
      } else {
        Assert.All(result, x => {
          Assert.Single(x.AssemblyPaths);
          Assert.All(x.AssemblyPaths, y => Path.IsPathRooted(y));
        });
        Assert.Equal(expectedAssemblies.OrderBy(x => x), result.Select(x => x.AssemblyPaths.Single()).Select(x => Path.GetFileName(x)).OrderBy(x => x));
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
