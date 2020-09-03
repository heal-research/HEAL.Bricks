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
using System.Threading.Channels;
using System.Threading.Tasks;

namespace HEAL.Bricks.Tests {
  [TestClass]
  public class IsolationTests : PackageTestsBase {
    [TestMethod]
    [TestCategory("WIP")]
    public async Task TestIsolation() {
      ISettings settings = CreateSettings(includePublicNuGetRepository: true);

      //NativeRunnerHost nativeRunnerHost = new NativeRunnerHost();
      //DiscoverPackagesRunner discoverPackagesRunner = new DiscoverPackagesRunner(settings);
      //await nativeRunnerHost.RunAsync(discoverPackagesRunner);
      //var message = nativeRunnerHost.Receive();
    }

    [TestMethod]
    public async Task TestEchoRunner() {
      //IChannel channel;
      //EchoRunner runner;
      //CancellationTokenSource cts;
      //CancellationToken token;
      //Task task;

      //cts = new CancellationTokenSource();
      //token = cts.Token;
      //channel = new AnonymousPipesProcessChannel(DotnetExeAbsolutePath, "\"" + Path.Combine(TestDeploymentPath, "HEAL.Bricks.Tests.BricksRunner.dll") + "\"" + " --TestRunner");
      //runner = new EchoRunner();
      //task = runner.RunAsync(channel, token);
      //for (int i = 0; i < 10; i++) {
      //  Trace.Write("Send Hello ... ");
      //  await runner.SendAsync("Hello " + i, channel, token);
      //  Trace.WriteLine("done");

      //  Trace.WriteLine("Receive Echo ... ");
      //  string message = await runner.ReceiveAsync(token);
      //  Trace.WriteLine(message);
      //  Trace.WriteLine("... done");
      //}
      //cts.Cancel();
      //await task;

      //cts = new CancellationTokenSource();
      //cts.CancelAfter(3000);
      //token = cts.Token;
      //channel = new AnonymousPipesProcessChannel(DotnetExeAbsolutePath, "\"" + Path.Combine(TestDeploymentPath, "HEAL.Bricks.Tests.BricksRunner.dll") + "\"" + " --TestRunner");
      //runner = new EchoRunner();
      //task = runner.RunAsync(channel, token);
      //int j = 0;
      //while (!token.IsCancellationRequested) {
      //  j++;
      //  Trace.Write("Send Hello ... ");
      //  await runner.SendAsync("Hello " + j, channel, token);
      //  Trace.WriteLine("done");

      //  Trace.WriteLine("Receive Echo ... ");
      //  string message = await runner.ReceiveAsync(token);
      //  Trace.WriteLine(message);
      //  Trace.WriteLine("... done");
      //}
      //await task;
    }

    [TestMethod]
    [TestCategory("WIP")]
    public async Task TestEchoApplication() {
      //NuGetConnector nuGetConnector = CreateNuGetConnector(includePublicNuGetRepository: true);
      //IPackageManager pm = PackageManager.CreateForTests(CreateSettings(includePublicNuGetRepository: true), nuGetConnector);

      //await pm.InstallMissingDependenciesAsync();

      //DiscoverApplicationsRunner discoverApplicationsRunner = new DiscoverApplicationsRunner(pm.Settings);
      //ApplicationInfo app = (await discoverApplicationsRunner.GetApplicationsAsync())[0];

      //ApplicationRunner applicationRunner = new ApplicationRunner(pm.Settings, app);
      //await applicationRunner.RunAsync();
    }
  }
}
