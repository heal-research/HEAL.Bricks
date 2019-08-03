#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;

namespace HEAL.Bricks {
  [Serializable]
  public class PluginFile : IPluginFile {
    public string Name { get; }
    public PluginFileType Type { get; }

    public PluginFile(string name, PluginFileType type) {
      Name = name;
      Type = type;
    }

    public override string ToString() {
      return Name;
    }
  }
}
