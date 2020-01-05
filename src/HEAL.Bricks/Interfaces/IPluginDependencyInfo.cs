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
  public interface IPluginDependencyInfo {
    string Name { get; }
    string VersionRange { get; }
    string MinVersion { get; }

    string ToString();
  }
}
