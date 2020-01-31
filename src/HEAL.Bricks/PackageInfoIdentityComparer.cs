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
  public sealed class PackageInfoIdentityComparer : IEqualityComparer<PackageInfo>, IComparer<PackageInfo> {
    internal readonly PackageIdentityComparer nuGetPackageIdentityComparer = PackageIdentityComparer.Default;

    public static PackageInfoIdentityComparer Default => new PackageInfoIdentityComparer();

    public bool Equals(PackageInfo x, PackageInfo y) {
      return nuGetPackageIdentityComparer.Equals(x.nuspecReader.GetIdentity(), y.nuspecReader.GetIdentity());
    }
    public int GetHashCode(PackageInfo obj) {
      return obj.nuspecReader.GetIdentity().GetHashCode();
    }
    public int Compare(PackageInfo x, PackageInfo y) {
      return nuGetPackageIdentityComparer.Compare(x.nuspecReader.GetIdentity(), y.nuspecReader.GetIdentity());
    }
  }
}
