#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HEAL.Bricks.Tests {
  [TestClass]
  // HEAL.Bricks package
  [DeploymentItem(Constants.pathBricks, Constants.localRepositoryRelativePath)]
  [DeploymentItem(Constants.pathBricks, Constants.remoteOfficialRepositoryRelativePath)]
  [DeploymentItem(Constants.pathBricks, Constants.remoteDevRepositoryRelativePath)]
  // local plugins
  [DeploymentItem(Constants.pathPluginA_010, Constants.localRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_010, Constants.localRepositoryRelativePath)]
  // released plugins
  [DeploymentItem(Constants.pathPluginA_010_alpha1, Constants.remoteOfficialRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_010_alpha2, Constants.remoteOfficialRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_010, Constants.remoteOfficialRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_020_alpha1, Constants.remoteOfficialRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_020, Constants.remoteOfficialRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_021, Constants.remoteOfficialRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_010_alpha1, Constants.remoteOfficialRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_010_alpha2, Constants.remoteOfficialRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_010, Constants.remoteOfficialRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_020_alpha1, Constants.remoteOfficialRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_020, Constants.remoteOfficialRepositoryRelativePath)]
  // plugins in development
  [DeploymentItem(Constants.pathPluginA_030_alpha1, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_030_beta1, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_030, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_030_alpha1, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_030_alpha2, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_030, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_031, Constants.remoteDevRepositoryRelativePath)]
  public class NuGetConnectorTests {
    public string localRepository = Constants.localRepositoryRelativePath;
    public string remoteOfficialRepository = Constants.remoteOfficialRepositoryRelativePath;
    public string remoteDevRepository = Constants.remoteDevRepositoryRelativePath;

    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void TestInitialize() {
      string appDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
      // make repository paths absolute
      localRepository = Path.Combine(appDir, localRepository);
      remoteOfficialRepository = Path.Combine(appDir, remoteOfficialRepository);
      remoteDevRepository = Path.Combine(appDir, remoteDevRepository);
    }

    #region TestCtor
    [TestMethod]
    public void TestCtor() {
      NuGetConnector nuGetConnector = new NuGetConnector(remoteOfficialRepository, remoteDevRepository);

      string appDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
      Assert.AreEqual(appDir, nuGetConnector.AppDirectory);
      Assert.AreEqual(".NETCoreApp,Version=v1.0", nuGetConnector.CurrentFramework.DotNetFrameworkName);
      Assert.AreEqual(appDir, nuGetConnector.LocalRepository.PackageSource.Source);
      Assert.AreEqual(2, nuGetConnector.RemoteRepositories.Count());
      Assert.AreEqual(remoteOfficialRepository, nuGetConnector.RemoteRepositories.First().PackageSource.Source);
      Assert.AreEqual(remoteDevRepository, nuGetConnector.RemoteRepositories.Skip(1).First().PackageSource.Source);
      Assert.AreEqual(3, nuGetConnector.AllRepositories.Count());
      Assert.AreEqual(appDir, nuGetConnector.AllRepositories.First().PackageSource.Source);
      Assert.AreEqual(remoteOfficialRepository, nuGetConnector.AllRepositories.Skip(1).First().PackageSource.Source);
      Assert.AreEqual(remoteDevRepository, nuGetConnector.AllRepositories.Skip(2).First().PackageSource.Source);
    }
    #endregion

    #region TestGetPackageAsync
    [TestMethod]
    public async Task TestGetPackageAsync() {
      NuGetConnector nuGetConnector = new NuGetConnector(localRepository, remoteOfficialRepository, remoteDevRepository);
      string package;
      string version;
      string foundPackage;

      package = Constants.namePluginA;
      version = Constants.version010;
      foundPackage = (await nuGetConnector.GetPackageAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), nuGetConnector.RemoteRepositories, default))?.Identity.ToString();
      Assert.AreEqual(package + "." + version, foundPackage);

      package = Constants.namePluginA;
      version = Constants.version010_alpha1;
      foundPackage = (await nuGetConnector.GetPackageAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), nuGetConnector.RemoteRepositories, default))?.Identity.ToString();
      Assert.AreEqual(package + "." + version, foundPackage);

      package = Constants.namePluginB;
      version = Constants.version020;
      foundPackage = (await nuGetConnector.GetPackageAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), nuGetConnector.RemoteRepositories, default))?.Identity.ToString();
      Assert.AreEqual(package + "." + version, foundPackage);

      package = Constants.namePluginB;
      version = Constants.version030_alpha1;
      foundPackage = (await nuGetConnector.GetPackageAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), nuGetConnector.RemoteRepositories, default))?.Identity.ToString();
      Assert.AreEqual(package + "." + version, foundPackage);

      package = Constants.namePluginA;
      version = Constants.version000;
      foundPackage = (await nuGetConnector.GetPackageAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), nuGetConnector.RemoteRepositories, default))?.Identity.ToString();
      Assert.IsNull(foundPackage);

      package = Constants.nameInvalid;
      version = Constants.version010;
      foundPackage = (await nuGetConnector.GetPackageAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), nuGetConnector.RemoteRepositories, default))?.Identity.ToString();
      Assert.IsNull(foundPackage);

      package = Constants.nameInvalid;
      version = Constants.version000;
      foundPackage = (await nuGetConnector.GetPackageAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), nuGetConnector.RemoteRepositories, default))?.Identity.ToString();
      Assert.IsNull(foundPackage);
    }
    #endregion

    #region TestGetPackagesAsync
    [TestMethod]
    public async Task TestGetPackagesAsync() {
      NuGetConnector nuGetConnector = new NuGetConnector(localRepository, remoteOfficialRepository, remoteDevRepository);
      string package;
      string[] expectedPackages;
      string[] foundPackages;
      bool includePreReleases;

      #region include pre-releases
      package = Constants.namePluginA;
      includePreReleases = true;
      expectedPackages = new[] { Constants.version010, Constants.version010_alpha1, Constants.version010_alpha2,
                                 Constants.version020_alpha1, Constants.version020, Constants.version021,
                                 Constants.version030_alpha1, Constants.version030_beta1, Constants.version030 }.Select(x => package + "." + x).ToArray();
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, nuGetConnector.RemoteRepositories, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      package = Constants.namePluginB;
      includePreReleases = true;
      expectedPackages = new[] { Constants.version010, Constants.version010_alpha1, Constants.version010_alpha2,
                                 Constants.version020_alpha1, Constants.version020,
                                 Constants.version030_alpha1, Constants.version030_alpha2, Constants.version030, Constants.version031 }.Select(x => package + "." + x).ToArray();
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, nuGetConnector.RemoteRepositories, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      package = Constants.nameInvalid;
      includePreReleases = true;
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, nuGetConnector.RemoteRepositories, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      Assert.AreEqual(0, foundPackages.Length, "Number of found packages is incorrect.");
      #endregion

      #region exclude pre-releases
      package = Constants.namePluginA;
      includePreReleases = false;
      expectedPackages = new[] { Constants.version010, Constants.version020, Constants.version021, Constants.version030 }.Select(x => package + "." + x).ToArray();
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, nuGetConnector.RemoteRepositories, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      package = Constants.namePluginB;
      includePreReleases = false;
      expectedPackages = new[] { Constants.version010, Constants.version020, Constants.version030, Constants.version031 }.Select(x => package + "." + x).ToArray();
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, nuGetConnector.RemoteRepositories, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      package = Constants.nameInvalid;
      includePreReleases = false;
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, nuGetConnector.RemoteRepositories, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      Assert.AreEqual(0, foundPackages.Length, "Number of found packages is incorrect.");
      #endregion
    }
    #endregion

    #region TestSearchPackagesAsync
    [TestMethod]
    public async Task TestSearchPackagesAsync() {
      NuGetConnector nuGetConnector = new NuGetConnector(localRepository, remoteOfficialRepository, remoteDevRepository);
      string searchString;
      string[] expectedPackages;
      string[] foundPackages;
      bool includePreReleases;

      searchString = "id:" + Constants.namePluginA;
      includePreReleases = false;
      expectedPackages = new[] { Constants.namePluginA + "." + Constants.version030 };
      foundPackages = (await nuGetConnector.SearchPackagesAsync(searchString, nuGetConnector.RemoteRepositories, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
//      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      searchString = "id:" + Constants.namePluginB;
      includePreReleases = true;
      expectedPackages = new[] { Constants.namePluginB + "." + Constants.version031 };
      foundPackages = (await nuGetConnector.SearchPackagesAsync(searchString, nuGetConnector.RemoteRepositories, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
//      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      searchString = "id:" + Constants.nameInvalid;
      includePreReleases = true;
      foundPackages = (await nuGetConnector.SearchPackagesAsync(searchString, nuGetConnector.RemoteRepositories, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
//      Assert.AreEqual(0, foundPackages.Length, "Number of found packages is incorrect.");

      searchString = "tags:HEALBricksPlugins";
      includePreReleases = true;
      expectedPackages = new[] { Constants.namePluginA + "." + Constants.version030, Constants.namePluginB + "." + Constants.version031 };
      foundPackages = (await nuGetConnector.SearchPackagesAsync(searchString, nuGetConnector.RemoteRepositories, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
//      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      searchString = "";
      includePreReleases = false;
      expectedPackages = new[] { Constants.namePluginA + "." + Constants.version030, Constants.namePluginB + "." + Constants.version031 };
      foundPackages = (await nuGetConnector.SearchPackagesAsync(searchString, nuGetConnector.RemoteRepositories, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
//      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      searchString = "";
      includePreReleases = true;
      expectedPackages = new[] {Constants.nameBricks + "." + Constants.versionBricks, Constants.namePluginA + "." + Constants.version030, Constants.namePluginB + "." + Constants.version031 };
      foundPackages = (await nuGetConnector.SearchPackagesAsync(searchString, nuGetConnector.RemoteRepositories, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
//      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      nuGetConnector = new NuGetConnector(Constants.nuGetGalleryURL);
      searchString = "id:HEAL.Attic";
      includePreReleases = true;
      expectedPackages = new[] { Constants.nameBricks + "." + Constants.versionBricks, Constants.namePluginA + "." + Constants.version030, Constants.namePluginB + "." + Constants.version031 };
      foundPackages = (await nuGetConnector.SearchPackagesAsync(searchString, nuGetConnector.RemoteRepositories, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
    }
    #endregion

    #region TestGetPackageDependenciesAsync
    [TestMethod]
    public async Task TestGetPackageDependenciesAsync() {
      NuGetConnector nuGetConnector = new NuGetConnector(localRepository, remoteOfficialRepository, remoteDevRepository);
      string package;
      string version;
      bool resolveDependenciesRecursively;

      package = Constants.namePluginA;
      version = Constants.version030;
      resolveDependenciesRecursively = false;
      foreach (var dependency in await nuGetConnector.GetPackageDependenciesAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), nuGetConnector.RemoteRepositories, resolveDependenciesRecursively, default)) {
        TestContext.WriteLine(dependency.ToString());
      }
      TestContext.WriteLine("");

      package = Constants.namePluginA;
      version = Constants.version030;
      resolveDependenciesRecursively = true;
      foreach (var dependency in await nuGetConnector.GetPackageDependenciesAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), nuGetConnector.RemoteRepositories, resolveDependenciesRecursively, default)) {
        TestContext.WriteLine(dependency.ToString());
      }
    }
    #endregion
  }
}
