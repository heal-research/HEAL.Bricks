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

namespace HEAL.Bricks {
  [Serializable]
  public abstract class ProcessRunner : Runner {
    [NonSerialized]
    protected ProcessStartInfo processStartInfo = new ProcessStartInfo();
    [NonSerialized]
    protected Process process;

    protected ProcessRunner(IProcessRunnerStartInfo processRunnerStartInfo) {
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

    protected sealed override void InitializeHostStreams() {
      process = new Process {
        StartInfo = processStartInfo,
        EnableRaisingEvents = true
      };
      process.Start();
      outputStream = process.StandardInput.BaseStream;
      inputStream = process.StandardOutput.BaseStream;
    }
    protected sealed override void InitializeClientStreams() {
      inputStream = Console.OpenStandardInput();
      outputStream = Console.OpenStandardOutput();
    }
    protected sealed override Task<bool> RegisterCancellation(CancellationToken cancellationToken) {
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
          SendMessageAsync(new CancelRunnerMessage()).Wait();
          process.WaitForExit();
        }
        tcs.SetResult(cancellationToken.IsCancellationRequested);
      });
      return tcs.Task;
    }
  }
}
