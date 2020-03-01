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
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  [Serializable]
  public abstract class Runner : IRunner {
    public static void ReceiveAndExecuteRunner(Stream stream) {
      IRunner runner = (RunnerMessage.ReadFromStream(stream) as StartRunnerMessage)?.Data;
      if (runner == null) throw new InvalidOperationException("Cannot deserialize runner from stream.");
      runner.Execute();
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

      RunnerMessage.WriteToStream(new StartRunnerMessage(this), outputStream);
      RunnerMessage.ReadFromStream<RunnerStartedMessage>(inputStream);
      Status = RunnerStatus.Running;

      if (await task) Status = RunnerStatus.Cancelled;
      else Status = RunnerStatus.Stopped;
    }

    public void Execute() {
      try {
        if (Status != RunnerStatus.Starting) throw new InvalidOperationException($"Runner status is not \"{nameof(RunnerStatus.Starting)}\".");

        // initialize input and output stream on client side
        InitializeClientStreams();

        Status = RunnerStatus.Running;
        SendMessage(new RunnerStartedMessage());

        Process();
      }
      catch (Exception ex) {
        SendException(ex);
      }
    }
    protected abstract void InitializeHostStreams();
    protected abstract void InitializeClientStreams();
    protected abstract Task<bool> RegisterCancellation(CancellationToken cancellationToken);
    protected abstract void Process();

    public void SendMessage(IRunnerMessage message) {
      if (Status != RunnerStatus.Running) throw new InvalidOperationException($"Runner status is not \"{nameof(RunnerStatus.Running)}\".");
      RunnerMessage.WriteToStream(message, outputStream);
    }
    public void SendException(Exception exception) {
      SendMessage(new RunnerExceptionMessage(exception));
    }
    public T ReceiveMessage<T>() where T : IRunnerMessage {
//      if (Status != RunnerStatus.Running) throw new InvalidOperationException($"Runner status is not \"{nameof(RunnerStatus.Running)}\".");
      return RunnerMessage.ReadFromStream<T>(inputStream);
    }
    public IRunnerMessage ReceiveMessage() {
      return ReceiveMessage<IRunnerMessage>();
    }
  }
}
