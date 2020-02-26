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
  public abstract class ProcessRunner : IRunner {
    [NonSerialized]
    private ProcessStartInfo processStartInfo;
    [NonSerialized]
    protected Stream inputStream, outputStream;

    public RunnerStatus Status { get; private set; } = RunnerStatus.Created;

    public ProcessRunner(string program, string arguments = null, string userName = null, string userDomain = null, string password = null) {
      string programPath = program ?? throw new ArgumentNullException(nameof(program));
      if (program == "") throw new ArgumentException($"{nameof(program)} is empty", nameof(program));

      if (!Path.IsPathRooted(programPath)) {
        string appDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        programPath = Path.Combine(appDir, programPath);
      }

      processStartInfo = new ProcessStartInfo {
        FileName = programPath,
        Arguments = arguments ?? "",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardInput = true,
        RedirectStandardError = true,
        CreateNoWindow = false,
        UserName = userName ?? "", // to use Local accounts (LocalService, LocalSystem, ...) the process has to run already as Service
        Domain = userDomain ?? "",
        PasswordInClearText = password ?? "",
        WorkingDirectory = Path.GetDirectoryName(programPath)
      };
    }

    public void Run() => RunAsync().Wait();
    public async Task RunAsync(CancellationToken cancellationToken = default) {
      if (Status != RunnerStatus.Created) throw new InvalidOperationException($"Runner status is not \"{nameof(RunnerStatus.Created)}\".");

      Status = RunnerStatus.Starting;
      Process process = new Process {
        StartInfo = processStartInfo,
        EnableRaisingEvents = true
      };
      process.Start();
      outputStream = process.StandardInput.BaseStream;
      inputStream = process.StandardOutput.BaseStream;

      // registers a task for cancellation, prevents the use of polling (while-loop)
      Task<bool> task = RegisterCancellation(process, cancellationToken);

      RunnerMessage.WriteToStream(new StartRunnerMessage(this), outputStream);
      RunnerMessage.ReadFromStream<RunnerStartedMessage>(inputStream);
      Status = RunnerStatus.Running;

      if (await task) Status = RunnerStatus.Cancelled;
      else Status = RunnerStatus.Stopped;
    }

    public virtual void Execute() {
      if (Status != RunnerStatus.Starting) throw new InvalidOperationException($"Runner status is not \"{nameof(RunnerStatus.Starting)}\".");

      inputStream = Console.OpenStandardInput();
      outputStream = Console.OpenStandardOutput();

      Status = RunnerStatus.Running;
      SendMessage(new RunnerStartedMessage());
    }

    public void SendMessage(IRunnerMessage message) {
      if (Status != RunnerStatus.Running) throw new InvalidOperationException($"Runner status is not \"{nameof(RunnerStatus.Running)}\".");
      RunnerMessage.WriteToStream(message, outputStream);
    }
    public T ReceiveMessage<T>() where T : IRunnerMessage {
      if (Status != RunnerStatus.Running) throw new InvalidOperationException($"Runner status is not \"{nameof(RunnerStatus.Running)}\".");
      return RunnerMessage.ReadFromStream<T>(inputStream);
    }
    public IRunnerMessage ReceiveMessage() {
      return ReceiveMessage<IRunnerMessage>();
    }

    /// <summary>
    /// Creates a new LinkedTokenSource and a TaskCompletionSource. 
    /// When the specified token gets cancelled, a cancel requests gets send to the childprocess. 
    /// Afterwards the main process waits for the exit of the child process and sets a result of the TaskCompletionSource.
    /// When the child process gets finished without requested cancellation, the linked token gets cancelled and a result set. 
    /// </summary>
    private Task<bool> RegisterCancellation(Process process, CancellationToken cancellationToken) {
      var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      var tcs = new TaskCompletionSource<bool>();

      process.Exited += (s, e) => cts.Cancel();
      cts.Token.Register(() => {
        if (!process.HasExited) {
          SendMessage(new CancelRunnerMessage());
          process.WaitForExit();
        }
        tcs.SetResult(cancellationToken.IsCancellationRequested);
      });
      return tcs.Task;
    }
  }
}
