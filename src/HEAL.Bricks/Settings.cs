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
using System.IO;
using System.Linq;
using System.Reflection;

namespace HEAL.Bricks {
  public class Settings : ISettings {
    public static ISettings Default => new Settings();
    public static string PublicNuGetRepository => "https://api.nuget.org/v3/index.json";

    private string packagesPath;

    public string PluginTag { get; set; }
    IEnumerable<string> ISettings.Repositories => Repositories;
    public List<string> Repositories { get; }
    public string AppPath { get; }
    public string PackagesPath {
      get { return packagesPath; }
      set {
        packagesPath = value;
        if (!Path.IsPathRooted(packagesPath))
          packagesPath = Path.Combine(AppPath, packagesPath);
      }
    }

    public Settings() {
      PluginTag = "HEALBricksPlugin";
      Repositories = new List<string>(new[] { PublicNuGetRepository });
      AppPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
      PackagesPath = "packages";
    }
  }
}
