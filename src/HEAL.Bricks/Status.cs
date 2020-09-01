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
    Undefined
  }
  public enum PackageDependencyStatus {
    OK,
    Missing,
    Undefined
  }
  public enum PackageManagerStatus {
    OK,
    InvalidPackages,
    Undefined
  }
  public enum RunnerStatus {
    Created,
    Starting,
    Running,
    Stopped,
    Canceled,
    Faulted
  }
}
