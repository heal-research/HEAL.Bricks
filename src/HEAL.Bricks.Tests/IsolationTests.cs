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
  public class IsolationTests : PluginTestsBase {
    [TestMethod]
    [TestCategory("WIP")]
    public async Task TestIsolation() {
      ISettings settings = CreateSettings(includePublicNuGetRepository: true);

      //NativeRunnerHost nativeRunnerHost = new NativeRunnerHost();
      //DiscoverPluginsRunner discoverPluginsRunner = new DiscoverPluginsRunner(settings);
      //await nativeRunnerHost.RunAsync(discoverPluginsRunner);
      //var message = nativeRunnerHost.Receive();
    }

    [TestMethod]
    public async Task TestEchoRunner() {
      EchoRunner runner;
      CancellationTokenSource cts;
      CancellationToken token;
      Task task;

      cts = new CancellationTokenSource();
      token = cts.Token;
      runner = new EchoRunner(CreateBricksRunnerStartInfo());
      task = runner.RunAsync(token);
      for (int i = 0; i < 10; i++) {
        Trace.Write("Send Hello ... ");
        await runner.SendAsync("Hello " + i, token);
        Trace.WriteLine("done");

        Trace.WriteLine("Receive Echo ... ");
        string message = await runner.ReceiveAsync(token);
        Trace.WriteLine(message);
        Trace.WriteLine("... done");
      }
      cts.Cancel();
      await task;

      cts = new CancellationTokenSource();
      cts.CancelAfter(3000);
      token = cts.Token;
      runner = new EchoRunner(CreateBricksRunnerStartInfo());
      task = runner.RunAsync(token);
      int j = 0;
      while (!token.IsCancellationRequested) {
        j++;
        Trace.Write("Send Hello ... ");
        await runner.SendAsync("Hello " + j, token);
        Trace.WriteLine("done");

        Trace.WriteLine("Receive Echo ... ");
        string message = await runner.ReceiveAsync(token);
        Trace.WriteLine(message);
        Trace.WriteLine("... done");
      }
      await task;
    }

    [TestMethod]
    [TestCategory("WIP")]
    public async Task TestEchoApplication() {
      NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      PluginManager pluginManager = new PluginManager(CreateSettings(includePublicNuGetRepository: true), nuGetConnector);

      await pluginManager.InstallMissingDependenciesAsync();

      DiscoverApplicationsRunner discoverApplicationsRunner = new DiscoverApplicationsRunner(pluginManager.Settings);
      ApplicationInfo app = (await discoverApplicationsRunner.GetApplicationsAsync())[0];

      ApplicationRunner applicationRunner = new ApplicationRunner(pluginManager.Settings, app);
      await applicationRunner.RunAsync();
    }

    #region Helpers
    private IProcessRunnerStartInfo CreateBricksRunnerStartInfo() {
      string runnerPath = Path.Combine(TestDeploymentPath, "HEAL.Bricks.Tests.BricksRunner.exe");
      return new GenericProgramStartInfo(runnerPath);
    }
    #endregion
  }
}
