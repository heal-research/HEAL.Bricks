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

namespace HEAL.Bricks {
  [Serializable]
  public class Settings : ISettings {
    public static ISettings Default => new Settings();
    public static string PublicNuGetRepository => "https://api.nuget.org/v3/index.json";

    private string packagesPath;
    private string packagesCachePath;

    public string PluginTag { get; set; }
    IEnumerable<string> ISettings.Repositories => Repositories;
    public List<string> Repositories { get; }
    public string AppPath { get; private set; }
    public string PackagesPath {
      get { return packagesPath; }
      set {
        if (string.IsNullOrEmpty(value)) throw new ArgumentException($"{nameof(PackagesPath)} cannot be null or empty.", nameof(PackagesPath));
        packagesPath = value;
        if (!Path.IsPathRooted(packagesPath))
          packagesPath = Path.Combine(AppPath, packagesPath);
      }
    }
    public string PackagesCachePath {
      get { return packagesCachePath; }
      set {
        if (string.IsNullOrEmpty(value)) throw new ArgumentException($"{nameof(PackagesCachePath)} cannot be null or empty.", nameof(PackagesCachePath));
        packagesCachePath = value;
        if (!Path.IsPathRooted(packagesCachePath))
          packagesCachePath = Path.Combine(AppPath, packagesCachePath);
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
      AppPath = appPath;
    }
  }
}
