#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Xunit;
using Xunit.Abstractions;

namespace HEAL.Bricks.XTests {
  public class PackageManagerUnitTests {
    private readonly ITestOutputHelper output;

    public PackageManagerUnitTests(ITestOutputHelper output) {
      this.output = output;
    }

    #region Create
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
      settings.Repositories.Add(repository);

      var e = Assert.Throws<ArgumentException>(() => PackageManager.Create(settings));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    #endregion

    #region CreateForTests
    [Fact]
    public void CreateForTests_CreatesValidInstance() {
      ISettings settings = new Settings() {
        PackagesPath = Path.GetTempPath(),
        PackageTag = "testPackageTag"
      };
      LocalPackageInfo[] packages = new[] {
        LocalPackageInfo.CreateForTests("a", "2.0.0.0"),
        LocalPackageInfo.CreateForTests("b", "1.0.0.0", new[] { PackageDependency.CreateForTests("a", "1.0.0.0") }),
      };
      Mock<INuGetConnector> nuGetConnectorMock = Mock.Of<INuGetConnector>().GetLocalPackages(settings.PackagesPath, settings.PackageTag, packages);

      IPackageManager pm = PackageManager.CreateForTests(settings, nuGetConnectorMock.Object);

      nuGetConnectorMock.VerifyAll();
      Assert.NotNull(pm);
      Assert.NotNull(pm.Settings);
      Assert.Equal(packages, pm.InstalledPackages);
      Assert.True(pm.InstalledPackages.All(x => x.Status == PackageStatus.OK));
      Assert.Equal(PackageManagerStatus.OK, pm.Status);
      WriteInstalledPackagesToOutput(pm);
    }
    [Fact]
    public void CreateForTests_WithMissingDependencies_StatusIsInvalidPackages() {
      LocalPackageInfo[] packages = new[] {
        LocalPackageInfo.CreateForTests("a", "2.0.0.0"),
        LocalPackageInfo.CreateForTests("b", "1.0.0.0", new[] { PackageDependency.CreateForTests("a", "1.0.0.0") }),
        LocalPackageInfo.CreateForTests("c", "1.0.0.0", new[] { PackageDependency.CreateForTests("x", "1.0.0.0") }),
        LocalPackageInfo.CreateForTests("d", "1.0.0.0", new[] { PackageDependency.CreateForTests("c", "0.9.0.0") }),
      };
      Mock<INuGetConnector> nuGetConnectorMock = Mock.Of<INuGetConnector>().GetLocalPackages(packages);

      IPackageManager pm = PackageManager.CreateForTests(Settings.Default, nuGetConnectorMock.Object);

      Assert.Equal(PackageManagerStatus.InvalidPackages, pm.Status);
      Assert.Equal(PackageStatus.OK, pm.InstalledPackages.Where(x => x.Id == "a").Single().Status);
      Assert.Equal(PackageStatus.OK, pm.InstalledPackages.Where(x => x.Id == "b").Single().Status);
      Assert.Equal(PackageDependencyStatus.OK, pm.InstalledPackages.Where(x => x.Id == "b").Single().Dependencies.First().Status);
      Assert.Equal(PackageStatus.DependenciesMissing, pm.InstalledPackages.Where(x => x.Id == "c").Single().Status);
      Assert.Equal(PackageDependencyStatus.Missing, pm.InstalledPackages.Where(x => x.Id == "c").Single().Dependencies.First().Status);
      Assert.Equal(PackageStatus.IndirectDependenciesMissing, pm.InstalledPackages.Where(x => x.Id == "d").Single().Status);
      Assert.Equal(PackageDependencyStatus.OK, pm.InstalledPackages.Where(x => x.Id == "d").Single().Dependencies.First().Status);
      WriteInstalledPackagesToOutput(pm);
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
