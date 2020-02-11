#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using NuGet.Protocol.Core.Types;
using System;
using System.Linq;

namespace HEAL.Bricks {
  public sealed class RemotePackageInfo : PackageInfo {
    internal readonly IPackageSearchMetadata packageSearchMetadata;
    internal readonly SourcePackageDependencyInfo sourcePackageDependencyInfo;

    public string Source => sourcePackageDependencyInfo.Source.PackageSource.Source;

    internal RemotePackageInfo(IPackageSearchMetadata packageSearchMetadata, SourcePackageDependencyInfo sourcePackageDependencyInfo) : base(sourcePackageDependencyInfo) {
      if (packageSearchMetadata == null) throw new ArgumentNullException(nameof(packageSearchMetadata));
      if (sourcePackageDependencyInfo == null) throw new ArgumentNullException(nameof(sourcePackageDependencyInfo));
      if (sourcePackageDependencyInfo.Dependencies == null) throw new ArgumentException($"{nameof(sourcePackageDependencyInfo)}.Dependencies is null.", nameof(sourcePackageDependencyInfo));

      this.packageSearchMetadata = packageSearchMetadata;
      this.sourcePackageDependencyInfo = sourcePackageDependencyInfo;
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
  }
}
