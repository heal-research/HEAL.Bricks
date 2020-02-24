#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;

namespace HEAL.Bricks {
  [Serializable]
  public abstract class CommandLineArgument<T> : ICommandLineArgument<T> {
    object ICommandLineArgument.Value { get { return Value; } }
    public T Value { get; private set; }
    public bool Valid { get { return CheckValidity(); } }

    protected CommandLineArgument(T value) {
      Value = value;
    }

    public override bool Equals(object obj) {
      if (obj == null || this.GetType() != obj.GetType()) return false;
      var other = (ICommandLineArgument<T>)obj;
      return this.Value.Equals(other.Value);
    }

    public override int GetHashCode() {
      return GetType().GetHashCode() ^ Value.GetHashCode();
    }

    protected abstract bool CheckValidity();
  }
}
