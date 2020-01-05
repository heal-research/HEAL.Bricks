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
