#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace HEAL.Bricks {
  [Serializable]
  public class DiscoverApplicationsRunner : ProcessRunner {
    public ISettings Settings { get; }

    public DiscoverApplicationsRunner(ISettings settings, ProcessRunnerStartInfo startInfo = null) : base(startInfo ?? new ProcessRunnerStartInfo()) {
      Settings = settings;
    }

    protected override void Process() {
      IPluginManager pluginManager = PluginManager.Create(Settings);
      pluginManager.Initialize();

      if (pluginManager.Status != PluginManagerStatus.OK) {
        SendException(new InvalidOperationException($"{nameof(PluginManager)}.{nameof(pluginManager.Status)} is not {nameof(PluginManagerStatus.OK)}."));
      }

      pluginManager.LoadPackageAssemblies();

      ITypeDiscoverer typeDiscoverer = TypeDiscoverer.Create();
      IEnumerable<IApplication> applications = typeDiscoverer.GetInstances<IApplication>();
      ApplicationInfo[] applicationInfos = applications.Select(x => new ApplicationInfo(x)).OrderBy(x => x.Name).ToArray();
      SendMessage(new DiscoveredApplicationsMessage(applicationInfos));
    }
  }
}
