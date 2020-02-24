#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.IO;

namespace HEAL.Bricks {
  [Serializable]
  public class OpenArgument : CommandLineArgument<string> {
    public OpenArgument(string value)
      : base(value) {
    }

    protected override bool CheckValidity() {
      return File.Exists(Value);
    }
  }
}
