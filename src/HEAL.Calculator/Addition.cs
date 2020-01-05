using System;

namespace HEAL.Calculator.Operators.Addition {
  public class Addition : IBinaryOperator {
    public double Apply(double x, double y) {
      return x + y;
    }
  }
}
