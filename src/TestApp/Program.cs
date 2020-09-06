using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HEAL.Bricks;

namespace TestApp {
  class Program {
    static async Task Main(string[] args) {
      IChannel channel = ProcessChannel.CreateFromCLIArguments(args);
      if (channel != null) {
        await Runner.ReceiveAndExecuteAsync(channel);
        return;
      }

      Settings settings = new Settings() {
        PackageTag = "HEALBricksPlugin"
      };
      settings.Repositories.Add(@"C:\# Daten\NuGet");
      Directory.CreateDirectory(settings.PackagesPath);
      Directory.CreateDirectory(settings.PackagesCachePath);
      IPackageManager pm = PackageManager.Create(settings);

      ApplicationInfo[] applications;
      channel = new AnonymousPipesProcessChannel("dotnet", "\"" + Assembly.GetEntryAssembly().Location + "\"");
      DiscoverApplicationsRunner discoverApplicationsRunner = new DiscoverApplicationsRunner(pm.GetPackageLoadInfos());
      applications = await discoverApplicationsRunner.GetApplicationsAsync(channel);

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
          channel = new StdInOutProcessChannel("dotnet", "\"" + Assembly.GetEntryAssembly().Location + "\"");
          ApplicationRunner applicationRunner = new ApplicationRunner(pm.GetPackageLoadInfos(), applications[index]);
          await applicationRunner.RunAsync(channel);
        }
      } while (index != -1);

      CancellationTokenSource cts = new CancellationTokenSource();
      CancellationToken token = cts.Token;
      cts.CancelAfter(1000);

      channel = new AnonymousPipesProcessChannel("dotnet", "\"" + Assembly.GetEntryAssembly().Location + "\"");
      EchoRunner echoRunner = new EchoRunner();
      Task done = echoRunner.RunAsync(channel, token);

      int i = 0;
      while (!token.IsCancellationRequested) {
        i++;
        Console.Write("Send Hello ... ");
        await echoRunner.SendAsync("Hello " + i, channel, token);
        Console.WriteLine("done");

        Console.WriteLine("Receive Echo ... ");
        string message = await echoRunner.ReceiveAsync(token);
        Console.WriteLine(message);
        Console.WriteLine("... done");
      }
      try { await done; } catch { }

      Console.WriteLine("Done.");
    }
  }
}
