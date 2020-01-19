#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HEAL.Bricks {
  public sealed class PackageDependency : IEquatable<PackageDependency> {
    internal readonly NuGet.Packaging.Core.PackageDependency nuGetPackageDependency;

    public string Id => nuGetPackageDependency.Id;
    public PackageVersionRange VersionRange { get; }
    public PackageDependencyStatus Status { get; internal set; }
    
    internal PackageDependency(NuGet.Packaging.Core.PackageDependency nuGetPackageDependency) {
      this.nuGetPackageDependency = nuGetPackageDependency ?? throw new ArgumentNullException(nameof(nuGetPackageDependency));
      this.VersionRange = new PackageVersionRange(nuGetPackageDependency.VersionRange);
      this.Status = PackageDependencyStatus.Unknown;
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
