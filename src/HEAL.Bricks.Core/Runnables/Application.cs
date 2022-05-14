﻿#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks {
  public abstract class Application : Runnable, IApplication {
    public abstract ApplicationKind Kind { get; }

    public abstract Task StartAsync(string[] args, CancellationToken cancellationToken);
  }
}
