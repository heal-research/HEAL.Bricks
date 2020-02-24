#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HEAL.Bricks {
  /// <summary>
  /// Factory to create an instance of a plugin loader. 
  /// </summary>
  public static class PluginLoaderFactory {
    /// <summary>
    /// Creates a new instance of a plugin loader.
    /// </summary>
    public static IPluginLoader Create() {
      return new PluginLoader(new AssemblyLoader());
    }
  }
}
