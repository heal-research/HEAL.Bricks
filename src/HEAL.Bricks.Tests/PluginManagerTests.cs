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
  public class PluginManagerTests {
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

    #region TestCreate
    [TestMethod]
    public void TestCreate() {
      IPluginManager pluginManager;

      pluginManager = PluginManager.Create(null);
      Assert.IsNotNull(pluginManager);
      Assert.IsNotNull(pluginManager.RemoteRepositories);
      Assert.AreEqual(0, pluginManager.RemoteRepositories.Count());
      Assert.AreEqual("", pluginManager.PluginTag);
      Assert.IsNotNull(pluginManager.Packages);
      Assert.AreEqual(0, pluginManager.Packages.Count());
      Assert.AreEqual(PluginManagerStatus.Uninitialized, pluginManager.Status);

      pluginManager = PluginManager.Create("HEALBricksPlugin");
      Assert.IsNotNull(pluginManager);
      Assert.IsNotNull(pluginManager.RemoteRepositories);
      Assert.AreEqual(0, pluginManager.RemoteRepositories.Count());
      Assert.AreEqual("HEALBricksPlugin", pluginManager.PluginTag);
      Assert.IsNotNull(pluginManager.Packages);
      Assert.AreEqual(0, pluginManager.Packages.Count());
      Assert.AreEqual(PluginManagerStatus.Uninitialized, pluginManager.Status);

      pluginManager = PluginManager.Create("HEALBricksPlugin", remoteOfficialRepository, remoteDevRepository);
      Assert.IsNotNull(pluginManager);
      Assert.AreEqual(2, pluginManager.RemoteRepositories.Count());
      Assert.AreEqual(remoteOfficialRepository, pluginManager.RemoteRepositories.First());
      Assert.AreEqual(remoteDevRepository, pluginManager.RemoteRepositories.Skip(1).First());
      Assert.AreEqual("HEALBricksPlugin", pluginManager.PluginTag);
      Assert.IsNotNull(pluginManager.Packages);
      Assert.AreEqual(0, pluginManager.Packages.Count());
      Assert.AreEqual(PluginManagerStatus.Uninitialized, pluginManager.Status);
    }
    #endregion

    #region TestCtor
    [TestMethod]
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
      Assert.IsNotNull(pluginManager.Packages);
      Assert.AreEqual(0, pluginManager.Packages.Count());
      Assert.AreEqual(PluginManagerStatus.Uninitialized, pluginManager.Status);

      pluginManager = new PluginManager("HEALBricksPlugin", nuGetConnector);
      Assert.IsNotNull(pluginManager);
      Assert.IsNotNull(pluginManager.RemoteRepositories);
      CollectionAssert.AreEqual(nuGetConnector.RemoteRepositories.Select(x => x.PackageSource.Source).ToArray(), pluginManager.RemoteRepositories.ToArray());
      Assert.AreEqual("HEALBricksPlugin", pluginManager.PluginTag);
      Assert.IsNotNull(pluginManager.Packages);
      Assert.AreEqual(0, pluginManager.Packages.Count());
      Assert.AreEqual(PluginManagerStatus.Uninitialized, pluginManager.Status);
    }
    #endregion

    #region TestInitializeAsync
    [TestMethod]
    public async Task TestInitializeAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      PluginManager pluginManager = new PluginManager("HEALBricksPlugin", nuGetConnector);

      await pluginManager.InitializeAsync();

      foreach (var plugin in pluginManager.Packages) {
        TestContext.WriteLine(plugin.ToStringWithDependencies());
      }

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
