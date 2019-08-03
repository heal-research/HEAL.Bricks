#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;

namespace HEAL.Bricks {
  public interface IPluginDescription {
    string Name { get; }
    Version Version { get; }
    string Description { get; }

    IEnumerable<IPluginDescription> Dependencies { get; }
    IEnumerable<IPluginFile> Files { get; }

    string ContactName { get; }
    string ContactEmail { get; }

    string LicenseText { get; }

    PluginState PluginState { get; }

    string LoadingErrorInformation { get; }
  }
}
