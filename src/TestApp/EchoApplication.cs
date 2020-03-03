using System;
using System.Threading;
using System.Threading.Tasks;
using HEAL.Bricks;

namespace TestApp {
  class EchoApplication : IApplication {
    public string Name => "EchoApplication";
    public string Description => "Reads strings and returns their echo.";

    public void Run(ICommandLineArgument[] args) {
      Console.WriteLine("EchoApplication started");
      Console.Write("message > ");
      string message = Console.ReadLine();
      while (!string.IsNullOrEmpty(message)) {
        Console.WriteLine("echo > " + message);
        Console.Write("message > ");
        message = Console.ReadLine();
      }
      Console.WriteLine("EchoApplication done");
    }

    public async Task RunAsync(ICommandLineArgument[] args, CancellationToken cancellationToken = default) {
      await Task.Run(() => {
        Run(args);
      }, cancellationToken);
    }

    public void OnCancel() {
      throw new NotImplementedException();
    }
    public void OnPause() {
      throw new NotImplementedException();
    }
    public void OnResume() {
      throw new NotImplementedException();
    }
  }
}
