#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;

namespace HEAL.Bricks {
  public sealed class AnonymousPipesProcessChannel : ProcessChannel {
    public static string InputConnectionArgument => "--InputConnection=";
    public static string OutputConnectionArgument => "--OutputConnection=";

    private AnonymousPipeServerStream anonymousPipeOutputStream, anonymousPipeInputStream;

    public AnonymousPipesProcessChannel(string programPath, string arguments = null) : base(programPath, arguments) { }
    private AnonymousPipesProcessChannel() {
      // used to create a new channel on the client-side
    }

    protected override void ReadCLIArguments(string[] arguments) {
      base.ReadCLIArguments(arguments);
      try {
        string inputConnection = arguments.Where(x => x.Contains(InputConnectionArgument)).Select(x => x.Split('=')[1]).SingleOrDefault() ?? string.Empty;
        inputStream = new AnonymousPipeClientStream(PipeDirection.In, inputConnection);
      }
      catch (Exception) { }
      try {
        string outputConnection = arguments.Where(x => x.Contains(OutputConnectionArgument)).Select(x => x.Split('=')[1]).SingleOrDefault() ?? string.Empty;
        outputStream = new AnonymousPipeClientStream(PipeDirection.Out, outputConnection);
      }
      catch (Exception) { }
    }

    protected override ProcessStartInfo CreateProcessStartInfo() {
      ProcessStartInfo startInfo = base.CreateProcessStartInfo();

      anonymousPipeOutputStream = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
      startInfo.Arguments += " " + InputConnectionArgument + anonymousPipeOutputStream.GetClientHandleAsString();

      anonymousPipeInputStream = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
      startInfo.Arguments += " " + OutputConnectionArgument + anonymousPipeInputStream.GetClientHandleAsString();

      return startInfo;
    }

    protected override void PostStartActions() {
      base.PostStartActions();
      anonymousPipeOutputStream.DisposeLocalCopyOfClientHandle();
      outputStream = anonymousPipeOutputStream;
      anonymousPipeInputStream.DisposeLocalCopyOfClientHandle();
      inputStream = anonymousPipeInputStream;
    }

    protected override void DisposeMembers() {
      base.DisposeMembers();
      anonymousPipeInputStream = null;
      anonymousPipeOutputStream = null;
    }
  }
}
