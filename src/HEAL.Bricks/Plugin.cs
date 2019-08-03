#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks {
  public abstract class Plugin : IPlugin {
    protected Plugin() { }

    private PluginAttribute PluginAttribute {
      get {
        object[] pluginAttributes = this.GetType().GetCustomAttributes(typeof(PluginAttribute), false);
        // exactly one attribute of the type PluginDescriptionAttribute must be given
        if (pluginAttributes.Length == 0) {
          throw new InvalidPluginException("PluginAttribute on type " + this.GetType() + " is missing.");
        } else if (pluginAttributes.Length > 1) {
          throw new InvalidPluginException("Found multiple PluginAttributes on type " + this.GetType());
        }
        return (PluginAttribute)pluginAttributes[0];
      }
    }

    public string Name => PluginAttribute.Name;

    public virtual void OnLoad() { }
    public virtual void OnUnload() { }
  }
}
