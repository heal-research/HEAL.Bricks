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
  public class PackageManagerTests : PackageTestsBase {
    #region TestCreate
    [TestMethod]
    public void TestCreate() {
      ISettings settings;
      ArgumentNullException argumentNullException;

      settings = null;
      argumentNullException = Assert.ThrowsException<ArgumentNullException>(() => { PackageManager.Create(settings); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));
    }
    #endregion

    #region TestCtor
    [TestMethod]
    public void TestCtor() {
      ISettings settings = CreateSettings(includePublicNuGetRepository: true);
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      PackageIdentity[] expectedPackages;
      LocalPackageInfo[] installedPackages;
      List<PackageIdentity> expectedPackagesList = new List<PackageIdentity>();
      foreach (PackageFolderReader expectedPackageReader in nuGetConnector.GetInstalledPackages(LocalPackagesAbsolutePath)) {
        expectedPackagesList.Add(expectedPackageReader.GetIdentity());
        expectedPackageReader.Dispose();
      }
      expectedPackages = expectedPackagesList.ToArray();

      IPackageManager pm = PackageManager.CreateForTests(settings, nuGetConnector);

      Assert.IsNotNull(pm);
      Assert.IsNotNull(pm.Settings);
      Assert.IsNotNull(pm.InstalledPackages);
      installedPackages = pm.InstalledPackages.ToArray();
      CollectionAssert.AreEqual(expectedPackages, installedPackages, PackageInfoComparer.Default);
      Assert.IsTrue((pm.Status == PackageManagerStatus.OK) || (pm.Status == PackageManagerStatus.InvalidPackages));
      foreach (LocalPackageInfo installedPackage in installedPackages) {
        Assert.IsTrue(installedPackage.Status != PackageStatus.Undefined);
        Assert.IsTrue(installedPackage.Dependencies.All(x => x.Status != PackageDependencyStatus.Undefined));
      }

      WriteLogToTestContextAndClear();
    }
    #endregion

    #region TestGetMissingDependenciesAsync
    [TestMethod]
    public async Task TestGetMissingDependenciesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      IPackageManager pm = PackageManager.CreateForTests(CreateSettings(includePublicNuGetRepository: true), nuGetConnector);
      PackageIdentity[] expectedPackages;
      RemotePackageInfo[] missingPackages;

      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginA, Constants.version030) };
      missingPackages = (await pm.GetMissingDependenciesAsync()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, missingPackages, PackageInfoComparer.Default);

      WriteLogToTestContextAndClear();
    }
    #endregion

    #region TestInstallMissingDependenciesAsync
    [TestMethod]
    public async Task TestInstallMissingDependenciesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      IPackageManager pm = PackageManager.CreateForTests(CreateSettings(includePublicNuGetRepository: true), nuGetConnector);
      PackageIdentity[] expectedPackages;

      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginA, Constants.version030) };
      await pm.InstallMissingDependenciesAsync();
      Assert.IsTrue(expectedPackages.All(x => Directory.Exists(Path.Combine(pm.Settings.PackagesPath, x.Id + "." + x.Version.ToString()))));

      WriteLogToTestContextAndClear();
    }
    #endregion

    #region TestSearchRemotePackageAsync
    [TestMethod]
    public async Task TestSearchRemotePackageAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: false);
      IPackageManager pm = PackageManager.CreateForTests(CreateSettings(includePublicNuGetRepository: false), nuGetConnector);
      string searchString;
      bool includePreReleases;
      PackageIdentity[] expectedPackages;
      RemotePackageInfo[] foundPackages;
      ArgumentNullException argumentNullException;

      searchString = Constants.namePluginA;
      includePreReleases = false;
      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginA, Constants.version021),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030) };
      foundPackages = (await pm.SearchRemotePackagesAsync(searchString, 0, int.MaxValue, includePreReleases)).Select(x => x.Package).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages, PackageInfoComparer.Default);

      searchString = Constants.namePluginB;
      includePreReleases = false;
      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginB, Constants.version020),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version031) };
      foundPackages = (await pm.SearchRemotePackagesAsync(searchString, 0, int.MaxValue, includePreReleases)).Select(x => x.Package).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages, PackageInfoComparer.Default);

      searchString = Constants.nameInvalid;
      includePreReleases = false;
      foundPackages = (await pm.SearchRemotePackagesAsync(searchString, 0, int.MaxValue, includePreReleases)).Select(x => x.Package).ToArray();
      Assert.AreEqual(0, foundPackages.Length, "Number of found packages is incorrect.");

      searchString = null;
      includePreReleases = false;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return pm.SearchRemotePackagesAsync(searchString, 0, int.MaxValue, includePreReleases); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      searchString = null;
      includePreReleases = true;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return pm.SearchRemotePackagesAsync(searchString, 0, int.MaxValue, includePreReleases); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      WriteLogToTestContextAndClear();
    }
    #endregion

    #region TestGetRemotePackageAsync
    [TestMethod]
    public async Task TestGetRemotePackageAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      IPackageManager pm = PackageManager.CreateForTests(CreateSettings(includePublicNuGetRepository: true), nuGetConnector);
      string packageId;
      string version;
      RemotePackageInfo foundPackage;
      ArgumentNullException argumentNullException;
      ArgumentException argumentException;

      packageId = Constants.namePluginA;
      version = Constants.version030_beta1;
      foundPackage = await pm.GetRemotePackageAsync(packageId, version);
      Assert.AreEqual(packageId, foundPackage.Id);
      Assert.AreEqual(version, foundPackage.Version.ToString());

      packageId = Constants.namePluginA;
      version = Constants.version030;
      foundPackage = await pm.GetRemotePackageAsync(packageId, version);
      Assert.AreEqual(packageId, foundPackage.Id);
      Assert.AreEqual(version, foundPackage.Version.ToString());

      packageId = Constants.nameCollections;
      version = Constants.versionCollections;
      foundPackage = await pm.GetRemotePackageAsync(packageId, version);
      Assert.AreEqual(packageId, foundPackage.Id);
      Assert.AreEqual(version, foundPackage.Version.ToString());

      packageId = Constants.nameInvalid;
      version = Constants.version000;
      foundPackage = await pm.GetRemotePackageAsync(packageId, version);
      Assert.IsNull(foundPackage);

      packageId = null;
      version = Constants.version000;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return pm.GetRemotePackageAsync(packageId, version); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      packageId = "";
      version = Constants.version000;
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return pm.GetRemotePackageAsync(packageId, version); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      packageId = Constants.namePluginA;
      version = null;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return pm.GetRemotePackageAsync(packageId, version); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      packageId = Constants.namePluginA;
      version = "";
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return pm.GetRemotePackageAsync(packageId, version); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      packageId = Constants.namePluginA;
      version = Constants.versionInvalid;
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return pm.GetRemotePackageAsync(packageId, version); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      WriteLogToTestContextAndClear();
    }
    #endregion

    #region TestGetRemotePackagesAsync
    [TestMethod]
    public async Task TestGetRemotePackagesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      IPackageManager pm = PackageManager.CreateForTests(CreateSettings(includePublicNuGetRepository: true), nuGetConnector);
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
      foundPackages = (await pm.GetRemotePackagesAsync(packageId, includePreReleases)).ToArray();
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
      foundPackages = (await pm.GetRemotePackagesAsync(packageId, includePreReleases)).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages, PackageInfoComparer.Default);

      packageId = Constants.nameInvalid;
      includePreReleases = true;
      foundPackages = (await pm.GetRemotePackagesAsync(packageId, includePreReleases)).ToArray();
      Assert.AreEqual(0, foundPackages.Length, "Number of found packages is incorrect.");

      packageId = null;
      includePreReleases = false;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return pm.GetRemotePackagesAsync(packageId, includePreReleases); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      packageId = null;
      includePreReleases = true;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return pm.GetRemotePackagesAsync(packageId, includePreReleases); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      packageId = "";
      includePreReleases = false;
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return pm.GetRemotePackagesAsync(packageId, includePreReleases); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      packageId = "";
      includePreReleases = true;
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return pm.GetRemotePackagesAsync(packageId, includePreReleases); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      WriteLogToTestContextAndClear();
    }
    #endregion

    #region TestInstallRemotePackageAsync
    [TestMethod]
    public async Task TestInstallRemotePackageAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      IPackageManager pm = PackageManager.CreateForTests(CreateSettings(includePublicNuGetRepository: true), nuGetConnector);
      RemotePackageInfo package;
      bool installMissingDependencies;
      PackageIdentity[] expectedPackages;
      ArgumentNullException argumentNullException;

      package = await pm.GetRemotePackageAsync(Constants.namePluginB, Constants.version030_alpha1);
      installMissingDependencies = false;
      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginB, Constants.version030_alpha1) };
      await pm.InstallRemotePackageAsync(package, installMissingDependencies);
      Assert.IsTrue(expectedPackages.All(x => pm.InstalledPackages.Select(y => y.nuspecReader.GetIdentity()).Contains(x, PackageIdentityComparer.Default)));
      Assert.IsTrue(expectedPackages.All(x => Directory.Exists(Path.Combine(pm.Settings.PackagesPath, x.Id + "." + x.Version.ToString()))));

      package = await pm.GetRemotePackageAsync(Constants.namePluginB, Constants.version031);
      installMissingDependencies = true;
      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginB, Constants.version031),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030) };
      await pm.InstallRemotePackageAsync(package, installMissingDependencies);
      Assert.IsTrue(expectedPackages.All(x => pm.InstalledPackages.Select(y => y.nuspecReader.GetIdentity()).Contains(x, PackageIdentityComparer.Default)));
      Assert.IsTrue(expectedPackages.All(x => Directory.Exists(Path.Combine(pm.Settings.PackagesPath, x.Id + "." + x.Version.ToString()))));

      package = null;
      installMissingDependencies = true;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return pm.InstallRemotePackageAsync(package, installMissingDependencies); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      package = null;
      installMissingDependencies = false;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return pm.InstallRemotePackageAsync(package, installMissingDependencies); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      WriteLogToTestContextAndClear();
    }
    #endregion

    #region TestInstallRemotePackagesAsync
    [TestMethod]
    public async Task TestInstallRemotePackagesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      IPackageManager pm = PackageManager.CreateForTests(CreateSettings(includePublicNuGetRepository: true), nuGetConnector);
      RemotePackageInfo[] packages;
      bool installMissingDependencies;
      PackageIdentity[] expectedPackages;
      ArgumentNullException argumentNullException;
      ArgumentException argumentException;

      packages = new[] { await pm.GetRemotePackageAsync(Constants.namePluginB, Constants.version030_alpha1),
                         await pm.GetRemotePackageAsync(Constants.namePluginB, Constants.version030_alpha2) };
      installMissingDependencies = false;
      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginB, Constants.version030_alpha1),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version030_alpha2) };
      await pm.InstallRemotePackagesAsync(packages, installMissingDependencies);
      Assert.IsTrue(expectedPackages.All(x => pm.InstalledPackages.Select(y => y.nuspecReader.GetIdentity()).Contains(x, PackageIdentityComparer.Default)));
      Assert.IsTrue(expectedPackages.All(x => Directory.Exists(Path.Combine(pm.Settings.PackagesPath, x.Id + "." + x.Version.ToString()))));

      packages = new[] { await pm.GetRemotePackageAsync(Constants.namePluginB, Constants.version030),
                         await pm.GetRemotePackageAsync(Constants.namePluginB, Constants.version031) };
      installMissingDependencies = true;
      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginB, Constants.version030),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version031),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030) };
      await pm.InstallRemotePackagesAsync(packages, installMissingDependencies);
      Assert.IsTrue(expectedPackages.All(x => pm.InstalledPackages.Select(y => y.nuspecReader.GetIdentity()).Contains(x, PackageIdentityComparer.Default)));
      Assert.IsTrue(expectedPackages.All(x => Directory.Exists(Path.Combine(pm.Settings.PackagesPath, x.Id + "." + x.Version.ToString()))));

      packages = new[] { await pm.GetRemotePackageAsync(Constants.namePluginB, Constants.version010_alpha2),
                         await pm.GetRemotePackageAsync(Constants.namePluginB, Constants.version010_alpha2) };
      installMissingDependencies = false;
      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginB, Constants.version010_alpha2) };
      await pm.InstallRemotePackagesAsync(packages, installMissingDependencies);
      Assert.IsTrue(expectedPackages.All(x => pm.InstalledPackages.Select(y => y.nuspecReader.GetIdentity()).Contains(x, PackageIdentityComparer.Default)));
      Assert.IsTrue(expectedPackages.All(x => Directory.Exists(Path.Combine(pm.Settings.PackagesPath, x.Id + "." + x.Version.ToString()))));

      packages = Array.Empty<RemotePackageInfo>();
      installMissingDependencies = false;
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return pm.InstallRemotePackagesAsync(packages, installMissingDependencies); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      packages = null;
      installMissingDependencies = false;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return pm.InstallRemotePackagesAsync(packages, installMissingDependencies); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      packages = new RemotePackageInfo[] { null };
      installMissingDependencies = false;
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return pm.InstallRemotePackagesAsync(packages, installMissingDependencies); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      WriteLogToTestContextAndClear();
    }
    #endregion

    #region TestRemoveInstalledPackage
    [TestMethod]
    public void TestRemoveInstalledPackage() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      IPackageManager pm = PackageManager.CreateForTests(CreateSettings(includePublicNuGetRepository: true), nuGetConnector);
      LocalPackageInfo package;
      ArgumentNullException argumentNullException;

      package = pm.InstalledPackages.First();
      pm.RemoveInstalledPackage(package);
      Assert.IsFalse(pm.InstalledPackages.Contains(package, PackageInfoIdentityComparer.Default));
      Assert.IsFalse(Directory.Exists(Path.Combine(pm.Settings.PackagesPath, package.Id + "." + package.Version.ToString())));

      package = null;
      argumentNullException = Assert.ThrowsException<ArgumentNullException>(() => { pm.RemoveInstalledPackage(package); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      WriteLogToTestContextAndClear();
    }
    #endregion

    #region TestRemoveInstalledPackages
    [TestMethod]
    public void TestRemoveInstalledPackages() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      IPackageManager pm = PackageManager.CreateForTests(CreateSettings(includePublicNuGetRepository: true), nuGetConnector);
      LocalPackageInfo[] packages;
      ArgumentNullException argumentNullException;
      ArgumentException argumentException;
      InvalidOperationException invalidOperationException;

      packages = pm.InstalledPackages.Take(2).ToArray();
      pm.RemoveInstalledPackages(packages);
      Assert.IsFalse(packages.Any(x => pm.InstalledPackages.Contains(x, PackageInfoIdentityComparer.Default)));
      Assert.IsFalse(packages.Any(x => Directory.Exists(Path.Combine(pm.Settings.PackagesPath, x.Id + "." + x.Version.ToString()))));

      packages = new[] { pm.InstalledPackages.First(),
                         pm.InstalledPackages.First() };
      invalidOperationException = Assert.ThrowsException<InvalidOperationException>(() => { pm.RemoveInstalledPackages(packages); });

      packages = Array.Empty<LocalPackageInfo>();
      argumentException = Assert.ThrowsException<ArgumentException>(() => { pm.RemoveInstalledPackages(packages); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      packages = null;
      argumentNullException = Assert.ThrowsException<ArgumentNullException>(() => { pm.RemoveInstalledPackages(packages); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      packages = new LocalPackageInfo[] { null };
      argumentException = Assert.ThrowsException<ArgumentException>(() => { pm.RemoveInstalledPackages(packages); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      WriteLogToTestContextAndClear();
    }
    #endregion

    #region TestGetPackageUpdateAsync
    [TestMethod]
    public async Task TestGetPackageUpdateAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      IPackageManager pm = PackageManager.CreateForTests(CreateSettings(includePublicNuGetRepository: true), nuGetConnector);
      LocalPackageInfo package;
      PackageIdentity expectedPackage;
      RemotePackageInfo foundPackage;
      ArgumentNullException argumentNullException;

      package = pm.InstalledPackages.Where(x => x.Id == Constants.namePluginB).First();
      expectedPackage = CreatePackageIdentity(Constants.namePluginB, Constants.version031);
      foundPackage = await pm.GetPackageUpdateAsync(package);
      Assert.AreEqual(expectedPackage, foundPackage.packageIdentity);

      package = null;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return pm.GetPackageUpdateAsync(package); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      WriteLogToTestContextAndClear();
    }
    #endregion

    #region TestGetPackageUpdatesAsync
    [TestMethod]
    public async Task TestGetPackageUpdatesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      IPackageManager pm = PackageManager.CreateForTests(CreateSettings(includePublicNuGetRepository: true), nuGetConnector);
      LocalPackageInfo[] packages;
      PackageIdentity[] expectedPackages;
      PackageIdentity[] foundPackages;
      ArgumentNullException argumentNullException;
      ArgumentException argumentException;

      packages = pm.InstalledPackages.Where(x => x.Id == Constants.namePluginB).ToArray();
      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginB, Constants.version031) };
      foundPackages = (await pm.GetPackageUpdatesAsync(packages)).Select(x => x.packageIdentity).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      packages = new[] { pm.InstalledPackages.Where(x => x.Id == Constants.namePluginB).First(),
                         pm.InstalledPackages.Where(x => x.Id == Constants.namePluginB).First() };
      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginB, Constants.version031) };
      foundPackages = (await pm.GetPackageUpdatesAsync(packages)).Select(x => x.packageIdentity).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginB, Constants.version031) };
      foundPackages = (await pm.GetPackageUpdatesAsync()).Select(x => x.packageIdentity).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      packages = Array.Empty<LocalPackageInfo>();
      foundPackages = (await pm.GetPackageUpdatesAsync(packages)).Select(x => x.packageIdentity).ToArray();
      Assert.AreEqual(0, foundPackages.Length, "Number of found packages is incorrect.");

      packages = null;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return pm.GetPackageUpdatesAsync(packages); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      packages = new LocalPackageInfo[] { null };
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return pm.GetPackageUpdatesAsync(packages); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      WriteLogToTestContextAndClear();
    }
    #endregion

    #region TestInstallPackageUpdatesAsync
    [TestMethod]
    public async Task TestInstallPackageUpdatesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      IPackageManager pm = PackageManager.CreateForTests(CreateSettings(includePublicNuGetRepository: true), nuGetConnector);
      PackageIdentity[] expectedPackages;
      bool installMissingDependencies;

      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginB, Constants.version031),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030) };
      installMissingDependencies = true;
      await pm.InstallPackageUpdatesAsync(installMissingDependencies);
      Assert.IsTrue(expectedPackages.All(x => pm.InstalledPackages.Select(y => y.nuspecReader.GetIdentity()).Contains(x, PackageIdentityComparer.Default)));
      Assert.IsTrue(expectedPackages.All(x => Directory.Exists(Path.Combine(pm.Settings.PackagesPath, x.Id + "." + x.Version.ToString()))));

      WriteLogToTestContextAndClear();
    }
    #endregion

    #region TestLoadPackageAssemblies
    [TestMethod]
    public void TestLoadPackageAssemblies() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      IPackageManager pm = PackageManager.CreateForTests(CreateSettings(includePublicNuGetRepository: true), nuGetConnector);
      LocalPackageInfo package;

      package = pm.InstalledPackages.Where(x => x.Id == Constants.nameBricksPluginTypes).Single();
      pm.LoadPackageAssemblies(package);
      Assert.IsTrue(package.Status == PackageStatus.Loaded);

      pm.InstallMissingDependenciesAsync().Wait();
      pm.LoadPackageAssemblies();
      Assert.IsTrue(pm.InstalledPackages.All(x => (x.Status == PackageStatus.Loaded) || (x.Status == PackageStatus.Outdated)));

      WriteLogToTestContextAndClear();
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
