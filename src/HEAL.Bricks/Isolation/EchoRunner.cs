﻿#region License Information
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
    private readonly BlockingCollection<string> responses = new BlockingCollection<string>();

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
      switch (message) {
        case RunnerStoppedMessage _:
          responses.CompleteAdding();
          break;
        case RunnerTextMessage textMessage:
          responses.Add(textMessage.Data);
          break;
        case RunnerExceptionMessage exceptionMessage:
          throw exceptionMessage.Data;
        default:
          throw new InvalidOperationException($"Cannot process message {message.GetType().Name}.");
      }
      return Task.CompletedTask;
    }

    public async Task SendAsync(string text, CancellationToken cancellationToken = default) {
      try {
        await SendMessageAsync(new RunnerTextMessage(text), cancellationToken);
      }
      catch (OperationCanceledException) { }
    }
    public async Task<string> ReceiveAsync(CancellationToken cancellationToken = default) {
      try {
        return await Task.Run<string>(() => {
          return responses.Take(cancellationToken);
        }, cancellationToken);
      }
      catch (OperationCanceledException) { }
      return null;
    }
  }
}
