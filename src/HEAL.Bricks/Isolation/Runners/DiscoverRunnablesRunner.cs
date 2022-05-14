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
  [Serializable]
  public sealed class DiscoverRunnablesRunner : PackageLoaderRunner {
    [NonSerialized]
    private RunnableInfo[] runnableInfos = Array.Empty<RunnableInfo>();

    public DiscoverRunnablesRunner(IEnumerable<PackageLoadInfo> packages) : base(packages) { }

    public async Task<RunnableInfo[]> GetRunnablesAsync(IChannel channel, CancellationToken cancellationToken = default) {
      Guard.Argument(channel, nameof(channel)).NotNull();

      await RunAsync(channel, cancellationToken);
      return runnableInfos;
    }

    protected override async Task ExecuteOnClientAsync(IChannel channel, CancellationToken cancellationToken) {
      await base.ExecuteOnClientAsync(channel, cancellationToken);
      ITypeDiscoverer typeDiscoverer = new TypeDiscoverer();
      IEnumerable<IRunnable> runnables = typeDiscoverer.GetInstances<IRunnable>();
      List<RunnableInfo> runnableInfos = new();
      runnableInfos.AddRange(runnables.OfType<IApplication>().Select(x => new ApplicationInfo(x)));
      runnableInfos.AddRange(runnables.OfType<IService>().Select(x => new ServiceInfo(x)));
      await channel.SendMessageAsync(new DiscoveredRunnablesMessage(runnableInfos.ToArray()), cancellationToken);
    }

    protected override async Task ExecuteOnHostAsync(IChannel channel, CancellationToken cancellationToken) {
      runnableInfos = (await channel.ReceiveMessageAsync<DiscoveredRunnablesMessage>(cancellationToken)).Data;
    }
  }
}
