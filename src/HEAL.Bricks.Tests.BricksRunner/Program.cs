#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.Diagnostics;

namespace HEAL.Bricks.Tests.BricksRunner {
  class Program {
    static async Task Main(string[] args) {
      if (args.Any(x => x == "--Debug")) {
        Debugger.Launch();
      }

      if (args.Any(x => x == "--TestChannel")) {
        using ProcessChannel channel = ProcessChannel.CreateFromCLIArguments(args) ?? throw new ArgumentException("Cannot retrieve channel from CLI arguments.", nameof(args));
        IMessage message = await channel.ReceiveMessageAsync();
        while (message.Command != Message.Commands.Terminate) {
          await channel.SendMessageAsync(message);
          message = await channel.ReceiveMessageAsync();
        }
      } else if (args.Any(x => x == "--TestMessageHandler")) {
        IChannel channel = ProcessChannel.CreateFromCLIArguments(args) ?? throw new ArgumentException("Cannot retrieve channel from CLI arguments.", nameof(args));
        await MessageHandler.Factory.ClientMessageHandler().ReceiveMessagesAsync(channel);
      } else {
        throw new ArgumentException("Cannot retrieve kind of test from CLI arguments.", nameof(args));
      }
    }
  }
}
