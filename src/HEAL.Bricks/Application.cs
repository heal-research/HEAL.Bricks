#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;

namespace HEAL.Bricks {
  [Serializable]
  public abstract class Application : IApplication {
    protected Application() { }

    private ApplicationAttribute ApplicationAttribute {
      get {
        object[] appAttributes = GetType().GetCustomAttributes(typeof(ApplicationAttribute), false);

        // exactly one attribute of the type ClassInfoAttribute must be given
        if (appAttributes.Length == 0) {
          throw new InvalidPluginException("ApplicationAttribute on type " + GetType() + " is missing.");
        } else if (appAttributes.Length > 1) {
          throw new InvalidPluginException("Found multiple ApplicationAttributes on type " + GetType());
        }

        return (ApplicationAttribute)appAttributes[0];
      }
    }

    public string Name => ApplicationAttribute.Name;
    public string Description => ApplicationAttribute.Description;

    public abstract void Run(ICommandLineArgument[] args);
    public virtual void OnCancel() { }
    public virtual void OnPause() { }
    public virtual void OnResume() { }
  }
}
