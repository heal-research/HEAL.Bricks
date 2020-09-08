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
using System.Net.NetworkInformation;

namespace HEAL.Bricks {
  public sealed class LocalPackageInfo : PackageInfo, IEquatable<LocalPackageInfo>, IComparable<LocalPackageInfo> {
    internal static LocalPackageInfo CreateForTests(string id, string version, IEnumerable<PackageDependency> dependencies = null, string packagePath = null, IEnumerable<string> referenceItems = null, bool frameworkNotSupported = false) {
      LocalPackageInfo lpi = new LocalPackageInfo(id, version, packagePath, referenceItems) {
        Dependencies = dependencies ?? Enumerable.Empty<PackageDependency>(),
        status = frameworkNotSupported ? PackageStatus.IncompatibleFramework : PackageStatus.Undefined
      };
      return lpi;
    }
    internal static LocalPackageInfo CreateForTestsFromRemotePackageInfo(RemotePackageInfo package, string packagePath = null, IEnumerable<string> referenceItems = null, bool frameworkNotSupported = false) {
      LocalPackageInfo lpi = new LocalPackageInfo(package.Id, package.Version.ToString(), packagePath, referenceItems) {
        Dependencies = package.Dependencies,
        status = frameworkNotSupported ? PackageStatus.IncompatibleFramework : PackageStatus.Undefined
      };
      return lpi;
    }

    internal readonly NuspecReader nuspecReader;
    private PackageStatus status = PackageStatus.Undefined;

    public string Description => nuspecReader?.GetDescription();
    public string PackagePath { get; }
    public IEnumerable<string> ReferenceItems { get; }
    public PackageStatus Status {
      get { return status; }
      internal set {
        if (status != PackageStatus.IncompatibleFramework) status = value;
      }
    } 

    internal LocalPackageInfo(PackageFolderReader packageReader, NuGetFramework currentFramework) : base(packageReader?.GetIdentity()) {
      Guard.Argument(packageReader, nameof(packageReader)).NotNull().Member(p => p.NuspecReader, n => n.NotNull());
      Guard.Argument(currentFramework, nameof(currentFramework)).NotNull();

      nuspecReader = packageReader.NuspecReader;
      Dependencies = NuGetFrameworkUtility.GetNearest(nuspecReader.GetDependencyGroups(), currentFramework).Packages.Select(x => new PackageDependency(x)).ToArray();
      PackagePath = System.IO.Path.GetDirectoryName(packageReader.GetNuspecFile());
      ReferenceItems = NuGetFrameworkUtility.GetNearest(packageReader.GetReferenceItems(), currentFramework).Items.Select(x => Path.Combine(PackagePath, x)).ToArray();
      bool frameworkNotSupported = new FrameworkReducer().GetNearest(currentFramework, packageReader.GetSupportedFrameworks()) == null;
      status = frameworkNotSupported ? PackageStatus.IncompatibleFramework : PackageStatus.Undefined;
    }
    private LocalPackageInfo(string id, string version, string packagePath, IEnumerable<string> referenceItems) : base(id, version) {
      // required for unit tests
      PackagePath = packagePath;
      ReferenceItems = referenceItems;
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

    public bool Equals(LocalPackageInfo other) {
      return base.Equals(other);
    }
    public int CompareTo(LocalPackageInfo other) {
      return base.CompareTo(other);
    }
  }
}
