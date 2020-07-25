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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks.Tests {
  [TestClass]
  public class DiscoverApplicationsRunnerTests : PluginTestsBase {
    [TestMethod]
    public async Task TestGetApplicationsAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      PluginManager pluginManager = new PluginManager(CreateSettings(includePublicNuGetRepository: true), nuGetConnector);
      await pluginManager.InstallMissingDependenciesAsync();

      IProcessRunnerStartInfo startInfo = CreateBricksRunnerStartInfo();
      DiscoverApplicationsRunner discoverApplicationsRunner = new DiscoverApplicationsRunner(pluginManager.Settings, startInfo);
      ApplicationInfo[] app = (await discoverApplicationsRunner.GetApplicationsAsync());



    }

    #region Helpers
    private IProcessRunnerStartInfo CreateBricksRunnerStartInfo() {
      string runnerPath = Path.Combine(TestDeploymentPath, "HEAL.Bricks.Tests.BricksRunner.exe");
      return new GenericProgramStartInfo(runnerPath);
    }
    #endregion
  }
}
