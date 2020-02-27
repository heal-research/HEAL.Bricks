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
    protected ProcessStartInfo processStartInfo = new ProcessStartInfo();
    [NonSerialized]
    protected Process process;
    [NonSerialized]
    protected Stream inputStream, outputStream;

    public RunnerStatus Status { get; private set; } = RunnerStatus.Created;

    protected ProcessRunner(ProcessRunnerStartInfo processRunnerStartInfo) {
      if (processRunnerStartInfo == null) throw new ArgumentNullException(nameof(processRunnerStartInfo));

      processStartInfo = new ProcessStartInfo {
        FileName = processRunnerStartInfo.ProgramPath,
        Arguments = processRunnerStartInfo.Arguments ?? "",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardInput = true,
        CreateNoWindow = false,
        UserName = processRunnerStartInfo.UserName ?? "", // to use built-in Windows system accounts (e.g., LocalService, LocalSystem, ...) the process has to be run as service
        Domain = processRunnerStartInfo.UserDomain ?? "",
        PasswordInClearText = processRunnerStartInfo.UserPassword ?? "",
        WorkingDirectory = Path.GetDirectoryName(processRunnerStartInfo.ProgramPath)
      };
    }

    public void Run() => RunAsync().Wait();
    public async Task RunAsync(CancellationToken cancellationToken = default) {
      if (Status != RunnerStatus.Created) throw new InvalidOperationException($"Runner status is not \"{nameof(RunnerStatus.Created)}\".");

      Status = RunnerStatus.Starting;
      process = new Process {
        StartInfo = processStartInfo,
        EnableRaisingEvents = true
      };
      process.Start();
      outputStream = process.StandardInput.BaseStream;
      inputStream = process.StandardOutput.BaseStream;

      // registers a task for cancellation (prevents polling in a loop)
      Task<bool> task = RegisterCancellation(process, cancellationToken);

      RunnerMessage.WriteToStream(new StartRunnerMessage(this), outputStream);
      RunnerMessage.ReadFromStream<RunnerStartedMessage>(inputStream);
      Status = RunnerStatus.Running;

      if (await task) Status = RunnerStatus.Cancelled;
      else Status = RunnerStatus.Stopped;
    }

    public void Execute() {
      try {
        if (Status != RunnerStatus.Starting) throw new InvalidOperationException($"Runner status is not \"{nameof(RunnerStatus.Starting)}\".");

        inputStream = Console.OpenStandardInput();
        outputStream = Console.OpenStandardOutput();

        Status = RunnerStatus.Running;
        SendMessage(new RunnerStartedMessage());

        Process();
      }
      catch (Exception ex) {
        SendException(ex);
      }
    }

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

    private Task<bool> RegisterCancellation(Process process, CancellationToken cancellationToken) {
      // Create a new LinkedTokenSource and a TaskCompletionSource. 
      // When the specified token is cancelled, a cancel requests is sent to the started process. 
      // Then the main process waits for the started process to exit and sets a result of the TaskCompletionSource.
      // When the started process is finished normally (i.e., no cancellation), the linked token is cancelled and
      // the result is set (true if started process was cancelled, otherwise false). 

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

  #region ProcessRunnerStartInfo
  public class ProcessRunnerStartInfo {
    private string programPath;
    public string ProgramPath { get => programPath; set => SetProgramPath(value); }
    public string Program { get => Path.GetFileName(programPath); }
    public string Arguments { get; set; }
    public string UserName { get; set; }
    public string UserDomain { get; set; }
    public string UserPassword { get; set; }

    public ProcessRunnerStartInfo() {
      ProgramPath = "HEAL.Bricks.Runner.exe";
      Arguments = "";
    }
    public ProcessRunnerStartInfo(string program, string arguments = null) {
      ProgramPath = program;
      Arguments = arguments;
    }
    public ProcessRunnerStartInfo(string program, string arguments = null, string userName = null, string userDomain = null, string userPassword = null) : this(program, arguments) {
      UserName = userName;
      UserDomain = userDomain;
      UserPassword = userPassword;
    }

    private void SetProgramPath(string path) {
      if (path == null) throw new ArgumentNullException(nameof(ProgramPath));
      if (path == "") throw new ArgumentException($"{nameof(ProgramPath)} is empty", nameof(ProgramPath));

      programPath = path;
      if (!Path.IsPathRooted(programPath)) {
        string appDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        programPath = Path.Combine(appDir, programPath);
      }
    }
  }
  #endregion
}
