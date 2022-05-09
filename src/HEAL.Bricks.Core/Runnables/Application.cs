#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  public abstract class Application : IApplication {
    public abstract string Name { get; }
    public virtual string Description => string.Empty;
    public abstract ApplicationKind Kind { get; }

    public abstract Task StartAsync(string[] args, CancellationToken cancellationToken);
  }
}
