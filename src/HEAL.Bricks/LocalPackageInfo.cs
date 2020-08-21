#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using NuGet.Frameworks;
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HEAL.Bricks {
  public sealed class LocalPackageInfo : PackageInfo {
    internal static LocalPackageInfo CreateForTests(string id, string version, IEnumerable<PackageDependency> dependencies = null, bool isBricksPackage = true, string packagePath = null, IEnumerable<string> referenceItems = null, PackageStatus status = PackageStatus.Unknown) {
      LocalPackageInfo lpi = new LocalPackageInfo(id, version, isBricksPackage, packagePath, referenceItems, status) {
        Dependencies = dependencies ?? Enumerable.Empty<PackageDependency>()
      };
      return lpi;
    }

    internal readonly NuspecReader nuspecReader;

    public string Description => nuspecReader.GetDescription();
    public bool IsBricksPackage { get; }
    public string PackagePath { get; }
    public IEnumerable<string> ReferenceItems { get; }
    public PackageStatus Status { get; internal set; }

    internal LocalPackageInfo(PackageFolderReader packageReader, NuGetFramework currentFramework, string packageTag = "") : base(packageReader?.GetIdentity()) {
      Guard.Argument(packageReader, nameof(packageReader)).NotNull().Member(p => p.NuspecReader, n => n.NotNull());
      Guard.Argument(currentFramework, nameof(currentFramework)).NotNull();

      nuspecReader = packageReader.NuspecReader;
      Dependencies = NuGetFrameworkUtility.GetNearest(nuspecReader.GetDependencyGroups(), currentFramework).Packages.Select(x => new PackageDependency(x)).ToArray();
      IsBricksPackage = !string.IsNullOrEmpty(packageTag) && nuspecReader.GetTags().Contains(packageTag);
      PackagePath = System.IO.Path.GetDirectoryName(packageReader.GetNuspecFile());
      ReferenceItems = NuGetFrameworkUtility.GetNearest(packageReader.GetReferenceItems(), currentFramework).Items.Select(x => Path.Combine(PackagePath, x)).ToArray();
      bool frameworkNotSupported = new FrameworkReducer().GetNearest(currentFramework, packageReader.GetSupportedFrameworks()) == null;
      Status = frameworkNotSupported ? PackageStatus.IncompatibleFramework : PackageStatus.Unknown;
    }
    private LocalPackageInfo(string id, string version, bool isBricksPackage, string packagePath, IEnumerable<string> referenceItems, PackageStatus status) : base(id, version) {
      // required for unit tests
      IsBricksPackage = isBricksPackage;
      PackagePath = packagePath;
      ReferenceItems = referenceItems;
      Status = status;
    }

    public override string ToString() {
      return $"{Id} ({Version}) [{Status}]";
    }
    public string ToStringWithDependencies() {
      string s = $"{Id} ({Version}) [{Status}]";
      if (Dependencies.Any())
        s += Dependencies.Aggregate("", (a, b) => a.ToString() + "\n  - " + b.ToString());
      return s;
    }
  }
}
