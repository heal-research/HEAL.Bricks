#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  [Serializable]
  public abstract class PackageLoaderRunner : Runner {
    private readonly PackageLoadInfo[] packageLoadInfos;
    public IEnumerable<PackageLoadInfo> PackageLoadInfos => packageLoadInfos;

    protected PackageLoaderRunner(IEnumerable<PackageLoadInfo> packages) {
      Guard.Argument(packages, nameof(packages)).NotNull();

      packageLoadInfos = packages.ToArray();
    }

    protected override Task ExecuteOnClientAsync(IChannel channel, CancellationToken cancellationToken) {
      PackageLoader.Instance.LoadPackageAssemblies(packageLoadInfos);
      return Task.CompletedTask;
    }
  }
}
