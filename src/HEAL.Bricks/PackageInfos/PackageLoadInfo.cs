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
    internal static PackageLoadInfo CreateForTests(string id, string version, string packagesPath, params string[] assemblyPaths) {
      return new PackageLoadInfo(id, version, packagesPath, assemblyPaths, Array.Empty<string>());
    }

    public string Id { get; }
    public string Version { get; }
    
    public string PackagesPath { get; }

    private readonly string[] assemblyPaths;
    public IEnumerable<string> AssemblyPaths => assemblyPaths;

    private readonly string[] files;
    public IEnumerable<string> Files => files;


    internal PackageLoadInfo(LocalPackageInfo package, string packagesPath, string appPath) {
      Guard.Argument(package, nameof(package)).NotNull()
                                              .Member(p => p.Id, i => i.NotNull().NotEmpty().NotWhiteSpace())
                                              .Member(p => p.Version, v => v.NotNull())
                                              .Member(p => p.ReferenceItems, r => r.NotNull().DoesNotContainNull())
                                              .Member(p => p.Files, f => f.NotNull().DoesNotContainNull());
      Guard.Argument(packagesPath, nameof(packagesPath)).NotNull().NotEmpty().NotWhiteSpace();

      Id = package.Id;
      Version = package.Version.ToString();
      PackagesPath = packagesPath.StartsWith(appPath) ? packagesPath[(appPath.Length + 1)..] : packagesPath;
      assemblyPaths = package.ReferenceItems.ToArray();
      files = package.Files.ToArray();
    }
    private PackageLoadInfo(string id, string version, string packagesPath, string[] assemblyPaths, string[] files) {
      // required for unit tests
      Id = id;
      Version = version;
      PackagesPath = packagesPath;
      this.assemblyPaths = assemblyPaths;
      this.files = files;
    }
  }
}
