#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace HEAL.Bricks.Tests {
  [TestClass]
  public class TypeExtensionsTests {
    #region TestIsNonDiscoverableType
    private class DiscoverableType { }

    [NonDiscoverableType]
    private class NonDiscoverableType { }

    [TestMethod]
    public void TestIsNonDiscoverableType() {
      Assert.IsTrue(typeof(NonDiscoverableType).IsNonDiscoverableType(), "Type extension IsNonDiscoverableType failed for non-discoverable type.");
      Assert.IsFalse(typeof(DiscoverableType).IsNonDiscoverableType(), "Type extension IsNonDiscoverableType failed for discoverable type.");
    }
    #endregion

    #region TestBuildType
    private class GenericType<T> { }
    private class GenericTypeWithTwoParameters<T, U> { }
    private class GenericTypeWithReferenceTypeConstraint<T> where T : class { }
    private class GenericTypeWithDefaultConstructorConstraint<T> where T : new() { }
    private class GenericTypeWithParameterConstraint<T> where T : IComparable { }
    private class TypeWithoutDefaultConstructor {
#pragma warning disable IDE0060
      public TypeWithoutDefaultConstructor(int i) { }
#pragma warning restore IDE0060
    }

    [TestMethod]
    public void TestBuildType() {
      ArgumentNullException e;
      e = Assert.ThrowsException<ArgumentNullException>(() => { Type t = null; t.BuildType(typeof(object)); });
      Assert.IsFalse(string.IsNullOrEmpty(e.ParamName));
      e = Assert.ThrowsException<ArgumentNullException>(() => { typeof(object).BuildType(null); });
      Assert.IsFalse(string.IsNullOrEmpty(e.ParamName));

      Assert.AreEqual(typeof(object), typeof(object).BuildType(typeof(object)));
      Assert.AreEqual(typeof(GenericType<>), typeof(GenericType<>).BuildType(typeof(GenericType<>)));
      Assert.AreEqual(typeof(GenericType<>), typeof(GenericType<>).BuildType(typeof(object)));
      Assert.AreEqual(null, typeof(GenericType<>).BuildType(typeof(GenericTypeWithTwoParameters<int, int>)));
      Assert.AreEqual(null, typeof(GenericTypeWithReferenceTypeConstraint<>).BuildType(typeof(GenericType<int>)));
      Assert.AreEqual(null, typeof(GenericTypeWithDefaultConstructorConstraint<>).BuildType(typeof(GenericType<TypeWithoutDefaultConstructor>)));
      Assert.AreEqual(null, typeof(GenericTypeWithParameterConstraint<>).BuildType(typeof(GenericType<object>)));

      Assert.AreEqual(typeof(GenericType<int>), typeof(GenericType<>).BuildType(typeof(List<int>)));
      Assert.AreEqual(typeof(GenericTypeWithReferenceTypeConstraint<object>), typeof(GenericTypeWithReferenceTypeConstraint<>).BuildType(typeof(List<object>)));
      Assert.AreEqual(typeof(GenericTypeWithDefaultConstructorConstraint<object>), typeof(GenericTypeWithDefaultConstructorConstraint<>).BuildType(typeof(List<object>)));
      Assert.AreEqual(typeof(GenericTypeWithParameterConstraint<int>), typeof(GenericTypeWithParameterConstraint<>).BuildType(typeof(List<int>)));
    }
    #endregion

    #region TestIsSubTypeOf
    private interface IA { }
    private class A : IA { }
    private interface IB : IA { }
    private class B : A, IB { }
    private interface IC<T> : IB { }
    private class C<T> : B, IC<T> { }
    private interface ID<T> : IC<T> { }
    private class D<T> : C<T>, ID<T> { }

    [TestMethod]
    public void TestIsSubTypeOf() {
      // base-types are all non-generic types
      Assert.IsTrue(typeof(A).IsSubTypeOf(typeof(A)));
      Assert.IsTrue(typeof(A).IsSubTypeOf(typeof(IA)));
      Assert.IsTrue(typeof(IA).IsSubTypeOf(typeof(IA)));
      Assert.IsTrue(typeof(B).IsSubTypeOf(typeof(A)));
      Assert.IsTrue(typeof(B).IsSubTypeOf(typeof(IA)));
      Assert.IsTrue(typeof(IB).IsSubTypeOf(typeof(IA)));
      Assert.IsTrue(typeof(C<int>).IsSubTypeOf(typeof(A)));
      Assert.IsTrue(typeof(C<int>).IsSubTypeOf(typeof(IA)));
      Assert.IsTrue(typeof(IC<int>).IsSubTypeOf(typeof(IA)));
      Assert.IsTrue(typeof(C<>).IsSubTypeOf(typeof(A)));
      Assert.IsTrue(typeof(C<>).IsSubTypeOf(typeof(IA)));
      Assert.IsTrue(typeof(IC<>).IsSubTypeOf(typeof(IA)));

      // reversed type hierarchy -> all should fail
      Assert.IsFalse(typeof(IA).IsSubTypeOf(typeof(A)));
      Assert.IsFalse(typeof(A).IsSubTypeOf(typeof(B)));
      Assert.IsFalse(typeof(IA).IsSubTypeOf(typeof(B)));
      Assert.IsFalse(typeof(IA).IsSubTypeOf(typeof(IB)));
      Assert.IsFalse(typeof(A).IsSubTypeOf(typeof(C<int>)));
      Assert.IsFalse(typeof(IA).IsSubTypeOf(typeof(C<int>)));
      Assert.IsFalse(typeof(IA).IsSubTypeOf(typeof(IC<int>)));
      Assert.IsFalse(typeof(A).IsSubTypeOf(typeof(C<>)));
      Assert.IsFalse(typeof(IA).IsSubTypeOf(typeof(C<>)));
      Assert.IsFalse(typeof(IA).IsSubTypeOf(typeof(IC<>)));

      // generic type definitions on both sides
      Assert.IsTrue(typeof(D<>).IsSubTypeOf(typeof(D<>)));
      Assert.IsTrue(typeof(D<>).IsSubTypeOf(typeof(ID<>)));
      Assert.IsTrue(typeof(D<>).IsSubTypeOf(typeof(C<>)));
      Assert.IsTrue(typeof(D<>).IsSubTypeOf(typeof(IC<>)));
      Assert.IsTrue(typeof(ID<>).IsSubTypeOf(typeof(ID<>)));
      Assert.IsTrue(typeof(ID<>).IsSubTypeOf(typeof(IC<>)));

      // fully specified generic types on both sides
      Assert.IsTrue(typeof(D<int>).IsSubTypeOf(typeof(D<int>)));
      Assert.IsTrue(typeof(D<int>).IsSubTypeOf(typeof(ID<int>)));
      Assert.IsTrue(typeof(D<int>).IsSubTypeOf(typeof(C<int>)));
      Assert.IsTrue(typeof(D<int>).IsSubTypeOf(typeof(IC<int>)));
      Assert.IsTrue(typeof(ID<int>).IsSubTypeOf(typeof(ID<int>)));
      Assert.IsTrue(typeof(ID<int>).IsSubTypeOf(typeof(IC<int>)));

      // sub-type is fully specified, base-type is a generic type definition -> all should fail
      Assert.IsFalse(typeof(D<int>).IsSubTypeOf(typeof(D<>)));
      Assert.IsFalse(typeof(D<int>).IsSubTypeOf(typeof(ID<>)));
      Assert.IsFalse(typeof(D<int>).IsSubTypeOf(typeof(C<>)));
      Assert.IsFalse(typeof(D<int>).IsSubTypeOf(typeof(IC<>)));
      Assert.IsFalse(typeof(ID<int>).IsSubTypeOf(typeof(ID<>)));
      Assert.IsFalse(typeof(ID<int>).IsSubTypeOf(typeof(IC<>)));

      // sub-type is a generic type definition, base-type is fully specified -> all should fail
      Assert.IsFalse(typeof(D<>).IsSubTypeOf(typeof(D<int>)));
      Assert.IsFalse(typeof(D<>).IsSubTypeOf(typeof(ID<int>)));
      Assert.IsFalse(typeof(D<>).IsSubTypeOf(typeof(C<int>)));
      Assert.IsFalse(typeof(D<>).IsSubTypeOf(typeof(IC<int>)));
      Assert.IsFalse(typeof(ID<>).IsSubTypeOf(typeof(ID<int>)));
      Assert.IsFalse(typeof(ID<>).IsSubTypeOf(typeof(IC<int>)));

      // fully specified generic types on both sides, type parameters do not match -> all should fail
      Assert.IsFalse(typeof(D<double>).IsSubTypeOf(typeof(D<int>)));
      Assert.IsFalse(typeof(D<double>).IsSubTypeOf(typeof(ID<int>)));
      Assert.IsFalse(typeof(D<double>).IsSubTypeOf(typeof(C<int>)));
      Assert.IsFalse(typeof(D<double>).IsSubTypeOf(typeof(IC<int>)));
      Assert.IsFalse(typeof(ID<double>).IsSubTypeOf(typeof(ID<int>)));
      Assert.IsFalse(typeof(ID<double>).IsSubTypeOf(typeof(IC<int>)));
    }
    #endregion
  }
}
