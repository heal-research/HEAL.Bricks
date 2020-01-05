using System;

namespace HEAL.Calculator.Operators.Division {
  public class Division : IBinaryOperator {
    public double Apply(double x, double y) {
      return x / y;
    }
  }
}
