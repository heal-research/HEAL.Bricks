#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks {
  public interface IService : IRunnable {
    Task StartAsync(string[] args, CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);

    Task<IMessage> ProcessRequestAsync(IMessage message, CancellationToken cancellationToken);
  }
}
