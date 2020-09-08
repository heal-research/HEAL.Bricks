#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  public sealed class ApplicationManager : IApplicationManager {
    public static async Task<IApplicationManager> CreateAsync(ISettings settings, CancellationToken cancellationToken = default) {
      Guard.Argument(settings, nameof(settings)).NotNull().Member(s => s.Isolation, x => x.Defined())
                                                          .Member(s => s.DotnetCommand, x => x.NotNull().NotEmpty().NotWhiteSpace())
                                                          .Member(s => s.DockerCommand, x => x.NotNull().NotEmpty().NotWhiteSpace())
                                                          .Member(s => s.DockerImage, x => x.NotNull().NotEmpty().NotWhiteSpace())
                                                          .Member(s => s.StarterAssembly, x => x.NotNull().NotEmpty().NotWhiteSpace().RelativePath());

      ApplicationManager applicationManager = new ApplicationManager(settings) {
        PackageManager = Bricks.PackageManager.Create(settings)
      };
      await applicationManager.InitializeAsync(cancellationToken);
      return applicationManager;
    }
    public static IApplicationManager Create(ISettings settings) {
      return CreateAsync(settings).Result;
    }
    internal static IApplicationManager CreateForTests(ISettings settings, IPackageManager packageManager) {
      ApplicationManager applicationManager = new ApplicationManager(settings) {
        PackageManager = packageManager
      };
      applicationManager.InitializeAsync(default).Wait();
      return applicationManager;
    }

    public ISettings Settings { get; }
    public IPackageManager PackageManager { get; private set; }
    public IEnumerable<ApplicationInfo> InstalledApplications { get; private set; } = Enumerable.Empty<ApplicationInfo>();

    private ApplicationManager(ISettings settings) {
      Settings = settings;
    }

    private async Task InitializeAsync(CancellationToken cancellationToken) {
      using (IChannel channel = CreateRunnerChannel()) {
        DiscoverApplicationsRunner discoverApplicationsRunner = new DiscoverApplicationsRunner(PackageManager.GetPackageLoadInfos());
        InstalledApplications = await discoverApplicationsRunner.GetApplicationsAsync(channel, cancellationToken);
      }
    }

    public async Task RunAsync(ApplicationInfo application, ICommandLineArgument[] arguments = null, CancellationToken cancellationToken = default) {
      Guard.Argument(application, nameof(application)).NotNull();

      using (IChannel channel = CreateRunnerChannel()) {
        ApplicationRunner applicationRunner = new ApplicationRunner(PackageManager.GetPackageLoadInfos(), application, arguments);
        await applicationRunner.RunAsync(channel, cancellationToken);
      }
    }

    public IChannel CreateRunnerChannel() {
      switch (Settings.Isolation) {
        case Isolation.None:
          return new MemoryChannel((channel, token) => MemoryChannelClientCode(channel, token).Wait());
        case Isolation.AnonymousPipes:
          return new AnonymousPipesProcessChannel(Settings.DotnetCommand, $"\"{Settings.StarterAssembly}\"");
        case Isolation.StdInOut:
          return new StdInOutProcessChannel(Settings.DotnetCommand, $"\"{Settings.StarterAssembly}\"");
        case Isolation.Docker:
          return new DockerChannel(Settings.DockerCommand, Settings.DockerImage, Settings.AppPath, "dotnet", $"\"{Settings.StarterAssembly}\"");
        default:
          throw new NotSupportedException($"Isolation {Settings.Isolation} is not supported.");
      }
    }

    #region Helpers
    private static async Task MemoryChannelClientCode(IChannel channel, CancellationToken cancellationToken) {
      await Runner.ReceiveAndExecuteAsync(channel, cancellationToken);
    }
    #endregion
  }
}
