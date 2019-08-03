#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;

namespace HEAL.Bricks {
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
  public class PluginDependencyAttribute : Attribute {
    public string Dependency { get; }
    public Version Version { get; }

    public PluginDependencyAttribute(string dependency, string version) {
      #region Parameter Validation
      if (string.IsNullOrWhiteSpace(dependency)) {
        throw (dependency == null) ? new ArgumentNullException(nameof(dependency)) :
                                     new ArgumentException($"{nameof(PluginDependencyAttribute)}.{nameof(Dependency)} must not be empty or all whitespace.", nameof(dependency));
      }
      if (string.IsNullOrWhiteSpace(version)) {
        throw (version == null) ? new ArgumentNullException(nameof(version)) :
                                  new ArgumentException($"{nameof(PluginDependencyAttribute)}.{nameof(Version)} must not be empty or all whitespace.", nameof(version));
      }
      #endregion

      Dependency = dependency;
      try {
        Version = new Version(version);
      } catch (Exception e) {
        // throws exception if version string cannot be parsed into a valid version
        throw new ArgumentException($"{nameof(PluginDependencyAttribute)}.{nameof(Version)} cannot be parsed into a valid version", nameof(version), e);
      }
    }
  }
}
