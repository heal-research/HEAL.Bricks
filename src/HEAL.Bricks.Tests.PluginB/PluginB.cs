using System.Diagnostics;
using System.Reflection;

namespace HEAL.Bricks.Tests.PluginB {
  public class PluginB : IPlugin {
    public string Name => "PluginB";
    public string Description => "Description of Plugin B";

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
