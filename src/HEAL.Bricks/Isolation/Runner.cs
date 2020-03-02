#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  [Serializable]
  public abstract class Runner {
    public static string StartRunnerArgument => "--StartRunner";
    public static async Task ReceiveAndExecuteRunnerAsync(Stream stream, CancellationToken cancellationToken = default) {
      Runner runner = (await ReceiveMessageAsync(stream) as StartRunnerMessage)?.Data;
      if (runner == null) throw new InvalidOperationException("Cannot deserialize runner from stream.");
      await runner.ExecuteAsync(cancellationToken);
    }

    [NonSerialized]
    protected Stream inputStream, outputStream;

    public RunnerStatus Status { get; protected set; } = RunnerStatus.Created;

    public void Run() => RunAsync().Wait();
    public async Task RunAsync(CancellationToken cancellationToken = default) {
      if (Status != RunnerStatus.Created) throw new InvalidOperationException($"Runner status is not \"{nameof(RunnerStatus.Created)}\".");

      Status = RunnerStatus.Starting;

      // initialize input and output stream on host side
      InitializeHostStreams();
      // registers a task for cancellation (prevents polling in a loop)
      Task<bool> task = RegisterCancellation(cancellationToken);

      await SendMessageAsync(new StartRunnerMessage(this));
      await ReceiveMessageAsync<RunnerStartedMessage>();
      Status = RunnerStatus.Running;

      await ExecuteOnHostAsync(cancellationToken);

      if (await task) Status = RunnerStatus.Cancelled;
      else Status = RunnerStatus.Stopped;
    }

    private async Task ExecuteAsync(CancellationToken cancellationToken) {
      try {
        if (Status != RunnerStatus.Starting) throw new InvalidOperationException($"Runner status is not \"{nameof(RunnerStatus.Starting)}\".");

        // initialize input and output stream on client side
        InitializeClientStreams();

        Status = RunnerStatus.Running;
        await SendMessageAsync(new RunnerStartedMessage(), cancellationToken);
        await ExecuteOnClientAsync(cancellationToken);
        Status = RunnerStatus.Stopped;
      }
      catch (Exception ex) {
        await SendExceptionAsync(ex, cancellationToken);
      }
    }
    protected abstract void InitializeHostStreams();
    protected abstract void InitializeClientStreams();
    protected abstract Task<bool> RegisterCancellation(CancellationToken cancellationToken);
    protected abstract Task ExecuteOnHostAsync(CancellationToken cancellationToken);
    protected abstract Task ExecuteOnClientAsync(CancellationToken cancellationToken);

    protected async Task SendMessageAsync(IRunnerMessage message, CancellationToken cancellationToken = default) {
      await SendMessageAsync(message, outputStream, cancellationToken);
    }
    protected async Task SendExceptionAsync(Exception exception, CancellationToken cancellationToken = default) {
      await SendMessageAsync(new RunnerExceptionMessage(exception), cancellationToken);
    }
    protected async Task<T> ReceiveMessageAsync<T>(CancellationToken cancellationToken = default) where T : IRunnerMessage {
      return await ReceiveMessageAsync<T>(inputStream, cancellationToken);
    }
    protected async Task<IRunnerMessage> ReceiveMessageAsync(CancellationToken cancellationToken = default) {
      return await ReceiveMessageAsync<IRunnerMessage>(cancellationToken);
    }

    private static async Task SendMessageAsync(IRunnerMessage message, Stream stream, CancellationToken cancellationToken = default) {
      await Task.Run(() => {
        IFormatter serializer = new BinaryFormatter();
        serializer.Serialize(stream, message);
        stream.Flush();
      }, cancellationToken).ConfigureAwait(false);
    }
    private static async Task<T> ReceiveMessageAsync<T>(Stream stream, CancellationToken cancellationToken = default) where T : IRunnerMessage {
      return await Task.Run<T>(() => {
        IFormatter serializer = new BinaryFormatter();
        return (T)serializer.Deserialize(stream);
      }, cancellationToken).ConfigureAwait(false);
    }
    private static async Task<IRunnerMessage> ReceiveMessageAsync(Stream stream, CancellationToken cancellationToken = default) {
      return await ReceiveMessageAsync<IRunnerMessage>(stream, cancellationToken);
    }
  }
}
