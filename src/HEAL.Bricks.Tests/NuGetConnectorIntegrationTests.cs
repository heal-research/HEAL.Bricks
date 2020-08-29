#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGetPackageDependency = NuGet.Packaging.Core.PackageDependency;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Common;

namespace HEAL.Bricks.Tests {
  [TestClass]
  public class NuGetConnectorIntegrationTests : PackageTestsBase {
    #region TestCtor
    [TestMethod]
    public void TestCtor() {
      NuGetConnector nuGetConnector = new NuGetConnector(Enumerable.Empty<string>(), NuGetLogger.NoLogger);

      Assert.AreEqual(".NETCoreApp,Version=v1.0", nuGetConnector.CurrentFramework.DotNetFrameworkName);
    }
    [TestMethod]
    public void TestCreateForUnitTests() {
      string frameworkName = ".NETCoreApp,Version=v3.1";

      NuGetConnector nuGetConnector = NuGetConnector.CreateForTests(frameworkName, Enumerable.Empty<string>(), NuGetLogger.NoLogger);

      Assert.AreEqual(frameworkName, nuGetConnector.CurrentFramework.DotNetFrameworkName);
    }
    #endregion

    #region TestGetPackageAsync
    [TestMethod]
    public async Task TestGetPackageAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      PackageIdentity package;
      string foundPackage;

      package = CreatePackageIdentity(Constants.namePluginA, Constants.version010);
      foundPackage = (await nuGetConnector.GetPackageAsync(package, default))?.Identity.ToString();
      Assert.AreEqual(package.ToString(), foundPackage);

      package = CreatePackageIdentity(Constants.namePluginA, Constants.version010_alpha1);
      foundPackage = (await nuGetConnector.GetPackageAsync(package, default))?.Identity.ToString();
      Assert.AreEqual(package.ToString(), foundPackage);

      package = CreatePackageIdentity(Constants.namePluginB, Constants.version020);
      foundPackage = (await nuGetConnector.GetPackageAsync(package, default))?.Identity.ToString();
      Assert.AreEqual(package.ToString(), foundPackage);

      package = CreatePackageIdentity(Constants.namePluginB, Constants.version030_alpha1);
      foundPackage = (await nuGetConnector.GetPackageAsync(package, default))?.Identity.ToString();
      Assert.AreEqual(package.ToString(), foundPackage);

      package = CreatePackageIdentity(Constants.nameCollections, Constants.versionCollections);
      foundPackage = (await nuGetConnector.GetPackageAsync(package, default))?.Identity.ToString();
      Assert.AreEqual(package.ToString(), foundPackage);

      package = CreatePackageIdentity(Constants.namePluginA, Constants.version000);
      foundPackage = (await nuGetConnector.GetPackageAsync(package, default))?.Identity.ToString();
      Assert.IsNull(foundPackage);

      package = CreatePackageIdentity(Constants.nameInvalid, Constants.version010);
      foundPackage = (await nuGetConnector.GetPackageAsync(package, default))?.Identity.ToString();
      Assert.IsNull(foundPackage);

      package = CreatePackageIdentity(Constants.nameInvalid, Constants.version000);
      foundPackage = (await nuGetConnector.GetPackageAsync(package, default))?.Identity.ToString();
      Assert.IsNull(foundPackage);

      WriteLogToTestContextAndClear();
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
      #endregion

