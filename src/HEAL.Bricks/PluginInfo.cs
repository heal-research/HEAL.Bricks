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
  public class PluginInfo : IPluginInfo {
    private readonly IPackageSearchMetadata nugetPackageMetadata = null;
    private readonly SourcePackageDependencyInfo nugetPackageDependencyInfo = null;

    public string Name { get; }
    public string Version { get; }
    public string Description { get; }

    public PluginInfo(string name, string version, string description = "") {
      Name = name;
      Version = version;
      Description = description;
    }
    internal PluginInfo(IPackageSearchMetadata nugetPackageMetadata, SourcePackageDependencyInfo nugetPackageDependencyInfo) {
      this.nugetPackageMetadata = nugetPackageMetadata;
      this.nugetPackageDependencyInfo = nugetPackageDependencyInfo;
      Name = nugetPackageMetadata.Identity.Id;
      Version = nugetPackageMetadata.Identity.Version.ToString();
      Description = nugetPackageMetadata.Description;
    }

    public IEnumerable<IPluginDependencyInfo> GetDependencies() {
      if (nugetPackageMetadata == null)
        throw new NotSupportedException("GetDependencies is only supported, if PluginInfo has been created from a NuGet IPackageSearchMetadata.");

      if (nugetPackageDependencyInfo != null) {
        foreach (PackageDependency dependency in nugetPackageDependencyInfo.Dependencies) {
          yield return new PluginDependencyInfo(dependency);
        }
      }
    }

    public override string ToString() {
      return $"{Name} ({Version})";
    }
    public string ToStringWithDependencies() {
      string s = $"{Name} ({Version})";
      IEnumerable<IPluginDependencyInfo> dependencies = GetDependencies();
      if (dependencies.Any())
        s += dependencies.Aggregate("", (a, b) => a.ToString() + "\n  - " + b.ToString());
      return s;
    }
  }
}
