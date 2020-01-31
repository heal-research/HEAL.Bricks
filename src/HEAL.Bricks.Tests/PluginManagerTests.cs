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
using NuGet.Resolver;
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
  [DeploymentItem(Constants.pathPluginA_010_alpha2, Constants.localPackagesRelativePath)]
  [DeploymentItem(Constants.pathPluginA_010, Constants.localPackagesRelativePath)]
  [DeploymentItem(Constants.pathPluginA_020_alpha1, Constants.localPackagesRelativePath)]
  [DeploymentItem(Constants.pathPluginB_010_alpha2, Constants.localPackagesRelativePath)]
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
  [DeploymentItem(Constants.pathPluginA_030_alpha1, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_030_beta1, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_030, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_030_alpha1, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_030_alpha2, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_030, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_031, Constants.remoteDevRepositoryRelativePath)]
  public class PluginManagerTests {
    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void TestInitialize() {
    }

    #region TestCreate
    /*[TestMethod]
    public void TestCreate() {
      IPluginManager pluginManager;

      pluginManager = PluginManager.Create(null);
      Assert.IsNotNull(pluginManager);
      Assert.IsNotNull(pluginManager.RemoteRepositories);
      Assert.AreEqual(0, pluginManager.RemoteRepositories.Count());
      Assert.AreEqual("", pluginManager.PluginTag);
      Assert.IsNotNull(pluginManager.RemotePackages);
      Assert.AreEqual(0, pluginManager.RemotePackages.Count());
      Assert.AreEqual(PluginManagerStatus.Uninitialized, pluginManager.Status);

      pluginManager = PluginManager.Create("HEALBricksPlugin");
      Assert.IsNotNull(pluginManager);
      Assert.IsNotNull(pluginManager.RemoteRepositories);
      Assert.AreEqual(0, pluginManager.RemoteRepositories.Count());
      Assert.AreEqual("HEALBricksPlugin", pluginManager.PluginTag);
      Assert.IsNotNull(pluginManager.RemotePackages);
      Assert.AreEqual(0, pluginManager.RemotePackages.Count());
      Assert.AreEqual(PluginManagerStatus.Uninitialized, pluginManager.Status);

      pluginManager = PluginManager.Create("HEALBricksPlugin", remoteOfficialRepository, remoteDevRepository);
      Assert.IsNotNull(pluginManager);
      Assert.AreEqual(2, pluginManager.RemoteRepositories.Count());
      Assert.AreEqual(remoteOfficialRepository, pluginManager.RemoteRepositories.First());
      Assert.AreEqual(remoteDevRepository, pluginManager.RemoteRepositories.Skip(1).First());
      Assert.AreEqual("HEALBricksPlugin", pluginManager.PluginTag);
      Assert.IsNotNull(pluginManager.RemotePackages);
      Assert.AreEqual(0, pluginManager.RemotePackages.Count());
      Assert.AreEqual(PluginManagerStatus.Uninitialized, pluginManager.Status);
    }*/
    #endregion

    #region TestCtor
    /*[TestMethod]
    public void TestCtor() {
      NuGetConnector nuGetConnector = CreateNuGetConnector();
      IPluginManager pluginManager;

      ArgumentNullException e;
      e = Assert.ThrowsException<ArgumentNullException>(() => new PluginManager("", null));
      Assert.IsFalse(string.IsNullOrEmpty(e.ParamName));

      pluginManager = new PluginManager(null, nuGetConnector);
      Assert.IsNotNull(pluginManager);
      Assert.IsNotNull(pluginManager.RemoteRepositories);
      CollectionAssert.AreEqual(nuGetConnector.RemoteRepositories.Select(x => x.PackageSource.Source).ToArray(), pluginManager.RemoteRepositories.ToArray());
      Assert.AreEqual("", pluginManager.PluginTag);
      Assert.IsNotNull(pluginManager.RemotePackages);
      Assert.AreEqual(0, pluginManager.RemotePackages.Count());
      Assert.AreEqual(PluginManagerStatus.Uninitialized, pluginManager.Status);

      pluginManager = new PluginManager("HEALBricksPlugin", nuGetConnector);
      Assert.IsNotNull(pluginManager);
      Assert.IsNotNull(pluginManager.RemoteRepositories);
      CollectionAssert.AreEqual(nuGetConnector.RemoteRepositories.Select(x => x.PackageSource.Source).ToArray(), pluginManager.RemoteRepositories.ToArray());
      Assert.AreEqual("HEALBricksPlugin", pluginManager.PluginTag);
      Assert.IsNotNull(pluginManager.RemotePackages);
      Assert.AreEqual(0, pluginManager.RemotePackages.Count());
      Assert.AreEqual(PluginManagerStatus.Uninitialized, pluginManager.Status);
    }*/
    #endregion

    #region TestInitializeAsync
    /*[TestMethod]
    [TestCategory("WIP")]
    public async Task TestInitializeAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      PluginManager pluginManager = new PluginManager("HEALBricksPlugin", nuGetConnector);

      SourcePackageDependencyInfo package = (await nuGetConnector.GetPackageDependenciesAsync(new PackageIdentity(Constants.namePluginB, new NuGetVersion(Constants.version031)), nuGetConnector.Repositories, false, default)).Single();
      DownloadResourceResult downloadResult = await nuGetConnector.DownloadPackageAsync(package, default);
      await nuGetConnector.InstallPackageAsync(downloadResult, default);

      pluginManager.Initialize();

      foreach (var plugin in pluginManager.InstalledPackages) {
        TestContext.WriteLine(plugin.ToStringWithDependencies());
      }

      WriteLogToTestContextAndClear(nuGetConnector);
      Assert.Fail("This unit test is incomplete and is still work in progress.");
    }*/
    #endregion

    #region TestResolveMissingDependenciesAsync
    /*[TestMethod]
    [TestCategory("WIP")]
    public async Task TestResolveMissingDependenciesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      PluginManager pluginManager = new PluginManager("HEALBricksPlugin", nuGetConnector);

      IEnumerable<RemotePackageInfo> missingDependencies = await pluginManager.ResolveMissingDependenciesAsync();

      foreach (var dependency in missingDependencies) {
        TestContext.WriteLine(dependency.ToStringWithDependencies());
      }

      WriteLogToTestContextAndClear(nuGetConnector);
      Assert.Fail("This unit test is incomplete and is still work in progress.");
    }*/
    #endregion

    #region TestDownloadMissingDependenciesAsync
    /*[TestMethod]
    [TestCategory("WIP")]
    public async Task TestDownloadMissingDependenciesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      PluginManager pluginManager = new PluginManager("HEALBricksPlugin", nuGetConnector);

      IEnumerable<RemotePackageInfo> missingDependencies = await pluginManager.ResolveMissingDependenciesAsync();

      foreach (var missingDependency in missingDependencies) {
        await pluginManager.InstallRemotePackageAsync(missingDependency);
      }

      WriteLogToTestContextAndClear(nuGetConnector);
      Assert.Fail("This unit test is incomplete and is still work in progress.");
    }*/
    #endregion

    #region TestInstallPackagesAsync
    /*[TestMethod]
    [TestCategory("WIP")]
    public async Task TestInstallPackagesAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      PluginManager pluginManager = new PluginManager("HEALBricksPlugin", nuGetConnector);

      await pluginManager.InitializeAsync();
      await pluginManager.ResolveMissingDependenciesAsync();
      await pluginManager.InstallPackagesAsync();

      WriteLogToTestContextAndClear(nuGetConnector);
      Assert.Fail("This unit test is incomplete and is still work in progress.");
    }*/
    #endregion

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
