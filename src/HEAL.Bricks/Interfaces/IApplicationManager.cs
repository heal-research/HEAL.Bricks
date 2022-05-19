﻿#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  public interface IApplicationManager {
    IPackageManager PackageManager { get; }
    IEnumerable<RunnableInfo> InstalledRunnables { get; }

    Task RunAsync(RunnableInfo runnable, string[]? args = null, CancellationToken cancellationToken = default);
    Task RunAutoStartAsync(string[]? args = null, CancellationToken cancellationToken = default);
    Task ReloadAsync(CancellationToken cancellationToken = default);

    IChannel CreateRunnerChannel(Isolation isolation, string dockerImage = "");
  }
}