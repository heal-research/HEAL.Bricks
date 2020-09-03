using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HEAL.Bricks;

namespace TestApp {
  class Program {
    static async Task Main(string[] args) {
      if (Runner.ParseArguments(args, out CommunicationMode communicationMode, out string inputConnection, out string outputConnection)) {
        await Runner.ReceiveAndExecuteRunnerAsync(communicationMode, inputConnection, outputConnection);
        return;
      }

      Settings settings = new Settings() {
        PackageTag = "HEALBricksPlugin"
      };
      settings.Repositories.Add(@"C:\# Daten\NuGet");
      Directory.CreateDirectory(settings.PackagesPath);
      Directory.CreateDirectory(settings.PackagesCachePath);
      IPackageManager pm = PackageManager.Create(settings);

      IChannel channel = new AnonymousPipesProcessChannel("dotnet", "\"" + Assembly.GetEntryAssembly().Location + "\"");
      DiscoverApplicationsRunner discoverApplicationsRunner = new DiscoverApplicationsRunner(pm.GetPackageLoadInfos());
      ApplicationInfo[] applications = await discoverApplicationsRunner.GetApplicationsAsync(channel);

      if (applications.Length == 0) {
        Console.WriteLine("No applications found.");
        return;
      }
      
      int index;
      do {
        for (index = 1; index <= applications.Length; index++) {
          Console.WriteLine($"[{index}] {applications[index - 1]}");
        }
        Console.Write("application > ");
        index = int.TryParse(Console.ReadLine(), out index) ? index - 1 : -1;

        if (index != -1) {
          ApplicationRunner applicationRunner = new ApplicationRunner(pm.GetPackageLoadInfos(), applications[index]);
          await applicationRunner.RunAsync();
        }
      } while (index != -1);

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
