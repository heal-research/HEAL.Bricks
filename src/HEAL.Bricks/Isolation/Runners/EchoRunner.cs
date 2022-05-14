#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  [Serializable]
  public sealed class EchoRunner : MessageRunner {
    [NonSerialized]
    private readonly BlockingCollection<string> responses = new();

    protected override async Task ProcessRunnerMessageOnClientAsync(IMessage message, IChannel channel, CancellationToken cancellationToken) {
      switch (message) {
        case CancelMessage _:
          break;
        case TextMessage textMessage:
          await channel.SendMessageAsync(new TextMessage("ECHO: " + textMessage.Data), cancellationToken);
          break;
        default:
          await channel.SendMessageAsync(new ExceptionMessage(new InvalidOperationException($"Cannot process message {message.GetType().Name}.")), cancellationToken);
          break;
      }
    }

    protected override Task ProcessRunnerMessageOnHostAsync(IMessage message, IChannel channel, CancellationToken cancellationToken) {
      switch (message) {
        case RunnerStoppedMessage _:
          responses.CompleteAdding();
          break;
        case TextMessage textMessage:
          responses.Add(textMessage.Data, cancellationToken);
          break;
        case ExceptionMessage exceptionMessage:
          throw exceptionMessage.Data;
        default:
          throw new InvalidOperationException($"Cannot process message {message.GetType().Name}.");
      }
      return Task.CompletedTask;
    }

    public async Task SendAsync(string text, IChannel channel, CancellationToken cancellationToken = default) {
      try {
        await channel.SendMessageAsync(new TextMessage(text), cancellationToken);
      }
      catch (OperationCanceledException) { }
    }
    public async Task<string?> ReceiveAsync(CancellationToken cancellationToken = default) {
      try {
        return await Task.Run<string>(() => {
          return responses.Take(cancellationToken);
        }, cancellationToken);
      }
      catch (OperationCanceledException) { }
      return null;
    }

    public async Task SendCancel(IChannel channel, CancellationToken cancellationToken = default) {
      try {
        await channel.SendMessageAsync(new CancelMessage(), cancellationToken);
      }
      catch (OperationCanceledException) { }
    }
  }
}
