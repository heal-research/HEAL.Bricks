#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  [Serializable]
  public class EchoRunner : ProcessRunner {
    public EchoRunner(string program, string arguments = null, string userName = null, string userDomain = null, string password = null) : base(program, arguments, userName, userDomain, password) {
    }

    public override void Execute() {
      base.Execute();

      while (true) {
        IRunnerMessage message = ReceiveMessage();
        switch (message) {
          case CancelRunnerMessage _:
            return;
          case RunnerTextMessage trm:
            SendMessage(new RunnerTextMessage("ECHO: " + trm.Data));
            break;
        }
      }
    }
  }
}
