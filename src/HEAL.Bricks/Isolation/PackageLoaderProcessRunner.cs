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
  public abstract class PackageLoaderProcessRunner : ProcessRunner {
    private readonly PackageLoadInfo[] packageLoadInfos;
    public IEnumerable<PackageLoadInfo> PackageLoadInfos => packageLoadInfos;

    protected PackageLoaderProcessRunner(IEnumerable<PackageLoadInfo> packages, IProcessRunnerStartInfo startInfo) : base(startInfo) {
      packageLoadInfos = packages.ToArray();
    }

    protected override Task ExecuteOnClientAsync(CancellationToken cancellationToken) {
      PackageLoader.LoadPackageAssemblies(packageLoadInfos);
      return Task.CompletedTask;
    }
  }
}
