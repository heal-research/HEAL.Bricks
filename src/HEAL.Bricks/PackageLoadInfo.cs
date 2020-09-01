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

namespace HEAL.Bricks {
  [Serializable]
  public sealed class PackageLoadInfo {
    internal static PackageLoadInfo CreateForTests(string id, string version, params string[] assemblyPaths) {
      return new PackageLoadInfo(id, version, assemblyPaths);
    }
    public string Id { get; }
    public string Version { get; }

    private readonly string[] assemblyPaths;
    public IEnumerable<string> AssemblyPaths => assemblyPaths;

    internal PackageLoadInfo(LocalPackageInfo package) {
      Guard.Argument(package, nameof(package)).NotNull()
                                              .Member(p => p.Id, i => i.NotNull().NotEmpty())
                                              .Member(p => p.Version, v => v.NotNull());

      Id = package.Id;
      Version = package.Version.ToString();
      assemblyPaths = package.ReferenceItems?.ToArray() ?? Array.Empty<string>();
    }
    private PackageLoadInfo(string id, string version, string[] assemblyPaths) {
      // required for unit tests
      Id = id;
      Version = version;
      this.assemblyPaths = assemblyPaths;
    }
  }
}
