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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  public abstract class ProcessChannel : IChannel, IDisposable {
    private static int CancelMessageTimeoutBeforeKill => 2000;
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

    protected Process process;
    protected Stream outputStream, inputStream;

    public ProcessChannel(string programPath, string arguments = null) {
      this.programPath = Guard.Argument(programPath, nameof(programPath)).NotNull().NotEmpty().NotWhiteSpace();
      this.arguments = arguments ?? string.Empty;
    }
    protected ProcessChannel() { }

    public void Open() {
      Guard.Operation(process == null);

      process = new Process {
        StartInfo = CreateProcessStartInfo(),
        EnableRaisingEvents = true
      };
      process.Start();
      PostStartActions();
    }
    public void Close() {
      Guard.Operation(process != null);

      try {
        if (!process.HasExited) {
          SendMessageAsync(new CancelRunnerMessage()).Wait();
          if (!process.WaitForExit(CancelMessageTimeoutBeforeKill)) {
            process.Kill();
            process.WaitForExit();
          }
        }
      }
      finally {
        outputStream?.Close();
        inputStream?.Close();
        process?.Close();
      }
    }

    public async Task SendMessageAsync(IMessage message, CancellationToken cancellationToken = default) {
      Guard.Argument(message, nameof(message)).NotNull();
      Guard.Operation((outputStream != null) && outputStream.CanWrite);

      await Task.Run(() => {
        cancellationToken.ThrowIfCancellationRequested();
        IFormatter serializer = new BinaryFormatter();
        serializer.Serialize(outputStream, message);
        outputStream.Flush();
      }, cancellationToken).ConfigureAwait(false);
    }
    public async Task<IMessage> ReceiveMessageAsync(CancellationToken cancellationToken = default) => await ReceiveMessageAsync<IMessage>(cancellationToken);
    public async Task<T> ReceiveMessageAsync<T>(CancellationToken cancellationToken = default) where T : IMessage {
      Guard.Operation((inputStream != null) && inputStream.CanRead);

      return await Task.Run<T>(() => {
        cancellationToken.ThrowIfCancellationRequested();
        IFormatter serializer = new BinaryFormatter();
        object message = serializer.Deserialize(inputStream);
        T t = (T)message;
        return t;
      }, cancellationToken).ConfigureAwait(false);
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

    #region Dispose
    private bool disposedValue;
    protected virtual void Dispose(bool disposing) {
      if (!disposedValue) {
        if (disposing) {
          Close();
        }
        disposedValue = true;
      }
    }
    void IDisposable.Dispose() {
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
    #endregion
  }
}
