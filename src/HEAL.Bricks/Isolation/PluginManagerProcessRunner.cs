﻿#region License Information
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
  public abstract class PluginManagerProcessRunner : ProcessRunner {
    public ISettings Settings { get; }

    protected PluginManagerProcessRunner(ISettings settings, IProcessRunnerStartInfo startInfo) : base(startInfo) {
      Settings = settings;
    }

    protected sealed override async Task ExecuteOnClientAsync(CancellationToken cancellationToken) {
      IPluginManager pluginManager = PluginManager.Create(Settings);
      if (pluginManager.Status != PluginManagerStatus.OK) {
        throw new InvalidOperationException($"{nameof(PluginManager)}.{nameof(pluginManager.Status)} is not {nameof(PluginManagerStatus.OK)}.");
      }

      pluginManager.LoadPackageAssemblies();
      await ExecuteOnClientAsync(pluginManager, cancellationToken);
    }
    protected abstract Task ExecuteOnClientAsync(IPluginManager pluginManager, CancellationToken cancellationToken);
  }
}
