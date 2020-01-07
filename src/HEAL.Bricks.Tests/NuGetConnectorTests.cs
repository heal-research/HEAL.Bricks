#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System.Collections.Generic;
using System.Diagnostics;
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
  [DeploymentItem(Constants.pathPluginA_010_alpha2, Constants.localRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_010, Constants.localRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_020_alpha1, Constants.localRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_010_alpha2, Constants.localRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_010, Constants.localRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_020_alpha1, Constants.localRepositoryRelativePath)]
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
    private string appDir => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
    private string localRepository = Constants.localRepositoryRelativePath;
    private string remoteOfficialRepository = Constants.remoteOfficialRepositoryRelativePath;
    private string remoteDevRepository = Constants.remoteDevRepositoryRelativePath;
    private string publicNuGetRepository = "https://api.nuget.org/v3/index.json";

    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void TestInitialize() {
      // make remote repository paths absolute
      remoteOfficialRepository = Path.Combine(appDir, remoteOfficialRepository);
      remoteDevRepository = Path.Combine(appDir, remoteDevRepository);
    }

    #region TestCtor
    [TestMethod]
    public void TestCtor() {
      NuGetConnector nuGetConnector;

      nuGetConnector = new NuGetConnector(remoteOfficialRepository, remoteDevRepository);
      Assert.AreEqual(appDir, nuGetConnector.AppDirectory);
      Assert.AreEqual(".NETCoreApp,Version=v1.0", nuGetConnector.CurrentFramework.DotNetFrameworkName);
      Assert.AreEqual(1, nuGetConnector.LocalRepositories.Count());
      Assert.AreEqual(appDir, nuGetConnector.LocalRepositories.First().PackageSource.Source);
      Assert.AreEqual(2, nuGetConnector.RemoteRepositories.Count());
      Assert.AreEqual(remoteOfficialRepository, nuGetConnector.RemoteRepositories.First().PackageSource.Source);
      Assert.AreEqual(remoteDevRepository, nuGetConnector.RemoteRepositories.Skip(1).First().PackageSource.Source);
      Assert.AreEqual(3, nuGetConnector.AllRepositories.Count());
      Assert.AreEqual(appDir, nuGetConnector.AllRepositories.First().PackageSource.Source);
      Assert.AreEqual(remoteOfficialRepository, nuGetConnector.AllRepositories.Skip(1).First().PackageSource.Source);
      Assert.AreEqual(remoteDevRepository, nuGetConnector.AllRepositories.Skip(2).First().PackageSource.Source);

      nuGetConnector = new NuGetConnector(new[] {"", Constants.localRepositoryRelativePath }, new[] { remoteOfficialRepository, remoteDevRepository });
      Assert.AreEqual(appDir, nuGetConnector.AppDirectory);
      Assert.AreEqual(".NETCoreApp,Version=v1.0", nuGetConnector.CurrentFramework.DotNetFrameworkName);
      Assert.AreEqual(2, nuGetConnector.LocalRepositories.Count());
      Assert.AreEqual(appDir, nuGetConnector.LocalRepositories.First().PackageSource.Source);
      Assert.AreEqual(2, nuGetConnector.RemoteRepositories.Count());
      Assert.AreEqual(remoteOfficialRepository, nuGetConnector.RemoteRepositories.First().PackageSource.Source);
      Assert.AreEqual(remoteDevRepository, nuGetConnector.RemoteRepositories.Skip(1).First().PackageSource.Source);
      Assert.AreEqual(4, nuGetConnector.AllRepositories.Count());
      Assert.AreEqual(appDir, nuGetConnector.AllRepositories.First().PackageSource.Source);
      Assert.AreEqual(Path.Combine(appDir, Constants.localRepositoryRelativePath), nuGetConnector.LocalRepositories.Skip(1).First().PackageSource.Source);
      Assert.AreEqual(remoteOfficialRepository, nuGetConnector.AllRepositories.Skip(2).First().PackageSource.Source);
      Assert.AreEqual(remoteDevRepository, nuGetConnector.AllRepositories.Skip(3).First().PackageSource.Source);
    }
    #endregion

    #region TestGetPackageAsync
    [TestMethod]
    public async Task TestGetPackageAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector();
      string package;
      string version;
      string foundPackage;

      package = Constants.namePluginA;
      version = Constants.version010;
      foundPackage = (await nuGetConnector.GetPackageAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), nuGetConnector.AllRepositories, default))?.Identity.ToString();
      Assert.AreEqual(package + "." + version, foundPackage);

      package = Constants.namePluginA;
      version = Constants.version010_alpha1;
      foundPackage = (await nuGetConnector.GetPackageAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), nuGetConnector.AllRepositories, default))?.Identity.ToString();
      Assert.AreEqual(package + "." + version, foundPackage);

      package = Constants.namePluginB;
      version = Constants.version020;
      foundPackage = (await nuGetConnector.GetPackageAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), nuGetConnector.AllRepositories, default))?.Identity.ToString();
      Assert.AreEqual(package + "." + version, foundPackage);

      package = Constants.namePluginB;
      version = Constants.version030_alpha1;
      foundPackage = (await nuGetConnector.GetPackageAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), nuGetConnector.AllRepositories, default))?.Identity.ToString();
      Assert.AreEqual(package + "." + version, foundPackage);

      package = Constants.namePluginA;
      version = Constants.version000;
      foundPackage = (await nuGetConnector.GetPackageAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), nuGetConnector.AllRepositories, default))?.Identity.ToString();
      Assert.IsNull(foundPackage);

      package = Constants.nameInvalid;
      version = Constants.version010;
      foundPackage = (await nuGetConnector.GetPackageAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), nuGetConnector.AllRepositories, default))?.Identity.ToString();
      Assert.IsNull(foundPackage);

      package = Constants.nameInvalid;
      version = Constants.version000;
      foundPackage = (await nuGetConnector.GetPackageAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), nuGetConnector.AllRepositories, default))?.Identity.ToString();
      Assert.IsNull(foundPackage);

      WriteLogToTestContextAndClear(nuGetConnector);
    }
    #endregion

    #region TestGetPackagesAsync
    [TestMethod]
    public async Task TestGetPackagesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector();
      string package;
      string[] expectedPackages;
      string[] foundPackages;
      bool includePreReleases;

      #region include pre-releases
      package = Constants.namePluginA;
      includePreReleases = true;
      expectedPackages = new[] { Constants.version010_alpha2, Constants.version010, Constants.version020_alpha1,
                                 Constants.version010_alpha1, Constants.version020, Constants.version021,
                                 Constants.version030_alpha1, Constants.version030_beta1, Constants.version030 }.Select(x => package + "." + x).ToArray();
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, nuGetConnector.AllRepositories, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      package = Constants.namePluginB;
      includePreReleases = true;
      expectedPackages = new[] { Constants.version010_alpha2, Constants.version010, Constants.version020_alpha1,
                                 Constants.version010_alpha1, Constants.version020,
                                 Constants.version030_alpha1, Constants.version030_alpha2, Constants.version030, Constants.version031 }.Select(x => package + "." + x).ToArray();
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, nuGetConnector.AllRepositories, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      package = Constants.nameInvalid;
      includePreReleases = true;
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, nuGetConnector.AllRepositories, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      Assert.AreEqual(0, foundPackages.Length, "Number of found packages is incorrect.");
      #endregion

      #region exclude pre-releases
      package = Constants.namePluginA;
      includePreReleases = false;
      expectedPackages = new[] { Constants.version010, Constants.version020, Constants.version021, Constants.version030 }.Select(x => package + "." + x).ToArray();
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, nuGetConnector.AllRepositories, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      package = Constants.namePluginB;
      includePreReleases = false;
      expectedPackages = new[] { Constants.version010, Constants.version020, Constants.version030, Constants.version031 }.Select(x => package + "." + x).ToArray();
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, nuGetConnector.AllRepositories, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      package = Constants.nameInvalid;
      includePreReleases = false;
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, nuGetConnector.AllRepositories, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      Assert.AreEqual(0, foundPackages.Length, "Number of found packages is incorrect.");
      #endregion

      WriteLogToTestContextAndClear(nuGetConnector);
    }
    #endregion

    #region TestGetLocalPackagesAsync
    [TestMethod]
    public async Task TestGetLocalPackagesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector();
      string[] expectedPackages;
      string[] foundPackages;
      bool includePreReleases;

      includePreReleases = true;
      expectedPackages = new[] { Constants.nameVersionBricks, Constants.namePluginA + "." + Constants.version020_alpha1, Constants.namePluginB + "." + Constants.version020_alpha1 };
      foundPackages = (await nuGetConnector.GetLocalPackagesAsync(includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      includePreReleases = false;
      expectedPackages = new[] { Constants.namePluginA + "." + Constants.version010, Constants.namePluginB + "." + Constants.version010 };
      foundPackages = (await nuGetConnector.GetLocalPackagesAsync(includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      WriteLogToTestContextAndClear(nuGetConnector);
    }
    #endregion

    #region TestSearchRemotePackagesAsync
    [TestMethod]
    public async Task TestSearchRemotePackagesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      string searchString;
      string[] expectedPackages;
      string[] foundPackages;
      bool includePreReleases;

      searchString = "PackageId:NuGet.Protocol";
      includePreReleases = false;
      expectedPackages = new[] { "NuGet.Protocol.5.4.0" };
      foundPackages = (await nuGetConnector.SearchRemotePackagesAsync(searchString, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      searchString = "PackageId:NuGet.Protocol";
      includePreReleases = true;
      expectedPackages = new[] { "NuGet.Protocol.5.5.0-preview.1.6319" };
      foundPackages = (await nuGetConnector.SearchRemotePackagesAsync(searchString, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      searchString = "PackageId:HEAL.Attic version:1.0.0";
      includePreReleases = true;
      foundPackages = (await nuGetConnector.SearchRemotePackagesAsync(searchString, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      Assert.AreEqual(0, foundPackages.Length, "Number of found packages is incorrect.");

      WriteLogToTestContextAndClear(nuGetConnector);
    }
    #endregion

    #region TestGetPackageDependenciesAsync
    [TestMethod]
    public async Task TestGetPackageDependenciesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      string package;
      string version;
      bool resolveDependenciesRecursively;
      IEnumerable<SourcePackageDependencyInfo> foundDependencies;
      IEnumerable<PackageDependency> packageDependencies;
      Stopwatch sw = new Stopwatch();

      package = Constants.namePluginB;
      version = Constants.version031;
      resolveDependenciesRecursively = false;
      sw.Restart();
      foundDependencies = await nuGetConnector.GetPackageDependenciesAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), nuGetConnector.AllRepositories, resolveDependenciesRecursively, default);
      sw.Stop();
      Assert.AreEqual(1, foundDependencies.Count(), "Number of found dependencies is incorrect.");
      Assert.AreEqual(package, foundDependencies.First().Id);
      Assert.AreEqual(version, foundDependencies.First().Version.ToString());
      packageDependencies = foundDependencies.First().Dependencies;
      Assert.AreEqual(1, packageDependencies.Count(), "Number of package dependencies is incorrect.");
      Assert.AreEqual(Constants.namePluginA, packageDependencies.First().Id);
      Assert.AreEqual(Constants.version030, packageDependencies.First().VersionRange.MinVersion.ToString());
      TestContext.WriteLine($"{(resolveDependenciesRecursively ? "Recursive" : "Non-recursive")} dependency resolution of {package}.{version}");
      TestContext.WriteLine("Duration: " + sw.ElapsedMilliseconds);
      WriteLogToTestContextAndClear(nuGetConnector);
      TestContext.WriteLine("");

      package = Constants.namePluginB;
      version = Constants.version031;
      resolveDependenciesRecursively = true;
      sw.Restart();
      foundDependencies = await nuGetConnector.GetPackageDependenciesAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), nuGetConnector.AllRepositories, resolveDependenciesRecursively, default);
      sw.Stop();
      Assert.AreEqual(83, foundDependencies.Count(), "Number of found dependencies is incorrect.");
      Assert.AreEqual(package, foundDependencies.First().Id);
      Assert.AreEqual(version, foundDependencies.First().Version.ToString());
      packageDependencies = foundDependencies.First().Dependencies;
      Assert.AreEqual(1, packageDependencies.Count(), "Number of package dependencies is incorrect.");
      Assert.AreEqual(Constants.namePluginA, packageDependencies.First().Id);
      Assert.AreEqual(Constants.version030, packageDependencies.First().VersionRange.MinVersion.ToString());
      TestContext.WriteLine($"{(resolveDependenciesRecursively ? "Recursive" : "Non-recursive")} dependency resolution of {package}.{version}");
      TestContext.WriteLine("Duration: " + sw.ElapsedMilliseconds);
      WriteLogToTestContextAndClear(nuGetConnector);
      TestContext.WriteLine("");

      package = Constants.nameCollections;
      version = Constants.versionCollections;
      resolveDependenciesRecursively = false;
      sw.Restart();
      foundDependencies = await nuGetConnector.GetPackageDependenciesAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), nuGetConnector.AllRepositories, resolveDependenciesRecursively, default);
      sw.Stop();
      Assert.AreEqual(1, foundDependencies.Count(), "Number of found dependencies is incorrect.");
      Assert.AreEqual(package, foundDependencies.First().Id);
      Assert.AreEqual(version, foundDependencies.First().Version.ToString());
      packageDependencies = foundDependencies.First().Dependencies;
      Assert.AreEqual(3, packageDependencies.Count(), "Number of package dependencies is incorrect.");
      TestContext.WriteLine($"{(resolveDependenciesRecursively ? "Recursive" : "Non-recursive")} dependency resolution of {package}.{version}");
      TestContext.WriteLine("Duration: " + sw.ElapsedMilliseconds);
      WriteLogToTestContextAndClear(nuGetConnector);
      TestContext.WriteLine("");

      package = Constants.nameCollections;
      version = Constants.versionCollections;
      resolveDependenciesRecursively = true;
      sw.Restart();
      foundDependencies = await nuGetConnector.GetPackageDependenciesAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), nuGetConnector.AllRepositories, resolveDependenciesRecursively, default);
      sw.Stop();
      Assert.AreEqual(4, foundDependencies.Count(), "Number of found dependencies is incorrect.");
      Assert.AreEqual(package, foundDependencies.First().Id);
      Assert.AreEqual(version, foundDependencies.First().Version.ToString());
      packageDependencies = foundDependencies.First().Dependencies;
      Assert.AreEqual(3, packageDependencies.Count(), "Number of package dependencies is incorrect.");
      TestContext.WriteLine($"{(resolveDependenciesRecursively ? "Recursive" : "Non-recursive")} dependency resolution of {package}.{version}");
      TestContext.WriteLine("Duration: " + sw.ElapsedMilliseconds);
      WriteLogToTestContextAndClear(nuGetConnector);

      package = Constants.nameInvalid;
      version = Constants.version000;
      resolveDependenciesRecursively = true;
      sw.Restart();
      foundDependencies = await nuGetConnector.GetPackageDependenciesAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), nuGetConnector.AllRepositories, resolveDependenciesRecursively, default);
      sw.Stop();
      Assert.AreEqual(0, foundDependencies.Count(), "Number of found dependencies is incorrect.");
      TestContext.WriteLine($"{(resolveDependenciesRecursively ? "Recursive" : "Non-recursive")} dependency resolution of {package}.{version}");
      TestContext.WriteLine("Duration: " + sw.ElapsedMilliseconds);
      WriteLogToTestContextAndClear(nuGetConnector);
    }
    #endregion

    #region TestGetPackageDownloaderAsync
    [TestMethod]
    public async Task TestGetPackageDownloaderAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector();
      string package;
      string version;
      IPackageDownloader foundPackageDownloader;

      package = Constants.namePluginA;
      version = Constants.version010_alpha1;
      foundPackageDownloader = await nuGetConnector.GetPackageDownloaderAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), default);
      Assert.IsNotNull(foundPackageDownloader);
      Assert.AreEqual(remoteOfficialRepository, foundPackageDownloader.Source);

      package = Constants.namePluginA;
      version = Constants.version010;
      foundPackageDownloader = await nuGetConnector.GetPackageDownloaderAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), default);
      Assert.IsNotNull(foundPackageDownloader);
      Assert.AreEqual(remoteOfficialRepository, foundPackageDownloader.Source);

      package = Constants.namePluginA;
      version = Constants.version030_beta1;
      foundPackageDownloader = await nuGetConnector.GetPackageDownloaderAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), default);
      Assert.IsNotNull(foundPackageDownloader);
      Assert.AreEqual(remoteDevRepository, foundPackageDownloader.Source);

      package = Constants.namePluginA;
      version = Constants.version030;
      foundPackageDownloader = await nuGetConnector.GetPackageDownloaderAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), default);
      Assert.IsNotNull(foundPackageDownloader);
      Assert.AreEqual(remoteDevRepository, foundPackageDownloader.Source);

      package = Constants.namePluginA;
      version = Constants.version000;
      foundPackageDownloader = await nuGetConnector.GetPackageDownloaderAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), default);
      Assert.IsNull(foundPackageDownloader);

      package = Constants.nameInvalid;
      version = Constants.version010;
      foundPackageDownloader = await nuGetConnector.GetPackageDownloaderAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), default);
      Assert.IsNull(foundPackageDownloader);

      package = Constants.nameInvalid;
      version = Constants.version000;
      foundPackageDownloader = await nuGetConnector.GetPackageDownloaderAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), default);
      Assert.IsNull(foundPackageDownloader);

      WriteLogToTestContextAndClear(nuGetConnector);
    }
    #endregion

    #region Helpers
    private NuGetConnector CreateNuGetConnector(bool includePublicNuGetRepository = false) {
      NuGetConnector nuGetConnector = includePublicNuGetRepository switch {
        false => new NuGetConnector(new[] { localRepository }, new[] { remoteOfficialRepository, remoteDevRepository }),
        true => new NuGetConnector(new[] { localRepository }, new[] { remoteOfficialRepository, remoteDevRepository, publicNuGetRepository })
      };
      nuGetConnector.EnableLogging(LogLevel.Debug);
      nuGetConnector.SetFrameworkForUnitTests(".NETCoreApp,Version=v3.1");
      return nuGetConnector;
    }

    private void WriteLogToTestContextAndClear(NuGetConnector nuGetConnector, string header = null) {
      string[] log = nuGetConnector.GetLog();
      if (log.Length > 0) {
        if (header != null) TestContext.WriteLine(header);
        TestContext.WriteLine("NuGetConnector Log:");
        foreach (string line in log)
          TestContext.WriteLine(line);
        TestContext.WriteLine("");
        nuGetConnector.ClearLog();
      }
    }
    #endregion
  }
}
