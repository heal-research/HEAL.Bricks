#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  [Serializable]
  public abstract class PluginManagerProcessRunner : ProcessRunner {
    public ISettings Settings { get; }

    protected PluginManagerProcessRunner(ISettings settings, IProcessRunnerStartInfo startInfo) : base(startInfo) {
      Settings = settings;
    }

    protected sealed override void ExecuteOnClient() {
      IPluginManager pluginManager = PluginManager.Create(Settings);
      pluginManager.Initialize();

      if (pluginManager.Status != PluginManagerStatus.OK) {
        SendException(new InvalidOperationException($"{nameof(PluginManager)}.{nameof(pluginManager.Status)} is not {nameof(PluginManagerStatus.OK)}."));
      }

      ExecuteOnClient(pluginManager);
    }
    protected abstract void ExecuteOnClient(IPluginManager pluginManager);
  }
}
