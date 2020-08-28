#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HEAL.Bricks {
  public sealed class RemotePackageInfo : PackageInfo, IEquatable<RemotePackageInfo>, IComparable<RemotePackageInfo> {
    internal static RemotePackageInfo CreateForTests(string id, string version, IEnumerable<PackageDependency> dependencies = null) {
      RemotePackageInfo rpi = new RemotePackageInfo(id, version) {
        Dependencies = dependencies ?? Enumerable.Empty<PackageDependency>()
      };
      return rpi;
    }

    internal readonly IPackageSearchMetadata packageSearchMetadata;
    internal readonly SourcePackageDependencyInfo sourcePackageDependencyInfo;

    public string Source => sourcePackageDependencyInfo?.Source.PackageSource.Source;

    internal RemotePackageInfo(IPackageSearchMetadata packageSearchMetadata, SourcePackageDependencyInfo sourcePackageDependencyInfo) : base(sourcePackageDependencyInfo) {
      Guard.Argument(packageSearchMetadata, nameof(packageSearchMetadata)).NotNull().Member(p => p.Identity.Id, i => i.Equal(sourcePackageDependencyInfo.Id, StringComparison.OrdinalIgnoreCase))
                                                                                    .Member(p => p.Identity.Version, v => v.Equal(sourcePackageDependencyInfo.Version));
      Guard.Argument(sourcePackageDependencyInfo, nameof(sourcePackageDependencyInfo)).NotNull().Member(s => s.Dependencies, d => d.NotNull());

      this.packageSearchMetadata = packageSearchMetadata;
      this.sourcePackageDependencyInfo = sourcePackageDependencyInfo;
      Dependencies = sourcePackageDependencyInfo.Dependencies.Select(x => new PackageDependency(x)).ToArray();
    }
    private RemotePackageInfo(string id, string version) : base(id, version) {
      // required for unit tests
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

    public bool Equals(RemotePackageInfo other) {
      return base.Equals(other);
    }
    public int CompareTo(RemotePackageInfo other) {
      return base.CompareTo(other);
    }
  }
}
