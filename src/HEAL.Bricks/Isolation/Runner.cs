#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  [Serializable]
  public abstract class Runner {
    public static string StartRunnerArgument => "--StartRunner";
    public static string CommunicationModeArgument => "--CommunicationMode";
    public static string InputConnectionArgument => "--InputConnection";
    public static string OutputConnectionArgument => "--OutputConnection";

    public static bool ParseArguments(string[] args, out CommunicationMode communicationMode, out string inputConnection, out string outputConnection) {
      bool startRunner = args.Any(x => x == StartRunnerArgument);
      if (startRunner) {
        try {
          communicationMode = (CommunicationMode)Enum.Parse(typeof(CommunicationMode), args.Where(x => x.Contains(CommunicationModeArgument)).Select(x => x.Split('=')[1]).SingleOrDefault() ?? string.Empty);
        }
        catch (Exception) { communicationMode = CommunicationMode.AnonymousPipes; }
        try {
          inputConnection = args.Where(x => x.Contains(InputConnectionArgument)).Select(x => x.Split('=')[1]).SingleOrDefault() ?? string.Empty;
        }
        catch (Exception) { inputConnection = string.Empty; }
        try {
          outputConnection = args.Where(x => x.Contains(OutputConnectionArgument)).Select(x => x.Split('=')[1]).SingleOrDefault() ?? string.Empty;
        }
        catch (Exception) { outputConnection = string.Empty; }
      } else {
        communicationMode = default;
        inputConnection = null;
        outputConnection = null;
      }
      return startRunner;
    }
    public static async Task ReceiveAndExecuteRunnerAsync(CommunicationMode communicationMode, string inputConnection, string outputConnection, CancellationToken cancellationToken = default) {
      Stream stream = null;
      switch (communicationMode) {
        case CommunicationMode.StdInOut:
          stream = Console.OpenStandardInput();
          break;
        case CommunicationMode.AnonymousPipes:
          stream = new AnonymousPipeClientStream(PipeDirection.In, inputConnection);
          break;
      }

      // handshake at runner startup has to be done synchronously
      IRunnerMessage runnerMessage = ReceiveMessageAsync(stream, cancellationToken).Result;
      Runner runner = (runnerMessage as StartRunnerMessage)?.Data;
      if (runner == null) throw new InvalidOperationException("Cannot deserialize runner from stream.");
      await runner.ExecuteAsync(communicationMode, inputConnection, outputConnection, cancellationToken);
    }

    [NonSerialized]
    protected Stream inputStream, outputStream;

    public RunnerStatus Status { get; protected set; } = RunnerStatus.Created;

    // TODO: review use of synchronization contexts (i.e. await ...ConfigureAwait(true/false))

    public void Run() => RunAsync().Wait();
    public async Task RunAsync(CancellationToken cancellationToken = default) {
      try {
        if (Status != RunnerStatus.Created) throw new InvalidOperationException($"Runner status is not \"{nameof(RunnerStatus.Created)}\".");

        Status = RunnerStatus.Starting;
        StartCommunicationWithClient(cancellationToken, out Task<RunnerStatus> clientDone);

        // handshake at runner startup is done synchronously
        SendMessageAsync(new StartRunnerMessage(this), cancellationToken).Wait(cancellationToken);
        ReceiveMessageAsync<RunnerStartedMessage>(cancellationToken).Wait(cancellationToken);

        Status = RunnerStatus.Running;
        await ExecuteOnHostAsync(cancellationToken);
        Status = await clientDone;
      }
      catch (OperationCanceledException) {
        Status = RunnerStatus.Canceled;
      }
      catch (Exception ex) {
        Status = RunnerStatus.Faulted;
        throw ex;
      }
      finally {
        CloseCommunicationWithClient();
      }
    }

    private async Task ExecuteAsync(CommunicationMode communicationMode, string inputConnection, string outputConnection, CancellationToken cancellationToken) {
      try {
        Status = RunnerStatus.Starting;
        StartCommunicationWithHost(communicationMode, inputConnection, outputConnection, cancellationToken);

        // handshake at runner startup has to be done synchronously
        SendMessageAsync(new RunnerStartedMessage(), cancellationToken).Wait(cancellationToken);

        Status = RunnerStatus.Running;
        await ExecuteOnClientAsync(cancellationToken);
        Status = (cancellationToken.IsCancellationRequested ? RunnerStatus.Canceled : RunnerStatus.Stopped);
      }
      catch (OperationCanceledException) {
        Status = RunnerStatus.Canceled;
      }
      catch (Exception ex) {
        Status = RunnerStatus.Faulted;
        await SendExceptionAsync(ex, cancellationToken);
      }
      finally {
        CloseCommunicationWithHost(communicationMode);
      }
    }
    protected abstract void StartCommunicationWithClient(CancellationToken cancellationToken, out Task<RunnerStatus> clientDone);
    protected abstract void StartCommunicationWithHost(CommunicationMode communicationMode, string inputHandle, string outputHandle, CancellationToken cancellationToken);
    protected abstract Task ExecuteOnHostAsync(CancellationToken cancellationToken);
    protected abstract Task ExecuteOnClientAsync(CancellationToken cancellationToken);
    protected abstract void CloseCommunicationWithClient();
    protected abstract void CloseCommunicationWithHost(CommunicationMode communicationMode);

    protected async Task SendMessageAsync(IRunnerMessage message, CancellationToken cancellationToken) => await SendMessageAsync(message, outputStream, cancellationToken).ConfigureAwait(false);
    protected async Task SendExceptionAsync(Exception exception, CancellationToken cancellationToken) => await SendMessageAsync(new RunnerExceptionMessage(exception), cancellationToken).ConfigureAwait(false);
    protected async Task<IRunnerMessage> ReceiveMessageAsync(CancellationToken cancellationToken) => await ReceiveMessageAsync<IRunnerMessage>(cancellationToken).ConfigureAwait(false);
    protected async Task<T> ReceiveMessageAsync<T>(CancellationToken cancellationToken) where T : IRunnerMessage => await ReceiveMessageAsync<T>(inputStream, cancellationToken).ConfigureAwait(false);

    private static async Task SendMessageAsync(IRunnerMessage message, Stream stream, CancellationToken cancellationToken) {
      await Task.Run(() => {
        IFormatter serializer = new BinaryFormatter();
        serializer.Serialize(stream, message);
        stream.Flush();
        cancellationToken.ThrowIfCancellationRequested();
      }, cancellationToken).ConfigureAwait(false);
    }
    private static async Task<IRunnerMessage> ReceiveMessageAsync(Stream stream, CancellationToken cancellationToken) => await ReceiveMessageAsync<IRunnerMessage>(stream, cancellationToken);
    private static async Task<T> ReceiveMessageAsync<T>(Stream stream, CancellationToken cancellationToken) where T : IRunnerMessage {
      return await Task.Run<T>(() => {
        IFormatter serializer = new BinaryFormatter();
        object message = null;
        try {
          message = serializer.Deserialize(stream);
        }
        catch (Exception) { }
        T t = (T)message;
        cancellationToken.ThrowIfCancellationRequested();
        return t;
      }, cancellationToken).ConfigureAwait(false);
    }
  }
}
