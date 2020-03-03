using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HEAL.Bricks;

namespace TestApp {
  class Program {
    static async Task Main(string[] args) {
      if ((args.Length == 1) && (args[0] == Runner.StartRunnerArgument)) {
        await Runner.ReceiveAndExecuteRunnerAsync(Console.OpenStandardInput());
        return;
      }

      Settings settings = new Settings();
      settings.PluginTag = "HEALBricksPlugin";
      settings.Repositories.Add(@"C:\# Daten\NuGet");
      Directory.CreateDirectory(settings.PackagesPath);
      Directory.CreateDirectory(settings.PackagesCachePath);
      IPluginManager pluginManager = PluginManager.Create(settings);
      pluginManager.Initialize();

      DiscoverApplicationsRunner discoverApplicationsRunner = new DiscoverApplicationsRunner(pluginManager.Settings);
      ApplicationInfo[] applications = await discoverApplicationsRunner.GetApplicationsAsync();

      ConsoleApplicationRunner applicationRunner = new ConsoleApplicationRunner(pluginManager.Settings, applications[0]);
      await applicationRunner.RunAsync();


      CancellationTokenSource cts = new CancellationTokenSource();
      CancellationToken token = cts.Token;
      cts.CancelAfter(2000);

      EchoRunner echoRunner = new EchoRunner();
      var t = echoRunner.RunAsync(token);

      int i = 0;
      while (!token.IsCancellationRequested) {
        i++;
        Console.Write("Send Hello ... ");
        await echoRunner.SendAsync("Hello " + i, token);
        Console.WriteLine("done");

        Console.WriteLine("Receive Echo ... ");
        string message = await echoRunner.ReceiveAsync(token);
        Console.WriteLine(message);
        Console.WriteLine("... done");
      }

      await t;
      Console.WriteLine("EchoRunner Status = " + echoRunner.Status);
    }
  }
}
