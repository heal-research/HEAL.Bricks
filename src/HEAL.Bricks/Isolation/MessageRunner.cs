#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  [Serializable]
  public abstract class MessageRunner : ProcessRunner {
    protected MessageRunner(IProcessRunnerStartInfo processRunnerStartInfo) : base(processRunnerStartInfo) { }

    protected sealed override void ExecuteOnClient() {
      while (true) {
        IRunnerMessage message = ReceiveMessage();
        switch (message) {
          case CancelRunnerMessage _:
            return;
          default:
            ProcessRunnerMessageOnClient(message);
            break;
        }
      }
    }

    protected sealed override Task ExecuteOnHostAsync(CancellationToken cancellationToken) {
      while (!cancellationToken.IsCancellationRequested) {
        IRunnerMessage message = ReceiveMessage();
        ProcessRunnerMessageOnHost(message);
      }
      return Task.CompletedTask;
    }

    protected abstract void ProcessRunnerMessageOnClient(IRunnerMessage message);
    protected abstract void ProcessRunnerMessageOnHost(IRunnerMessage message);
  }
}
