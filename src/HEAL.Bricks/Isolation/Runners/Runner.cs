#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  [Serializable]
  public abstract class Runner {

    // TODO: review use of synchronization contexts (i.e. await ...ConfigureAwait(true/false))

    public static async Task ReceiveAndExecuteAsync(IChannel channel, CancellationToken cancellationToken = default) {
      Guard.Argument(channel, nameof(channel)).NotNull();

      try {
        var message = await channel.ReceiveMessageAsync<StartRunnerMessage>(cancellationToken);
        var runner = message.Data;
        channel.SendMessageAsync(new RunnerStartedMessage(), cancellationToken).Wait(cancellationToken);
        await runner.ExecuteOnClientAsync(channel, cancellationToken);
      }
      catch (Exception e) {
        channel.SendMessageAsync(new ExceptionMessage(e), cancellationToken).Wait(cancellationToken);
      }
    }

    public async Task RunAsync(IChannel channel, CancellationToken cancellationToken = default) {
      Guard.Argument(channel, nameof(channel)).NotNull();


      try {
        channel.Open();
        channel.SendMessageAsync(new StartRunnerMessage(this), cancellationToken).Wait(cancellationToken);
        channel.ReceiveMessageAsync<RunnerStartedMessage>(cancellationToken).Wait(cancellationToken);
        cancellationToken.Register(() => channel.Close());
        await ExecuteOnHostAsync(channel, cancellationToken);
      }
      finally {
        channel.Close();
      }
    }

    protected abstract Task ExecuteOnHostAsync(IChannel channel, CancellationToken cancellationToken);
    protected abstract Task ExecuteOnClientAsync(IChannel channel, CancellationToken cancellationToken);
  }
}
