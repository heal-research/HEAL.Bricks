#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  public interface IRunnerHost {
    RunnerStatus State { get; }

    bool QuietMode { get; set; }

    void Run(IRunner runner);
    Task RunAsync(IRunner runner, CancellationToken? token = null);

    void Send(RunnerMessage runnerMessage);
    RunnerMessage Receive();
  }
}
