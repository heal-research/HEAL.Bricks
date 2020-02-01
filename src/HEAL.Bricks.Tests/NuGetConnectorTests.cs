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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NuGetPackageDependency = NuGet.Packaging.Core.PackageDependency;

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

//    [TestInitialize]
    [ClassInitialize]
    public static void UnpackLocalPackages(TestContext testContext) {
      string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      string packagesPath = Path.Combine(appPath, Constants.localPackagesRelativePath);
      string packagesCachePath = Path.Combine(appPath, Constants.localPackagesCacheRelativePath);
      PackagePathResolver packagePathResolver = new PackagePathResolver(packagesPath);
      PackageExtractionContext packageExtractionContext = new PackageExtractionContext(PackageSaveMode.Defaultv3, XmlDocFileSaveMode.Skip, null, NullLogger.Instance);

      Directory.CreateDirectory(packagesPath);
      foreach (string package in Directory.GetDirectories(packagesPath)) {
        Directory.Delete(package, true);
      }
      Directory.CreateDirectory(packagesCachePath);
      foreach (string package in Directory.GetDirectories(packagesCachePath)) {
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
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      PackageIdentity package;
      string foundPackage;
      ArgumentNullException argumentNullException;
      ArgumentException argumentException;

      package = new PackageIdentity(Constants.namePluginA, new NuGetVersion(Constants.version010));
      foundPackage = (await nuGetConnector.GetPackageAsync(package, default))?.Identity.ToString();
      Assert.AreEqual(package.ToString(), foundPackage);

      package = new PackageIdentity(Constants.namePluginA, new NuGetVersion(Constants.version010_alpha1));
      foundPackage = (await nuGetConnector.GetPackageAsync(package, default))?.Identity.ToString();
      Assert.AreEqual(package.ToString(), foundPackage);

      package = new PackageIdentity(Constants.namePluginB, new NuGetVersion(Constants.version020));
      foundPackage = (await nuGetConnector.GetPackageAsync(package, default))?.Identity.ToString();
      Assert.AreEqual(package.ToString(), foundPackage);

      package = new PackageIdentity(Constants.namePluginB, new NuGetVersion(Constants.version030_alpha1));
      foundPackage = (await nuGetConnector.GetPackageAsync(package, default))?.Identity.ToString();
      Assert.AreEqual(package.ToString(), foundPackage);

      package = new PackageIdentity(Constants.nameCollections, new NuGetVersion(Constants.versionCollections));
      foundPackage = (await nuGetConnector.GetPackageAsync(package, default))?.Identity.ToString();
      Assert.AreEqual(package.ToString(), foundPackage);

      package = new PackageIdentity(Constants.namePluginA, new NuGetVersion(Constants.version000));
      foundPackage = (await nuGetConnector.GetPackageAsync(package, default))?.Identity.ToString();
      Assert.IsNull(foundPackage);

      package = new PackageIdentity(Constants.nameInvalid, new NuGetVersion(Constants.version010));
      foundPackage = (await nuGetConnector.GetPackageAsync(package, default))?.Identity.ToString();
      Assert.IsNull(foundPackage);

      package = new PackageIdentity(Constants.nameInvalid, new NuGetVersion(Constants.version000));
      foundPackage = (await nuGetConnector.GetPackageAsync(package, default))?.Identity.ToString();
      Assert.IsNull(foundPackage);

      package = null;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return nuGetConnector.GetPackageAsync(package, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      package = new PackageIdentity("", new NuGetVersion(Constants.version000));
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return nuGetConnector.GetPackageAsync(package, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      package = new PackageIdentity(Constants.namePluginA, null);
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return nuGetConnector.GetPackageAsync(package, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      WriteLogToTestContextAndClear(nuGetConnector);
    }
    #endregion

    #region TestGetPackagesAsync
    [TestMethod]
    public async Task TestGetPackagesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      string package;
      string[] packages;
      PackageIdentity[] packageIdentities;
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

      package = Constants.nameCollections;
      includePreReleases = true;
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      Assert.IsTrue(foundPackages.Length > 0);

      package = Constants.nameInvalid;
      includePreReleases = true;
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      Assert.AreEqual(0, foundPackages.Length, "Number of found packages is incorrect.");

      package = null;
      includePreReleases = true;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return nuGetConnector.GetPackagesAsync(package, includePreReleases, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      package = "";
      includePreReleases = true;
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return nuGetConnector.GetPackagesAsync(package, includePreReleases, default); });
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

      package = Constants.nameCollections;
      includePreReleases = false;
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      Assert.IsTrue(foundPackages.Length > 0);

      package = Constants.nameInvalid;
      includePreReleases = false;
      foundPackages = (await nuGetConnector.GetPackagesAsync(package, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      Assert.AreEqual(0, foundPackages.Length, "Number of found packages is incorrect.");

      package = null;
      includePreReleases = false;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return nuGetConnector.GetPackagesAsync(package, includePreReleases, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      package = "";
      includePreReleases = false;
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return nuGetConnector.GetPackagesAsync(package, includePreReleases, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));
      #endregion
      #endregion

      #region multiple packages as input
      #region get by package id string
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

      packages = new[] { Constants.namePluginA, Constants.namePluginB, "" };
      includePreReleases = false;
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return nuGetConnector.GetPackagesAsync(packages, includePreReleases, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));
      #endregion

      #region get by PackageIdentities
      packageIdentities = new[] { new PackageIdentity(Constants.namePluginA, new NuGetVersion(Constants.version010_alpha1)),
                                  new PackageIdentity(Constants.namePluginA, new NuGetVersion(Constants.version030)) };
      expectedPackages = new[] { Constants.version010_alpha1, Constants.version030 }.Select(x => Constants.namePluginA + "." + x).ToArray();
      foundPackages = (await nuGetConnector.GetPackagesAsync(packageIdentities, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      packageIdentities = new[] { new PackageIdentity(Constants.namePluginA, new NuGetVersion(Constants.version020)),
                                  new PackageIdentity(Constants.namePluginB, new NuGetVersion(Constants.version031)) };
      expectedPackages = new[] { Constants.namePluginA + "." + Constants.version020, Constants.namePluginB + "." + Constants.version031 };
      foundPackages = (await nuGetConnector.GetPackagesAsync(packageIdentities, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      packageIdentities = new[] { new PackageIdentity(Constants.namePluginA, new NuGetVersion(Constants.version020)),
                                  new PackageIdentity(Constants.namePluginB, new NuGetVersion(Constants.version031)),
                                  new PackageIdentity(Constants.namePluginA, new NuGetVersion(Constants.version020)),
                                  new PackageIdentity(Constants.namePluginB, new NuGetVersion(Constants.version031)) };
      expectedPackages = new[] { Constants.namePluginA + "." + Constants.version020, Constants.namePluginB + "." + Constants.version031 };
      foundPackages = (await nuGetConnector.GetPackagesAsync(packageIdentities, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      packageIdentities = null;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return nuGetConnector.GetPackagesAsync(packageIdentities, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      packageIdentities = new[] { new PackageIdentity(Constants.namePluginA, new NuGetVersion(Constants.version010_alpha1)),
                                  new PackageIdentity(Constants.namePluginA, new NuGetVersion(Constants.version030)),
                                  null };
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return nuGetConnector.GetPackagesAsync(packageIdentities, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      packageIdentities = new[] { new PackageIdentity(Constants.namePluginA, new NuGetVersion(Constants.version010_alpha1)),
                                  new PackageIdentity("", new NuGetVersion(Constants.version000)) };
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return nuGetConnector.GetPackagesAsync(packageIdentities, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      packageIdentities = new[] { new PackageIdentity(Constants.namePluginA, new NuGetVersion(Constants.version010_alpha1)),
                                  new PackageIdentity(Constants.namePluginA, null) };
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return nuGetConnector.GetPackagesAsync(packageIdentities, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));
      #endregion
      #endregion

      WriteLogToTestContextAndClear(nuGetConnector);
    }
    #endregion

    #region TestSearchPackagesAsync
    [TestMethod]
    public async Task TestSearchPackagesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      string searchString;
      string[] expectedPackages;
      string[] foundPackages;
      bool includePreReleases;
      ArgumentNullException argumentNullException;

      searchString = "PackageId:NuGet.Protocol";
      includePreReleases = false;
      expectedPackages = new[] { "NuGet.Protocol.5.4.0" };
      foundPackages = (await nuGetConnector.SearchPackagesAsync(searchString, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      searchString = "PackageId:NuGet.Protocol";
      includePreReleases = true;
      expectedPackages = new[] { "NuGet.Protocol.5.5.0-preview.2.6382" };
      foundPackages = (await nuGetConnector.SearchPackagesAsync(searchString, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      searchString = "PackageId:HEAL.Attic version:1.0.0";
      includePreReleases = false;
      foundPackages = (await nuGetConnector.SearchPackagesAsync(searchString, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      Assert.AreEqual(0, foundPackages.Length, "Number of found packages is incorrect.");

      searchString = "PackageId:HEAL.Attic version:1.0.0";
      includePreReleases = true;
      foundPackages = (await nuGetConnector.SearchPackagesAsync(searchString, includePreReleases, default)).Select(x => x.Identity.ToString()).ToArray();
      Assert.AreEqual(0, foundPackages.Length, "Number of found packages is incorrect.");

      searchString = null;
      includePreReleases = false;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return nuGetConnector.SearchPackagesAsync(searchString, includePreReleases, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      searchString = null;
      includePreReleases = true;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return nuGetConnector.SearchPackagesAsync(searchString, includePreReleases, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      WriteLogToTestContextAndClear(nuGetConnector);
    }
    #endregion

    #region TestGetPackageDependenciesAsync
    [TestMethod]
    public async Task TestGetPackageDependenciesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      PackageIdentity package;
      PackageIdentity[] packages;
      bool resolveDependenciesRecursively;
      PackageIdentity[] expectedPackages;
      NuGetPackageDependency[][] expectedDependencies;
      SourcePackageDependencyInfo[] foundPackages;
      NuGetPackageDependency[] foundDependencies;
      PackageIdentityComparer packageIdentityComparer = PackageIdentityComparer.Default;
      ArgumentNullException argumentNullException;
      ArgumentException argumentException;
      Stopwatch sw = new Stopwatch();

      #region single package as input
      #region PluginB 0.3.1, non-recursive
      package = new PackageIdentity(Constants.namePluginB, new NuGetVersion(Constants.version031));
      resolveDependenciesRecursively = false;
      // expected dependencies: PluginB 0.3.1 -> PluginA [0.3.0, )
      expectedPackages = new[] { package };
      expectedDependencies = new[] { new[] { new NuGetPackageDependency(Constants.namePluginA, new VersionRange(new NuGetVersion(Constants.version030))) } };
      sw.Restart();
      foundPackages = (await nuGetConnector.GetPackageDependenciesAsync(package, resolveDependenciesRecursively, default)).ToArray();
      sw.Stop();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);
      for (int i = 0; i < foundPackages.Length; i++) {
        foundDependencies = foundPackages[i].Dependencies.ToArray();
        CollectionAssert.AreEqual(expectedDependencies[i], foundDependencies, NuGetPackageDependencyComparer.Default);
      }
      TestContext.WriteLine($"{(resolveDependenciesRecursively ? "Recursive" : "Non-recursive")} dependency resolution of {package.ToString()}");
      TestContext.WriteLine("Duration: " + sw.ElapsedMilliseconds);
      WriteLogToTestContextAndClear(nuGetConnector);
      TestContext.WriteLine("");
      #endregion

      #region PluginB 0.3.1, recursive
      package = new PackageIdentity(Constants.namePluginB, new NuGetVersion(Constants.version031));
      resolveDependenciesRecursively = true;
      // expected dependencies: PluginB 0.3.1 -> PluginA [0.3.0, )
      //                        PluginA 0.3.0 -> PluginTypes
      //                        PluginTypes   -> no dependencies
      expectedPackages = new[] { package,
                                 new PackageIdentity(Constants.namePluginA, new NuGetVersion(Constants.version030)),
                                 new PackageIdentity(Constants.nameBricksPluginTypes, new NuGetVersion(Constants.versionBricksPluginTypes)) };
      expectedDependencies = new[] { new[] { new NuGetPackageDependency(Constants.namePluginA, new VersionRange(new NuGetVersion(Constants.version030))) },
                                     new[] { new NuGetPackageDependency(Constants.nameBricksPluginTypes, new VersionRange(new NuGetVersion(Constants.versionBricksPluginTypes))) },
                                     Array.Empty<NuGetPackageDependency>() };
      sw.Restart();
      foundPackages = (await nuGetConnector.GetPackageDependenciesAsync(package, resolveDependenciesRecursively, default)).ToArray();
      sw.Stop();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);
      for (int i = 0; i < foundPackages.Length; i++) {
        foundDependencies = foundPackages[i].Dependencies.ToArray();
        CollectionAssert.AreEqual(expectedDependencies[i], foundDependencies, NuGetPackageDependencyComparer.Default);
      }
      TestContext.WriteLine($"{(resolveDependenciesRecursively ? "Recursive" : "Non-recursive")} dependency resolution of {package.ToString()}");
      TestContext.WriteLine("Duration: " + sw.ElapsedMilliseconds);
      WriteLogToTestContextAndClear(nuGetConnector);
      TestContext.WriteLine("");
      #endregion

      #region System.Collections 4.3.0, non-recursive
      package = new PackageIdentity(Constants.nameCollections, new NuGetVersion(Constants.versionCollections));
      resolveDependenciesRecursively = false;
      // expected dependencies: System.Collections 4.3.0 -> Microsoft.NETCore.Targets [1.1.0, ), System.Runtime [4.3.0, ), Microsoft.NETCore.Platforms [1.1.0, )
      expectedPackages = new[] { package };
      expectedDependencies = new[] { new[] { new NuGetPackageDependency("Microsoft.NETCore.Targets", new VersionRange(new NuGetVersion("1.1.0"))),
                                             new NuGetPackageDependency("System.Runtime", new VersionRange(new NuGetVersion("4.3.0"))),
                                             new NuGetPackageDependency("Microsoft.NETCore.Platforms", new VersionRange(new NuGetVersion("1.1.0"))) } };
      sw.Restart();
      foundPackages = (await nuGetConnector.GetPackageDependenciesAsync(package, resolveDependenciesRecursively, default)).ToArray();
      sw.Stop();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);
      for (int i = 0; i < foundPackages.Length; i++) {
        foundDependencies = foundPackages[i].Dependencies.ToArray();
        CollectionAssert.AreEqual(expectedDependencies[i], foundDependencies, NuGetPackageDependencyComparer.Default);
      }
      TestContext.WriteLine($"{(resolveDependenciesRecursively ? "Recursive" : "Non-recursive")} dependency resolution of {package.ToString()}");
      TestContext.WriteLine("Duration: " + sw.ElapsedMilliseconds);
      WriteLogToTestContextAndClear(nuGetConnector);
      TestContext.WriteLine("");
      #endregion

      #region System.Collections 4.3.0, recursive
      package = new PackageIdentity(Constants.nameCollections, new NuGetVersion(Constants.versionCollections));
      resolveDependenciesRecursively = true;
      // expected dependencies: System.Collections 4.3.0          -> Microsoft.NETCore.Targets [1.1.0, ), System.Runtime [4.3.0, ), Microsoft.NETCore.Platforms [1.1.0, )
      //                        Microsoft.NETCore.Targets 1.1.0   -> no dependencies
      //                        System.Runtime 4.3.0              -> Microsoft.NETCore.Targets [1.1.0, ), Microsoft.NETCore.Platforms [1.1.0, )
      //                        Microsoft.NETCore.Platforms 1.1.0 -> no dependencies
      expectedPackages = new[] { package,
                                 new PackageIdentity("Microsoft.NETCore.Targets", new NuGetVersion("1.1.0")),
                                 new PackageIdentity("System.Runtime", new NuGetVersion("4.3.0")),
                                 new PackageIdentity("Microsoft.NETCore.Platforms", new NuGetVersion("1.1.0")) };
      expectedDependencies = new[] { new[] { new NuGetPackageDependency("Microsoft.NETCore.Targets", new VersionRange(new NuGetVersion("1.1.0"))),
                                             new NuGetPackageDependency("System.Runtime", new VersionRange(new NuGetVersion("4.3.0"))),
                                             new NuGetPackageDependency("Microsoft.NETCore.Platforms", new VersionRange(new NuGetVersion("1.1.0"))) },
                                     Array.Empty<NuGetPackageDependency>(),
                                     new[] { new NuGetPackageDependency("Microsoft.NETCore.Targets", new VersionRange(new NuGetVersion("1.1.0"))),
                                             new NuGetPackageDependency("Microsoft.NETCore.Platforms", new VersionRange(new NuGetVersion("1.1.0"))) },
                                     Array.Empty<NuGetPackageDependency>() };
      sw.Restart();
      foundPackages = (await nuGetConnector.GetPackageDependenciesAsync(package, resolveDependenciesRecursively, default)).ToArray();
      sw.Stop();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);
      for (int i = 0; i < foundPackages.Length; i++) {
        foundDependencies = foundPackages[i].Dependencies.ToArray();
        CollectionAssert.AreEqual(expectedDependencies[i], foundDependencies, NuGetPackageDependencyComparer.Default);
      }
      TestContext.WriteLine($"{(resolveDependenciesRecursively ? "Recursive" : "Non-recursive")} dependency resolution of {package.ToString()}");
      TestContext.WriteLine("Duration: " + sw.ElapsedMilliseconds);
      WriteLogToTestContextAndClear(nuGetConnector);
      TestContext.WriteLine("");
      #endregion

      #region InvalidPluginName 0.0.0, recursive
      package = new PackageIdentity(Constants.nameInvalid, new NuGetVersion(Constants.version000));
      resolveDependenciesRecursively = true;
      expectedPackages = Array.Empty<PackageIdentity>();
      sw.Restart();
      foundPackages = (await nuGetConnector.GetPackageDependenciesAsync(package, resolveDependenciesRecursively, default)).ToArray();
      sw.Stop();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);
      TestContext.WriteLine($"{(resolveDependenciesRecursively ? "Recursive" : "Non-recursive")} dependency resolution of {package.ToString()}");
      TestContext.WriteLine("Duration: " + sw.ElapsedMilliseconds);
      WriteLogToTestContextAndClear(nuGetConnector);
      TestContext.WriteLine("");
      #endregion

      package = null;
      resolveDependenciesRecursively = false;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return nuGetConnector.GetPackageDependenciesAsync(package, resolveDependenciesRecursively, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      package = new PackageIdentity("", new NuGetVersion(Constants.version000));
      resolveDependenciesRecursively = false;
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return nuGetConnector.GetPackageDependenciesAsync(package, resolveDependenciesRecursively, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      package = new PackageIdentity(Constants.namePluginA, null);
      resolveDependenciesRecursively = false;
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return nuGetConnector.GetPackageDependenciesAsync(package, resolveDependenciesRecursively, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));
      #endregion

      #region multiple packages as input
      #region PluginB 0.3.1 and PluginB 0.2.0, recursive
      packages = new[] { new PackageIdentity(Constants.namePluginB, new NuGetVersion(Constants.version031)),
                         new PackageIdentity(Constants.namePluginB, new NuGetVersion(Constants.version020)) };
      resolveDependenciesRecursively = true;
      // expected dependencies: PluginB 0.3.1 -> PluginA [0.3.0, )
      //                        PluginA 0.3.0 -> PluginTypes
      //                        PluginTypes   -> no dependencies
      //                        PluginB 0.2.0 -> PluginA [0.2.0, )
      //                        PluginA 0.2.0 -> PluginTypes
      expectedPackages = new[] { packages[0],
                                 new PackageIdentity(Constants.namePluginA, new NuGetVersion(Constants.version030)),
                                 new PackageIdentity(Constants.nameBricksPluginTypes, new NuGetVersion(Constants.versionBricksPluginTypes)),
                                 packages[1],
                                 new PackageIdentity(Constants.namePluginA, new NuGetVersion(Constants.version020)) };
      expectedDependencies = new[] { new[] { new NuGetPackageDependency(Constants.namePluginA, new VersionRange(new NuGetVersion(Constants.version030))) },
                                     new[] { new NuGetPackageDependency(Constants.nameBricksPluginTypes, new VersionRange(new NuGetVersion(Constants.versionBricksPluginTypes))) },
                                     Array.Empty<NuGetPackageDependency>(),
                                     new[] { new NuGetPackageDependency(Constants.namePluginA, new VersionRange(new NuGetVersion(Constants.version020))) },
                                     new[] { new NuGetPackageDependency(Constants.nameBricksPluginTypes, new VersionRange(new NuGetVersion(Constants.versionBricksPluginTypes))) } };
      sw.Restart();
      foundPackages = (await nuGetConnector.GetPackageDependenciesAsync(packages, resolveDependenciesRecursively, default)).ToArray();
      sw.Stop();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);
      for (int i = 0; i < foundPackages.Length; i++) {
        foundDependencies = foundPackages[i].Dependencies.ToArray();
        CollectionAssert.AreEqual(expectedDependencies[i], foundDependencies, NuGetPackageDependencyComparer.Default);
      }
      TestContext.WriteLine($"{(resolveDependenciesRecursively ? "Recursive" : "Non-recursive")} dependency resolution of {packages[0].ToString()} and {packages[1].ToString()}");
      TestContext.WriteLine("Duration: " + sw.ElapsedMilliseconds);
      WriteLogToTestContextAndClear(nuGetConnector);
      TestContext.WriteLine("");
      #endregion

      packages = null;
      resolveDependenciesRecursively = false;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return nuGetConnector.GetPackageDependenciesAsync(packages, resolveDependenciesRecursively, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      packages = new[] { new PackageIdentity(Constants.namePluginB, new NuGetVersion(Constants.version031)),
                         null };
      resolveDependenciesRecursively = false;
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return nuGetConnector.GetPackageDependenciesAsync(packages, resolveDependenciesRecursively, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      packages = new[] { new PackageIdentity(Constants.namePluginB, new NuGetVersion(Constants.version031)),
                         new PackageIdentity("", new NuGetVersion(Constants.version000)) };
      resolveDependenciesRecursively = false;
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return nuGetConnector.GetPackageDependenciesAsync(packages, resolveDependenciesRecursively, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      packages = new[] { new PackageIdentity(Constants.namePluginB, new NuGetVersion(Constants.version031)),
                         new PackageIdentity(Constants.namePluginA, null) };
      resolveDependenciesRecursively = false;
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return nuGetConnector.GetPackageDependenciesAsync(packages, resolveDependenciesRecursively, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));
      #endregion
    }
    #endregion

    #region TestResolveDependencies
    [TestMethod]
    public void TestResolveDependencies() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      SourcePackageDependencyInfo[] allAvailablePackages;
      SourcePackageDependencyInfo[] allAvailablePackagesPluginB;

      string[] requiredIds;
      SourcePackageDependencyInfo[] availablePackages;
      PackageIdentity[] expectedPackages;
      SourcePackageDependencyInfo[] resolvedPackages;
      bool resolveSucceeded;
      ArgumentNullException argumentNullException;
      ArgumentException argumentException;

      #region Create different sets of available packages
      PackageIdentity[] packages;
      packages = nuGetConnector.GetPackagesAsync(new[] { Constants.namePluginA, Constants.namePluginB }, true, default).Result.Select(x => x.Identity).ToArray();
      allAvailablePackages = nuGetConnector.GetPackageDependenciesAsync(packages, true, default).Result.ToArray();
      packages = nuGetConnector.GetPackagesAsync(new[] { Constants.namePluginB }, true, default).Result.Select(x => x.Identity).ToArray();
      allAvailablePackagesPluginB = nuGetConnector.GetPackageDependenciesAsync(packages, false, default).Result.ToArray();
      #endregion

      requiredIds = new[] { Constants.namePluginB };
      availablePackages = allAvailablePackages;
      expectedPackages = new[] { new PackageIdentity(Constants.nameBricksPluginTypes, new NuGetVersion(Constants.versionBricksPluginTypes)),
                                 new PackageIdentity(Constants.namePluginA, new NuGetVersion(Constants.version030)),
                                 new PackageIdentity(Constants.namePluginB, new NuGetVersion(Constants.version031)) };
      resolvedPackages = nuGetConnector.ResolveDependencies(requiredIds, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsTrue(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);

      requiredIds = new[] { Constants.namePluginB, Constants.namePluginA };
      availablePackages = allAvailablePackages;
      expectedPackages = new[] { new PackageIdentity(Constants.nameBricksPluginTypes, new NuGetVersion(Constants.versionBricksPluginTypes)),
                                 new PackageIdentity(Constants.namePluginA, new NuGetVersion(Constants.version030)),
                                 new PackageIdentity(Constants.namePluginB, new NuGetVersion(Constants.version031)) };
      resolvedPackages = nuGetConnector.ResolveDependencies(requiredIds, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsTrue(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);

      requiredIds = new[] { Constants.namePluginA };
      availablePackages = allAvailablePackages;
      expectedPackages = new[] { new PackageIdentity(Constants.nameBricksPluginTypes, new NuGetVersion(Constants.versionBricksPluginTypes)),
                                 new PackageIdentity(Constants.namePluginA, new NuGetVersion(Constants.version030)) };
      resolvedPackages = nuGetConnector.ResolveDependencies(requiredIds, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsTrue(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);

      requiredIds = new[] { Constants.namePluginB };
      availablePackages = allAvailablePackagesPluginB;
      expectedPackages = Array.Empty<PackageIdentity>();
      resolvedPackages = nuGetConnector.ResolveDependencies(requiredIds, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsFalse(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);

      requiredIds = new[] { Constants.namePluginA };
      availablePackages = allAvailablePackagesPluginB;
      expectedPackages = Array.Empty<PackageIdentity>();
      resolvedPackages = nuGetConnector.ResolveDependencies(requiredIds, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsFalse(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);

      requiredIds = null;
      availablePackages = allAvailablePackages;
      argumentNullException = Assert.ThrowsException<ArgumentNullException>(() => { return nuGetConnector.ResolveDependencies(requiredIds, availablePackages, default, out resolveSucceeded); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      requiredIds = new[] { Constants.namePluginA, null };
      availablePackages = allAvailablePackages;
      argumentException = Assert.ThrowsException<ArgumentException>(() => { return nuGetConnector.ResolveDependencies(requiredIds, availablePackages, default, out resolveSucceeded); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      requiredIds = new[] { Constants.namePluginA, "" };
      availablePackages = allAvailablePackages;
      argumentException = Assert.ThrowsException<ArgumentException>(() => { return nuGetConnector.ResolveDependencies(requiredIds, availablePackages, default, out resolveSucceeded); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      requiredIds = new[] { Constants.namePluginA };
      availablePackages = null;
      argumentNullException = Assert.ThrowsException<ArgumentNullException>(() => { return nuGetConnector.ResolveDependencies(requiredIds, availablePackages, default, out resolveSucceeded); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      requiredIds = new[] { Constants.namePluginA };
      availablePackages = allAvailablePackages.Append(null).ToArray();
      argumentException = Assert.ThrowsException<ArgumentException>(() => { return nuGetConnector.ResolveDependencies(requiredIds, availablePackages, default, out resolveSucceeded); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      WriteLogToTestContextAndClear(nuGetConnector);
    }
    #endregion

    #region TestGetInstalledPackages
    [TestMethod]
    public void TestGetInstalledPackages() {
      NuGetConnector nuGetConnector = CreateNuGetConnector();
      string[] expectedPackages;
      PackageFolderReader[] packageReaders;
      string[] foundPackages;

      expectedPackages = new[] { Constants.nameVersionBricksPluginTypes,
                                 Constants.namePluginB + "." + Constants.version010,
                                 Constants.namePluginB + "." + Constants.version020_alpha1 };
      packageReaders = nuGetConnector.GetInstalledPackages().ToArray();
      foundPackages = packageReaders.Select(x => x.GetIdentity().ToString()).ToArray();
      foreach (PackageFolderReader packageReader in packageReaders) packageReader.Dispose();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      WriteLogToTestContextAndClear(nuGetConnector);
    }
    #endregion

    #region TestGetPackageDownloaderAsync
    [TestMethod]
    public async Task TestGetPackageDownloaderAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector();
      string remoteOfficialRepository = nuGetConnector.Settings.Repositories.First();
      string remoteDevRepository = nuGetConnector.Settings.Repositories.Skip(1).First();

      PackageIdentity package;
      IPackageDownloader foundPackageDownloader;
      ArgumentNullException argumentNullException;
      ArgumentException argumentException;

      package = new PackageIdentity(Constants.namePluginA, new NuGetVersion(Constants.version010_alpha1));
      foundPackageDownloader = await nuGetConnector.GetPackageDownloaderAsync(package, default);
      Assert.IsNotNull(foundPackageDownloader);
      Assert.AreEqual(package, await foundPackageDownloader.CoreReader.GetIdentityAsync(default));
      Assert.AreEqual(remoteOfficialRepository, foundPackageDownloader.Source);

      package = new PackageIdentity(Constants.namePluginA, new NuGetVersion(Constants.version010));
      foundPackageDownloader = await nuGetConnector.GetPackageDownloaderAsync(package, default);
      Assert.IsNotNull(foundPackageDownloader);
      Assert.AreEqual(package, await foundPackageDownloader.CoreReader.GetIdentityAsync(default));
      Assert.AreEqual(remoteOfficialRepository, foundPackageDownloader.Source);

      package = new PackageIdentity(Constants.namePluginA, new NuGetVersion(Constants.version030_beta1));
      foundPackageDownloader = await nuGetConnector.GetPackageDownloaderAsync(package, default);
      Assert.IsNotNull(foundPackageDownloader);
      Assert.AreEqual(package, await foundPackageDownloader.CoreReader.GetIdentityAsync(default));
      Assert.AreEqual(remoteDevRepository, foundPackageDownloader.Source);

      package = new PackageIdentity(Constants.namePluginA, new NuGetVersion(Constants.version030));
      foundPackageDownloader = await nuGetConnector.GetPackageDownloaderAsync(package, default);
      Assert.IsNotNull(foundPackageDownloader);
      Assert.AreEqual(package, await foundPackageDownloader.CoreReader.GetIdentityAsync(default));
      Assert.AreEqual(remoteDevRepository, foundPackageDownloader.Source);

      package = new PackageIdentity(Constants.namePluginA, new NuGetVersion(Constants.version000));
      foundPackageDownloader = await nuGetConnector.GetPackageDownloaderAsync(package, default);
      Assert.IsNull(foundPackageDownloader);

      package = new PackageIdentity(Constants.nameInvalid, new NuGetVersion(Constants.version010));
      foundPackageDownloader = await nuGetConnector.GetPackageDownloaderAsync(package, default);
      Assert.IsNull(foundPackageDownloader);

      package = new PackageIdentity(Constants.nameInvalid, new NuGetVersion(Constants.version000));
      foundPackageDownloader = await nuGetConnector.GetPackageDownloaderAsync(package, default);
      Assert.IsNull(foundPackageDownloader);

      package = null;
      argumentNullException = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => { return nuGetConnector.GetPackageDownloaderAsync(package, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));

      package = new PackageIdentity("", new NuGetVersion(Constants.version000));
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return nuGetConnector.GetPackageDownloaderAsync(package, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      package = new PackageIdentity(Constants.namePluginA, null);
      argumentException = await Assert.ThrowsExceptionAsync<ArgumentException>(() => { return nuGetConnector.GetPackageDownloaderAsync(package, default); });
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));

      WriteLogToTestContextAndClear(nuGetConnector);
    }
    #endregion

    [TestMethod]
    [TestCategory("WIP")]
    public async Task TestDownloadPackageAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      SourcePackageDependencyInfo package = (await nuGetConnector.GetPackageDependenciesAsync(new PackageIdentity("SimSharp", new NuGetVersion("3.3.0")), false, default)).Single();
      DownloadResourceResult downloadResult = await nuGetConnector.DownloadPackageAsync(package, default);
      Assert.AreEqual(DownloadResourceResultStatus.Available, downloadResult.Status);

      WriteLogToTestContextAndClear(nuGetConnector);
      Assert.Fail("This unit test is incomplete and is still work in progress.");
    }

    [TestMethod]
    [TestCategory("WIP")]
    public async Task TestInstallPackageAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      SourcePackageDependencyInfo package = (await nuGetConnector.GetPackageDependenciesAsync(new PackageIdentity("SimSharp", new NuGetVersion("3.3.0")), false, default)).Single();
      DownloadResourceResult downloadResult = await nuGetConnector.DownloadPackageAsync(package, default);
      await nuGetConnector.InstallPackageAsync(downloadResult, default);

      WriteLogToTestContextAndClear(nuGetConnector);
      Assert.Fail("This unit test is incomplete and is still work in progress.");
    }

    #region Helpers
    private NuGetConnector CreateNuGetConnector(bool includePublicNuGetRepository = false) {
      Settings settings = new Settings() { PackagesPath = Constants.localPackagesRelativePath, PackagesCachePath = Constants.localPackagesCacheRelativePath, PluginTag = "HEALBricksPlugin" };
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

    #region NuGetPackageDependencyComparer
    private class NuGetPackageDependencyComparer : IComparer {
      public static NuGetPackageDependencyComparer Default => new NuGetPackageDependencyComparer();
      private readonly IVersionRangeComparer versionRangeComparer = VersionRangeComparer.Default;
      public int Compare(object x, object y) {
        if ((x is NuGetPackageDependency a) && (y is NuGetPackageDependency b))
          return (a.Id == b.Id) && versionRangeComparer.Equals(a.VersionRange, b.VersionRange) ? 0 : 1;
        else
          return x.Equals(y) ? 0 : 1;
      }
    }
    #endregion
  }
}
