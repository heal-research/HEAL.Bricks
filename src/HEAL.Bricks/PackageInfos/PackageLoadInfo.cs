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
using System.Text.Json.Serialization;

namespace HEAL.Bricks {
  [Serializable]
  public sealed class PackageLoadInfo {
    internal static PackageLoadInfo CreateForTests(string id, string version, string packagesPath, params string[] assemblyPaths) {
      return new PackageLoadInfo(id, version, packagesPath, assemblyPaths, Array.Empty<string>());
    }

    public string Id { get; }
    public string Version { get; }
    public string PackagesPath { get; }
    public IEnumerable<string> AssemblyPaths { get; }
    public IEnumerable<string> Files { get; }

    [JsonConstructor]
    public PackageLoadInfo(string id, string version, string packagesPath, IEnumerable<string> assemblyPaths, IEnumerable<string> files) {
      // required for JSON serialization and unit tests
      Id = Guard.Argument(id, nameof(id)).NotNull().NotEmpty().NotWhiteSpace().Value;
      Version = Guard.Argument(version, nameof(version)).NotNull().Value;
      PackagesPath = Guard.Argument(packagesPath, nameof(packagesPath)).NotNull().NotEmpty().NotWhiteSpace().Value;
      AssemblyPaths = Guard.Argument(assemblyPaths, nameof(assemblyPaths)).NotNull().DoesNotContainNull().Value;
      Files = Guard.Argument(files, nameof(files)).NotNull().DoesNotContainNull().Value;
    }
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
      AssemblyPaths = package.ReferenceItems.ToArray();
      Files = package.Files.ToArray();
    }
  }
}
