#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using NuGet.Packaging.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using NuGetPackageDependency = NuGet.Packaging.Core.PackageDependency;

namespace HEAL.Bricks {
  public class PackageInfo : IEquatable<PackageInfo>, IComparable<PackageInfo> {
    internal readonly PackageIdentity packageIdentity;
    public static PackageInfoIdentityComparer Comparer => PackageInfoIdentityComparer.Default;

    public string Id => packageIdentity.Id;
    public PackageVersion Version { get; }
    public IEnumerable<PackageDependency> Dependencies { get; protected set; }

    internal PackageInfo(PackageIdentity identity) {
      packageIdentity = Guard.Argument(identity, nameof(identity)).NotNull().Member(i => i.Id, s => s.NotNull().NotEmpty());
      Version = new PackageVersion(identity.Version);
      Dependencies = Enumerable.Empty<PackageDependency>();
    }
    internal PackageInfo(PackageIdentity identity, IEnumerable<NuGetPackageDependency> dependencies) : this(identity) {
      Guard.Argument(dependencies, nameof(dependencies)).NotNull();

      Dependencies = dependencies.Select(x => new PackageDependency(x)).ToArray();
    }

    public override string ToString() {
      return packageIdentity.ToString();
    }

    public bool Equals(PackageInfo other) {
      return packageIdentity.Equals(other.packageIdentity);
    }
    public override bool Equals(object obj) {
      return packageIdentity.Equals(obj);
    }
    public override int GetHashCode() {
      return packageIdentity.GetHashCode();
    }
    public int CompareTo(PackageInfo other) {
      return packageIdentity.CompareTo(other.packageIdentity);
    }
  }
}
