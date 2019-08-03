#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;

namespace HEAL.Bricks {
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
  public class PluginFileAttribute : Attribute {
    public string Name { get; }
    public PluginFileType Type { get; }

    public PluginFileAttribute(string name, PluginFileType type = PluginFileType.Assembly) {
      #region Parameter Validation
      // NB: does not check if file with given fileName actually exists
      if (string.IsNullOrWhiteSpace(name)) {
        throw (name == null) ? new ArgumentNullException(nameof(name)) :
                               new ArgumentException($"{nameof(PluginFileAttribute)}.{nameof(name)} not be empty or all whitespace.", nameof(name));
      }
      #endregion

      Name = name;
      Type = type;
    }
  }
}
