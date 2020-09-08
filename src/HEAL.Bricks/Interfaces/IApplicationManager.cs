#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  public interface IApplicationManager {
    ISettings Settings { get; }
    IPackageManager PackageManager { get; }
    IEnumerable<ApplicationInfo> InstalledApplications { get; }

    Task RunAsync(ApplicationInfo application, ICommandLineArgument[] arguments = null, CancellationToken cancellationToken = default);

    IChannel CreateRunnerChannel();
  }
}