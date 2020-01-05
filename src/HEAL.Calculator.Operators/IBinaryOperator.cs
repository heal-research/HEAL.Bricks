#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;

namespace HEAL.Calculator.Operators {
  public interface IBinaryOperator {
    double Apply(double x, double y);
  }
}
