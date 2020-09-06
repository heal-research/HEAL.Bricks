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

namespace HEAL.Bricks.XTests {
  public static class TestHelpers {
    public static string GetWorkingDir() {
      return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }

    public static IChannel CreateChannel(Type channelType, string programPath, string arguments, Func<IChannel, CancellationToken, Task> clientCode) {
      if (typeof(ProcessChannel).IsAssignableFrom(channelType)) {
        return Activator.CreateInstance(channelType, programPath, arguments) as IChannel;
      }
      else if (typeof(MemoryChannel).IsAssignableFrom(channelType)) {
        return Activator.CreateInstance(channelType, new Action<MemoryChannel, CancellationToken>((channel, token) => clientCode(channel, token).Wait())) as MemoryChannel;
      }
      else {
        return Activator.CreateInstance(channelType) as IChannel;
      }
    }
    public static async Task TestChannelAsync(IChannel channel, CancellationToken cancellationToken) {
      IMessage message = await channel.ReceiveMessageAsync(cancellationToken);
      while (!(message is CancelRunnerMessage) && !cancellationToken.IsCancellationRequested) {
        await channel.SendMessageAsync(message, cancellationToken);
        message = await channel.ReceiveMessageAsync(cancellationToken);
      }
    }
    public static async Task TestRunnerAsync(IChannel channel, CancellationToken cancellationToken) {
      await Runner.ReceiveAndExecuteAsync(channel, cancellationToken);
    }
  }
}
