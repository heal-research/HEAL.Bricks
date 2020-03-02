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
    public static void ReceiveAndExecuteRunner(Stream stream) {
      IFormatter serializer = new BinaryFormatter();
      Runner runner = (serializer.Deserialize(stream) as StartRunnerMessage)?.Data;
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

      SendMessage(new StartRunnerMessage(this));
      ReceiveMessage<RunnerStartedMessage>();
      Status = RunnerStatus.Running;

      await ExecuteOnHostAsync(cancellationToken);

      if (await task) Status = RunnerStatus.Cancelled;
      else Status = RunnerStatus.Stopped;
    }

    private void Execute() {
      try {
        if (Status != RunnerStatus.Starting) throw new InvalidOperationException($"Runner status is not \"{nameof(RunnerStatus.Starting)}\".");

        // initialize input and output stream on client side
        InitializeClientStreams();

        Status = RunnerStatus.Running;
        SendMessage(new RunnerStartedMessage());
        ExecuteOnClient();
        Status = RunnerStatus.Stopped;
      }
      catch (Exception ex) {
        SendException(ex);
      }
    }
    protected abstract void InitializeHostStreams();
    protected abstract void InitializeClientStreams();
    protected abstract Task<bool> RegisterCancellation(CancellationToken cancellationToken);
    protected abstract Task ExecuteOnHostAsync(CancellationToken cancellationToken);
    protected abstract void ExecuteOnClient();

    protected void SendMessage(IRunnerMessage message) {
      IFormatter serializer = new BinaryFormatter();
      serializer.Serialize(outputStream, message);
      outputStream.Flush();
    }
    protected void SendException(Exception exception) {
      SendMessage(new RunnerExceptionMessage(exception));
    }
    protected T ReceiveMessage<T>() where T : IRunnerMessage {
      IFormatter serializer = new BinaryFormatter();
      return (T)serializer.Deserialize(inputStream);
    }
    protected Task<T> ReceiveMessageAsync<T>() where T : IRunnerMessage {
      IFormatter serializer = new BinaryFormatter();
      return Task.FromResult((T)serializer.Deserialize(inputStream));
    }
    protected IRunnerMessage ReceiveMessage() {
      return ReceiveMessage<IRunnerMessage>();
    }
  }
}
