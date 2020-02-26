#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.IO;

namespace HEAL.Bricks.Runner {
  class Program {
    static void Main(string[] args) {
      Stream stdin = Console.OpenStandardInput();
      IRunner runner = (RunnerMessage.ReadFromStream(stdin) as StartRunnerMessage)?.Data;

      if (runner != null) {
        runner.Execute();
      } else {
        Console.Error.WriteLine("Cannot deserialize runner from console standard input.");
      }
    }
  }
}
