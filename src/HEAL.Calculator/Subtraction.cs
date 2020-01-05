using System;

namespace HEAL.Calculator.Operators.Subtraction {
  public class Subtraction : IBinaryOperator {
    public double Apply(double x, double y) {
      return x - y;
    }
  }
}
