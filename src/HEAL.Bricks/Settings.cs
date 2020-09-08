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

    private string packageTag;
    private string packagesPath;
    private string packagesCachePath;
    private string dotnetCommand;
    private string dockerCommand;
    private string dockerImage;
    private string starterAssembly;

    public string PackageTag {
      get { return packageTag; }
      set { packageTag = Guard.Argument(value, nameof(PackageTag)).NotNull().NotWhiteSpace(); }
    }
    IEnumerable<string> ISettings.Repositories => Repositories;
    public List<string> Repositories { get; }
    public string AppPath { get; private set; }
    public string PackagesPath {
      get { return packagesPath; }
      set {
        packagesPath = Guard.Argument(value, nameof(PackagesPath)).NotNull().NotEmpty().NotWhiteSpace().Modify(s => Path.IsPathRooted(s) ? s : Path.Combine(AppPath, s));
      }
    }
    public string PackagesCachePath {
      get { return packagesCachePath; }
      set {
        packagesCachePath = Guard.Argument(value, nameof(PackagesCachePath)).NotNull().NotEmpty().NotWhiteSpace().Modify(s => Path.IsPathRooted(s) ? s : Path.Combine(AppPath, s));
      }
    }
    public Isolation Isolation { get; set; }
    public string DotnetCommand {
      get { return dotnetCommand; }
      set { dotnetCommand = Guard.Argument(value, nameof(DotnetCommand)).NotNull().NotEmpty().NotWhiteSpace(); }
    }
    public string DockerCommand {
      get { return dockerCommand; }
      set { dockerCommand = Guard.Argument(value, nameof(DockerCommand)).NotNull().NotEmpty().NotWhiteSpace(); }
    }
    public string DockerImage {
      get { return dockerImage; }
      set { dockerImage = Guard.Argument(value, nameof(DockerImage)).NotNull().NotEmpty().NotWhiteSpace(); }
    }
    public string StarterAssembly {
      get { return starterAssembly; }
      set { starterAssembly = Guard.Argument(value, nameof(StarterAssembly)).NotNull().NotEmpty().NotWhiteSpace().RelativePath(); }
    }

    public Settings() {
      PackageTag = "HEALBricksPlugin";
      Repositories = new List<string>(new[] { PublicNuGetRepository });
      AppPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
      PackagesPath = "packages";
      PackagesCachePath = "packages_cache";
      Isolation = Isolation.AnonymousPipes;
      DotnetCommand = "dotnet";
      DockerCommand = "docker";
      DockerImage = "mcr.microsoft.com/dotnet/core/runtime:3.1";
      StarterAssembly = Path.GetFileName(Assembly.GetEntryAssembly().Location);
    }

    internal void SetAppPath(string appPath) {
      // only used for unit tests to set AppPath manually
      // Explanation: In unit tests it is required to store packages in another directory and not at the location of the entry assembly.
      Guard.Argument(appPath, nameof(appPath)).NotNull().NotWhiteSpace().Require(Path.IsPathRooted(appPath), _ => $"{nameof(appPath)} must be an absolute path");
      AppPath = appPath;
    }
  }
}
