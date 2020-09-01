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
using System.Runtime.Loader;

namespace HEAL.Bricks {
  public static class PackageLoader {
    public static void LoadPackageAssemblies(PackageLoadInfo package) {
      Guard.Argument(package, nameof(package)).NotNull();

      LoadPackageAssemblies(Enumerable.Repeat(package, 1));
    }
    public static void LoadPackageAssemblies(IEnumerable<PackageLoadInfo> packages) {
      Guard.Argument(packages, nameof(packages)).NotNull().DoesNotContainNull();

      foreach (PackageLoadInfo package in packages) {
        foreach (string assemblyPath in package.AssemblyPaths) {
          AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
        }
      }
    }
  }
}
