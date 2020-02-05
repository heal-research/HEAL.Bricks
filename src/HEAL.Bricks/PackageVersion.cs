#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using NuGet.Versioning;
using System;

namespace HEAL.Bricks {
  public sealed class PackageVersion : IComparable, IComparable<PackageVersion>, IEquatable<PackageVersion> {
    internal readonly NuGetVersion nuGetVersion;

    public int Major => nuGetVersion.Major;
    public int Minor => nuGetVersion.Minor;
    public int Patch => nuGetVersion.Patch;
    public string Release => nuGetVersion.Release;
    public bool IsPrerelease => nuGetVersion.IsPrerelease;

    internal PackageVersion(NuGetVersion nuGetVersion) {
      this.nuGetVersion = nuGetVersion ?? throw new ArgumentNullException(nameof(nuGetVersion));
    }

    public override string ToString() {
      return nuGetVersion.ToFullString();
    }
    public bool Equals(PackageVersion other) {
      return nuGetVersion.Equals(other.nuGetVersion);
    }
    public override bool Equals(object obj) {
      return nuGetVersion.Equals(obj);
    }
    public override int GetHashCode() {
      return nuGetVersion.GetHashCode();
    }
    public int CompareTo(object obj) {
      return nuGetVersion.CompareTo(obj);
    }
    public int CompareTo(PackageVersion other) {
      return nuGetVersion.CompareTo(other.nuGetVersion);
    }
  }
}
