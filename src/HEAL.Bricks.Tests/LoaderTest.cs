#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace HEAL.Bricks.Tests {
  [TestClass]
  public class LoaderTests {
    [TestMethod]
    public void LoaderTest() {
      var path = Directory.GetCurrentDirectory();

      var loader = new AssemblyLoader();
      loader.LoadAssemblies(path);

      foreach (var asm in loader.Assemblies)
        Trace.WriteLine(asm.FullName);

      Trace.WriteLine("");

      foreach (var t in loader.Types)
        Trace.WriteLine(t.FullName);

      Trace.WriteLine("");
    }
  }
}
