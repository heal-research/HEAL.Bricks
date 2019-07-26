#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;

namespace HEAL.Bricks {
  [AttributeUsage(AttributeTargets.Class)]
  public sealed class PluginAttribute : Attribute {
    public string Name { get; }
    public string Description { get; }
    public Version Version { get; }

    public PluginAttribute(string name, string version) : this(name, string.Empty, version) { }
    public PluginAttribute(string name, string description, string version) {
      #region Parameter Validation
      if (string.IsNullOrWhiteSpace(name)) {
        throw (name == null) ? new ArgumentNullException(nameof(name)) :
                               new ArgumentException("PluginAttribute name must not be empty or all whitespace.", nameof(name));
      }
      if (description == null) throw new ArgumentNullException(nameof(description));
      if (string.IsNullOrWhiteSpace(version)) {
        throw (version == null) ? new ArgumentNullException(nameof(version)) :
                                  new ArgumentException("PluginAttribute version must not be empty or all whitespace.", nameof(version));
      }
      #endregion
      Name = name;
      Description = description;
      Version = new Version(version); // throws format exception if the version string cannot be parsed
    }
  }
}
