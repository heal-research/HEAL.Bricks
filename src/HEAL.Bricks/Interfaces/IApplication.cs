#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks {
  public interface IApplication {
    string Name { get; }
    string Description { get; }
    void Run(ICommandLineArgument[] args);

    void OnCancel();
    void OnPause();
    void OnResume();
  }
}
