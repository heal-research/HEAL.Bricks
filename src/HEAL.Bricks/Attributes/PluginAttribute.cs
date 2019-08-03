#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;

namespace HEAL.Bricks {
  [AttributeUsage(AttributeTargets.Class)]
  public class PluginAttribute : Attribute {
    public string Name { get; }
    public string Description { get; }
    public Version Version { get; }

    public PluginAttribute(string name, string version, string description = "") {
      #region Parameter Validation
      if (string.IsNullOrWhiteSpace(name)) {
        throw (name == null) ? new ArgumentNullException(nameof(name)) :
                               new ArgumentException($"{nameof(PluginAttribute)}.{nameof(Name)} must not be empty or all whitespace.", nameof(name));
      }
      if (string.IsNullOrWhiteSpace(version)) {
        throw (version == null) ? new ArgumentNullException(nameof(version)) :
                                  new ArgumentException($"{nameof(PluginAttribute)}.{nameof(Version)} must not be empty or all whitespace.", nameof(version));
      }
      if (description == null) throw new ArgumentNullException(nameof(description));
      #endregion

      Name = name;
      Description = description;
      try {
        Version = new Version(version);
      } catch (Exception e) {
        // throws exception if version string cannot be parsed into a valid version
        throw new ArgumentException($"{nameof(PluginAttribute)}.{nameof(Version)} cannot be parsed into a valid version", nameof(version), e);
      }
    }
  }
}
