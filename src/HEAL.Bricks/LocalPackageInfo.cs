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
  public sealed class LocalPackageInfo : PackageInfo, IEquatable<LocalPackageInfo>, IComparable<LocalPackageInfo> {
    internal static LocalPackageInfo CreateForTests(string id, string version, IEnumerable<PackageDependency>? dependencies = null, string packagePath = "", IEnumerable<string>? referenceItems = null, IEnumerable<string>? files = null, bool frameworkNotSupported = false) {
      LocalPackageInfo lpi = new(id, version, packagePath, referenceItems, files) {
        Dependencies = dependencies ?? Enumerable.Empty<PackageDependency>(),
        status = frameworkNotSupported ? PackageStatus.IncompatibleFramework : PackageStatus.Undefined
      };
      return lpi;
    }
    internal static LocalPackageInfo CreateForTestsFromRemotePackageInfo(RemotePackageInfo package, string packagePath = "", IEnumerable<string>? referenceItems = null, IEnumerable<string>? files = null, bool frameworkNotSupported = false) {
      LocalPackageInfo lpi = new(package.Id, package.Version.ToString(), packagePath, referenceItems, files) {
        Dependencies = package.Dependencies,
        status = frameworkNotSupported ? PackageStatus.IncompatibleFramework : PackageStatus.Undefined
      };
      return lpi;
    }

    private PackageStatus status = PackageStatus.Undefined;

    public string Description { get; } = string.Empty;
    public string PackagePath { get; }
    public IEnumerable<string> ReferenceItems { get; }
    public IEnumerable<string> Files { get; }
    public PackageStatus Status {
      get { return status; }
      internal set {
        if (status != PackageStatus.IncompatibleFramework) status = value;
      }
    } 

    internal LocalPackageInfo(PackageFolderReader packageReader, NuGetFramework currentFramework) : base(packageReader.GetIdentity()) {
      Guard.Argument(packageReader, nameof(packageReader)).NotNull().Member(p => p.NuspecReader, n => n.NotNull());
      Guard.Argument(currentFramework, nameof(currentFramework)).NotNull();

      PackagePath = Path.GetFileName(Path.GetDirectoryName(packageReader.GetNuspecFile())) ?? string.Empty;
      Description = packageReader.NuspecReader.GetDescription();

      PackageDependencyGroup dependencyGroup = NuGetFrameworkUtility.GetNearest(packageReader.GetPackageDependencies(), currentFramework);
      Dependencies = dependencyGroup?.Packages.Select(x => new PackageDependency(x)).ToArray() ?? Enumerable.Empty<PackageDependency>();

      FrameworkSpecificGroup referenceGroup = NuGetFrameworkUtility.GetNearest(packageReader.GetReferenceItems(), currentFramework);
      ReferenceItems = referenceGroup?.Items.Select(x => Path.Combine(PackagePath, x)).ToArray() ?? Enumerable.Empty<string>();

      Files = packageReader.GetFiles().Select(x => Path.Combine(PackagePath, x)).ToArray();

      bool frameworkNotSupported = new FrameworkReducer().GetNearest(currentFramework, packageReader.GetSupportedFrameworks()) == null;
      status = frameworkNotSupported ? PackageStatus.IncompatibleFramework : PackageStatus.Undefined;
    }
    private LocalPackageInfo(string id, string version, string packagePath, IEnumerable<string>? referenceItems, IEnumerable<string>? files) : base(id, version) {
      // required for unit tests
      PackagePath = packagePath;
      ReferenceItems = referenceItems ?? Enumerable.Empty<string>();
      Files = files ?? Enumerable.Empty<string>();
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

    public override bool Equals(object? obj) {
      return base.Equals(obj);
    }
    public bool Equals(LocalPackageInfo? other) {
      return base.Equals(other);
    }
    public int CompareTo(LocalPackageInfo? other) {
      return base.CompareTo(other);
    }
    public override int GetHashCode() {
      return base.GetHashCode();
    }
  }
}
