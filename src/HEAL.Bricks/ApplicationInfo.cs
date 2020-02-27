#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;

namespace HEAL.Bricks {
  [Serializable]
  public class ApplicationInfo {
    public string Name { get; }
    public string Description { get; }
    public string TypeName { get; }

    internal ApplicationInfo(IApplication application) {
      Name = application.Name;
      Description = application.Description;
      TypeName = application.GetType().FullName;
    }

    public override string ToString() {
      return Name;
    }
  }
}
