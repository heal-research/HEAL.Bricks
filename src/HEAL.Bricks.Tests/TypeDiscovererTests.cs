#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;

namespace HEAL.Bricks.Tests {
  [TestClass]
  public class TypeDiscovererTests {
    public TestContext TestContext { get; set; }

    #region TestCreate
    [TestMethod]
    public void TestCreate() {
      ITypeDiscoverer td = TypeDiscoverer.Create();
      Assert.IsNotNull(td);
      Assert.AreEqual(typeof(TypeDiscoverer), td.GetType());
    }
    #endregion

    #region TestGetTypes
    public interface I1 { }
    public interface I2 { }
    public class A : I1 { }
    public class B : I1 { }
    public class C<T> : I1 { }
    public class D : I2 { }
    public class E : I1, I2 { }

    [TestMethod]
    public void TestGetTypes() {
      Assembly assembly = Assembly.GetExecutingAssembly();
      ITypeDiscoverer td = TypeDiscoverer.Create();

      CollectionAssert.AreEquivalent(new[] { typeof(A), typeof(B), typeof(E) }, td.GetTypes(typeof(I1)).ToArray());
      CollectionAssert.AreEquivalent(new[] { typeof(A), typeof(B), typeof(E) }, td.GetTypes(typeof(I1), onlyInstantiable: true, excludeGenericTypeDefinitions: true).ToArray());
      CollectionAssert.AreEquivalent(new[] { typeof(I1), typeof(A), typeof(B), typeof(E) }, td.GetTypes(typeof(I1), onlyInstantiable: false, excludeGenericTypeDefinitions: true).ToArray());
      CollectionAssert.AreEquivalent(new[] { typeof(A), typeof(B), typeof(C<>), typeof(E) }, td.GetTypes(typeof(I1), onlyInstantiable: true, excludeGenericTypeDefinitions: false).ToArray());
      CollectionAssert.AreEquivalent(new[] { typeof(I1), typeof(A), typeof(B), typeof(C<>), typeof(E) }, td.GetTypes(typeof(I1), onlyInstantiable: false, excludeGenericTypeDefinitions: false).ToArray());

      CollectionAssert.AreEquivalent(new[] { typeof(E) }, td.GetTypes(new[] { typeof(I1), typeof(I2) }).ToArray());
      CollectionAssert.AreEquivalent(new[] { typeof(E) }, td.GetTypes(new[] { typeof(I1), typeof(I2) }, assignableToAllTypes: true).ToArray());
      CollectionAssert.AreEquivalent(new[] { typeof(A), typeof(B), typeof(D), typeof(E) }, td.GetTypes(new[] { typeof(I1), typeof(I2) }, assignableToAllTypes: false).ToArray());

      CollectionAssert.AreEquivalent(new[] { typeof(A), typeof(B), typeof(E) }, td.GetTypes(typeof(I1), assembly).ToArray());
      CollectionAssert.AreEquivalent(new[] { typeof(A), typeof(B), typeof(E) }, td.GetTypes(typeof(I1), assembly, onlyInstantiable: true, excludeGenericTypeDefinitions: true).ToArray());
      CollectionAssert.AreEquivalent(new[] { typeof(I1), typeof(A), typeof(B), typeof(E) }, td.GetTypes(typeof(I1), assembly, onlyInstantiable: false, excludeGenericTypeDefinitions: true).ToArray());
      CollectionAssert.AreEquivalent(new[] { typeof(A), typeof(B), typeof(C<>), typeof(E) }, td.GetTypes(typeof(I1), assembly, onlyInstantiable: true, excludeGenericTypeDefinitions: false).ToArray());
      CollectionAssert.AreEquivalent(new[] { typeof(I1), typeof(A), typeof(B), typeof(C<>), typeof(E) }, td.GetTypes(typeof(I1), assembly, onlyInstantiable: false, excludeGenericTypeDefinitions: false).ToArray());

      CollectionAssert.AreEquivalent(new[] { typeof(E) }, td.GetTypes(new[] { typeof(I1), typeof(I2) }, assembly).ToArray());
      CollectionAssert.AreEquivalent(new[] { typeof(E) }, td.GetTypes(new[] { typeof(I1), typeof(I2) }, assembly, assignableToAllTypes: true).ToArray());
      CollectionAssert.AreEquivalent(new[] { typeof(A), typeof(B), typeof(D), typeof(E) }, td.GetTypes(new[] { typeof(I1), typeof(I2) }, assembly, assignableToAllTypes: false).ToArray());
    }
    #endregion

    #region TestGetInstances
    [TestMethod]
    public void TestGetInstances() {
      ITypeDiscoverer td = TypeDiscoverer.Create();
      CollectionAssert.AreEquivalent(new[] { typeof(A), typeof(B), typeof(E) }, td.GetInstances<I1>().Select(x => x.GetType()).ToArray());
    }
    #endregion

  }
}
