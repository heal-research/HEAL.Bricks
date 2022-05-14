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
    public static async Task<IApplicationManager> CreateAsync(BricksOptions options, CancellationToken cancellationToken = default) {
      ApplicationManager applicationManager = new(options, new PackageManager(options));
      await applicationManager.InitializeAsync(cancellationToken);
      return applicationManager;
    }
    public static IApplicationManager Create(BricksOptions options) {
      return CreateAsync(options).Result;
    }

    private BricksOptions Options { get; }
    public IPackageManager PackageManager { get; private set; }
    public IEnumerable<ApplicationInfo> InstalledApplications { get; private set; } = Enumerable.Empty<ApplicationInfo>();
    public IEnumerable<ServiceInfo> InstalledServices { get; private set; } = Enumerable.Empty<ServiceInfo>();
    public IReadOnlyDictionary<RunnableInfo, RunnableSettings> RunnableSettings { get; private set; } = new Dictionary<RunnableInfo, RunnableSettings>();

    public ApplicationManager(IOptions<BricksOptions> options, IPackageManager packageManager) : this(options.Value, packageManager) { }
    public ApplicationManager(BricksOptions options, IPackageManager packageManager) {
      Options = Guard.Argument(options, nameof(options)).NotNull().Member(s => s.DefaultIsolation, x => x.Defined())
                                                                  .Member(s => s.DotnetCommand, x => x.NotNull().NotEmpty().NotWhiteSpace())
                                                                  .Member(s => s.StarterAssembly, x => x.NotNull().NotEmpty().NotWhiteSpace().RelativePath())
                                                                  .Member(s => s.DockerCommand, x => x.NotNull().NotEmpty().NotWhiteSpace())
                                                                  .Member(s => s.DefaultDockerImage, x => x.NotNull().NotEmpty().NotWhiteSpace());
      PackageManager = packageManager;
    }

    private async Task InitializeAsync(CancellationToken cancellationToken) {
      await ReloadAsync(cancellationToken);
    }

    public async Task RunAsync(ApplicationInfo applicationInfo, string[]? args = null, CancellationToken cancellationToken = default) {
      Guard.Argument(applicationInfo, nameof(applicationInfo)).NotNull();

      using IChannel channel = CreateRunnerChannel(RunnableSettings[applicationInfo].Isolation, applicationInfo.DockerImage);
      ApplicationRunner applicationRunner = new(PackageManager.GetPackageLoadInfos(), applicationInfo, args);
      await applicationRunner.RunAsync(channel, cancellationToken);
    }

    public async Task RunAutoStartAsync(string[]? args = null, CancellationToken cancellationToken = default) {
      List<Task> tasks = new();
      foreach (var runnable in RunnableSettings.Where(x => x.Value.AutoStart).Select(x => x.Key).OfType<ApplicationInfo>()) {
        tasks.Add(RunAsync(runnable, args, cancellationToken));
      }
      await Task.WhenAll(tasks);
    }

    public async Task ReloadAsync(CancellationToken cancellationToken = default) {
      using IChannel channel = CreateRunnerChannel(Options.DefaultIsolation);
      DiscoverRunnablesRunner discoverRunnablesRunner = new(PackageManager.GetPackageLoadInfos());
      RunnableInfo[] runnables = await discoverRunnablesRunner.GetRunnablesAsync(channel, cancellationToken);
      InstalledApplications = runnables.OfType<ApplicationInfo>().OrderBy(x => x.Name).ToArray();
      InstalledServices = runnables.OfType<ServiceInfo>().OrderBy(x => x.Name).ToArray();

      Dictionary<RunnableInfo, RunnableSettings> settings = new();
      foreach (var runnable in runnables) {
        settings.Add(runnable, new RunnableSettings {
          Isolation = Options.DefaultIsolation,
          AutoStart = runnable.AutoStart
        });
      }
      RunnableSettings = settings;
    }

    public IChannel CreateRunnerChannel(Isolation isolation, string dockerImage = "") {
      if (string.IsNullOrWhiteSpace(dockerImage)) dockerImage = Options.DefaultDockerImage;

      return isolation switch {
        Isolation.None => new MemoryChannel((channel, token) => MemoryChannelClientCode(channel, token).Wait(token)),
        Isolation.AnonymousPipes => new AnonymousPipesProcessChannel(Options.DotnetCommand, $"\"{Options.StarterAssembly}\""),
        Isolation.StdInOut => new StdInOutProcessChannel(Options.DotnetCommand, $"\"{Options.StarterAssembly}\""),
        Isolation.Docker => new DockerChannel(Options.DockerCommand, dockerImage, Options.UseWindowsContainer, Options.AppPath, "dotnet", $"\"{Options.StarterAssembly}\""),
        _ => throw new NotSupportedException($"Isolation {isolation} is not supported."),
      };
    }

    #region Helpers
    private static async Task MemoryChannelClientCode(IChannel channel, CancellationToken cancellationToken) {
      await Runner.ReceiveAndExecuteAsync(channel, cancellationToken);
    }
    #endregion
  }
}
