#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using NuGet.Frameworks;
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HEAL.Bricks {
  public sealed class PackageInfo : IEquatable<PackageInfo>, IComparable<PackageInfo> {
    internal readonly NuspecReader nuspecReader;

    public static PackageInfoIdentityComparer Comparer => PackageInfoIdentityComparer.Default;

    public string Id => nuspecReader.GetId();
    public PackageVersion Version { get; }
    public string Description => nuspecReader.GetDescription();
    public IEnumerable<PackageDependency> Dependencies { get; }
    public bool IsPlugin { get; }
    public PackageStatus Status { get; internal set; }

    internal PackageInfo(PackageFolderReader packageReader, NuGetFramework currentFramework, string pluginTag = "") {
      this.nuspecReader = packageReader?.NuspecReader ?? throw new ArgumentNullException(nameof(packageReader));
      Version = new PackageVersion(nuspecReader.GetVersion());
      Dependencies = NuGetFrameworkUtility.GetNearest(nuspecReader.GetDependencyGroups(), currentFramework).Packages.Select(x => new PackageDependency(x)).ToArray();
      IsPlugin = !string.IsNullOrEmpty(pluginTag) && nuspecReader.GetTags().Contains(pluginTag);
      bool frameworkNotSupported = new FrameworkReducer().GetNearest(currentFramework, packageReader.GetSupportedFrameworks()) == null;
      Status = frameworkNotSupported ? PackageStatus.IncompatibleFramework : PackageStatus.Unknown;
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
      return nuspecReader.GetIdentity().Equals(other.nuspecReader.GetIdentity());
    }
    public override bool Equals(object obj) {
      return nuspecReader.GetIdentity().Equals(obj);
    }
    public override int GetHashCode() {
      return nuspecReader.GetIdentity().GetHashCode();
    }
    public int CompareTo(PackageInfo other) {
      return nuspecReader.GetIdentity().CompareTo(other.nuspecReader.GetIdentity());
    }
  }
}
