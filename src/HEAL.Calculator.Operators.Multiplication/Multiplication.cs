using System;

namespace HEAL.Calculator.Operators.Multiplication {
  public class Multiplication : IBinaryOperator {
    public double Apply(double x, double y) {
      return x * y;
    }
  }
}
