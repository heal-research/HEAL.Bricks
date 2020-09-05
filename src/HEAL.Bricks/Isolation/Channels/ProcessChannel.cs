#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HEAL.Bricks {
  public abstract class ProcessChannel : StreamChannel {
    public static string ChannelTypeArgument => "--ChannelType";

    public static ProcessChannel CreateFromCLIArguments(string[] arguments) {
      Guard.Argument(arguments, nameof(arguments)).NotNull();

      ProcessChannel channel = null;
      if (arguments.Any(x => x.StartsWith(ChannelTypeArgument))) {
        try {
          string channelTypeName = arguments.Where(x => x.StartsWith(ChannelTypeArgument)).Select(x => x.Split('=')[1]).Single();
          Type channelType = Type.GetType(channelTypeName);
          channel = (ProcessChannel)Activator.CreateInstance(channelType, nonPublic: true);
        }
        catch (Exception e) {
          throw new ArgumentException("Cannot create instance of channel type.", nameof(arguments), e);
        }
        channel.ReadCLIArguments(arguments);
      }
      return channel;
    }

    private readonly string programPath, arguments;

    protected Process process = null;

    public ProcessChannel(string programPath, string arguments = null) {
      this.programPath = Guard.Argument(programPath, nameof(programPath)).NotNull().NotEmpty().NotWhiteSpace();
      this.arguments = arguments ?? string.Empty;
    }
    protected ProcessChannel() { }

    public override void Open() {
      base.Open();
      process = new Process {
        StartInfo = CreateProcessStartInfo(),
        EnableRaisingEvents = true
      };
      process.Start();
      PostStartActions();
    }
    protected override void DisposeMembers() {
      try {
        if ((process != null) && !process.HasExited) {
          process.Kill();
          process.WaitForExit();
        }
      }
      finally {
        process?.Dispose();
        process = null;
      }
      base.DisposeMembers();
    }

    protected virtual void ReadCLIArguments(string[] arguments) { }
    protected virtual ProcessStartInfo CreateProcessStartInfo() {
      return new ProcessStartInfo {
        FileName = programPath,
        Arguments = arguments + " " + ChannelTypeArgument + "=" + this.GetType().FullName,
        UseShellExecute = false,
        CreateNoWindow = true,
        ErrorDialog = true,
        WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
      };
    }
    protected virtual void PostStartActions() { }
  }
}
