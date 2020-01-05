#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using NuGet.Packaging.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HEAL.Bricks {
  public class PluginDependencyInfo : IPluginDependencyInfo {
    public string Name { get; }
    public string VersionRange { get; }
    public string MinVersion { get; }

    public PluginDependencyInfo(string name, string versionRange, string minVersion = "") {
      Name = name;
      VersionRange = versionRange;
      MinVersion = minVersion;
    }
    internal PluginDependencyInfo(PackageDependency packageDependency) {
      Name = packageDependency.Id;
      VersionRange = packageDependency.VersionRange.PrettyPrint();
      MinVersion = packageDependency.VersionRange.MinVersion.ToString();
    }

    public override string ToString() {
      return $"{Name} {VersionRange}";
    }
  }
}
