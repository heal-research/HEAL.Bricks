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
  public sealed class EchoRunner : MessageRunner {
    public EchoRunner(IProcessRunnerStartInfo startInfo = null) : base(startInfo ?? new NetCoreEntryAssemblyStartInfo()) { }

    protected override async Task ProcessRunnerMessageOnClientAsync(IRunnerMessage message, CancellationToken cancellationToken) {
      switch (message) {
        case RunnerTextMessage textMessage:
          await SendMessageAsync(new RunnerTextMessage("ECHO: " + textMessage.Data), cancellationToken);
          break;
        default:
          await SendExceptionAsync(new InvalidOperationException($"Cannot process message {message.GetType().Name}."), cancellationToken);
          break;
      }
    }

    protected override Task ProcessRunnerMessageOnHostAsync(IRunnerMessage message, CancellationToken cancellationToken) {
      throw new NotImplementedException();
    }

    public async Task SendAsync(string text, CancellationToken cancellationToken = default) {
      await SendMessageAsync(new RunnerTextMessage(text), cancellationToken);
    }
    public async Task<string> ReceiveAsync(CancellationToken cancellationToken = default) {
      return (await ReceiveMessageAsync<RunnerTextMessage>(cancellationToken)).Data;
    }
  }
}
