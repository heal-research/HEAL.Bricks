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
  public sealed class RemotePackageInfoIdentityComparer : IEqualityComparer<RemotePackageInfo>, IComparer<RemotePackageInfo> {
    internal readonly PackageIdentityComparer nuGetPackageIdentityComparer = PackageIdentityComparer.Default;

    public static RemotePackageInfoIdentityComparer Default => new RemotePackageInfoIdentityComparer();

    public bool Equals(RemotePackageInfo x, RemotePackageInfo y) {
      return nuGetPackageIdentityComparer.Equals(x.sourcePackageDependencyInfo, y.sourcePackageDependencyInfo);
    }
    public int GetHashCode(RemotePackageInfo obj) {
      return obj.sourcePackageDependencyInfo.GetHashCode();
    }
    public int Compare(RemotePackageInfo x, RemotePackageInfo y) {
      return nuGetPackageIdentityComparer.Compare(x.sourcePackageDependencyInfo, y.sourcePackageDependencyInfo);
    }
  }
}
