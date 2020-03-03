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

    protected sealed override async Task ExecuteOnClientAsync(CancellationToken cancellationToken) {
      while (!cancellationToken.IsCancellationRequested) {
        IRunnerMessage message = await ReceiveMessageAsync(cancellationToken);
        switch (message) {
          case null:
          case CancelRunnerMessage _:
            return;
          default:
            await ProcessRunnerMessageOnClientAsync(message, cancellationToken);
            break;
        }
      }
    }

    protected sealed override async Task ExecuteOnHostAsync(CancellationToken cancellationToken) {
      while (!cancellationToken.IsCancellationRequested) {
        IRunnerMessage message = await ReceiveMessageAsync(cancellationToken);
        switch (message) {
          case null:
          case RunnerStoppedMessage _:
            return;
          default:
            await ProcessRunnerMessageOnHostAsync(message, cancellationToken);
            break;
        }
      }
    }

    protected abstract Task ProcessRunnerMessageOnClientAsync(IRunnerMessage message, CancellationToken cancellationToken);
    protected abstract Task ProcessRunnerMessageOnHostAsync(IRunnerMessage message, CancellationToken cancellationToken);
  }
}
