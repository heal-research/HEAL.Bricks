#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks {
  public interface IProcessRunnerStartInfo {
    string ProgramPath { get; }
    string Arguments { get; }
    string UserName { get; }
    string UserDomain { get; }
    string UserPassword { get; }
  }
}