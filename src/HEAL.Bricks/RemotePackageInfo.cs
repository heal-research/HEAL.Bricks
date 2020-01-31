#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HEAL.Bricks {
  public sealed class RemotePackageInfo : IEquatable<RemotePackageInfo>, IComparable<RemotePackageInfo> {
    internal readonly SourcePackageDependencyInfo sourcePackageDependencyInfo;

    public static RemotePackageInfoIdentityComparer Comparer => RemotePackageInfoIdentityComparer.Default;

    public string Id => sourcePackageDependencyInfo.Id;
    public PackageVersion Version { get; }
    public IEnumerable<PackageDependency> Dependencies { get; }
    public string Source => sourcePackageDependencyInfo.Source.PackageSource.Source;

    internal RemotePackageInfo(SourcePackageDependencyInfo sourcePackageDependencyInfo) {
      this.sourcePackageDependencyInfo = sourcePackageDependencyInfo ?? throw new ArgumentNullException(nameof(sourcePackageDependencyInfo));
      Version = new PackageVersion(sourcePackageDependencyInfo.Version);
      Dependencies = sourcePackageDependencyInfo.Dependencies.Select(x => new PackageDependency(x)).ToArray();
    }

    public override string ToString() {
      return $"{Id} ({Version})";
    }
    public string ToStringWithDependencies() {
      string s = $"{Id} ({Version})";
      if (Dependencies.Any())
        s += Dependencies.Aggregate("", (a, b) => a.ToString() + "\n  - " + b.ToString());
      return s;
    }

    public bool Equals(RemotePackageInfo other) {
      return sourcePackageDependencyInfo.Equals(other.sourcePackageDependencyInfo);
    }
    public override bool Equals(object obj) {
      return sourcePackageDependencyInfo.Equals(obj);
    }
    public override int GetHashCode() {
      return sourcePackageDependencyInfo.GetHashCode();
    }
    public int CompareTo(RemotePackageInfo other) {
      return sourcePackageDependencyInfo.CompareTo(other.sourcePackageDependencyInfo);
    }
  }
}
