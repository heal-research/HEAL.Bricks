#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HEAL.Bricks {
  public sealed class PackageInfo : IEquatable<PackageInfo>, IComparable<PackageInfo> {
    internal readonly IPackageSearchMetadata nuGetPackageMetadata;

    public static PackageInfoIdentityComparer Comparer => PackageInfoIdentityComparer.Default;

    public string Id => nuGetPackageMetadata.Identity.Id;
    public PackageVersion Version { get; }
    public string Description => nuGetPackageMetadata.Description;
    public IEnumerable<PackageDependency> Dependencies { get; }
    public bool IsPlugin { get; }
    public PackageStatus Status { get; internal set; }

    internal PackageInfo(IPackageSearchMetadata nuGetPackageMetadata, IEnumerable<NuGet.Packaging.Core.PackageDependency> nuGetPackageDependencies, string pluginTag = "") {
      this.nuGetPackageMetadata = nuGetPackageMetadata ?? throw new ArgumentNullException(nameof(nuGetPackageMetadata));
      Version = new PackageVersion(nuGetPackageMetadata.Identity.Version);
      Dependencies = nuGetPackageDependencies?.Select(x => new PackageDependency(x)).ToArray() ?? throw new ArgumentNullException(nameof(nuGetPackageDependencies));
      IsPlugin = !string.IsNullOrEmpty(pluginTag) && nuGetPackageMetadata.Tags.Contains(pluginTag);
      Status = Dependencies.Count() == 0 ? PackageStatus.OK : PackageStatus.Unknown;
    }

    public override string ToString() {
      return $"{Id} ({Version}) [{Status.ToString()}]";
    }
    public string ToStringWithDependencies() {
      string s = $"{Id} ({Version}) [{Status.ToString()}]";
      if (Dependencies.Any())
        s += Dependencies.Aggregate("", (a, b) => a.ToString() + "\n  - " + b.ToString());
      return s;
    }

    public bool Equals(PackageInfo other) {
      return nuGetPackageMetadata.Identity.Equals(other.nuGetPackageMetadata.Identity);
    }
    public override bool Equals(object obj) {
      return nuGetPackageMetadata.Identity.Equals(obj);
    }
    public override int GetHashCode() {
      return nuGetPackageMetadata.Identity.GetHashCode();
    }
    public int CompareTo(PackageInfo other) {
      return nuGetPackageMetadata.Identity.CompareTo(other.nuGetPackageMetadata.Identity);
    }
  }
}
