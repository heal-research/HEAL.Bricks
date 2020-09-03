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
  public class DemoRunner : Runner {
    protected override async Task ExecuteOnHostAsync(IChannel channel, CancellationToken cancellationToken) {
      for (int i = 0; i < 5; i++) {
        cancellationToken.ThrowIfCancellationRequested();
        await channel.SendMessageAsync(new TextMessage($"message {i}"), cancellationToken);
        await Task.Delay(1000);
        var msg = await channel.ReceiveMessageAsync<TextMessage>(cancellationToken);
        Console.WriteLine(msg.Data);
      }
    }

    protected override async Task ExecuteOnClientAsync(IChannel channel, CancellationToken cancellationToken) {
      for (int i = 0; i < 5; i++) {
        cancellationToken.ThrowIfCancellationRequested();
        var msg = await channel.ReceiveMessageAsync<TextMessage>(cancellationToken);
        await Task.Delay(1000);
        await channel.SendMessageAsync(new TextMessage("ECHO: " + msg.Data), cancellationToken);
      }
    }
  }
}
