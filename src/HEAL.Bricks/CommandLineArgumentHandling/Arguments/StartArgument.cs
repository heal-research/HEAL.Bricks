#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;

namespace HEAL.Bricks {
  [Serializable]
  public class StartArgument : CommandLineArgument<string> {
    public const string TOKEN = "start";

    public StartArgument(string value)
      : base(value) {
    }

    protected override bool CheckValidity() {
      return !string.IsNullOrEmpty(Value) && !string.IsNullOrWhiteSpace(Value);
    }
  }
}
