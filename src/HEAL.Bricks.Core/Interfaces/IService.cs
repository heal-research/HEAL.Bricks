#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  public interface IService : IRunnable {
    string Name { get; }
    string Description { get; }

    Task StartAsync(string[] args, CancellationToken cancellationToken);
    Task StopAsync(string[] args, CancellationToken cancellationToken);
  }
}
