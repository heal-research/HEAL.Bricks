#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HEAL.Attic;

namespace HEAL.Bricks {
  public enum RunnerState {
    Created,
    Cancelled,
    Starting,
    Running,
    Stopped
  }

  public abstract class RunnerHost : IRunnerHost {

    #region Vars
    protected Process process;
    private ProtoBufSerializer serializer = new ProtoBufSerializer();
    #endregion

    #region Properties
    /// <summary>
    /// Set this to true, if console output should be disabled.
    /// </summary>
    public bool QuietMode { get; set; }


    /// <summary>
    /// The programm, which should be used. For example 'docker'.
    /// </summary>
    protected string Program { get; private set; }
    /// <summary>
    /// The specific start arguments for the programm.
    /// </summary>
    protected string StartArgument { get; private set; }
    protected string UserName { get; private set; }
    protected string Password { get; private set; }
    protected string Domain { get; private set; }
    protected IRunner Runner { get; set; }

    public RunnerState State { get; protected set; } = RunnerState.Created;
    #endregion

    #region Constructors

    protected RunnerHost(string program, string startArgument, string userName, string password, string domain) {
      Program = program;
      StartArgument = startArgument + " --StartAsRunnerHost";
      UserName = userName;
      Password = password;
      Domain = domain;
    }
    #endregion

    public virtual void Run(IRunner runner) => RunAsync(runner, null).Wait();
    public virtual async Task RunAsync(IRunner runner, CancellationToken? token = null) {
      if (State != RunnerState.Created)
        throw new InvalidOperationException("Runner must be in state 'Created'.");
      State = RunnerState.Starting;
      Runner = runner;
      process = new Process {
        StartInfo = new ProcessStartInfo {
          FileName = Program,
          Arguments = StartArgument,
          UseShellExecute = false,
          RedirectStandardOutput = true,
          RedirectStandardInput = true,
          RedirectStandardError = true,
          CreateNoWindow = false,
          UserName = string.IsNullOrEmpty(UserName) ? null : UserName, // to use Local accounts (LocalService, LocalSystem, ...) the process has to run already as Service
          PasswordInClearText = string.IsNullOrEmpty(Password) ? null : Password,
          Domain = string.IsNullOrEmpty(Domain) ? null : Domain,
          WorkingDirectory = Directory.GetCurrentDirectory()
        },
        EnableRaisingEvents = true
      };
      process.Start();

      // registers a task for cancellation, prevents the use of polling (while-loop)
      var task = RegisterCancellation(token.HasValue ? token.Value : CancellationToken.None);

      // set runnerhost in runner 
      Runner r = Runner as Runner;
      if (r != null) r.Host = this;

      process.BeginOutputReadLine();
      process.BeginErrorReadLine();

      if (!QuietMode) {
        process.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
        process.ErrorDataReceived += (s, e) => Console.WriteLine(e.Data);
      }

      // write config to standardinput, runner listens on this and deserializes the config
      SendMessage(new TransportRunnerMessage(Runner));

      State = RunnerState.Running;
      if (await task) State = RunnerState.Cancelled;
      else State = RunnerState.Stopped;
    }


    public void Send(RunnerMessage runnerMessage) {
      if (State != RunnerState.Running) throw new InvalidOperationException("Runner must be in state 'Running'!");
      SendMessage(runnerMessage);
    }

    // because we need to transfer the runner with a TransportRunnerMessage in the starting state and the 
    // original send method should not be available until running state
    protected virtual void SendMessage(RunnerMessage runnerMessage) {
      runnerMessage.SendTime = DateTime.Now;
      byte[] bytes = serializer.Serialize(runnerMessage);
      byte[] size = BitConverter.GetBytes(bytes.Length);
      process.StandardInput.BaseStream.Write(size, 0, size.Length);
      process.StandardInput.BaseStream.Flush();
      process.StandardInput.BaseStream.Write(bytes, 0, bytes.Length);
      process.StandardInput.BaseStream.Flush();
    }

    /// <summary>
    /// Creates a new LinkedTokenSource and a TaskCompletionSource. 
    /// When the specified token gets cancelled, a cancel requests gets send to the childprocess. 
    /// Afterwards the main process waits for the exit of the child process and sets a result of the TaskCompletionSource.
    /// When the child process gets finished without requested cancellation, the linked token gets cancelled and a result set. 
    /// </summary>
    protected Task<bool> RegisterCancellation(CancellationToken token) {
      if (process != null && State == RunnerState.Starting) {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        var tcs = new TaskCompletionSource<bool>();

        process.Exited += (s, e) => cts.Cancel();
        cts.Token.Register(() => {
          if (!process.HasExited) {
            Runner.Cancel();
            process.WaitForExit();
          }
          tcs.SetResult(token.IsCancellationRequested);
        });
        return tcs.Task;
      }
      return null;
    }
  }
}
