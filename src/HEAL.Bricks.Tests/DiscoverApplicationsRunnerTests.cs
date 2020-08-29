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

      IProcessRunnerStartInfo startInfo = CreateBricksRunnerStartInfo();
      DiscoverApplicationsRunner discoverApplicationsRunner = new DiscoverApplicationsRunner(pm.Settings, startInfo);
      ApplicationInfo[] app = (await discoverApplicationsRunner.GetApplicationsAsync());
    }

    #region Helpers
    private IProcessRunnerStartInfo CreateBricksRunnerStartInfo() {
      string runnerPath = Path.Combine(TestDeploymentPath, "HEAL.Bricks.Tests.BricksRunner.dll");
      string dotnetPath = GetOSPlatform() switch {
        Platform.Windows => "C:\\Program Files\\dotnet\\dotnet.exe",
        Platform.Linux => "/usr/bin/dotnet",
        _ => throw new PlatformNotSupportedException()
      };
      return new GenericProgramStartInfo(dotnetPath, runnerPath);
    }
    private enum Platform {
      Unknown,
      Windows,
      Linux,
      OSX,
      FreeBSD
    }
    private Platform GetOSPlatform() {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return Platform.Windows;
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return Platform.Linux;
      if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return Platform.OSX;
      if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)) return Platform.FreeBSD;
      return Platform.Unknown;
    }
    #endregion
  }
}
