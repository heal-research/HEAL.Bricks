#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using NuGetPackageDependency = NuGet.Packaging.Core.PackageDependency;

namespace HEAL.Bricks {
  public sealed class PackageDependency : IEquatable<PackageDependency> {
    internal readonly NuGetPackageDependency nuGetPackageDependency;

    public string Id => nuGetPackageDependency.Id;
    public PackageVersionRange VersionRange { get; }
    public PackageDependencyStatus Status { get; internal set; }
    
    internal PackageDependency(NuGetPackageDependency nuGetPackageDependency) {
      if (nuGetPackageDependency == null) throw new ArgumentNullException(nameof(nuGetPackageDependency));
      if (string.IsNullOrEmpty(nuGetPackageDependency.Id)) throw new ArgumentException($"{nameof(nuGetPackageDependency)}.Id is null or empty.", nameof(nuGetPackageDependency));

      this.nuGetPackageDependency = nuGetPackageDependency;
      VersionRange = new PackageVersionRange(nuGetPackageDependency.VersionRange);
      Status = PackageDependencyStatus.Unknown;
    }

    public override string ToString() {
      return nuGetPackageDependency.ToString() + " [" + Status + "]";
    }
    public bool Equals(PackageDependency other) {
      return nuGetPackageDependency.Equals(other.nuGetPackageDependency);
    }
    public override bool Equals(object obj) {
      return nuGetPackageDependency.Equals(obj);
    }
    public override int GetHashCode() {
      return nuGetPackageDependency.GetHashCode();
    }
  }
}
