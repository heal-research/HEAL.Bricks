#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using System;

namespace HEAL.Bricks {
  public class MessageHandler {
    public Func<PackageLoadInfo[], IChannel, CancellationToken, Task>? LoadPackagesAsync { get; set; }
    public Func<Task>? PackagesLoadedAsync { get; set; }
    public Func<IChannel, CancellationToken, Task>? DiscoverRunnablesAsync { get; set; }
    public Func<RunnableInfo[], Task>? RunnablesDiscoveredAsync { get; set; }
    public Func<Tuple<RunnableInfo, string[]>, IChannel, CancellationTokenSource, Task>? RunRunnableAsync { get; set; }
    public Func<string, Task>? LogAsync { get; set; }
    public Func<CancellationTokenSource, Task>? TerminateAsync { get; set; }
    public Func<IMessage, Task>? CustomMessageAsync { get; set; }

    public MessageHandler() { }

    public async Task ReceiveMessagesAsync(IChannel channel, CancellationToken cancellationToken = default) {
      Guard.Argument(channel, nameof(channel)).NotNull();
      CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

      while (!cts.Token.IsCancellationRequested) {
        IMessage message = await channel.ReceiveMessageAsync(cancellationToken);
        await ProcessMessageAsync(message, channel, cts);
      }
    }

    private async Task ProcessMessageAsync(IMessage message, IChannel channel, CancellationTokenSource cts) {
      Guard.Argument(message, nameof(message)).NotNull().Member(m => m.Command, x => x.NotNull().NotEmpty().NotWhiteSpace());

      Task? t = message.Command switch {
        Message.Commands.LoadPackages        => LoadPackagesAsync?.Invoke(message.DeserializePayload<PackageLoadInfo[]>(), channel, cts.Token),
        Message.Commands.PackagesLoaded      => PackagesLoadedAsync?.Invoke(),
        Message.Commands.DiscoverRunnables   => DiscoverRunnablesAsync?.Invoke(channel, cts.Token),
        Message.Commands.RunnablesDiscovered => RunnablesDiscoveredAsync?.Invoke(message.DeserializePayload<RunnableInfo[]>()),
        Message.Commands.RunRunnable         => RunRunnableAsync?.Invoke(message.DeserializePayload<Tuple<RunnableInfo, string[]>>(), channel, cts),
        Message.Commands.Log                 => LogAsync?.Invoke(message.Payload),
        Message.Commands.Terminate           => TerminateAsync?.Invoke(cts),
        _ => CustomMessageAsync?.Invoke(message)
      };
      if (t == null) throw new NotSupportedException($"Message handler for message '{message.Command}' is not defined.");
      await t;
    }

    #region DefaultHandlers
    public static class DefaultHandlers {
      public static async Task LoadPackagesAsync(PackageLoadInfo[] packages, IChannel channel, CancellationToken cancellationToken) {
        PackageLoader.LoadPackageAssemblies(packages);
        await channel.SendMessageAsync(Message.Factory.PackagesLoaded(), cancellationToken);
      }

      public static Task PackagesLoadedAsync() {
        return Task.CompletedTask;
      }

      public static async Task DiscoverRunnablesAsync(IChannel channel, CancellationToken cancellationToken) {
        ITypeDiscoverer typeDiscoverer = new TypeDiscoverer();
        IEnumerable<RunnableInfo> runnables = typeDiscoverer.GetInstances<IRunnable>().Select(x => new RunnableInfo(x));
        await channel.SendMessageAsync(Message.Factory.RunnablesDiscovered(runnables), cancellationToken);
      }

      public static Task RunnablesDiscoveredAsync(RunnableInfo[] _) {
        return Task.CompletedTask;
      }

      public static async Task RunRunnableAsync(Tuple<RunnableInfo, string[]> info, IChannel channel, CancellationTokenSource cts) {
        (RunnableInfo runnableInfo, string[] args) = info;
        IRunnable runnable = runnableInfo.CreateRunnable();
        await runnable.StartAsync(args, cts.Token);
        cts.Cancel();
      }

      public static Task LogAsync(string _) {
        return Task.CompletedTask;
      }

      public static Task TerminateAsync(CancellationTokenSource cts) {
        cts.Cancel();
        return Task.CompletedTask;
      }

      public static Task UnknownMessage(IMessage message) {
        throw new NotSupportedException($"Unknown message command '{message.Command}'.");
      }
    }
    #endregion

    #region Factory
    public static class Factory {
      public static MessageHandler ClientMessageHandler() {
        return new MessageHandler {
          LoadPackagesAsync      = DefaultHandlers.LoadPackagesAsync,
          DiscoverRunnablesAsync = DefaultHandlers.DiscoverRunnablesAsync,
          RunRunnableAsync       = DefaultHandlers.RunRunnableAsync,
          TerminateAsync         = DefaultHandlers.TerminateAsync,
          CustomMessageAsync     = DefaultHandlers.UnknownMessage
        };
      }

      public static MessageHandler HostMessageHandler() {
        return new MessageHandler {
          PackagesLoadedAsync      = DefaultHandlers.PackagesLoadedAsync,
          RunnablesDiscoveredAsync = DefaultHandlers.RunnablesDiscoveredAsync,
          LogAsync                 = DefaultHandlers.LogAsync,
          TerminateAsync           = DefaultHandlers.TerminateAsync,
          CustomMessageAsync       = DefaultHandlers.UnknownMessage,
        };
      }
    }
    #endregion
  }
}
