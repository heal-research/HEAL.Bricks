#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Dawn;

namespace HEAL.Bricks {
  [Serializable]
  public class Settings : ISettings {
    public static string PublicNuGetRepository => "https://api.nuget.org/v3/index.json";
    public static Settings Default { get; } = new Settings();

    private string pluginTag;
    private string packagesPath;
    private string packagesCachePath;

    public string PluginTag {
      get { return pluginTag; }
      set { pluginTag = Guard.Argument(value, nameof(PluginTag)).NotNull().NotWhiteSpace(); }
    }
    IEnumerable<string> ISettings.Repositories => Repositories;
    public List<string> Repositories { get; }
    public string AppPath { get; private set; }
    public string PackagesPath {
      get { return packagesPath; }
      set {
        packagesPath = Guard.Argument(value, nameof(PackagesPath)).NotNull().NotWhiteSpace().Modify(s => Path.IsPathRooted(s) ? s : Path.Combine(AppPath, s));
      }
    }
    public string PackagesCachePath {
      get { return packagesCachePath; }
      set {
        packagesCachePath = Guard.Argument(value, nameof(PackagesCachePath)).NotNull().NotWhiteSpace().Modify(s => Path.IsPathRooted(s) ? s : Path.Combine(AppPath, s));
      }
    }

    public Settings() {
      PluginTag = "HEALBricksPlugin";
      Repositories = new List<string>(new[] { PublicNuGetRepository });
      AppPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
      PackagesPath = "packages";
      PackagesCachePath = "packages_cache";
    }

    internal void SetAppPath(string appPath) {
      // only used for unit test to set AppPath manually
      // Explanation: In unit tests it is required to store packages in another directory and not at the location of the entry assembly.
      Guard.Argument(appPath, nameof(appPath)).NotNull().NotWhiteSpace().Require(Path.IsPathRooted(appPath), _ => $"{nameof(appPath)} must be an absolute path");
      AppPath = appPath;
    }
  }
}
