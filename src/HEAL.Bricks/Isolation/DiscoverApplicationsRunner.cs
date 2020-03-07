#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  [Serializable]
  public sealed class DiscoverApplicationsRunner : PluginManagerProcessRunner {
    [NonSerialized]
    private ApplicationInfo[] applicationInfos;

    public DiscoverApplicationsRunner(ISettings settings, IProcessRunnerStartInfo startInfo = null) : base(settings, startInfo ?? new NetCoreEntryAssemblyStartInfo()) { }

    public async Task<ApplicationInfo[]> GetApplicationsAsync(CancellationToken cancellationToken = default) {
      if (Status == RunnerStatus.Created) await RunAsync(cancellationToken);
      return applicationInfos;
    }

    protected override async Task ExecuteOnClientAsync(IPluginManager pluginManager, CancellationToken cancellationToken) {
      ITypeDiscoverer typeDiscoverer = TypeDiscoverer.Create();
      IEnumerable<IApplication> applications = typeDiscoverer.GetInstances<IApplication>();
      ApplicationInfo[] applicationInfos = applications.Select(x => new ApplicationInfo(x)).OrderBy(x => x.Name).ToArray();
      await SendMessageAsync(new DiscoveredApplicationsMessage(applicationInfos), cancellationToken);
    }

    protected override async Task ExecuteOnHostAsync(CancellationToken cancellationToken) {
      applicationInfos = (await ReceiveMessageAsync<DiscoveredApplicationsMessage>(cancellationToken)).Data;
    }
  }
}
