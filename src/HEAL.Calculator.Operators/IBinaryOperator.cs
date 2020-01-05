using System;

namespace HEAL.Calculator.Operators {
  public interface IBinaryOperator {
    double Apply(double x, double y);
  }
}
