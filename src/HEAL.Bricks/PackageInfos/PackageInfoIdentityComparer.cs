#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using NuGet.Packaging.Core;
using System.Collections.Generic;

namespace HEAL.Bricks {
  public sealed class PackageInfoIdentityComparer : IEqualityComparer<PackageInfo>, IComparer<PackageInfo> {
    internal readonly PackageIdentityComparer nuGetPackageIdentityComparer = PackageIdentityComparer.Default;

    public static PackageInfoIdentityComparer Default => new();

    public bool Equals(PackageInfo? x, PackageInfo? y) {
      return nuGetPackageIdentityComparer.Equals(x?.packageIdentity, y?.packageIdentity);
    }
    public int GetHashCode(PackageInfo obj) {
      return obj.packageIdentity.GetHashCode();
    }
    public int Compare(PackageInfo? x, PackageInfo? y) {
      return nuGetPackageIdentityComparer.Compare(x?.packageIdentity, y?.packageIdentity);
    }
  }
}
