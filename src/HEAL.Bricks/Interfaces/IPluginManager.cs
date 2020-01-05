#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  public interface IPluginManager {
    IEnumerable<string> RemoteRepositories { get; }

    Task<IEnumerable<IPluginInfo>> GetLocalPluginsAsync(string pluginTag, CancellationToken cancellationToken = default);
    Task<IEnumerable<IPluginInfo>> GetLocalPluginDependenciesAsync(string pluginTag, CancellationToken cancellationToken = default);
    Task<IEnumerable<IPluginInfo>> GetRemotePluginsAsync(string searchString, bool includePreReleases = true, bool allVersions = true, CancellationToken cancellationToken = default);
    Task<bool> DownloadPluginAsync(IPluginInfo pluginInfo, string targetFolder, string packageSource = "https://api.nuget.org/v3/index.json", CancellationToken cancellationToken = default);
  }
}