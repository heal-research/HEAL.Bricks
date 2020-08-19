#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using NuGet.Frameworks;
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HEAL.Bricks {
  public sealed class LocalPackageInfo : PackageInfo {
    internal readonly NuspecReader nuspecReader;

    public string Description => nuspecReader.GetDescription();
    public bool IsPlugin { get; }
    public string PackagePath { get; }
    public IEnumerable<string> ReferenceItems { get; }
    public PackageStatus Status { get; internal set; }

    internal LocalPackageInfo(PackageFolderReader packageReader, NuGetFramework currentFramework, string pluginTag = "") : base(packageReader?.GetIdentity()) {
      Guard.Argument(packageReader, nameof(packageReader)).NotNull().Member(p => p.NuspecReader, n => n.NotNull());
      Guard.Argument(currentFramework, nameof(currentFramework)).NotNull();

      nuspecReader = packageReader.NuspecReader;
      Dependencies = NuGetFrameworkUtility.GetNearest(nuspecReader.GetDependencyGroups(), currentFramework).Packages.Select(x => new PackageDependency(x)).ToArray();
      IsPlugin = !string.IsNullOrEmpty(pluginTag) && nuspecReader.GetTags().Contains(pluginTag);
      PackagePath = System.IO.Path.GetDirectoryName(packageReader.GetNuspecFile());
      ReferenceItems = NuGetFrameworkUtility.GetNearest(packageReader.GetReferenceItems(), currentFramework).Items.Select(x => Path.Combine(PackagePath, x)).ToArray();
      bool frameworkNotSupported = new FrameworkReducer().GetNearest(currentFramework, packageReader.GetSupportedFrameworks()) == null;
      Status = frameworkNotSupported ? PackageStatus.IncompatibleFramework : PackageStatus.Unknown;
    }

    public override string ToString() {
      return $"{Id} ({Version}) [{Status}]";
    }
    public string ToStringWithDependencies() {
      string s = $"{Id} ({Version}) [{Status}]";
      if (Dependencies.Any())
        s += Dependencies.Aggregate("", (a, b) => a.ToString() + "\n  - " + b.ToString());
      return s;
    }
  }
}
