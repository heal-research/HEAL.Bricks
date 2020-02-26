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
  public interface IRunner {
    RunnerStatus Status { get; }

    void Run();
    Task RunAsync(CancellationToken cancellationToken = default);
    void Execute();
  }
}
