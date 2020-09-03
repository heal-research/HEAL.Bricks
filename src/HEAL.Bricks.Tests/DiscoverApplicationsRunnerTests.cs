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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks.Tests {
  [TestClass]
  public class DiscoverApplicationsRunnerTests : PackageTestsBase {
    [TestMethod]
    public async Task TestGetApplicationsAsync() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      IPackageManager pm = PackageManager.CreateForTests(CreateSettings(includePublicNuGetRepository: true), nuGetConnector);
      await pm.InstallMissingDependenciesAsync();

      IChannel channel = new AnonymousPipesProcessChannel(DotnetExeAbsolutePath, "\"" + Path.Combine(TestDeploymentPath, "HEAL.Bricks.Tests.BricksRunner.dll") + "\"" + " --TestRunner");
      DiscoverApplicationsRunner discoverApplicationsRunner = new DiscoverApplicationsRunner(pm.GetPackageLoadInfos());
      ApplicationInfo[] app = (await discoverApplicationsRunner.GetApplicationsAsync(channel));
    }
  }
}
