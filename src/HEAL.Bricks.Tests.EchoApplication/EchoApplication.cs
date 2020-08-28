using System;

namespace HEAL.Bricks.Tests.EchoApplication {
  public class EchoApplication : IApplication {
    public string Name => "HEAL.Bricks.Tests.EchoApplication";
    public string Description => "";

    public void OnCancel() {
      throw new NotImplementedException();
    }

    public void OnPause() {
      throw new NotImplementedException();
    }

    public void OnResume() {
      throw new NotImplementedException();
    }

    public void Run(ICommandLineArgument[] args) {
      Console.Write("message > ");
      string message = Console.ReadLine();
      while (!string.IsNullOrEmpty(message)) {
        Console.WriteLine("echo > " + message);
        Console.Write("message > ");
        message = Console.ReadLine();
      }
    }
  }
}
