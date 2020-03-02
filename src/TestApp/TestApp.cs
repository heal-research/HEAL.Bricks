using System;
using HEAL.Bricks;

namespace TestApp {
  class TestApp : IApplication {
    public string Name => "TestApp";
    public string Description => "TestApp Description";

    public void Run(ICommandLineArgument[] args) {
      Console.WriteLine("TestApp started");
      Console.Write("input > ");
      string input = Console.ReadLine();
      Console.WriteLine("input: " + input);
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
