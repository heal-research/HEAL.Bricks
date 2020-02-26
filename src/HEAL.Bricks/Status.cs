#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks {
  public enum PackageStatus {
    OK,
    Outdated,
    DependenciesMissing,
    IndirectDependenciesMissing,
    IncompatibleFramework,
    Loaded,
    Unknown
  }
  public enum PackageDependencyStatus {
    OK,
    Missing,
    Unknown
  }
  public enum PluginManagerStatus {
    Uninitialized,
    OK,
    InvalidPlugins,
    Unknown
  }
  public enum RunnerStatus {
    Created,
    Cancelled,
    Starting,
    Running,
    Stopped
  }
}
