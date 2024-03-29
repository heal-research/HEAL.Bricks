﻿#region License Information
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
using System.Text.Json;
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

    private readonly List<RunningRunnableInfo> runningRunnables = new();

    private BricksOptions Options { get; }
    public IPackageManager PackageManager { get; private set; }
    public IEnumerable<RunnableInfo> InstalledRunnables { get; private set; } = Enumerable.Empty<RunnableInfo>();
    public IEnumerable<RunningRunnableInfo> RunningRunnables => runningRunnables;
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

    public async Task<RunningRunnableInfo?> RunAsync(RunnableInfo runnableInfo, string[]? args = null, CancellationToken cancellationToken = default) {
      Guard.Argument(runnableInfo, nameof(runnableInfo)).NotNull();

      Task? clientTerminated = null;
      if (runnableInfo.Kind == RunnableKind.ConsoleApplication) {
        using IChannel channel = CreateRunnerChannel(RunnableSettings[runnableInfo].Isolation, runnableInfo.DockerImage);
        channel.Open(out clientTerminated, cancellationToken);

        await channel.SendMessageAsync(MessageFactory.LoadPackages(PackageManager.GetPackageLoadInfos()), cancellationToken);
        await channel.ReceiveMessageAsync(Message.Commands.PackagesLoaded, cancellationToken);
        await channel.SendMessageAsync(MessageFactory.RunRunnable(runnableInfo, args ?? Array.Empty<string>()), cancellationToken);

        if (channel is ProcessChannel processChannel) {
          Task reader = Task.Run(() => {
            int ch = processChannel.StandardOutput.Read();
            while ((ch != -1) && !cancellationToken.IsCancellationRequested) {
              Console.Write((char)ch);
              ch = processChannel.StandardOutput.Read();
            }
          }, cancellationToken);

          //// alternative code for writer -> polling from console
          //// TODO: decide which version to use
          //Task writer = Task.Run(() => {
          //  while (Status == RunnerStatus.Running) {
          //    if (Console.KeyAvailable) {
          //      string s = Console.ReadLine();
          //      process.StandardInput.WriteLine(s);
          //    } else {
          //      while (!Console.KeyAvailable && (Status == RunnerStatus.Running)) {
          //        Task.Delay(250).Wait();
          //      }
          //    }
          //  }
          //}, cancellationToken);

          Task writer = Task.Run(() => {
            string? s = Console.ReadLine();
            while ((s != null) && !cancellationToken.IsCancellationRequested) {
              processChannel.StandardInput.WriteLine(s);
              s = Console.ReadLine();
            }
          }, cancellationToken);

          await reader;

          // not needed if writer polls
          TextReader stdIn = Console.In;
          Console.SetIn(new StringReader(""));
          Console.WriteLine($"Application {runnableInfo.Name} terminated. Press ENTER to continue.");

          await writer;

          // not needed if writer polls
          Console.SetIn(stdIn);
        }
        await (clientTerminated ?? throw new InvalidOperationException($"{nameof(clientTerminated)} is null"));
        return null;
      } else {  // not a ConsoleApplication
        IChannel channel = CreateRunnerChannel(RunnableSettings[runnableInfo].Isolation, runnableInfo.DockerImage);
        channel.Open(out clientTerminated, cancellationToken);

        await channel.SendMessageAsync(MessageFactory.LoadPackages(PackageManager.GetPackageLoadInfos()), cancellationToken);
        await channel.ReceiveMessageAsync(Message.Commands.PackagesLoaded, cancellationToken);
        await channel.SendMessageAsync(MessageFactory.RunRunnable(runnableInfo, args ?? Array.Empty<string>()), cancellationToken);

        CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.Token.Register(async () => {
          if (!clientTerminated.IsCompleted) {
            await channel.SendMessageAsync(MessageFactory.Terminate(), cancellationToken);
          }
        });

        MessageHandler handler = new();
        Task messageHandlerTerminated = Task.Run(async () => {
          await handler.ReceiveMessagesAsync(channel, cts.Token);
        }, cts.Token);

        Task cleanupTerminated = clientTerminated.ContinueWith(t => {
          cts.Cancel();
        }, cts.Token);

        RunningRunnableInfo runningRunnableInfo = new(runnableInfo, channel, handler, cts, clientTerminated);
        runningRunnables.Add(runningRunnableInfo);
        Task allTerminated = Task.WhenAll(cleanupTerminated, messageHandlerTerminated).ContinueWith(t => {
          channel.Close();
          runningRunnables.Remove(runningRunnableInfo);
        }, cancellationToken);

        runningRunnableInfo.Terminated = allTerminated;
        return runningRunnableInfo;
      }
    }

    public async Task RunAutoStartAsync(string[]? args = null, CancellationToken cancellationToken = default) {
      List<Task> tasks = new();
      foreach (var runnable in RunnableSettings.Where(x => x.Value.AutoStart).Select(x => x.Key)) {
        tasks.Add(RunAsync(runnable, args, cancellationToken));
      }
      await Task.WhenAll(tasks);
    }

    public async Task ReloadAsync(CancellationToken cancellationToken = default) {
      using IChannel channel = CreateRunnerChannel(Options.DefaultIsolation);
      Task? clientTerminated = null;
      try {
        channel.Open(out clientTerminated, cancellationToken);

        await channel.SendMessageAsync(MessageFactory.LoadPackages(PackageManager.GetPackageLoadInfos()), cancellationToken);
        await channel.ReceiveMessageAsync(Message.Commands.PackagesLoaded, cancellationToken);
        await channel.SendMessageAsync(MessageFactory.DiscoverRunnables(), cancellationToken);
        InstalledRunnables = await channel.ReceiveMessageAsync<IEnumerable<RunnableInfo>>(Message.Commands.RunnablesDiscovered, cancellationToken);
        await channel.SendMessageAsync(MessageFactory.Terminate(), cancellationToken);
        await (clientTerminated ?? throw new InvalidOperationException($"{nameof(clientTerminated)} is null"));

        Dictionary<RunnableInfo, RunnableSettings> settings = new();
        foreach (var runnable in InstalledRunnables) {
          settings.Add(runnable, new RunnableSettings {
            Isolation = Options.DefaultIsolation,
            AutoStart = runnable.AutoStart
          });
        }
        RunnableSettings = settings;
      }
      finally {
        channel.Close();
        await (clientTerminated ?? Task.CompletedTask);
      }
    }

    private IChannel CreateRunnerChannel(Isolation isolation, string dockerImage = "") {
      if (string.IsNullOrWhiteSpace(dockerImage)) dockerImage = Options.DefaultDockerImage;

      static async Task MemoryChannelClientCode(IChannel channel, CancellationToken cancellationToken) {
        await MessageHandler.Factory.ClientMessageHandler().ReceiveMessagesAsync(channel, cancellationToken);
      }

      return isolation switch {
        Isolation.None => new MemoryChannel((channel, token) => MemoryChannelClientCode(channel, token).Wait(token)),
        Isolation.AnonymousPipes => new AnonymousPipesProcessChannel(Options.DotnetCommand, $"\"{Options.StarterAssembly}\""),
        Isolation.StdInOut => new StdInOutProcessChannel(Options.DotnetCommand, $"\"{Options.StarterAssembly}\""),
        Isolation.Docker => new DockerChannel(Options.DockerCommand, dockerImage, Options.UseWindowsContainer, Options.AppPath, "dotnet", $"\"{Options.StarterAssembly}\""),
        _ => throw new NotSupportedException($"Isolation {isolation} is not supported."),
      };
    }
  }
}
