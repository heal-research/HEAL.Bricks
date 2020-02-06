#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HEAL.Bricks.Tests {
  [TestClass]
  public class PluginManagerTests : PluginTestsBase {
    #region TestCreate
    [TestMethod]
    public void TestCreate() {
      ISettings settings;
      IPluginManager pluginManager;
      ArgumentNullException argumentNullException;

      settings = Settings.Default;
      pluginManager = PluginManager.Create(settings);
      Assert.IsNotNull(pluginManager);
      Assert.IsNotNull(pluginManager.Settings);
      Assert.IsNotNull(pluginManager.InstalledPackages);
      Assert.AreEqual(0, pluginManager.InstalledPackages.Count());
      Assert.AreEqual(PluginManagerStatus.Uninitialized, pluginManager.Status);

      settings = null;
      argumentNullException = Assert.ThrowsException<ArgumentNullException>(() => { PluginManager.Create(settings); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));
    }
    #endregion

    #region TestInitialize
    [TestMethod]
    public void TestInitialize() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      PluginManager pluginManager = new PluginManager(nuGetConnector);
      PackageIdentity[] expectedPackages;
      LocalPackageInfo[] installedPackages;

      List<PackageIdentity> expectedPackagesList = new List<PackageIdentity>();
      foreach (PackageFolderReader expectedPackageReader in nuGetConnector.GetInstalledPackages()) {
        expectedPackagesList.Add(expectedPackageReader.GetIdentity());
        expectedPackageReader.Dispose();
      }
      expectedPackages = expectedPackagesList.ToArray();
      pluginManager.Initialize();
      installedPackages = pluginManager.InstalledPackages.ToArray();
      CollectionAssert.AreEqual(expectedPackages, installedPackages, PackageInfoComparer.Default);
      Assert.IsTrue((pluginManager.Status == PluginManagerStatus.OK) || (pluginManager.Status == PluginManagerStatus.InvalidPlugins));
      foreach (LocalPackageInfo installedPackage in installedPackages) {
        Assert.IsTrue(installedPackage.Status != PackageStatus.Unknown);
        Assert.IsTrue(installedPackage.Dependencies.All(x => x.Status != PackageDependencyStatus.Unknown));
      }

      WriteLogToTestContextAndClear(nuGetConnector);
    }
    #endregion

    #region TestResolveMissingDependenciesAsync
    [TestMethod]
    public async Task TestResolveMissingDependenciesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      PluginManager pluginManager = new PluginManager(nuGetConnector);
      PackageIdentity[] expectedPackages;
      RemotePackageInfo[] missingPackages;

      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginA, Constants.version030) };
      missingPackages = (await pluginManager.ResolveMissingDependenciesAsync()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, missingPackages, PackageInfoComparer.Default);

      WriteLogToTestContextAndClear(nuGetConnector);
    }
    #endregion

    #region TestInstallMissingDependenciesAsync
    [TestMethod]
    public async Task TestInstallMissingDependenciesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      PluginManager pluginManager = new PluginManager(nuGetConnector);
      PackageIdentity[] expectedPackages;
      RemotePackageInfo[] installedPackages;

      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginA, Constants.version030) };
      installedPackages = (await pluginManager.InstallMissingDependenciesAsync()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, installedPackages, PackageInfoComparer.Default);
      Assert.IsTrue(Directory.Exists(Path.Combine(pluginManager.Settings.PackagesPath, Constants.namePluginA + "." + Constants.version030)));

      WriteLogToTestContextAndClear(nuGetConnector);
    }
    #endregion

    #region TestRemoveInstalledPackage
    [TestMethod]
    public void TestRemoveInstalledPackage() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      PluginManager pluginManager = new PluginManager(nuGetConnector);

      pluginManager.Initialize();
      LocalPackageInfo package = pluginManager.InstalledPackages.First();
      pluginManager.RemoveInstalledPackage(package);
      Assert.IsFalse(pluginManager.InstalledPackages.Contains(package, PackageInfoIdentityComparer.Default));
      Assert.IsFalse(Directory.Exists(Path.Combine(pluginManager.Settings.PackagesPath, package.Id + "." + package.Version.ToString())));

      WriteLogToTestContextAndClear(nuGetConnector);
    }
    #endregion

    #region Helpers
    private class PackageInfoComparer : IComparer {
      public static PackageInfoComparer Default => new PackageInfoComparer();
      private readonly IPackageIdentityComparer packageIdentityComparer = PackageIdentityComparer.Default;
      public int Compare(object x, object y) {
        PackageIdentity a = (x as PackageIdentity) ?? (x as PackageInfo)?.packageIdentity;
        PackageIdentity b = (y as PackageIdentity) ?? (y as PackageInfo)?.packageIdentity;

        if ((a != null) && (b != null)) {
          return packageIdentityComparer.Compare(a, b);
        } else {
          return x.Equals(y) ? 0 : 1;
        }
      }
    }
    #endregion
  }
}
