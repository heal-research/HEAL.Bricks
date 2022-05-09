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
  public sealed class DiscoverApplicationsRunner : PackageLoaderRunner {
    [NonSerialized]
    private ApplicationInfo[] applicationInfos;

    public DiscoverApplicationsRunner(IEnumerable<PackageLoadInfo> packages) : base(packages) { }

    public async Task<ApplicationInfo[]> GetApplicationsAsync(IChannel channel, CancellationToken cancellationToken = default) {
      Guard.Argument(channel, nameof(channel)).NotNull();

      await RunAsync(channel, cancellationToken);
      return applicationInfos;
    }

    protected override async Task ExecuteOnClientAsync(IChannel channel, CancellationToken cancellationToken) {
      await base.ExecuteOnClientAsync(channel, cancellationToken);
      ITypeDiscoverer typeDiscoverer = new TypeDiscoverer();
      IEnumerable<IApplication> applications = typeDiscoverer.GetInstances<IApplication>();
      ApplicationInfo[] applicationInfos = applications.Select(x => new ApplicationInfo(x)).OrderBy(x => x.Name).ToArray();
      await channel.SendMessageAsync(new DiscoveredApplicationsMessage(applicationInfos), cancellationToken);
    }

    protected override async Task ExecuteOnHostAsync(IChannel channel, CancellationToken cancellationToken) {
      applicationInfos = (await channel.ReceiveMessageAsync<DiscoveredApplicationsMessage>(cancellationToken)).Data;
    }
  }
}