      #region get by PackageIdentities
      packageIdentities = new[] { CreatePackageIdentity(Constants.namePluginA, Constants.version010_alpha1),
                                  CreatePackageIdentity(Constants.namePluginA, Constants.version030) };
      expectedPackages = new[] { Constants.version010_alpha1, Constants.version030 }.Select(x => Constants.namePluginA + "." + x).ToArray();
      foundPackages = (await nuGetConnector.GetPackagesAsync(packageIdentities, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      packageIdentities = new[] { CreatePackageIdentity(Constants.namePluginA, Constants.version020),
                                  CreatePackageIdentity(Constants.namePluginB, Constants.version031) };
      expectedPackages = new[] { Constants.namePluginA + "." + Constants.version020, Constants.namePluginB + "." + Constants.version031 };
      foundPackages = (await nuGetConnector.GetPackagesAsync(packageIdentities, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      packageIdentities = new[] { CreatePackageIdentity(Constants.namePluginA, Constants.version020),
                                  CreatePackageIdentity(Constants.namePluginB, Constants.version031),
                                  CreatePackageIdentity(Constants.namePluginA, Constants.version020),
                                  CreatePackageIdentity(Constants.namePluginB, Constants.version031) };
      expectedPackages = new[] { Constants.namePluginA + "." + Constants.version020, Constants.namePluginB + "." + Constants.version031 };
      foundPackages = (await nuGetConnector.GetPackagesAsync(packageIdentities, default)).Select(x => x.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);
      #endregion
      #endregion

      WriteLogToTestContextAndClear();
    }
    #endregion

    #region TestSearchPackagesAsync
    [TestMethod]
    public async Task TestSearchPackagesAsync() {
      NuGetConnector nuGetConnector;
      string searchString;
      string[] expectedPackages;
      string[] foundPackages;
      bool includePreReleases;

      #region exclude public packages
      nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: false);

      searchString = Constants.namePluginA;
      includePreReleases = false;
      expectedPackages = new[] { Constants.namePluginA + "." + Constants.version021,
                                 Constants.namePluginA + "." + Constants.version030 };
      foundPackages = (await nuGetConnector.SearchPackagesAsync(searchString, includePreReleases, 0, int.MaxValue, default)).Select(x => x.Package.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      searchString = Constants.namePluginB;
      includePreReleases = false;
      expectedPackages = new[] { Constants.namePluginB + "." + Constants.version020,
                                 Constants.namePluginB + "." + Constants.version031 };
      foundPackages = (await nuGetConnector.SearchPackagesAsync(searchString, includePreReleases, 0, int.MaxValue, default)).Select(x => x.Package.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      searchString = Constants.nameInvalid;
      includePreReleases = false;
      foundPackages = (await nuGetConnector.SearchPackagesAsync(searchString, includePreReleases, 0, int.MaxValue, default)).Select(x => x.Package.Identity.ToString()).ToArray();
      Assert.AreEqual(0, foundPackages.Length, "Number of found packages is incorrect.");
      #endregion

      #region include public packages
      nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);

      searchString = "PackageId:NuGet.Protocol";
      includePreReleases = false;
      expectedPackages = new[] { "NuGet.Protocol.5.7.0" };
      foundPackages = (await nuGetConnector.SearchPackagesAsync(searchString, includePreReleases, 0, int.MaxValue, default)).Select(x => x.Package.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      searchString = "PackageId:NuGet.Protocol";
      includePreReleases = true;
      expectedPackages = new[] { "NuGet.Protocol.5.8.0-preview.2.6776" };
      foundPackages = (await nuGetConnector.SearchPackagesAsync(searchString, includePreReleases, 0, int.MaxValue, default)).Select(x => x.Package.Identity.ToString()).ToArray();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      searchString = "PackageId:HEAL.Attic version:1.0.0";
      includePreReleases = false;
      foundPackages = (await nuGetConnector.SearchPackagesAsync(searchString, includePreReleases, 0, int.MaxValue, default)).Select(x => x.Package.Identity.ToString()).ToArray();
      Assert.AreEqual(0, foundPackages.Length, "Number of found packages is incorrect.");

      searchString = "PackageId:HEAL.Attic version:1.0.0";
      includePreReleases = true;
      foundPackages = (await nuGetConnector.SearchPackagesAsync(searchString, includePreReleases, 0, int.MaxValue, default)).Select(x => x.Package.Identity.ToString()).ToArray();
      Assert.AreEqual(0, foundPackages.Length, "Number of found packages is incorrect.");

      searchString = "";
      includePreReleases = true;
      foundPackages = (await nuGetConnector.SearchPackagesAsync(searchString, includePreReleases, 0, 10, default)).Where(x => x.Repository == Constants.publicNuGetRepository).Select(x => x.Package.Identity.ToString()).ToArray();
      Assert.AreEqual(10, foundPackages.Length, "Number of found packages is incorrect.");

      searchString = "";
      includePreReleases = true;
      foundPackages = (await nuGetConnector.SearchPackagesAsync(searchString, includePreReleases, 10, 10, default)).Where(x => x.Repository == Constants.publicNuGetRepository).Select(x => x.Package.Identity.ToString()).ToArray();
      Assert.AreEqual(10, foundPackages.Length, "Number of found packages is incorrect.");
      #endregion

      WriteLogToTestContextAndClear();
    }
    #endregion

    #region TestGetPackageDependenciesAsync
    [TestMethod]
    public async Task TestGetPackageDependenciesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      PackageIdentity package;
      PackageIdentity[] packages;
      NuGetPackageDependency dependency;
      NuGetPackageDependency[] dependencies;
      bool getDependenciesRecursively;
      PackageIdentity[] expectedPackages;
      NuGetPackageDependency[][] expectedDependencies;
      SourcePackageDependencyInfo[] foundPackages;
      NuGetPackageDependency[] foundDependencies;
      PackageIdentityComparer packageIdentityComparer = PackageIdentityComparer.Default;
      Stopwatch sw = new Stopwatch();

      #region single package as input
      #region PluginB 0.3.1, non-recursive
      package = CreatePackageIdentity(Constants.namePluginB, Constants.version031);
      getDependenciesRecursively = false;
      // expected dependencies: PluginB 0.3.1 -> PluginA [0.3.0, )
      expectedPackages = new[] { package };
      expectedDependencies = new[] { new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version030) } };
      sw.Restart();
      foundPackages = (await nuGetConnector.GetPackageDependenciesAsync(package, getDependenciesRecursively, default)).ToArray();
      sw.Stop();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);
      for (int i = 0; i < foundPackages.Length; i++) {
        foundDependencies = foundPackages[i].Dependencies.ToArray();
        CollectionAssert.AreEqual(expectedDependencies[i], foundDependencies, NuGetPackageDependencyComparer.Default);
      }
      TestContext.WriteLine($"{(getDependenciesRecursively ? "Recursive" : "Non-recursive")} dependency resolution of {package}");
      TestContext.WriteLine("Duration: " + sw.ElapsedMilliseconds);
      WriteLogToTestContextAndClear();
      TestContext.WriteLine("");
      #endregion

      #region PluginB 0.3.1, recursive
      package = CreatePackageIdentity(Constants.namePluginB, Constants.version031);
      getDependenciesRecursively = true;
      // expected dependencies: PluginB 0.3.1 -> PluginA [0.3.0, )
      //                        PluginA 0.3.0 -> PluginTypes
      //                        PluginTypes   -> no dependencies
      expectedPackages = new[] { package,
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030),
                                 CreatePackageIdentity(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes) };
      expectedDependencies = new[] { new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version030) },
                                     new[] { CreateNuGetPackageDependency(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes) },
                                     Array.Empty<NuGetPackageDependency>() };
      sw.Restart();
      foundPackages = (await nuGetConnector.GetPackageDependenciesAsync(package, getDependenciesRecursively, default)).ToArray();
      sw.Stop();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);
      for (int i = 0; i < foundPackages.Length; i++) {
        foundDependencies = foundPackages[i].Dependencies.ToArray();
        CollectionAssert.AreEqual(expectedDependencies[i], foundDependencies, NuGetPackageDependencyComparer.Default);
      }
      TestContext.WriteLine($"{(getDependenciesRecursively ? "Recursive" : "Non-recursive")} dependency resolution of {package}");
      TestContext.WriteLine("Duration: " + sw.ElapsedMilliseconds);
      WriteLogToTestContextAndClear();
      TestContext.WriteLine("");
      #endregion

      #region System.Collections 4.3.0, non-recursive
      package = CreatePackageIdentity(Constants.nameCollections, Constants.versionCollections);
      getDependenciesRecursively = false;
      // expected dependencies: System.Collections 4.3.0 -> Microsoft.NETCore.Targets [1.1.0, ), System.Runtime [4.3.0, ), Microsoft.NETCore.Platforms [1.1.0, )
      expectedPackages = new[] { package };
      expectedDependencies = new[] { new[] { CreateNuGetPackageDependency("Microsoft.NETCore.Platforms", "1.1.0"),
                                             CreateNuGetPackageDependency("Microsoft.NETCore.Targets", "1.1.0"),
                                             CreateNuGetPackageDependency("System.Runtime", "4.3.0") } };
      sw.Restart();
      foundPackages = (await nuGetConnector.GetPackageDependenciesAsync(package, getDependenciesRecursively, default)).ToArray();
      sw.Stop();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);
      for (int i = 0; i < foundPackages.Length; i++) {
        foundDependencies = foundPackages[i].Dependencies.ToArray();
        CollectionAssert.AreEqual(expectedDependencies[i], foundDependencies, NuGetPackageDependencyComparer.Default);
      }
      TestContext.WriteLine($"{(getDependenciesRecursively ? "Recursive" : "Non-recursive")} dependency resolution of {package}");
      TestContext.WriteLine("Duration: " + sw.ElapsedMilliseconds);
      WriteLogToTestContextAndClear();
      TestContext.WriteLine("");
      #endregion

      #region System.Collections 4.3.0, recursive
      package = CreatePackageIdentity(Constants.nameCollections, Constants.versionCollections);
      getDependenciesRecursively = true;
      // dependencies: System.Collections 4.3.0          -> Microsoft.NETCore.Targets [1.1.0, ), System.Runtime [4.3.0, ), Microsoft.NETCore.Platforms [1.1.0, )
      //               Microsoft.NETCore.Targets 1.1.0   -> no dependencies
      //               System.Runtime 4.3.0              -> Microsoft.NETCore.Targets [1.1.0, ), Microsoft.NETCore.Platforms [1.1.0, )
      //               Microsoft.NETCore.Platforms 1.1.0 -> no dependencies
      sw.Restart();
      foundPackages = (await nuGetConnector.GetPackageDependenciesAsync(package, getDependenciesRecursively, default)).ToArray();
      sw.Stop();
      TestContext.WriteLine($"{(getDependenciesRecursively ? "Recursive" : "Non-recursive")} dependency resolution of {package}");
      TestContext.WriteLine("Duration: " + sw.ElapsedMilliseconds);
      WriteLogToTestContextAndClear();
      TestContext.WriteLine("");
      #endregion

      package = CreatePackageIdentity(Constants.nameInvalid, Constants.version000);
      getDependenciesRecursively = true;
      await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => { return nuGetConnector.GetPackageDependenciesAsync(package, getDependenciesRecursively, default); });
      #endregion

      #region multiple packages as input
      #region PluginB 0.3.1 and PluginB 0.2.0, recursive
      packages = new[] { CreatePackageIdentity(Constants.namePluginB, Constants.version031),
                         CreatePackageIdentity(Constants.namePluginB, Constants.version020) };
      getDependenciesRecursively = true;
      // expected dependencies: PluginB 0.3.1         -> PluginA [0.3.0, )
      //                        PluginA 0.3.0         -> PluginTypes
      //                        PluginTypes           -> no dependencies
      //                        PluginB 0.2.0         -> PluginA [0.2.0, )
      //                        PluginA 0.2.0         -> PluginTypes
      //                        PluginA 0.2.1         -> PluginTypes
      //                        PluginA 0.3.0-alpha.1 -> PluginTypes
      //                        PluginA 0.3.0-beta.1  -> PluginTypes
      expectedPackages = new[] { packages[0],
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030),
                                 CreatePackageIdentity(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes),
                                 packages[1],
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version020),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version021),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030_alpha1),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030_beta1) };
      expectedDependencies = new[] { new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version030) },
                                     new[] { CreateNuGetPackageDependency(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes) },
                                     Array.Empty<NuGetPackageDependency>(),
                                     new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version020) },
                                     new[] { CreateNuGetPackageDependency(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes) },
                                     new[] { CreateNuGetPackageDependency(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes) },
                                     new[] { CreateNuGetPackageDependency(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes) },
                                     new[] { CreateNuGetPackageDependency(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes) } };
      sw.Restart();
      foundPackages = (await nuGetConnector.GetPackageDependenciesAsync(packages, getDependenciesRecursively, default)).ToArray();
      sw.Stop();

      TestContext.WriteLine("EXPECTED");
      foreach (var pkg in expectedPackages) TestContext.WriteLine(pkg.ToString());
      TestContext.WriteLine("FOUND");
      foreach (var pkg in foundPackages) TestContext.WriteLine(pkg.ToString());

      CollectionAssert.AreEqual(expectedPackages.OrderBy(x => x.Id).ThenBy(x => x.Version).ToArray(), foundPackages.OrderBy(x => x.Id).ThenBy(x => x.Version).ToArray());
      for (int i = 0; i < foundPackages.Length; i++) {
        foundDependencies = foundPackages[i].Dependencies.ToArray();
        CollectionAssert.AreEqual(expectedDependencies[i], foundDependencies, NuGetPackageDependencyComparer.Default);
      }
      TestContext.WriteLine($"{(getDependenciesRecursively ? "Recursive" : "Non-recursive")} dependency resolution of {packages[0]} and {packages[1]}");
      TestContext.WriteLine("Duration: " + sw.ElapsedMilliseconds);
      WriteLogToTestContextAndClear();
      TestContext.WriteLine("");
      #endregion
      #endregion

      #region single dependency as input
      #region PluginB [0.3.0-alpha.2, ), non-recursive
      dependency = CreateNuGetPackageDependency(Constants.namePluginB, Constants.version030_alpha2);
      getDependenciesRecursively = false;
      // expected dependencies: PluginB 0.3.0-alpha.2 -> PluginA [0.3.0-beta.1, )
      //                        PluginB 0.3.0         -> PluginA [0.3.0, )
      //                        PluginB 0.3.1         -> PluginA [0.3.0, )
      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginB, Constants.version030_alpha2),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version030),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version031) };
      expectedDependencies = new[] { new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version030_beta1) },
                                     new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version030) },
                                     new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version030) } };
      sw.Restart();
      foundPackages = (await nuGetConnector.GetPackageDependenciesAsync(dependency, getDependenciesRecursively, default)).ToArray();
      sw.Stop();
      CollectionAssert.AreEqual(expectedPackages.OrderBy(x => x.Id).ThenBy(x => x.Version).ToArray(), foundPackages.OrderBy(x => x.Id).ThenBy(x => x.Version).ToArray());
      for (int i = 0; i < foundPackages.Length; i++) {
        foundDependencies = foundPackages[i].Dependencies.ToArray();
        CollectionAssert.AreEqual(expectedDependencies[i], foundDependencies, NuGetPackageDependencyComparer.Default);
      }
      TestContext.WriteLine($"{(getDependenciesRecursively ? "Recursive" : "Non-recursive")} dependency resolution of {dependency}");
      TestContext.WriteLine("Duration: " + sw.ElapsedMilliseconds);
      WriteLogToTestContextAndClear();
      TestContext.WriteLine("");
      #endregion

      #region PluginB (all versions), non-recursive
      dependency = CreateNuGetPackageDependency(Constants.namePluginB, null);
      getDependenciesRecursively = false;
      // expected dependencies: PluginB 0.1.0-alpha.1 -> PluginA [0.1.0, )
      //                        PluginB 0.1.0-alpha.2 -> PluginA [0.1.0, )
      //                        PluginB 0.1.0         -> PluginA [0.1.0, )
      //                        PluginB 0.2.0-alpha.1 -> PluginA [0.1.0, )
      //                        PluginB 0.2.0         -> PluginA [0.2.0, )
      //                        PluginB 0.3.0-alpha.1 -> PluginA [0.2.1, )
      //                        PluginB 0.3.0-alpha.2 -> PluginA [0.3.0-beta.1, )
      //                        PluginB 0.3.0         -> PluginA [0.3.0, )
      //                        PluginB 0.3.1         -> PluginA [0.3.0, )
      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginB, Constants.version010_alpha1),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version010_alpha2),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version010),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version020_alpha1),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version020),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version030_alpha1),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version030_alpha2),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version030),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version031) };
      expectedDependencies = new[] { new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version010) },
                                     new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version010) },
                                     new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version010) },
                                     new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version010) },
                                     new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version020) },
                                     new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version021) },
                                     new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version030_beta1) },
                                     new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version030) },
                                     new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version030) } };
      sw.Restart();
      foundPackages = (await nuGetConnector.GetPackageDependenciesAsync(dependency, getDependenciesRecursively, default)).ToArray();
      sw.Stop();
      CollectionAssert.AreEqual(expectedPackages.OrderBy(x => x.Id).ThenBy(x => x.Version).ToArray(), foundPackages.OrderBy(x => x.Id).ThenBy(x => x.Version).ToArray());
      for (int i = 0; i < foundPackages.Length; i++) {
        foundDependencies = foundPackages[i].Dependencies.ToArray();
        CollectionAssert.AreEqual(expectedDependencies[i], foundDependencies, NuGetPackageDependencyComparer.Default);
      }
      TestContext.WriteLine($"{(getDependenciesRecursively ? "Recursive" : "Non-recursive")} dependency resolution of {dependency}");
      TestContext.WriteLine("Duration: " + sw.ElapsedMilliseconds);
      WriteLogToTestContextAndClear();
      TestContext.WriteLine("");
      #endregion

      #region PluginB 0.2.0, recursive
      dependency = CreateNuGetPackageDependency(Constants.namePluginB, Constants.version020);
      getDependenciesRecursively = true;
      // expected dependencies: PluginB 0.2.0         -> PluginA [0.2.0, )
      //                        PluginA 0.2.0         -> PluginTypes
      //                        PluginTypes           -> no dependencies
      //                        PluginA 0.2.1         -> PluginTypes
      //                        PluginA 0.3.0-alpha.1 -> PluginTypes
      //                        PluginA 0.3.0-beta.1  -> PluginTypes
      //                        PluginA 0.3.0         -> PluginTypes
      //                        PluginB 0.3.0-alpha.1 -> PluginA [0.2.1, )
      //                        PluginB 0.3.0-alpha.2 -> PluginA [0.3.0-beta.1, )
      //                        PluginB 0.3.0         -> PluginA [0.3.0, )
      //                        PluginB 0.3.1         -> PluginA [0.3.0, )
      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginB, Constants.version020),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version020),
                                 CreatePackageIdentity(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version021),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030_alpha1),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030_beta1),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version030_alpha1),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version030_alpha2),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version030),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version031) };
      expectedDependencies = new[] { new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version020) },
                                     new[] { CreateNuGetPackageDependency(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes) },
                                     Array.Empty<NuGetPackageDependency>(),
                                     new[] { CreateNuGetPackageDependency(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes) },
                                     new[] { CreateNuGetPackageDependency(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes) },
                                     new[] { CreateNuGetPackageDependency(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes) },
                                     new[] { CreateNuGetPackageDependency(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes) },
                                     new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version021) },
                                     new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version030_beta1) },
                                     new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version030) },
                                     new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version030) } };
      sw.Restart();
      foundPackages = (await nuGetConnector.GetPackageDependenciesAsync(dependency, getDependenciesRecursively, default)).ToArray();
      sw.Stop();
      CollectionAssert.AreEqual(expectedPackages.OrderBy(x => x.Id).ThenBy(x => x.Version).ToArray(), foundPackages.OrderBy(x => x.Id).ThenBy(x => x.Version).ToArray());
      for (int i = 0; i < foundPackages.Length; i++) {
        foundDependencies = foundPackages[i].Dependencies.ToArray();
        CollectionAssert.AreEqual(expectedDependencies[i], foundDependencies, NuGetPackageDependencyComparer.Default);
      }
      TestContext.WriteLine($"{(getDependenciesRecursively ? "Recursive" : "Non-recursive")} dependency resolution of {dependency}");
      TestContext.WriteLine("Duration: " + sw.ElapsedMilliseconds);
      WriteLogToTestContextAndClear();
      TestContext.WriteLine("");
      #endregion

      dependency = CreateNuGetPackageDependency(Constants.nameInvalid, Constants.version000);
      getDependenciesRecursively = true;
      await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => { return nuGetConnector.GetPackageDependenciesAsync(dependency, getDependenciesRecursively, default); });
      #endregion

      #region multiple dependencies as input
      #region PluginA [0.3.0-alpha.1, ) and PluginB [0.3.0, ), non-recursive
      dependencies = new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version030_alpha1),
                             CreateNuGetPackageDependency(Constants.namePluginB, Constants.version030) };
      getDependenciesRecursively = false;
      // expected dependencies: PluginA 0.3.0-alpha.1 -> PluginTypes
      //                        PluginA 0.3.0-beta.1  -> PluginTypes
      //                        PluginA 0.3.0         -> PluginTypes
      //                        PluginB 0.3.0         -> PluginA [0.3.0, )
      //                        PluginB 0.3.1         -> PluginA [0.3.0, )
      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginA, Constants.version030_alpha1),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030_beta1),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version030),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version031) };
      expectedDependencies = new[] { new[] { CreateNuGetPackageDependency(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes) },
                                     new[] { CreateNuGetPackageDependency(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes) },
                                     new[] { CreateNuGetPackageDependency(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes) },
                                     new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version030) },
                                     new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version030) } };
      sw.Restart();
      foundPackages = (await nuGetConnector.GetPackageDependenciesAsync(dependencies, getDependenciesRecursively, default)).ToArray();
      sw.Stop();
      CollectionAssert.AreEqual(expectedPackages.OrderBy(x => x.Id).ThenBy(x => x.Version).ToArray(), foundPackages.OrderBy(x => x.Id).ThenBy(x => x.Version).ToArray());
      for (int i = 0; i < foundPackages.Length; i++) {
        foundDependencies = foundPackages[i].Dependencies.ToArray();
        CollectionAssert.AreEqual(expectedDependencies[i], foundDependencies, NuGetPackageDependencyComparer.Default);
      }
      TestContext.WriteLine($"{(getDependenciesRecursively ? "Recursive" : "Non-recursive")} dependency resolution of {dependencies[0]} and {dependencies[1]}");
      TestContext.WriteLine("Duration: " + sw.ElapsedMilliseconds);
      WriteLogToTestContextAndClear();
      TestContext.WriteLine("");
      #endregion

      #region PluginA [0.3.0-alpha.1, ) and PluginB [0.3.0.alpha.1, ), recursive
      dependencies = new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version030_alpha1),
                             CreateNuGetPackageDependency(Constants.namePluginB, Constants.version030_alpha1) };
      getDependenciesRecursively = true;
      // expected dependencies: PluginA 0.3.0-alpha.1 -> PluginTypes
      //                        PluginTypes           -> no dependencies
      //                        PluginA 0.3.0-beta.1  -> PluginTypes
      //                        PluginA 0.3.0         -> PluginTypes
      //                        PluginB 0.3.0-alpha.1 -> PluginA [0.2.1, )
      //                        PluginA 0.2.1         -> PluginTypes
      //                        PluginB 0.3.0-alpha.2 -> PluginA [0.3.0-beta.1, )
      //                        PluginB 0.3.0         -> PluginA [0.3.0, )
      //                        PluginB 0.3.1         -> PluginA [0.3.0, )
      expectedPackages = new[] { CreatePackageIdentity(Constants.namePluginA, Constants.version030_alpha1),
                                 CreatePackageIdentity(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030_beta1),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version030_alpha1),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version021),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version030_alpha2),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version030),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version031) };
      expectedDependencies = new[] { new[] { CreateNuGetPackageDependency(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes) },
                                     Array.Empty<NuGetPackageDependency>(),
                                     new[] { CreateNuGetPackageDependency(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes) },
                                     new[] { CreateNuGetPackageDependency(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes) },
                                     new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version021) },
                                     new[] { CreateNuGetPackageDependency(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes) },
                                     new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version030_beta1) },
                                     new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version030) },
                                     new[] { CreateNuGetPackageDependency(Constants.namePluginA, Constants.version030) } };
      sw.Restart();
      foundPackages = (await nuGetConnector.GetPackageDependenciesAsync(dependencies, getDependenciesRecursively, default)).ToArray();
      sw.Stop();
      CollectionAssert.AreEqual(expectedPackages.OrderBy(x => x.Id).ThenBy(x => x.Version).ToArray(), foundPackages.OrderBy(x => x.Id).ThenBy(x => x.Version).ToArray());
      for (int i = 0; i < foundPackages.Length; i++) {
        foundDependencies = foundPackages[i].Dependencies.ToArray();
        CollectionAssert.AreEqual(expectedDependencies[i], foundDependencies, NuGetPackageDependencyComparer.Default);
      }
      TestContext.WriteLine($"{(getDependenciesRecursively ? "Recursive" : "Non-recursive")} dependency resolution of {dependencies[0]} and {dependencies[1]}");
      TestContext.WriteLine("Duration: " + sw.ElapsedMilliseconds);
      WriteLogToTestContextAndClear();
      TestContext.WriteLine("");
      #endregion
      #endregion
    }
    #endregion

    #region TestResolveDependencies
    [TestMethod]
    public void TestResolveDependencies() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      SourcePackageDependencyInfo[] allAvailablePackages;
      SourcePackageDependencyInfo[] allAvailablePackagesPluginB;

      string[] additionalPackages;
      PackageIdentity[] existingPackages;
      SourcePackageDependencyInfo[] availablePackages;
      PackageIdentity[] expectedPackages;
      SourcePackageDependencyInfo[] resolvedPackages;
      bool resolveSucceeded;

      #region Create different sets of available packages
      NuGetPackageDependency pluginA = CreateNuGetPackageDependency(Constants.namePluginA, null);
      NuGetPackageDependency pluginB = CreateNuGetPackageDependency(Constants.namePluginB, null);
      allAvailablePackages = nuGetConnector.GetPackageDependenciesAsync(new[] { pluginA, pluginB }, true, default).Result.ToArray();
      allAvailablePackagesPluginB = nuGetConnector.GetPackageDependenciesAsync(pluginB, false, default).Result.ToArray();
      #endregion

      #region input additional packages
      additionalPackages = Array.Empty<string>();
      existingPackages = null;
      availablePackages = allAvailablePackages;
      expectedPackages = Array.Empty<PackageIdentity>();
      resolvedPackages = nuGetConnector.ResolveDependencies(additionalPackages, existingPackages, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsTrue(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);

      additionalPackages = new[] { Constants.namePluginA };
      existingPackages = null;
      availablePackages = allAvailablePackages;
      expectedPackages = new[] { CreatePackageIdentity(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030) };
      resolvedPackages = nuGetConnector.ResolveDependencies(additionalPackages, existingPackages, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsTrue(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);

      additionalPackages = new[] { Constants.namePluginA, Constants.namePluginA };
      existingPackages = null;
      availablePackages = allAvailablePackages;
      expectedPackages = new[] { CreatePackageIdentity(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030) };
      resolvedPackages = nuGetConnector.ResolveDependencies(additionalPackages, existingPackages, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsTrue(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);

      additionalPackages = new[] { Constants.namePluginB };
      existingPackages = null;
      availablePackages = allAvailablePackages;
      expectedPackages = new[] { CreatePackageIdentity(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version031) };
      resolvedPackages = nuGetConnector.ResolveDependencies(additionalPackages, existingPackages, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsTrue(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);

      additionalPackages = new[] { Constants.namePluginA, Constants.namePluginB };
      existingPackages = null;
      availablePackages = allAvailablePackages;
      expectedPackages = new[] { CreatePackageIdentity(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version031) };
      resolvedPackages = nuGetConnector.ResolveDependencies(additionalPackages, existingPackages, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsTrue(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);
      #endregion

      #region input existing packages
      additionalPackages = null;
      existingPackages = Array.Empty<PackageIdentity>();
      availablePackages = allAvailablePackages;
      expectedPackages = Array.Empty<PackageIdentity>();
      resolvedPackages = nuGetConnector.ResolveDependencies(additionalPackages, existingPackages, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsTrue(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);

      additionalPackages = null;
      existingPackages = new[] { CreatePackageIdentity(Constants.namePluginA, Constants.version010) };
      availablePackages = allAvailablePackages;
      expectedPackages = new[] { CreatePackageIdentity(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version010) };
      resolvedPackages = nuGetConnector.ResolveDependencies(additionalPackages, existingPackages, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsTrue(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);

      additionalPackages = null;
      existingPackages = new[] { CreatePackageIdentity(Constants.namePluginA, Constants.version010_alpha1) };
      availablePackages = allAvailablePackages;
      expectedPackages = new[] { CreatePackageIdentity(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version010_alpha1) };
      resolvedPackages = nuGetConnector.ResolveDependencies(additionalPackages, existingPackages, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsTrue(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);

      additionalPackages = null;
      existingPackages = new[] { CreatePackageIdentity(Constants.namePluginB, Constants.version010) };
      availablePackages = allAvailablePackages;
      expectedPackages = new[] { CreatePackageIdentity(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version010) };
      resolvedPackages = nuGetConnector.ResolveDependencies(additionalPackages, existingPackages, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsTrue(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);

      additionalPackages = null;
      existingPackages = new[] { CreatePackageIdentity(Constants.namePluginB, Constants.version010_alpha1) };
      availablePackages = allAvailablePackages;
      expectedPackages = new[] { CreatePackageIdentity(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version010_alpha1) };
      resolvedPackages = nuGetConnector.ResolveDependencies(additionalPackages, existingPackages, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsTrue(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);

      additionalPackages = null;
      existingPackages = new[] { CreatePackageIdentity(Constants.namePluginA, Constants.version010),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version020) };
      availablePackages = allAvailablePackages;
      expectedPackages = new[] { CreatePackageIdentity(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version020) };
      resolvedPackages = nuGetConnector.ResolveDependencies(additionalPackages, existingPackages, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsTrue(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);

      additionalPackages = null;
      existingPackages = new[] { CreatePackageIdentity(Constants.namePluginA, Constants.version020),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version020) };
      availablePackages = allAvailablePackages;
      expectedPackages = new[] { CreatePackageIdentity(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version020),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version020) };
      resolvedPackages = nuGetConnector.ResolveDependencies(additionalPackages, existingPackages, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsTrue(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);
      #endregion

      #region input additional packages and existing packages
      additionalPackages = new[] { Constants.namePluginB };
      existingPackages = new[] { CreatePackageIdentity(Constants.namePluginA, Constants.version010) };
      availablePackages = allAvailablePackages;
      expectedPackages = new[] { CreatePackageIdentity(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version031) };
      resolvedPackages = nuGetConnector.ResolveDependencies(additionalPackages, existingPackages, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsTrue(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);

      additionalPackages = new[] { Constants.namePluginB };
      existingPackages = new[] { CreatePackageIdentity(Constants.namePluginA, Constants.version030_beta1) };
      availablePackages = allAvailablePackages;
      expectedPackages = new[] { CreatePackageIdentity(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version031) };
      resolvedPackages = nuGetConnector.ResolveDependencies(additionalPackages, existingPackages, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsTrue(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);

      additionalPackages = new[] { Constants.namePluginA };
      existingPackages = new[] { CreatePackageIdentity(Constants.namePluginB, Constants.version020) };
      availablePackages = allAvailablePackages;
      expectedPackages = new[] { CreatePackageIdentity(Constants.nameBricksPluginTypes, Constants.versionBricksPluginTypes),
                                 CreatePackageIdentity(Constants.namePluginA, Constants.version030),
                                 CreatePackageIdentity(Constants.namePluginB, Constants.version020) };
      resolvedPackages = nuGetConnector.ResolveDependencies(additionalPackages, existingPackages, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsTrue(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);
      #endregion

      #region dependency resolution fails
      #region no available packages given
      additionalPackages = new[] { Constants.namePluginA };
      existingPackages = null;
      availablePackages = Array.Empty<SourcePackageDependencyInfo>();
      expectedPackages = Array.Empty<PackageIdentity>();
      resolvedPackages = nuGetConnector.ResolveDependencies(additionalPackages, existingPackages, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsFalse(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);
      #endregion

      #region additional packages are missing in available packages
      additionalPackages = new[] { Constants.namePluginA };
      existingPackages = null;
      availablePackages = allAvailablePackagesPluginB;
      expectedPackages = Array.Empty<PackageIdentity>();
      resolvedPackages = nuGetConnector.ResolveDependencies(additionalPackages, existingPackages, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsFalse(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);

      additionalPackages = null;
      existingPackages = new[] { CreatePackageIdentity(Constants.namePluginA, Constants.version010) };
      availablePackages = allAvailablePackagesPluginB;
      expectedPackages = Array.Empty<PackageIdentity>();
      resolvedPackages = nuGetConnector.ResolveDependencies(additionalPackages, existingPackages, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsFalse(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);
      #endregion

      #region dependencies are missing in available packages
      additionalPackages = new[] { Constants.namePluginB };
      existingPackages = null;
      availablePackages = allAvailablePackagesPluginB;
      expectedPackages = Array.Empty<PackageIdentity>();
      resolvedPackages = nuGetConnector.ResolveDependencies(additionalPackages, existingPackages, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsFalse(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);

      additionalPackages = null;
      existingPackages = new[] { CreatePackageIdentity(Constants.namePluginB, Constants.version010) };
      availablePackages = allAvailablePackagesPluginB;
      expectedPackages = Array.Empty<PackageIdentity>();
      resolvedPackages = nuGetConnector.ResolveDependencies(additionalPackages, existingPackages, availablePackages, default, out resolveSucceeded).ToArray();
      Assert.IsFalse(resolveSucceeded);
      CollectionAssert.AreEqual(expectedPackages, resolvedPackages);
      #endregion
      #endregion

      WriteLogToTestContextAndClear();
    }
    #endregion

    #region TestGetInstalledPackages
    [TestMethod]
    public void TestGetInstalledPackages() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: false);

      string[] expectedPackages;
      PackageFolderReader[] packageReaders;
      string[] foundPackages;

      List<string> expectedPackagesList = new List<string>();
      foreach (string expectedPackageDirectory in Directory.GetDirectories(LocalPackagesAbsolutePath)) {
        using PackageFolderReader reader = new PackageFolderReader(expectedPackageDirectory);
        expectedPackagesList.Add(reader.GetIdentity().ToString());
      }
      expectedPackages = expectedPackagesList.ToArray();
      packageReaders = nuGetConnector.GetInstalledPackages(LocalPackagesAbsolutePath).ToArray();
      foundPackages = packageReaders.Select(x => x.GetIdentity().ToString()).ToArray();
      foreach (PackageFolderReader packageReader in packageReaders) packageReader.Dispose();
      CollectionAssert.AreEqual(expectedPackages, foundPackages);

      WriteLogToTestContextAndClear();
    }
    #endregion

    #region TestGetPackageDownloaderAsync
    [TestMethod]
    public async Task TestGetPackageDownloaderAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: false);
      string remoteOfficialRepository = RemoteOfficialRepositoryAbsolutePath.Replace(@"\", "/");
      string remoteDevRepository = RemoteDevRepositoryAbsolutePath.Replace(@"\", "/");

      PackageIdentity package;
      IPackageDownloader foundPackageDownloader;

      package = CreatePackageIdentity(Constants.namePluginA, Constants.version010_alpha1);
      using (foundPackageDownloader = await nuGetConnector.GetPackageDownloaderAsync(package, default)) {
        Assert.IsNotNull(foundPackageDownloader);
        Assert.AreEqual(package, await foundPackageDownloader.CoreReader.GetIdentityAsync(default));
        Assert.AreEqual(remoteOfficialRepository, foundPackageDownloader.Source.Replace(@"\", "/"));
      }

      package = CreatePackageIdentity(Constants.namePluginA, Constants.version010);
      using (foundPackageDownloader = await nuGetConnector.GetPackageDownloaderAsync(package, default)) {
        Assert.IsNotNull(foundPackageDownloader);
        Assert.AreEqual(package, await foundPackageDownloader.CoreReader.GetIdentityAsync(default));
        Assert.AreEqual(remoteOfficialRepository, foundPackageDownloader.Source.Replace(@"\", "/"));
      }

      package = CreatePackageIdentity(Constants.namePluginA, Constants.version030_beta1);
      using (foundPackageDownloader = await nuGetConnector.GetPackageDownloaderAsync(package, default)) {
        Assert.IsNotNull(foundPackageDownloader);
        Assert.AreEqual(package, await foundPackageDownloader.CoreReader.GetIdentityAsync(default));
        Assert.AreEqual(remoteDevRepository, foundPackageDownloader.Source.Replace(@"\", "/"));
      }

      package = CreatePackageIdentity(Constants.namePluginA, Constants.version030);
      using (foundPackageDownloader = await nuGetConnector.GetPackageDownloaderAsync(package, default)) {
        Assert.IsNotNull(foundPackageDownloader);
        Assert.AreEqual(package, await foundPackageDownloader.CoreReader.GetIdentityAsync(default));
        Assert.AreEqual(remoteDevRepository, foundPackageDownloader.Source.Replace(@"\", "/"));
      }

      package = CreatePackageIdentity(Constants.namePluginA, Constants.version000);
      foundPackageDownloader = await nuGetConnector.GetPackageDownloaderAsync(package, default);
      Assert.IsNull(foundPackageDownloader);

      package = CreatePackageIdentity(Constants.nameInvalid, Constants.version010);
      foundPackageDownloader = await nuGetConnector.GetPackageDownloaderAsync(package, default);
      Assert.IsNull(foundPackageDownloader);

      package = CreatePackageIdentity(Constants.nameInvalid, Constants.version000);
      foundPackageDownloader = await nuGetConnector.GetPackageDownloaderAsync(package, default);
      Assert.IsNull(foundPackageDownloader);

      WriteLogToTestContextAndClear();
    }
    #endregion

    #region TestInstallPackageAsync
    [TestMethod]
    public async Task TestInstallPackageAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      PackageIdentity identity;
      SourcePackageDependencyInfo package;
      SourceRepository remoteOfficialRepository = Repository.CreateSource(Repository.Provider.GetCoreV3(), RemoteOfficialRepositoryAbsolutePath);

      identity = CreatePackageIdentity(Constants.namePluginA, Constants.version020);
      package = (await nuGetConnector.GetPackageDependenciesAsync(identity, false, default)).Single();
      await nuGetConnector.InstallPackageAsync(package, LocalPackagesAbsolutePath, LocalPackagesCacheAbsolutePath, default);
      Assert.IsTrue(Directory.Exists(Path.Combine(LocalPackagesAbsolutePath, identity.ToString())));

      identity = CreatePackageIdentity(Constants.namePluginA, Constants.version020);
      package = (await nuGetConnector.GetPackageDependenciesAsync(identity, false, default)).Single();
      await nuGetConnector.InstallPackageAsync(package, LocalPackagesAbsolutePath, LocalPackagesCacheAbsolutePath, default);
      Assert.IsTrue(Directory.Exists(Path.Combine(LocalPackagesAbsolutePath, identity.ToString())));

      identity = CreatePackageIdentity(Constants.namePluginB, Constants.version031);
      package = (await nuGetConnector.GetPackageDependenciesAsync(identity, false, default)).Single();
      await nuGetConnector.InstallPackageAsync(package, LocalPackagesAbsolutePath, LocalPackagesCacheAbsolutePath, default);
      Assert.IsTrue(Directory.Exists(Path.Combine(LocalPackagesAbsolutePath, identity.ToString())));

      identity = CreatePackageIdentity("SimSharp", "3.3.0");
      package = (await nuGetConnector.GetPackageDependenciesAsync(identity, false, default)).Single();
      await nuGetConnector.InstallPackageAsync(package, LocalPackagesAbsolutePath, LocalPackagesCacheAbsolutePath, default);
      Assert.IsTrue(Directory.Exists(Path.Combine(LocalPackagesAbsolutePath, identity.ToString())));

      package = new SourcePackageDependencyInfo(Constants.namePluginA, new NuGetVersion(Constants.version000), Enumerable.Empty<NuGetPackageDependency>(), true, remoteOfficialRepository);
      await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => { return nuGetConnector.InstallPackageAsync(package, LocalPackagesAbsolutePath, LocalPackagesCacheAbsolutePath, default); });

      WriteLogToTestContextAndClear();
    }
    #endregion

    #region TestInstallPackagesAsync
    [TestMethod]
    public async Task TestInstallPackagesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      PackageIdentity[] identities;
      SourcePackageDependencyInfo[] packages;
      SourceRepository remoteOfficialRepository = Repository.CreateSource(Repository.Provider.GetCoreV3(), RemoteOfficialRepositoryAbsolutePath);

      identities = new[] { CreatePackageIdentity(Constants.namePluginA, Constants.version020),
                           CreatePackageIdentity(Constants.namePluginB, Constants.version031),
                           CreatePackageIdentity("SimSharp", "3.3.0") };
      packages = (await nuGetConnector.GetPackageDependenciesAsync(identities, false, default)).ToArray();
      await nuGetConnector.InstallPackagesAsync(packages, LocalPackagesAbsolutePath, LocalPackagesCacheAbsolutePath, default);
      Assert.IsTrue(identities.All(x => Directory.Exists(Path.Combine(LocalPackagesAbsolutePath, x.ToString()))));

      identities = new[] { CreatePackageIdentity(Constants.namePluginB, Constants.version030),
                           CreatePackageIdentity(Constants.namePluginB, Constants.version030) };
      packages = (await nuGetConnector.GetPackageDependenciesAsync(identities, false, default)).ToArray();
      await nuGetConnector.InstallPackagesAsync(packages, LocalPackagesAbsolutePath, LocalPackagesCacheAbsolutePath, default);
      Assert.IsTrue(identities.All(x => Directory.Exists(Path.Combine(LocalPackagesAbsolutePath, x.ToString()))));

      packages = Array.Empty<SourcePackageDependencyInfo>();
      await nuGetConnector.InstallPackagesAsync(packages, LocalPackagesAbsolutePath, LocalPackagesCacheAbsolutePath, default);

      packages = new[] { new SourcePackageDependencyInfo(Constants.namePluginA, new NuGetVersion(Constants.version000), Enumerable.Empty<NuGetPackageDependency>(), true, remoteOfficialRepository) };
      await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => { return nuGetConnector.InstallPackagesAsync(packages, LocalPackagesAbsolutePath, LocalPackagesCacheAbsolutePath, default); });

      WriteLogToTestContextAndClear();
    }
    #endregion

    #region TestGetLatestVersionAsync
    [TestMethod]
    public async Task TestGetLatestVersionAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      string packageId;
      bool includePreReleases;
      NuGetVersion expectedVersion;
      NuGetVersion foundVersion;

      packageId = Constants.namePluginA;
      includePreReleases = false;
      expectedVersion = new NuGetVersion(Constants.version030);
      foundVersion = await nuGetConnector.GetLatestVersionAsync(packageId, includePreReleases, default);
      Assert.AreEqual(expectedVersion, foundVersion);

      packageId = Constants.namePluginB;
      includePreReleases = false;
      expectedVersion = new NuGetVersion(Constants.version031);
      foundVersion = await nuGetConnector.GetLatestVersionAsync(packageId, includePreReleases, default);
      Assert.AreEqual(expectedVersion, foundVersion);

      packageId = Constants.nameNuGetVersioning;
      includePreReleases = false;
      expectedVersion = new NuGetVersion(Constants.versionNuGetVersioning);
      foundVersion = await nuGetConnector.GetLatestVersionAsync(packageId, includePreReleases, default);
      Assert.AreEqual(expectedVersion, foundVersion);

      packageId = Constants.nameNuGetVersioning;
      includePreReleases = true;
      expectedVersion = new NuGetVersion(Constants.versionPreReleaseNuGetVersioning);
      foundVersion = await nuGetConnector.GetLatestVersionAsync(packageId, includePreReleases, default);
      Assert.AreEqual(expectedVersion, foundVersion);

      packageId = Constants.nameInvalid;
      includePreReleases = false;
      foundVersion = await nuGetConnector.GetLatestVersionAsync(packageId, includePreReleases, default);
      Assert.IsNull(foundVersion);

      packageId = Constants.nameInvalid;
      includePreReleases = true;
      foundVersion = await nuGetConnector.GetLatestVersionAsync(packageId, includePreReleases, default);
      Assert.IsNull(foundVersion);

      WriteLogToTestContextAndClear();
    }
    #endregion

    #region TestGetLatestVersionsAsync
    [TestMethod]
    public async Task TestGetLatestVersionsAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      string[] packageIds;
      bool includePreReleases;
      string[] expectedPackages;
      NuGetVersion[] expectedVersions;
      IEnumerable<(string PackageId, NuGetVersion Version)> foundVersions;

      packageIds = new[] { Constants.namePluginA, Constants.namePluginB };
      includePreReleases = false;
      expectedPackages = packageIds;
      expectedVersions = new[] { new NuGetVersion(Constants.version030), new NuGetVersion(Constants.version031) };
      foundVersions = await nuGetConnector.GetLatestVersionsAsync(packageIds, includePreReleases, default);
      CollectionAssert.AreEqual(expectedPackages, foundVersions.Select(x => x.PackageId).ToArray());
      CollectionAssert.AreEqual(expectedVersions, foundVersions.Select(x => x.Version).ToArray());

      packageIds = new[] { Constants.namePluginA, Constants.namePluginA, Constants.nameInvalid };
      includePreReleases = false;
      expectedPackages = new[] { Constants.namePluginA };
      expectedVersions = new[] { new NuGetVersion(Constants.version030) };
      foundVersions = await nuGetConnector.GetLatestVersionsAsync(packageIds, includePreReleases, default);
      CollectionAssert.AreEqual(expectedPackages, foundVersions.Select(x => x.PackageId).ToArray());
      CollectionAssert.AreEqual(expectedVersions, foundVersions.Select(x => x.Version).ToArray());

      WriteLogToTestContextAndClear();
    }
    #endregion

    #region Helpers
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
