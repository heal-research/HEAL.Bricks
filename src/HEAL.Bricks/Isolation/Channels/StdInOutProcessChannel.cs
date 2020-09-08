#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;

namespace HEAL.Bricks {
  public class StdInOutProcessChannel : ProcessChannel {
    public StdInOutProcessChannel(string programPath, string arguments = null) : base(programPath, arguments) { }
    protected StdInOutProcessChannel() {
      // used to create a new channel on the client-side
    }

    protected override void ReadCLIArguments(string[] arguments) {
      base.ReadCLIArguments(arguments);
      inputStream = Console.OpenStandardInput();
      outputStream = Console.OpenStandardOutput();
    }
    protected override void PostStartActions() {
      base.PostStartActions();
      outputStream = process.StandardInput.BaseStream;
      inputStream = process.StandardOutput.BaseStream;
    }
  }
}
