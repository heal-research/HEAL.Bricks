#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HEAL.Bricks {
  public partial class PackageLoader {
    public static PackageLoader Instance { get; } = new PackageLoader();

    public void LoadPackageAssemblies(PackageLoadInfo package) {
      Guard.Argument(package, nameof(package)).NotNull();

      LoadPackageAssemblies(Enumerable.Repeat(package, 1));
    }
    public void LoadPackageAssemblies(IEnumerable<PackageLoadInfo> packages) {
      Guard.Argument(packages, nameof(packages)).NotNull().DoesNotContainNull();

      if (packages.Count() == 0) return;
      Load(packages);
    }

    partial void Load(IEnumerable<PackageLoadInfo> packages);

    private void LoadManagedAssemblies(IEnumerable<PackageLoadInfo> packages, out IEnumerable<(string assembly, Exception exception)> errors) {
      List<(string assembly, Exception exception)> loadingFailed = new List<(string assembly, Exception exception)>();
      string packagesPath = PreparePackagesPath(packages);

      foreach (string assembly in packages.SelectMany(x => x.AssemblyPaths).Select(x => Path.Combine(packagesPath, x))) {
        try {
          Assembly.LoadFrom(assembly);
        }
        catch (Exception exception) {
          loadingFailed.Add((assembly, exception));
        }
      }

      errors = loadingFailed;
    }

    private void ThrowExceptionOnError(IEnumerable<(string assembly, Exception exception)> errors) {
      if ((errors != null) && (errors.Count() != 0)) {
        throw new AggregateException("Package loading failed. See inner exception for details.",
                                     errors.Select(x => new InvalidOperationException($"Loading of assembly '{x.assembly}' failed.", x.exception)));
      }
    }

    private string PreparePackagesPath(IEnumerable<PackageLoadInfo> packages) {
      string packagesPath = packages.First().PackagesPath;
      if (!Directory.Exists(packagesPath)) {
        packagesPath = "packages";
      }
      return packagesPath;
    }
  }
}
