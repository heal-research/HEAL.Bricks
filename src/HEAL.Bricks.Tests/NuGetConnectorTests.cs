#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HEAL.Bricks.Tests {
  [TestClass]
  // HEAL.Bricks.PluginTypes package
  [DeploymentItem(Constants.pathBricksPluginTypes, Constants.localPackagesRelativePath)]
  [DeploymentItem(Constants.pathBricksPluginTypes, Constants.remoteOfficialRepositoryRelativePath)]
  [DeploymentItem(Constants.pathBricksPluginTypes, Constants.remoteDevRepositoryRelativePath)]
  // local plugins
  [DeploymentItem(Constants.pathPluginB_010, Constants.localPackagesRelativePath)]
  [DeploymentItem(Constants.pathPluginB_020_alpha1, Constants.localPackagesRelativePath)]
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
  [DeploymentItem(Constants.pathPluginA_020_alpha1, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_020, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_021, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_030_alpha1, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_030_beta1, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_030, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_020_alpha1, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_020, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_030_alpha1, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_030_alpha2, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_030, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_031, Constants.remoteDevRepositoryRelativePath)]
  public class NuGetConnectorTests {
    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void UnpackLocalPackages() {
      string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      string packagesPath = Path.Combine(appPath, Constants.localPackagesRelativePath);
      PackagePathResolver packagePathResolver = new PackagePathResolver(packagesPath);
      PackageExtractionContext packageExtractionContext = new PackageExtractionContext(PackageSaveMode.Defaultv3, XmlDocFileSaveMode.Skip, null, NullLogger.Instance);

      foreach (string package in Directory.GetDirectories(packagesPath)) {
        Directory.Delete(package, true);
      }

      foreach (string package in Directory.GetFiles(packagesPath, "*.nupkg")) {
        PackageArchiveReader packageReader = new PackageArchiveReader(package);
        PackageExtractor.ExtractPackageAsync(package, packageReader, packagePathResolver, packageExtractionContext, default);
      }
    }

    #region TestCtor
    [TestMethod]
    public void TestCtor() {
      ISettings settings = Settings.Default;
      NuGetConnector nuGetConnector = new NuGetConnector(settings);
      Assert.AreEqual(settings, nuGetConnector.Settings);
      Assert.AreEqual(".NETCoreApp,Version=v1.0", nuGetConnector.CurrentFramework.DotNetFrameworkName);
      CollectionAssert.AreEqual(settings.Repositories.ToArray(), nuGetConnector.Repositories.Select(x => x.PackageSource.Source).ToArray());
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
      foundPackage = (await nuGetConnector.GetPackageAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), default))?.Identity.ToString();
      Assert.AreEqual(package + "." + version, foundPackage);

      package = Constants.namePluginA;
      version = Constants.version010_alpha1;
      foundPackage = (await nuGetConnector.GetPackageAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), default))?.Identity.ToString();
      Assert.AreEqual(package + "." + version, foundPackage);

      package = Constants.namePluginB;
      version = Constants.version020;
      foundPackage = (await nuGetConnector.GetPackageAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), default))?.Identity.ToString();
      Assert.AreEqual(package + "." + version, foundPackage);

      package = Constants.namePluginB;
      version = Constants.version030_alpha1;
      foundPackage = (await nuGetConnector.GetPackageAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), default))?.Identity.ToString();
      Assert.AreEqual(package + "." + version, foundPackage);

      package = Constants.namePluginA;
      version = Constants.version000;
      foundPackage = (await nuGetConnector.GetPackageAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), default))?.Identity.ToString();
      Assert.IsNull(foundPackage);

      package = Constants.nameInvalid;
      version = Constants.version010;
      foundPackage = (await nuGetConnector.GetPackageAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), default))?.Identity.ToString();
      Assert.IsNull(foundPackage);

      package = Constants.nameInvalid;
      version = Constants.version000;
      foundPackage = (await nuGetConnector.GetPackageAsync(new PackageIdentity(package, NuGetVersion.Parse(version)), default))?.Identity.ToString();
      Assert.IsNull(foundPackage);

      WriteLogToTestContextAndClear(nuGetConnector);
    }
    #endregion

    #region TestGetPackagesAsync
    [TestMethod]
    public async Task TestGetPackagesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector();
      string package;
      string[] packages;
      string[] expectedPackagesA;
      string[] expectedPackagesB;
      string[] expectedPackages;
      string[] foundPackages;
      bool includePreReleases;
      ArgumentNullException argumentNullException;
      ArgumentException argumentException;

      #region single package as input
      #region include pre-releases
      package = Constants.namePluginA;
      includePreReleases = true;
      expectedPackages = new[] { Constants.version010_alpha1, Constants.version010_alpha2, Constants.version010,
                                 Constants.version020_alpha1, Constants.version020, Constants.version021,
                                 Constants.version030_alpha1, Constants.version030_beta1, Constants.version030 }.Select(x => package + "." + x).ToArray();
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      package = Constants.namePluginB;
      includePreReleases = true;
      expectedPackages = new[] { Constants.version010_alpha1, Constants.version010_alpha2, Constants.version010,
                                 Constants.version020_alpha1, Constants.version020,
                                 Constants.version030_alpha1, Constants.version030_alpha2, Constants.version030, Constants.version031 }.Select(x => package + "." + x).ToArray();
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      package = Constants.nameInvalid;
      includePreReleases = true;
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      Assert.AreEqual(0, foundPackages.Length, "Number of found packages is incorrect.");

      package = "";
      includePreReleases = true;
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      Assert.AreEqual(0, foundPackages.Length, "Number of found packages is incorrect.");

      package = null;
      includePreReleases = true;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return nuGetConnector.GetPackagesAsync(package, includePreReleases, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));
      #endregion

      #region exclude pre-releases
      package = Constants.namePluginA;
      includePreReleases = false;
      expectedPackages = new[] { Constants.version010, Constants.version020, Constants.version021, Constants.version030 }.Select(x => package + "." + x).ToArray();
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      package = Constants.namePluginB;
      includePreReleases = false;
      expectedPackages = new[] { Constants.version010, Constants.version020, Constants.version030, Constants.version031 }.Select(x => package + "." + x).ToArray();
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      package = Constants.nameInvalid;
      includePreReleases = false;
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      Assert.AreEqual(0, foundPackages.Length, "Number of found packages is incorrect.");

      package = "";
      includePreReleases = false;
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      Assert.AreEqual(0, foundPackages.Length, "Number of found packages is incorrect.");

      package = null;
      includePreReleases = false;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return nuGetConnector.GetPackagesAsync(package, includePreReleases, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));
      #endregion
      #endregion

      #region multiple packages as input
      packages = new[] { Constants.namePluginA, Constants.namePluginB };
      includePreReleases = true;
      expectedPackagesA = new[] { Constants.version010_alpha1, Constants.version010_alpha2, Constants.version010,
                                 Constants.version020_alpha1, Constants.version020, Constants.version021,
                                 Constants.version030_alpha1, Constants.version030_beta1, Constants.version030 }.Select(x => Constants.namePluginA + "." + x).ToArray();
      expectedPackagesB = new[] { Constants.version010_alpha1, Constants.version010_alpha2, Constants.version010,
                                 Constants.version020_alpha1, Constants.version020,
                                 Constants.version030_alpha1, Constants.version030_alpha2, Constants.version030, Constants.version031 }.Select(x => Constants.namePluginB + "." + x).ToArray();
      expectedPackages = expectedPackagesA.Concat(expectedPackagesB).ToArray();
      foundPackages = (await nuGetConnector.GetPackagesAsync(packages, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      packages = new[] { Constants.namePluginA, Constants.namePluginB };
      includePreReleases = false;
      expectedPackagesA = new[] { Constants.version010, Constants.version020, Constants.version021, Constants.version030 }.Select(x => Constants.namePluginA + "." + x).ToArray();
      expectedPackagesB = new[] { Constants.version010, Constants.version020, Constants.version030, Constants.version031 }.Select(x => Constants.namePluginB + "." + x).ToArray();
      expectedPackages = expectedPackagesA.Concat(expectedPackagesB).ToArray();
      foundPackages = (await nuGetConnector.GetPackagesAsync(packages, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      packages = new[] { Constants.namePluginA, Constants.namePluginB, Constants.namePluginA, Constants.namePluginB, Constants.namePluginA, Constants.namePluginB };
      includePreReleases = false;
      expectedPackagesA = new[] { Constants.version010, Constants.version020, Constants.version021, Constants.version030 }.Select(x => Constants.namePluginA + "." + x).ToArray();
      expectedPackagesB = new[] { Constants.version010, Constants.version020, Constants.version030, Constants.version031 }.Select(x => Constants.namePluginB + "." + x).ToArray();
      expectedPackages = expectedPackagesA.Concat(expectedPackagesB).ToArray();
      foundPackages = (await nuGetConnector.GetPackagesAsync(packages, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      packages = null;
      includePreReleases = false;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return nuGetConnector.GetPackagesAsync(packages, includePreReleases, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      packages = new[] { Constants.namePluginA, Constants.namePluginB, null };
      includePreReleases = false;
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return nuGetConnector.GetPackagesAsync(packages, includePreReleases, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));
      #endregion

      WriteLogToTestContextAndClear(nuGetConnector);
    }
    #endregion

    #region TestGetLocalPackagesAsync
    /*[TestMethod]
    public async Task TestGetLocalPackagesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector();
      string[] expectedPackages;
      string[] foundPackages;
      bool includePreReleases;

      includePreReleases = true;
      expectedPackages = new[] { Constants.nameVersionBricksPluginTypes,
                                 Constants.namePluginA + "." + Constants.version010_alpha2,
                                 Constants.namePluginA + "." + Constants.version010,
                                 Constants.namePluginA + "." + Constants.version020_alpha1,
                                 Constants.namePluginB + "." + Constants.version010_alpha2,
                                 Constants.namePluginB + "." + Constants.version010,
                                 Constants.namePluginB + "." + Constants.version020_alpha1 };
      foundPackages = (await nuGetConnector.GetLocalPackagesAsync(includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      includePreReleases = false;
      expectedPackages = new[] { Constants.namePluginA + "." + Constants.version010,
                                 Constants.namePluginB + "." + Constants.version010 };
      foundPackages = (await nuGetConnector.GetLocalPackagesAsync(includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      WriteLogToTestContextAndClear(nuGetConnector);
    }*/
    #endregion

    #region TestSearchRemotePackagesAsync
    /*[TestMethod]
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
      expectedPackages = new[] { "NuGet.Protocol.5.5.0-preview.2.6382" };
      foundPackages = (await nuGetConnector.SearchRemotePackagesAsync(searchString, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      searchString = "PackageId:HEAL.Attic version:1.0.0";
      includePreReleases = true;
      foundPackages = (await nuGetConnector.SearchRemotePackagesAsync(searchString, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      Assert.AreEqual(0, foundPackages.Length, "Number of found packages is incorrect.");

      WriteLogToTestContextAndClear(nuGetConnector);
    }*/
    #endregion

    #region TestGetPackageDependenciesAsync
    /*[TestMethod]
    public async Task TestGetPackageDependenciesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      string package;
      string version;
      bool resolveDependenciesRecursively;
      IEnumerable<SourcePackageDependencyInfo> foundDependencies;
      IEnumerable<NuGet.Packaging.Core.PackageDependency> packageDependencies;
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
      Assert.AreEqual(3, foundDependencies.Count(), "Number of found dependencies is incorrect.");
      // PluginB 0.3.1 -> PluginA 0.3.0
      Assert.AreEqual(package, foundDependencies.First().Id);
      Assert.AreEqual(version, foundDependencies.First().Version.ToString());
      packageDependencies = foundDependencies.First().Dependencies;
      Assert.AreEqual(1, packageDependencies.Count(), "Number of package dependencies is incorrect.");
      Assert.AreEqual(Constants.namePluginA, packageDependencies.First().Id);
      Assert.AreEqual(Constants.version030, packageDependencies.First().VersionRange.MinVersion.ToString());
      // PluginA 0.3.0 -> PluginTypes
      Assert.AreEqual(Constants.namePluginA, foundDependencies.Skip(1).First().Id);
      Assert.AreEqual(Constants.version030, foundDependencies.Skip(1).First().Version.ToString());
      packageDependencies = foundDependencies.Skip(1).First().Dependencies;
      Assert.AreEqual(1, packageDependencies.Count(), "Number of package dependencies is incorrect.");
      Assert.AreEqual(Constants.nameBricksPluginTypes, packageDependencies.First().Id);
      Assert.AreEqual(Constants.versionBricksPluginTypes, packageDependencies.First().VersionRange.MinVersion.ToString());
      // PluginTypes -> no dependencies
      Assert.AreEqual(Constants.nameBricksPluginTypes, foundDependencies.Skip(2).First().Id);
      Assert.AreEqual(Constants.versionBricksPluginTypes, foundDependencies.Skip(2).First().Version.ToString());
      packageDependencies = foundDependencies.Skip(2).First().Dependencies;
      Assert.AreEqual(0, packageDependencies.Count(), "Number of package dependencies is incorrect.");
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
    }*/
    #endregion

    #region TestGetPackageDownloaderAsync
    /*[TestMethod]
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
    }*/
    #endregion

    /*[TestMethod]
    [TestCategory("WIP")]
    public async Task TestResolveDependenciesOfLocalPackagesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      IEnumerable<PackageIdentity> localPackageIdentities = (await nuGetConnector.GetLocalPackagesAsync(true, default)).Where(x => x.Tags.Contains("HEALBricksPlugin")).Select(x => x.Identity);
      IEnumerable<SourcePackageDependencyInfo> dependencies = await nuGetConnector.GetPackageDependenciesAsync(localPackageIdentities, nuGetConnector.AllRepositories, true, default);
      IEnumerable<SourcePackageDependencyInfo> resolvedDependencies = nuGetConnector.ResolveDependencies(localPackageIdentities.Select(x => x.Id), dependencies, default, out bool resolveSucceeded);
      Assert.AreEqual(true, resolveSucceeded);

      TestContext.WriteLine($"Resolved Dependencies: {resolvedDependencies.Count()}");
      foreach (var resolvedPackage in resolvedDependencies) {
        TestContext.WriteLine(resolvedPackage.ToString());
      }
      TestContext.WriteLine("");

      WriteLogToTestContextAndClear(nuGetConnector);
      Assert.Fail("This unit test is incomplete and is still work in progress.");
    }*/

    /*[TestMethod]
    [TestCategory("WIP")]
    public async Task TestDownloadPackageAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      SourcePackageDependencyInfo package = (await nuGetConnector.GetPackageDependenciesAsync(new PackageIdentity("SimSharp", new NuGetVersion("3.3.0")), nuGetConnector.Repositories, false, default)).Single();
      DownloadResourceResult downloadResult = await nuGetConnector.DownloadPackageAsync(package, default);
      Assert.AreEqual(DownloadResourceResultStatus.Available, downloadResult.Status);

      WriteLogToTestContextAndClear(nuGetConnector);
      Assert.Fail("This unit test is incomplete and is still work in progress.");
    }*/

    /*[TestMethod]
    [TestCategory("WIP")]
    public async Task TestInstallPackageAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      SourcePackageDependencyInfo package = (await nuGetConnector.GetPackageDependenciesAsync(new PackageIdentity("SimSharp", new NuGetVersion("3.3.0")), nuGetConnector.Repositories, false, default)).Single();
      DownloadResourceResult downloadResult = await nuGetConnector.DownloadPackageAsync(package, default);
      await nuGetConnector.InstallPackageAsync(downloadResult, default);

      WriteLogToTestContextAndClear(nuGetConnector);
      Assert.Fail("This unit test is incomplete and is still work in progress.");
    }*/

    #region Helpers
    private NuGetConnector CreateNuGetConnector(bool includePublicNuGetRepository = false) {
      Settings settings = new Settings() { PackagesPath = Constants.localPackagesRelativePath, PluginTag = "HEALBricksPlugin" };
      settings.Repositories.Clear();
      settings.Repositories.Add(Path.Combine(settings.AppPath, Constants.remoteOfficialRepositoryRelativePath));
      settings.Repositories.Add(Path.Combine(settings.AppPath, Constants.remoteDevRepositoryRelativePath));
      if (includePublicNuGetRepository)
        settings.Repositories.Add(Constants.publicNuGetRepository);

      NuGetConnector nuGetConnector = new NuGetConnector(settings);
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
