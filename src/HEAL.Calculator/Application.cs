#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using HEAL.Bricks;
using HEAL.Calculator.Operators;
using System;
using System.Runtime.CompilerServices;

namespace HEAL.Calculator {
  public class Application : IApplication {
    public string Name => "HEAL.Calculator";
    public string Description => "Simple calculator to demonstrate HEAL.Bricks plugins.";

    public void Run(ICommandLineArgument[] args) {
      ITypeDiscoverer td = TypeDiscoverer.Create();
      var ops = td.GetInstances<IBinaryOperator>();

      double x = 3.0;
      double y = 4.0;
      Console.WriteLine($"x = {x.ToString()}");
      Console.WriteLine($"y = {y.ToString()}");
      foreach (var op in ops) {
        Console.WriteLine($"{op.GetType().Name} = {op.Apply(x, y).ToString()}");
      }
      Console.WriteLine();
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
