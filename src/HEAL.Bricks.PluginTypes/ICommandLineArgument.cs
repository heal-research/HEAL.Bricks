#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks {
  public interface ICommandLineArgument {
    object Value { get; }
    bool Valid { get; }
  }

  public interface ICommandLineArgument<T> : ICommandLineArgument {
    new T Value { get; }
  }
}
