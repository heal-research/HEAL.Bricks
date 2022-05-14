#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  public sealed class ApplicationManager : IApplicationManager {
    public static async Task<IApplicationManager> CreateAsync(BricksOptions options, bool reloadApplications = true, CancellationToken cancellationToken = default) {
      ApplicationManager applicationManager = new(options, new PackageManager(options));
      await applicationManager.InitializeAsync(reloadApplications, cancellationToken);
      return applicationManager;
    }
    public static IApplicationManager Create(BricksOptions options, bool reloadApplications = true) {
      return CreateAsync(options, reloadApplications).Result;
    }

    private BricksOptions Options { get; }
    public IPackageManager PackageManager { get; private set; }
    public IEnumerable<ApplicationInfo> InstalledApplications { get; private set; } = Enumerable.Empty<ApplicationInfo>();

    public ApplicationManager(IOptions<BricksOptions> options, IPackageManager packageManager) : this(options.Value, packageManager) { }
    public ApplicationManager(BricksOptions options, IPackageManager packageManager) {
      Options = Guard.Argument(options, nameof(options)).NotNull().Member(s => s.DefaultIsolation, x => x.Defined())
                                                                  .Member(s => s.DotnetCommand, x => x.NotNull().NotEmpty().NotWhiteSpace())
                                                                  .Member(s => s.StarterAssembly, x => x.NotNull().NotEmpty().NotWhiteSpace().RelativePath())
                                                                  .Member(s => s.DockerCommand, x => x.NotNull().NotEmpty().NotWhiteSpace())
                                                                  .Member(s => s.DefaultDockerImage, x => x.NotNull().NotEmpty().NotWhiteSpace());
      PackageManager = packageManager;
    }

    private async Task InitializeAsync(bool reloadApplications, CancellationToken cancellationToken) {
      if (reloadApplications) {
        await ReloadAsync(cancellationToken);
      }
      else {
        //InstalledApplications = Options.ApplicationSettings.Select(x => x.ApplicationInfo).OrderBy(x => x.Name).ThenByDescending(x => x.Version).ToArray();
      }
    }

    public async Task RunAsync(ApplicationInfo applicationInfo, string arguments = "", CancellationToken cancellationToken = default) {
      Guard.Argument(applicationInfo, nameof(applicationInfo)).NotNull();

      //if (!Options.ApplicationSettings.Contains(applicationInfo)) {
      //  Options.ApplicationSettings.Add(new ApplicationSettings(applicationInfo));
      //}
      //ApplicationSettings appSettings = Options.ApplicationSettings[applicationInfo];

      //using (IChannel channel = CreateRunnerChannel(appSettings.Isolation, appSettings.ApplicationInfo.DockerImage)) {
      //  ApplicationRunner applicationRunner = new ApplicationRunner(PackageManager.GetPackageLoadInfos(), applicationInfo, arguments);
      //  await applicationRunner.RunAsync(channel, cancellationToken);
      //}
      await Task.CompletedTask;
    }

    public async Task RunAutoStartAsync(string arguments = "", CancellationToken cancellationToken = default) {
      //List<Task> tasks = new List<Task>();
      //foreach (ApplicationSettings appSettings in Options.ApplicationSettings.Where(x => x.AutoStart)) {
      //  tasks.Add(RunAsync(appSettings.ApplicationInfo, arguments, cancellationToken));
      //}
      //await Task.WhenAll(tasks);
      await Task.CompletedTask;
    }

    public async Task ReloadAsync(CancellationToken cancellationToken = default) {
      //using (IChannel channel = CreateRunnerChannel(Options.DefaultIsolation)) {
      //  DiscoverApplicationsRunner discoverApplicationsRunner = new DiscoverApplicationsRunner(PackageManager.GetPackageLoadInfos());
      //  InstalledApplications = await discoverApplicationsRunner.GetApplicationsAsync(channel, cancellationToken);

      //  List<ApplicationSettings> appSettings = new List<ApplicationSettings>();
      //  foreach (ApplicationInfo appInfo in InstalledApplications) {
      //    if (Options.ApplicationSettings.Contains(appInfo)) {
      //      appSettings.Add(new ApplicationSettings(appInfo, Options.ApplicationSettings[appInfo]));
      //    }
      //    else {
      //      appSettings.Add(new ApplicationSettings(appInfo));
      //    }
      //  }
      //  Options.ApplicationSettings.Clear();
      //  foreach (ApplicationSettings settings in appSettings) {
      //    Options.ApplicationSettings.Add(settings);
      //  }
      //}
      await Task.CompletedTask;
    }

    public IChannel CreateRunnerChannel(Isolation isolation, string dockerImage = "") {
      if (string.IsNullOrWhiteSpace(dockerImage)) dockerImage = Options.DefaultDockerImage;

      switch (isolation) {
        case Isolation.None:
          return new MemoryChannel((channel, token) => MemoryChannelClientCode(channel, token).Wait(token));
        case Isolation.AnonymousPipes:
          if (RuntimeInfo.CurrentRuntimeIsNETFramework) {
            return new AnonymousPipesProcessChannel(Options.StarterAssembly);
          }
          else {
            return new AnonymousPipesProcessChannel(Options.DotnetCommand, $"\"{Options.StarterAssembly}\"");
          }
        case Isolation.StdInOut:
          if (RuntimeInfo.CurrentRuntimeIsNETFramework) {
            return new StdInOutProcessChannel(Options.StarterAssembly);
          }
          else {
            return new StdInOutProcessChannel(Options.DotnetCommand, $"\"{Options.StarterAssembly}\"");
          }
        case Isolation.Docker:
          if (RuntimeInfo.CurrentRuntimeIsNETFramework) {
            return new DockerChannel(Options.DockerCommand, dockerImage, Options.UseWindowsContainer, Options.AppPath, Options.StarterAssembly);
          }
          else {
            return new DockerChannel(Options.DockerCommand, dockerImage, Options.UseWindowsContainer, Options.AppPath, "dotnet", $"\"{Options.StarterAssembly}\"");
          }
        default:
          throw new NotSupportedException($"Isolation {Options.DefaultIsolation} is not supported.");
      }
    }

    #region Helpers
    private static async Task MemoryChannelClientCode(IChannel channel, CancellationToken cancellationToken) {
      await Runner.ReceiveAndExecuteAsync(channel, cancellationToken);
    }
    #endregion
  }
}
