#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;

namespace HEAL.Bricks {
  /// <summary>
  /// Enumerator of available file types for a plugin.
  /// </summary>
  public enum PluginFileType {
    /// <summary>
    /// CLR assembly files are loaded by the plugin infrastructure.
    /// </summary>
    Assembly,
    /// <summary>
    /// Native DLL files are ignored by the plugin infrastructure.
    /// </summary>
    NativeDll,
    /// <summary>
    /// Data files are any kind of support file for your plugin.
    /// </summary>
    Data,
    /// <summary>
    /// License files contain the license text of the plugin (ASCII encoding).
    /// </summary>
    License
  };

  /// <summary>
  /// PluginFileAttribute can be used to declare which files make up an plugin.
  /// Multiple files can be associated to an plugin. Each file should be associated to only one plugin.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
  public sealed class PluginFileAttribute : System.Attribute {
    private string fileName;

    /// <summary>
    /// Gets the file name of the plugin.
    /// </summary>
    public string FileName {
      get { return fileName; }
    }

    private PluginFileType fileType = PluginFileType.Data;

    /// <summary>
    /// Gets the file type of the plugin file.
    /// </summary>
    public PluginFileType FileType {
      get { return fileType; }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="PluginFileAttribute"/>.
    /// <param name="fileName">Name of the file</param>
    /// <param name="fileType">Type of the file (Assembly, NativeDll, Data, License)</param>
    /// </summary>
    public PluginFileAttribute(string fileName, PluginFileType fileType) {
      if (string.IsNullOrEmpty(fileName)) throw new ArgumentException("File name is empty.", "fileName");
      // NB: doesn't check if the file actually exists
      this.fileName = fileName;
      this.fileType = fileType;
    }
  }
}
