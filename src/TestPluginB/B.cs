using HEAL.Bricks;
using System;
using System.Collections.Generic;
using System.Text;
using TestPluginA;

namespace TestPluginB {
  public class B : A {
    public override string ToString() {
      return base.ToString() + " - B";
    }
  }
}
