#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using NuGet.Frameworks;
using NuGet.Packaging;
using System;
using System.Linq;

namespace HEAL.Bricks {
  public sealed class LocalPackageInfo : PackageInfo {
    internal readonly NuspecReader nuspecReader;

    public string Description => nuspecReader.GetDescription();
    public bool IsPlugin { get; }
    public string Path { get; }
    public PackageStatus Status { get; internal set; }

    internal LocalPackageInfo(PackageFolderReader packageReader, NuGetFramework currentFramework, string pluginTag = "") : base(packageReader?.GetIdentity()) {
      if (packageReader == null) throw new ArgumentNullException(nameof(packageReader));
      if (packageReader.NuspecReader == null) throw new ArgumentException($"{nameof(packageReader)}.NuspecReader is null.", nameof(packageReader));
      if (currentFramework == null) throw new ArgumentNullException(nameof(currentFramework));

      nuspecReader = packageReader.NuspecReader;
      Dependencies = NuGetFrameworkUtility.GetNearest(nuspecReader.GetDependencyGroups(), currentFramework).Packages.Select(x => new PackageDependency(x)).ToArray();
      IsPlugin = !string.IsNullOrEmpty(pluginTag) && nuspecReader.GetTags().Contains(pluginTag);
      Path = System.IO.Path.GetDirectoryName(packageReader.GetNuspecFile());
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
  }
}
