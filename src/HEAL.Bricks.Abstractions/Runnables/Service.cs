#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks {
  public abstract class Service : Runnable, IService {
    public override bool AutoStart => true;

    protected Service() : base() { }
    protected Service(IChannel channel) : base(channel) { }

    public abstract Task StartAsync(string[] args, CancellationToken cancellationToken);
    public abstract Task StopAsync(CancellationToken cancellationToken);

    public abstract Task<IMessage> ProcessRequestAsync(IMessage message, CancellationToken cancellationToken);
  }
}
