#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.Reflection;
using Dawn;

namespace HEAL.Bricks {
  public class BricksOptions {
    public static Repository PublicNuGetRepository => new("https://api.nuget.org/v3/index.json");
    public static BricksOptions Default => new(PublicNuGetRepository);

    private string packagesPath;
    private string packagesCachePath;
    private string dotnetCommand;
    private string dockerCommand;
    private string dockerImage;
    private string starterAssembly;

    public List<Repository> Repositories { get; }
    public string AppPath { get; private set; }
    public string PackagesPath {
      get { return packagesPath; }
      set { packagesPath = Guard.Argument(value, nameof(PackagesPath)).NotNull().NotEmpty().NotWhiteSpace().Modify(s => Path.IsPathRooted(s) ? s : Path.Combine(AppPath, s)); }
    }
    public string PackagesCachePath {
      get { return packagesCachePath; }
      set { packagesCachePath = Guard.Argument(value, nameof(PackagesCachePath)).NotNull().NotEmpty().NotWhiteSpace().Modify(s => Path.IsPathRooted(s) ? s : Path.Combine(AppPath, s)); }
    }
    public Isolation DefaultIsolation { get; set; }
    public string DotnetCommand {
      get { return dotnetCommand; }
      set { dotnetCommand = Guard.Argument(value, nameof(DotnetCommand)).NotNull().NotEmpty().NotWhiteSpace(); }
    }
    public string StarterAssembly {
      get { return starterAssembly; }
      set { starterAssembly = Guard.Argument(value, nameof(StarterAssembly)).NotNull().NotEmpty().NotWhiteSpace().RelativePath(); }
    }
    public string DockerCommand {
      get { return dockerCommand; }
      set { dockerCommand = Guard.Argument(value, nameof(DockerCommand)).NotNull().NotEmpty().NotWhiteSpace(); }
    }
    public string DefaultDockerImage {
      get { return dockerImage; }
      set { dockerImage = Guard.Argument(value, nameof(DefaultDockerImage)).NotNull().NotEmpty().NotWhiteSpace(); }
    }
    public bool UseWindowsContainer { get; set; }

    public BricksOptions() {
      Repositories = new List<Repository>();
      AppPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty;
      packagesPath = Path.Combine(AppPath, "packages");
      packagesCachePath = Path.Combine(AppPath, "packages_cache");
      DefaultIsolation = Isolation.AnonymousPipes;
      dotnetCommand = "dotnet";
      dockerCommand = "docker";
      dockerImage = "mcr.microsoft.com/dotnet/runtime:latest";
      UseWindowsContainer = false;
      starterAssembly = Path.GetFileName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty;
    }
    public BricksOptions(params Repository[] repositories) : this() {
      Guard.Argument(repositories, nameof(repositories)).NotNull().DoesNotContainNull();
      Repositories.AddRange(repositories);
    }

    internal void SetAppPath(string appPath) {
      // only used for unit tests to set AppPath manually
      // Explanation: In unit tests it is required to store packages in another directory and not at the location of the entry assembly.
      Guard.Argument(appPath, nameof(appPath)).NotNull().NotWhiteSpace().Require(Path.IsPathRooted(appPath), _ => $"{nameof(appPath)} must be an absolute path");
      AppPath = appPath;
    }
  }
}
