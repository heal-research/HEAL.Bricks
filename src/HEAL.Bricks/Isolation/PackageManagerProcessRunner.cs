#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  [Serializable]
  public abstract class PackageManagerProcessRunner : ProcessRunner {
    public ISettings Settings { get; }

    protected PackageManagerProcessRunner(ISettings settings, IProcessRunnerStartInfo startInfo) : base(startInfo) {
      Settings = settings;
    }

    protected sealed override async Task ExecuteOnClientAsync(CancellationToken cancellationToken) {
      IPackageManager pm = PackageManager.Create(Settings);
      if (pm.Status != PackageManagerStatus.OK) {
        throw new InvalidOperationException($"{nameof(PackageManager)}.{nameof(pm.Status)} is not {nameof(PackageManagerStatus.OK)}.");
      }

      pm.LoadPackageAssemblies();
      await ExecuteOnClientAsync(pm, cancellationToken);
    }
    protected abstract Task ExecuteOnClientAsync(IPackageManager packageManager, CancellationToken cancellationToken);
  }
}
