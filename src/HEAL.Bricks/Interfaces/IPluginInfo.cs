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
  public interface IPluginInfo {
    string Name { get; }
    string Version { get; }
    string Description { get; }
    IEnumerable<IPluginDependencyInfo> GetDependencies();
    string ToString();
    string ToStringWithDependencies();
  }
}
