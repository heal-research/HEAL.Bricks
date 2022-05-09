using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HEAL.Bricks;

namespace TestApp {
  class Program {
    static async Task Main(string[] args) {
      using (IChannel channel = ProcessChannel.CreateFromCLIArguments(args)) {
        if (channel != null) {
          await Runner.ReceiveAndExecuteAsync(channel);
          return;
        }
      }

      BricksOptions options = BricksOptions.Default;
      options.DefaultIsolation = Isolation.AnonymousPipes;
      options.Repositories.Add(new Repository(@"C:\00-Daten\NuGet"));
      Directory.CreateDirectory(Path.Combine(options.AppPath, options.PackagesPath));
      Directory.CreateDirectory(options.PackagesCachePath);
      IApplicationManager am = ApplicationManager.Create(options);

      if (am.InstalledApplications.Count() == 0) {
        Console.WriteLine("No applications found.");
        return;
      }
      
      int index;
      do {
        index = 1;
        foreach (var app in am.InstalledApplications) {
          Console.WriteLine($"[{index}] {app}");
          index++;
        }
        Console.Write("application > ");
        index = int.TryParse(Console.ReadLine(), out index) ? index - 1 : -1;

        if (index != -1) {
          await am.RunAsync(am.InstalledApplications.ElementAt(index));
        }
      } while (index != -1);

      CancellationTokenSource cts = new CancellationTokenSource();
      CancellationToken token = cts.Token;
      cts.CancelAfter(2000);

      using (IChannel channel = am.CreateRunnerChannel(options.DefaultIsolation)) {
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
      }

      Console.WriteLine("Done.");
    }
  }
}
