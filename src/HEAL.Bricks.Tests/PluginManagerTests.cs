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

    #region TestGetMissingDependenciesAsync
    [TestMethod]
    public async Task TestGetMissingDependenciesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      PluginManager pluginManager = new PluginManager(nuGetConnector);
      PackageIdentity[] expectedPackages;
      RemotePackageInfo[] missingPackages;

      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginA, Constants.version030) };
      missingPackages = (await pluginManager.GetMissingDependenciesAsync()).ToArray();
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

      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginA, Constants.version030) };
      await pluginManager.InstallMissingDependenciesAsync();
      Assert.IsTrue(expectedPackages.All(x => Directory.Exists(Path.Combine(pluginManager.Settings.PackagesPath, x.Id + "." + x.Version.ToString()))));

      WriteLogToTestContextAndClear(nuGetConnector);
    }
    #endregion

    #region TestSearchRemotePackageAsync
    [TestMethod]
    public async Task TestSearchRemotePackageAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: false);
      PluginManager pluginManager = new PluginManager(nuGetConnector);
      string searchString;
      bool includePreReleases;
      PackageIdentity[] expectedPackages;
      RemotePackageInfo[] foundPackages;
      ArgumentNullException argumentNullException;

      searchString = Constants.namePluginA;
      includePreReleases = false;
      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginA, Constants.version021),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030) };
      foundPackages = (await pluginManager.SearchRemotePackagesAsync(searchString, includePreReleases, 0, int.MaxValue)).Select(x => x.Package).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages, PackageInfoComparer.Default);

      searchString = Constants.namePluginB;
      includePreReleases = false;
      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginB, Constants.version020),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version031) };
      foundPackages = (await pluginManager.SearchRemotePackagesAsync(searchString, includePreReleases, 0, int.MaxValue)).Select(x => x.Package).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages, PackageInfoComparer.Default);

      searchString = Constants.nameInvalid;
      includePreReleases = false;
      foundPackages = (await pluginManager.SearchRemotePackagesAsync(searchString, includePreReleases, 0, int.MaxValue)).Select(x => x.Package).ToArray();
      Assert.AreEqual(0, foundPackages.Length, "Number of found packages is incorrect.");

      searchString = null;
      includePreReleases = false;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return pluginManager.SearchRemotePackagesAsync(searchString, includePreReleases, 0, int.MaxValue); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      searchString = null;
      includePreReleases = true;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return pluginManager.SearchRemotePackagesAsync(searchString, includePreReleases, 0, int.MaxValue); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      WriteLogToTestContextAndClear(nuGetConnector);
    }
    #endregion

    #region TestGetRemotePackageAsync
    [TestMethod]
    public async Task TestGetRemotePackageAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      PluginManager pluginManager = new PluginManager(nuGetConnector);
      string packageId;
      string version;
      RemotePackageInfo foundPackage;
      ArgumentNullException argumentNullException;
      ArgumentException argumentException;

      packageId = Constants.namePluginA;
      version = Constants.version030_beta1;
      foundPackage = await pluginManager.GetRemotePackageAsync(packageId, version);
      Assert.AreEqual(packageId, foundPackage.Id);
      Assert.AreEqual(version, foundPackage.Version.ToString());

      packageId = Constants.namePluginA;
      version = Constants.version030;
      foundPackage = await pluginManager.GetRemotePackageAsync(packageId, version);
      Assert.AreEqual(packageId, foundPackage.Id);
      Assert.AreEqual(version, foundPackage.Version.ToString());

      packageId = Constants.nameCollections;
      version = Constants.versionCollections;
      foundPackage = await pluginManager.GetRemotePackageAsync(packageId, version);
      Assert.AreEqual(packageId, foundPackage.Id);
      Assert.AreEqual(version, foundPackage.Version.ToString());

      packageId = Constants.nameInvalid;
      version = Constants.version000;
      foundPackage = await pluginManager.GetRemotePackageAsync(packageId, version);
      Assert.IsNull(foundPackage);

      packageId = null;
      version = Constants.version000;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return pluginManager.GetRemotePackageAsync(packageId, version); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      packageId = "";
      version = Constants.version000;
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return pluginManager.GetRemotePackageAsync(packageId, version); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      packageId = Constants.namePluginA;
      version = null;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return pluginManager.GetRemotePackageAsync(packageId, version); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      packageId = Constants.namePluginA;
      version = "";
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return pluginManager.GetRemotePackageAsync(packageId, version); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      packageId = Constants.namePluginA;
      version = Constants.versionInvalid;
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return pluginManager.GetRemotePackageAsync(packageId, version); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      WriteLogToTestContextAndClear(nuGetConnector);
    }
    #endregion

    #region TestGetRemotePackageAsync
    [TestMethod]
    public async Task TestGetRemotePackagesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      PluginManager pluginManager = new PluginManager(nuGetConnector);
      string packageId;
      bool includePreReleases;
      PackageIdentity[] expectedPackages;
      RemotePackageInfo[] foundPackages;
      ArgumentNullException argumentNullException;
      ArgumentException argumentException;

      packageId = Constants.namePluginA;
      includePreReleases = false;
      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginA, Constants.version010),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version020),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version021),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030) };
      foundPackages = (await pluginManager.GetRemotePackagesAsync(packageId, includePreReleases)).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages, PackageInfoComparer.Default);

      packageId = Constants.namePluginA;
      includePreReleases = true;
      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginA, Constants.version010_alpha1),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version010_alpha2),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version010),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version020_alpha1),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version020),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version021),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030_alpha1),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030_beta1),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030) };
      foundPackages = (await pluginManager.GetRemotePackagesAsync(packageId, includePreReleases)).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages, PackageInfoComparer.Default);

      packageId = Constants.nameInvalid;
      includePreReleases = true;
      foundPackages = (await pluginManager.GetRemotePackagesAsync(packageId, includePreReleases)).ToArray();
      Assert.AreEqual(0, foundPackages.Length, "Number of found packages is incorrect.");

      packageId = null;
      includePreReleases = false;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return pluginManager.GetRemotePackagesAsync(packageId, includePreReleases); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      packageId = null;
      includePreReleases = true;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return pluginManager.GetRemotePackagesAsync(packageId, includePreReleases); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      packageId = "";
      includePreReleases = false;
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return pluginManager.GetRemotePackagesAsync(packageId, includePreReleases); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      packageId = "";
      includePreReleases = true;
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return pluginManager.GetRemotePackagesAsync(packageId, includePreReleases); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      WriteLogToTestContextAndClear(nuGetConnector);
    }
    #endregion

    #region TestInstallRemotePackageAsync
    [TestMethod]
    public async Task TestInstallRemotePackageAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      PluginManager pluginManager = new PluginManager(nuGetConnector);
      pluginManager.Initialize();
      RemotePackageInfo package;
      bool installMissingDependencies;
      PackageIdentity[] expectedPackages;
      ArgumentNullException argumentNullException;

      package = await pluginManager.GetRemotePackageAsync(Constants.namePluginB, Constants.version030_alpha1);
      installMissingDependencies = false;
      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginB, Constants.version030_alpha1) };
      await pluginManager.InstallRemotePackageAsync(package, installMissingDependencies);
      Assert.IsTrue(expectedPackages.All(x => pluginManager.InstalledPackages.Select(y => y.nuspecReader.GetIdentity()).Contains(x, PackageIdentityComparer.Default)));
      Assert.IsTrue(expectedPackages.All(x => Directory.Exists(Path.Combine(pluginManager.Settings.PackagesPath, x.Id + "." + x.Version.ToString()))));

      package = await pluginManager.GetRemotePackageAsync(Constants.namePluginB, Constants.version031);
      installMissingDependencies = true;
      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginB, Constants.version031),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030) };
      await pluginManager.InstallRemotePackageAsync(package, installMissingDependencies);
      Assert.IsTrue(expectedPackages.All(x => pluginManager.InstalledPackages.Select(y => y.nuspecReader.GetIdentity()).Contains(x, PackageIdentityComparer.Default)));
      Assert.IsTrue(expectedPackages.All(x => Directory.Exists(Path.Combine(pluginManager.Settings.PackagesPath, x.Id + "." + x.Version.ToString()))));

      package = null;
      installMissingDependencies = true;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return pluginManager.InstallRemotePackageAsync(package, installMissingDependencies); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      package = null;
      installMissingDependencies = false;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return pluginManager.InstallRemotePackageAsync(package, installMissingDependencies); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

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
