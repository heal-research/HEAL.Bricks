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
    [TestCategory("WIP")]
    public async Task TestProcessRunner() {
      EchoRunner runner = new EchoRunner("HEAL.Bricks.Runner.exe");
      
      CancellationTokenSource cts = new CancellationTokenSource();
      cts.CancelAfter(10000);

      var t = runner.RunAsync(cts.Token);

      while (!cts.Token.IsCancellationRequested) {
        Trace.Write("Send Hello ... ");
        runner.SendMessage(new RunnerTextMessage("Hello"));
        Trace.WriteLine("done");

        Trace.WriteLine("Receive Echo ... ");
        var message = runner.ReceiveMessage() as RunnerTextMessage;
        Trace.WriteLine(message.Data);
        Trace.WriteLine("... done");
      }

      await t;
    }
  }
}
