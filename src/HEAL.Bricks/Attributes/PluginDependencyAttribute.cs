#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;

namespace HEAL.Bricks {
  /// <summary>
  /// This attribute can be used to declare that a plugin depends on a another plugin.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
  public sealed class PluginDependencyAttribute : System.Attribute {
    private string dependency;

    /// <summary>
    /// Gets the name of the plugin that is needed to load a plugin.
    /// </summary>
    public string Dependency {
      get { return dependency; }
    }

    private Version version;
    /// <summary>
    /// Gets the version of the plugin dependency.
    /// </summary>
    public Version Version {
      get { return version; }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="PluginDependencyAttribute" />.
    /// </summary>
    /// <param name="dependency">Name of the plugin dependency.</param>
    /// <param name="version">Version of the plugin dependency.</param>
    public PluginDependencyAttribute(string dependency, string version) {
      if (string.IsNullOrEmpty(dependency)) throw new ArgumentException("Dependency name is null or empty.", "dependency");
      if (string.IsNullOrEmpty(version)) throw new ArgumentException("Dependency version is null or empty.", "version");
      this.dependency = dependency;
      this.version = new Version(version); // throws format exception if the version string can't be parsed
    }
  }
}
