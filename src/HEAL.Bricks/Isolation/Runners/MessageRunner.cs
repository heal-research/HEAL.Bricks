﻿#region License Information
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
  public abstract class MessageRunner : Runner {
    protected sealed override async Task ExecuteOnClientAsync(IChannel channel, CancellationToken cancellationToken) {
      while (!cancellationToken.IsCancellationRequested) {
        IMessage message = await channel.ReceiveMessageAsync(cancellationToken);
        switch (message) {
          case null:
            return;
          case CancelRunnerMessage _:
            await ProcessRunnerMessageOnClientAsync(message, channel, cancellationToken);
            return;
          default:
            await ProcessRunnerMessageOnClientAsync(message, channel, cancellationToken);
            break;
        }
      }
    }

    protected sealed override async Task ExecuteOnHostAsync(IChannel channel, CancellationToken cancellationToken) {
      while (!cancellationToken.IsCancellationRequested) {
        IMessage message = await channel.ReceiveMessageAsync(cancellationToken);
        switch (message) {
          case null:
            return;
          case RunnerStoppedMessage _:
            await ProcessRunnerMessageOnHostAsync(message, channel, cancellationToken);
            return;
          default:
            await ProcessRunnerMessageOnHostAsync(message, channel, cancellationToken);
            break;
        }
      }
    }

    protected abstract Task ProcessRunnerMessageOnClientAsync(IMessage message, IChannel channel, CancellationToken cancellationToken);
    protected abstract Task ProcessRunnerMessageOnHostAsync(IMessage message, IChannel channel, CancellationToken cancellationToken);
  }
}