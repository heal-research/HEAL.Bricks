#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Diagnostics;

namespace HEAL.Bricks {
  public sealed class StdInOutProcessChannel : ProcessChannel {
    public StdInOutProcessChannel(string programPath, string arguments = null) : base(programPath, arguments) { }
    private StdInOutProcessChannel() {
      // used to create a new channel on the client-side
    }

    protected override void ReadCLIArguments(string[] arguments) {
      base.ReadCLIArguments(arguments);
      inputStream = Console.OpenStandardInput();
      outputStream = Console.OpenStandardOutput();
    }
    protected override ProcessStartInfo CreateProcessStartInfo() {
      ProcessStartInfo startInfo = base.CreateProcessStartInfo();
      startInfo.RedirectStandardInput = true;
      startInfo.RedirectStandardOutput = true;
      return startInfo;
    }

    protected override void PostStartActions() {
      base.PostStartActions();
      outputStream = process.StandardInput.BaseStream;
      inputStream = process.StandardOutput.BaseStream;
    }
  }
}
