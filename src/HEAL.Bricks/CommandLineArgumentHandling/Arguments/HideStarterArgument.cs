#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;

namespace HEAL.Bricks {
  [Serializable]
  public class HideStarterArgument : CommandLineArgument<bool> {
    public const string TOKEN = "hideStarter";

    public HideStarterArgument(string value)
      : base(string.IsNullOrEmpty(value)) {
    }

    protected override bool CheckValidity() {
      return Value;
    }
  }
}
