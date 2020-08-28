#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Threading.Tasks;

namespace HEAL.Bricks.Tests.BricksRunner {
  class Program {
    static async Task Main(string[] args) {
      if (Runner.ParseArguments(args, out CommunicationMode communicationMode, out string inputConnection, out string outputConnection)) {
        await Runner.ReceiveAndExecuteRunnerAsync(communicationMode, inputConnection, outputConnection);
      } else {
        Console.WriteLine("Invalid command line arguments.");
      }
    }
  }
}
