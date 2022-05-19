#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.Threading.Tasks;
using System;
using System.Threading;
using System.IO;
using System.Reflection;

namespace HEAL.Bricks.Tests {
  public static class TestHelpers {
    public static string GetWorkingDir() {
      return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
    }

    public static IChannel CreateChannel(Type channelType, string programPath, string arguments, Func<IChannel, CancellationToken, Task>? clientCode) {
      IChannel? channel = null;
      if (typeof(ProcessChannel).IsAssignableFrom(channelType)) {
        channel = (IChannel?)Activator.CreateInstance(channelType, programPath, arguments);
      }
      else if (typeof(MemoryChannel).IsAssignableFrom(channelType)) {
        channel = (IChannel?)Activator.CreateInstance(channelType, new Action<MemoryChannel, CancellationToken>((channel, token) => clientCode?.Invoke(channel, token).Wait(token)));
      }
      else {
        channel = (IChannel?)Activator.CreateInstance(channelType);
      }
      return channel ?? throw new InvalidOperationException($"Cannot create channel of type {channelType.Name}");
    }

    public static async Task TestChannelAsync(IChannel channel, CancellationToken cancellationToken) {
      IMessage message = await channel.ReceiveMessageAsync(cancellationToken);
      while (message.Command != Message.Commands.Terminate && !cancellationToken.IsCancellationRequested) {
        await channel.SendMessageAsync(message, cancellationToken);
        message = await channel.ReceiveMessageAsync(cancellationToken);
      }
    }

    public static async Task TestMessageHandler(IChannel channel, CancellationToken cancellationToken) {
      await MessageHandler.Factory.ClientMessageHandler().ReceiveMessagesAsync(channel, cancellationToken);
    }
  }
}
