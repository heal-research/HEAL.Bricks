#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks {
  public interface IMessageHandler {
    Func<PackageLoadInfo[], IChannel, CancellationToken, Task>? LoadPackagesAsync { get; set; }
    Func<Task>? PackagesLoadedAsync { get; set; }
    Func<IChannel, CancellationToken, Task>? DiscoverRunnablesAsync { get; set; }
    Func<RunnableInfo[], Task>? RunnablesDiscoveredAsync { get; set; }
    Func<Tuple<RunnableInfo, string[]>, IChannel, CancellationTokenSource, Task>? RunRunnableAsync { get; set; }
    Func<string, Task>? LogAsync { get; set; }
    Func<CancellationTokenSource, Task>? TerminateAsync { get; set; }
    Func<IMessage, Task>? CustomMessageAsync { get; set; }
  }
}
