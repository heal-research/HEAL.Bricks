#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.Diagnostics;
using System.Reflection;

namespace HEAL.Bricks.Tests.PluginA {
  public class PluginA : IPlugin {
    public string Name => "PluginA";
    public string Description => "Description of Plugin A";

    public void OnLoad() {
      throw new System.NotImplementedException();
    }
    public void OnUnload() {
      throw new System.NotImplementedException();
    }

    public override string ToString() {
      Assembly assembly = Assembly.GetExecutingAssembly();
      FileVersionInfo version = FileVersionInfo.GetVersionInfo(assembly.Location);
      return Name + "_" + version.ProductVersion;
    }
  }
}
