#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;

namespace HEAL.Bricks {
  [Serializable]
  public sealed class EchoRunner : MessageRunner {
    public EchoRunner(IProcessRunnerStartInfo startInfo = null) : base(startInfo ?? new NetCoreEntryAssemblyStartInfo()) { }

    protected override void ProcessRunnerMessage(IRunnerMessage message) {
      switch (message) {
        case RunnerTextMessage textMessage:
          SendMessage(new RunnerTextMessage("ECHO: " + textMessage.Data));
          break;
        default:
          SendException(new InvalidOperationException($"Cannot process message {message.GetType().Name}."));
          break;
      }
    }
  }
}
