#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  [Serializable]
  public abstract class Runner {
    public static string StartRunnerArgument => "--StartRunner";
    public static async Task ReceiveAndExecuteRunnerAsync(Stream stream, CancellationToken cancellationToken = default) {
      Task<IRunnerMessage> task = ReceiveMessageAsync(stream, cancellationToken);
      // handshake at runner startup has to be done synchronously
      task.Wait(cancellationToken);
      Runner runner = (task.Result as StartRunnerMessage)?.Data;
      if (runner == null) throw new InvalidOperationException("Cannot deserialize runner from stream.");
      await runner.ExecuteAsync(cancellationToken);
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
        InitializeHostStreams();  // initialize input and output stream on host side
        Task<bool> task = RegisterCancellation(cancellationToken);  // registers a task for cancellation (prevents polling in a loop)

        // handshake at runner startup has to be done synchronously
        SendMessageAsync(new StartRunnerMessage(this), cancellationToken).Wait(cancellationToken);
        ReceiveMessageAsync<RunnerStartedMessage>(cancellationToken).Wait(cancellationToken);
        Status = RunnerStatus.Running;

        await ExecuteOnHostAsync(cancellationToken);

        if (await task) Status = RunnerStatus.Cancelled;
        else Status = RunnerStatus.Stopped;
      }
      catch (OperationCanceledException) {
        Status = RunnerStatus.Cancelled;
      }
      catch (Exception ex) {
        Status = RunnerStatus.Faulted;
        throw ex;
      }
    }

    private async Task ExecuteAsync(CancellationToken cancellationToken) {
      try {
        Status = RunnerStatus.Running;
        InitializeClientStreams();  // initialize input and output stream on client side

        // handshake at runner startup has to be done synchronously
        SendMessageAsync(new RunnerStartedMessage(), cancellationToken).Wait(cancellationToken);
        await ExecuteOnClientAsync(cancellationToken);
        Status = (cancellationToken.IsCancellationRequested ? RunnerStatus.Cancelled : RunnerStatus.Stopped);
      }
      catch (OperationCanceledException) {
        Status = RunnerStatus.Cancelled;
      }
      catch (Exception ex) {
        Status = RunnerStatus.Faulted;
        await SendExceptionAsync(ex, cancellationToken);
      }
    }
    protected abstract void InitializeHostStreams();
    protected abstract void InitializeClientStreams();
    protected abstract Task<bool> RegisterCancellation(CancellationToken cancellationToken);
    protected abstract Task ExecuteOnHostAsync(CancellationToken cancellationToken);
    protected abstract Task ExecuteOnClientAsync(CancellationToken cancellationToken);

    protected async Task SendMessageAsync(IRunnerMessage message, CancellationToken cancellationToken) => await SendMessageAsync(message, outputStream, cancellationToken);
    protected async Task SendExceptionAsync(Exception exception, CancellationToken cancellationToken) => await SendMessageAsync(new RunnerExceptionMessage(exception), cancellationToken);
    protected async Task<IRunnerMessage> ReceiveMessageAsync(CancellationToken cancellationToken) => await ReceiveMessageAsync<IRunnerMessage>(cancellationToken);
    protected async Task<T> ReceiveMessageAsync<T>(CancellationToken cancellationToken) where T : IRunnerMessage => await ReceiveMessageAsync<T>(inputStream, cancellationToken);

    private static async Task SendMessageAsync(IRunnerMessage message, Stream stream, CancellationToken cancellationToken) {
      await Task.Run(() => {
        IFormatter serializer = new BinaryFormatter();
        serializer.Serialize(stream, message);
        stream.Flush();
        cancellationToken.ThrowIfCancellationRequested();
      }, cancellationToken);
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
      }, cancellationToken);
    }
  }
}
