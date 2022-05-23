using System;
using System.Threading;
using System.Threading.Tasks;
using HEAL.Bricks;

namespace TestApp {
  class EchoApplication : Application {
    public override string Name => "EchoApplication";
    public override string Description => "Reads strings and returns their echo.";
    public override ApplicationKind Kind => ApplicationKind.Console;

    public EchoApplication() : base() { }
    public EchoApplication(IChannel channel) : base(channel) { }

    public override async Task RunAsync(string[] args, CancellationToken cancellationToken = default) {
      await Task.Run(() => {
        Console.WriteLine("EchoApplication started");
        Console.Write("message > ");
        string? message = Console.ReadLine();
        while (!string.IsNullOrEmpty(message)) {
          cancellationToken.ThrowIfCancellationRequested();
          Console.WriteLine("echo > " + message);
          Console.Write("message > ");
          message = Console.ReadLine();
        }
        Console.WriteLine("EchoApplication done");
      }, cancellationToken);
    }
  }
}
