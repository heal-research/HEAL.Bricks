﻿#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HEAL.Bricks {
  public sealed class PackageVersionRange : IEquatable<PackageVersionRange> {
    internal readonly VersionRange nuGetVersionRange;

    internal PackageVersion MinVersion { get; }
    internal PackageVersion MaxVersion { get; }
    
    internal PackageVersionRange(VersionRange nuGetVersionRange) {
      this.nuGetVersionRange = nuGetVersionRange ?? throw new ArgumentNullException(nameof(PackageVersionRange.nuGetVersionRange));
      MinVersion = nuGetVersionRange.MinVersion != null ? new PackageVersion(nuGetVersionRange.MinVersion) : null;
      MaxVersion = nuGetVersionRange.MaxVersion != null ? new PackageVersion(nuGetVersionRange.MaxVersion) : null;
    }

    internal PackageVersion FindBestMatch(IEnumerable<PackageVersion> versions) {
      NuGetVersion bestMatch = nuGetVersionRange.FindBestMatch(versions.Select(x => x.nuGetVersion));
      return bestMatch != null ? new PackageVersion(bestMatch) : null;
    }
    internal bool Satiesfies(PackageVersion version) {
      return nuGetVersionRange.Satisfies(version.nuGetVersion);
    }

    public override string ToString() {
      return nuGetVersionRange.PrettyPrint();
    }
    public bool Equals(PackageVersionRange other) {
      return nuGetVersionRange.Equals(other.nuGetVersionRange);
    }
    public override bool Equals(object obj) {
      return nuGetVersionRange.Equals(obj);
    }
    public override int GetHashCode() {
      return nuGetVersionRange.GetHashCode();
    }
  }
}
