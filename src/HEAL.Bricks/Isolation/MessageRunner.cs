#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;

namespace HEAL.Bricks {
  [Serializable]
  public abstract class MessageRunner : ProcessRunner {
    protected MessageRunner(ProcessRunnerStartInfo processRunnerStartInfo) : base(processRunnerStartInfo) { }

    protected sealed override void Process() {
      while (true) {
        IRunnerMessage message = ReceiveMessage();
        switch (message) {
          case CancelRunnerMessage _:
            return;
          default:
            ProcessRunnerMessage(message);
            break;
        }
      }
    }

    protected abstract void ProcessRunnerMessage(IRunnerMessage message);
  }
}
