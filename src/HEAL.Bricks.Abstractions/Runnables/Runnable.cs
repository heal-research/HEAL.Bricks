#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks {
  public abstract class Runnable : IRunnable {
    public abstract string Name { get; }
    public virtual string Description => string.Empty;
    public virtual string Version => GetType().Assembly.GetName().Version?.ToString() ?? string.Empty;
    public virtual string DockerImage => "mcr.microsoft.com/dotnet/runtime:latest";
    public virtual bool AutoStart => false;

    protected IChannel? Channel { get; } = null;

    protected Runnable() { }
    protected Runnable(IChannel channel) {
      Channel = channel ?? throw new ArgumentNullException(nameof(channel), "Channel must not be null.");
    }
  }
}
