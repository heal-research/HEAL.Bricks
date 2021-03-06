#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;

namespace HEAL.Bricks {
  [AttributeUsage(AttributeTargets.Class)]
  public class NonDiscoverableTypeAttribute : Attribute {
    public NonDiscoverableTypeAttribute() { }
  }
}
