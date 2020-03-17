#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  [Serializable]
  public abstract class ProcessRunner : Runner {
    [NonSerialized]
    protected Process process;
    [NonSerialized]
    private readonly IProcessRunnerStartInfo processRunnerStartInfo;

    public IProcessRunnerStartInfo ProcessRunnerStartInfo => processRunnerStartInfo;

    protected ProcessRunner(IProcessRunnerStartInfo processRunnerStartInfo) {
      this.processRunnerStartInfo = processRunnerStartInfo ?? throw new ArgumentNullException(nameof(processRunnerStartInfo));
    }

    protected sealed override void StartCommunicationWithClient(CancellationToken cancellationToken, out Task<RunnerStatus> clientDone) {
      AnonymousPipeServerStream anonymousPipeOutputStream = null, anonymousPipeInputStream = null;

      string arguments = " " + StartRunnerArgument + " " + CommunicationModeArgument + "=" + ProcessRunnerStartInfo.CommunicationMode.ToString();
      if (ProcessRunnerStartInfo.CommunicationMode == CommunicationMode.AnonymousPipes) {
        anonymousPipeOutputStream = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
        arguments += " " + InputConnectionArgument + "=" + anonymousPipeOutputStream.GetClientHandleAsString();
        anonymousPipeInputStream = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
        arguments += " " + OutputConnectionArgument + "=" + anonymousPipeInputStream.GetClientHandleAsString();
      }

      process = new Process {
        StartInfo = new ProcessStartInfo {
          FileName = ProcessRunnerStartInfo.ProgramPath,
          Arguments = (ProcessRunnerStartInfo.Arguments ?? "") + arguments,
          UseShellExecute = false,
          RedirectStandardOutput = ProcessRunnerStartInfo.CommunicationMode == CommunicationMode.StdInOut ? true : false,
          RedirectStandardInput = ProcessRunnerStartInfo.CommunicationMode == CommunicationMode.StdInOut ? true : false,
          CreateNoWindow = true,
          ErrorDialog = true,
          UserName = ProcessRunnerStartInfo.UserName ?? "", // to use built-in Windows system accounts (e.g., LocalService, LocalSystem, ...) the process has to be run as service
          Domain = ProcessRunnerStartInfo.UserDomain ?? "",
          PasswordInClearText = ProcessRunnerStartInfo.UserPassword ?? "",
          WorkingDirectory = Path.GetDirectoryName(ProcessRunnerStartInfo.ProgramPath)
        },
        EnableRaisingEvents = true
      };
      process.Start();
      clientDone = RegisterClientDoneTask(cancellationToken);

      switch (ProcessRunnerStartInfo.CommunicationMode) {
        case CommunicationMode.StdInOut:
          outputStream = process.StandardInput.BaseStream;
          inputStream = process.StandardOutput.BaseStream;
          break;
        case CommunicationMode.AnonymousPipes:
          outputStream = anonymousPipeOutputStream;
          inputStream = anonymousPipeInputStream;
          anonymousPipeOutputStream.DisposeLocalCopyOfClientHandle();
          anonymousPipeInputStream.DisposeLocalCopyOfClientHandle();
          break;
      }
    }
    protected override void CloseCommunicationWithClient() {
      if (outputStream != null) outputStream.Close();
      if (inputStream != null) inputStream.Close();
    }

    protected sealed override void StartCommunicationWithHost(CommunicationMode communicationMode, string inputConnection, string outputConnection, CancellationToken cancellationToken) {
      switch (communicationMode) {
        case CommunicationMode.StdInOut:
          inputStream = Console.OpenStandardInput();
          outputStream = Console.OpenStandardOutput();
          break;
        case CommunicationMode.AnonymousPipes:
          inputStream = new AnonymousPipeClientStream(PipeDirection.In, inputConnection);
          outputStream = new AnonymousPipeClientStream(PipeDirection.Out, outputConnection);
          break;
      }
    }
    protected override void CloseCommunicationWithHost(CommunicationMode communicationMode) {
      if (outputStream != null) outputStream.Close();
      if (inputStream != null) inputStream.Close();
    }

    private Task<RunnerStatus> RegisterClientDoneTask(CancellationToken cancellationToken) {
      // Create a new LinkedTokenSource and a TaskCompletionSource. 
      // When the specified token is canceled, a cancel requests is sent to the started process. 
      // Then the main process waits for the started process to exit and sets a result of the TaskCompletionSource.
      // When the started process is finished normally (i.e., no cancellation), the linked token is canceled.
      // The result reflects whether the client was canceled or terminated normally.

      var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      var tcs = new TaskCompletionSource<RunnerStatus>();

      process.Exited += (s, e) => cts.Cancel();
      cts.Token.Register(() => {
        if (!process.HasExited) {
          SendMessageAsync(new CancelRunnerMessage(), default).Wait();
          process.WaitForExit();
          CloseCommunicationWithClient();
        }
        tcs.SetResult(cancellationToken.IsCancellationRequested ? RunnerStatus.Canceled : RunnerStatus.Stopped);
      });
      return tcs.Task;
    }
  }
}
