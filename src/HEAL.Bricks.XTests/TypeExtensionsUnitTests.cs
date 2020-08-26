#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using Xunit;

namespace HEAL.Bricks.XTests {
  public class TypeExtensionsUnitTests {
    #region Type Definitions
    private class DiscoverableType { }
    [NonDiscoverableType]
    private class NonDiscoverableType { }

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

    private interface IA { }
    private class A : IA { }
    private interface IB : IA { }
    private class B : A, IB { }
    private interface IC<T> : IB { }
    private class C<T> : B, IC<T> { }
    private interface ID<T> : IC<T> { }
    private class D<T> : C<T>, ID<T> { }
    #endregion

    #region IsNonDiscoverableType
    [Theory]
    [InlineData(typeof(NonDiscoverableType), true)]
    [InlineData(typeof(DiscoverableType), false)]
    public void IsNonDiscoverableType_ReturnsTrueOrFalse(Type type, bool expectedResult) {
      bool result = type.IsNonDiscoverableType();

      Assert.Equal(expectedResult, result);
    }
    [Fact]
    public void IsNonDiscoverableType_WithNullType_ThrowsArgumentNullException() {
      Type type = null;
      var e = Assert.Throws<ArgumentNullException>(() => type.IsNonDiscoverableType());
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    #endregion

    #region BuildType
    [Theory]
    [InlineData(typeof(object),                                        typeof(object),                                     typeof(object))]
    [InlineData(typeof(GenericType<>),                                 typeof(GenericType<>),                              typeof(GenericType<>))]
    [InlineData(typeof(GenericType<>),                                 typeof(object),                                     typeof(GenericType<>))]
    [InlineData(typeof(GenericType<>),                                 typeof(GenericTypeWithTwoParameters<int, int>),     null)]
    [InlineData(typeof(GenericTypeWithReferenceTypeConstraint<>),      typeof(GenericType<int>),                           null)]
    [InlineData(typeof(GenericTypeWithDefaultConstructorConstraint<>), typeof(GenericType<TypeWithoutDefaultConstructor>), null)]
    [InlineData(typeof(GenericTypeWithParameterConstraint<>),          typeof(GenericType<object>),                        null)]
    [InlineData(typeof(GenericType<>),                                 typeof(List<int>),                                  typeof(GenericType<int>))]
    [InlineData(typeof(GenericTypeWithReferenceTypeConstraint<>),      typeof(List<object>),                               typeof(GenericTypeWithReferenceTypeConstraint<object>))]
    [InlineData(typeof(GenericTypeWithDefaultConstructorConstraint<>), typeof(List<object>),                               typeof(GenericTypeWithDefaultConstructorConstraint<object>))]
    [InlineData(typeof(GenericTypeWithParameterConstraint<>),          typeof(List<int>),                                  typeof(GenericTypeWithParameterConstraint<int>))]
    public void BuildType_WithProtoType_ReturnsTypeOrNull(Type type, Type protoType, Type expectedType) {
      Type result = type.BuildType(protoType);

      Assert.Equal(expectedType, result);
    }
    [Theory]
    [InlineData(null,           null)]
    [InlineData(null,           typeof(object))]
    [InlineData(typeof(object), null)]
    public void BuildType_WithNullParameter_ThrowsArgumentNullException(Type type, Type protoType) {
      var e = Assert.Throws<ArgumentNullException>(() => type.BuildType(protoType));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    #endregion

    #region TestIsSubTypeOf
    [Theory]
    // base-types are all non-generic types
    [InlineData(typeof(A),          typeof(A),       true)]
    [InlineData(typeof(A),          typeof(IA),      true)]
    [InlineData(typeof(IA),         typeof(IA),      true)]
    [InlineData(typeof(B),          typeof(A),       true)]
    [InlineData(typeof(B),          typeof(IA),      true)]
    [InlineData(typeof(IB),         typeof(IA),      true)]
    [InlineData(typeof(C<int>),     typeof(A),       true)]
    [InlineData(typeof(C<int>),     typeof(IA),      true)]
    [InlineData(typeof(IC<int>),    typeof(IA),      true)]
    [InlineData(typeof(C<>),        typeof(A),       true)]
    [InlineData(typeof(C<>),        typeof(IA),      true)]
    [InlineData(typeof(IC<>),       typeof(IA),      true)]
    // reversed type hierarchy -> all should fail
    [InlineData(typeof(IA),         typeof(A),       false)]
    [InlineData(typeof(A),          typeof(B),       false)]
    [InlineData(typeof(IA),         typeof(B),       false)]
    [InlineData(typeof(IA),         typeof(IB),      false)]
    [InlineData(typeof(A),          typeof(C<int>),  false)]
    [InlineData(typeof(IA),         typeof(C<int>),  false)]
    [InlineData(typeof(IA),         typeof(IC<int>), false)]
    [InlineData(typeof(A),          typeof(C<>),     false)]
    [InlineData(typeof(IA),         typeof(C<>),     false)]
    [InlineData(typeof(IA),         typeof(IC<>),    false)]
    // generic type definitions on both sides
    [InlineData(typeof(D<>),        typeof(D<>),     true)]
    [InlineData(typeof(D<>),        typeof(ID<>),    true)]
    [InlineData(typeof(D<>),        typeof(C<>),     true)]
    [InlineData(typeof(D<>),        typeof(IC<>),    true)]
    [InlineData(typeof(ID<>),       typeof(ID<>),    true)]
    [InlineData(typeof(ID<>),       typeof(IC<>),    true)]
    // fully specified generic types on both sides
    [InlineData(typeof(D<int>),     typeof(D<int>),  true)]
    [InlineData(typeof(D<int>),     typeof(ID<int>), true)]
    [InlineData(typeof(D<int>),     typeof(C<int>),  true)]
    [InlineData(typeof(D<int>),     typeof(IC<int>), true)]
    [InlineData(typeof(ID<int>),    typeof(ID<int>), true)]
    [InlineData(typeof(ID<int>),    typeof(IC<int>), true)]
    // sub-type is fully specified, base-type is a generic type definition -> all should fail
    [InlineData(typeof(D<int>),     typeof(D<>),     false)]
    [InlineData(typeof(D<int>),     typeof(ID<>),    false)]
    [InlineData(typeof(D<int>),     typeof(C<>),     false)]
    [InlineData(typeof(D<int>),     typeof(IC<>),    false)]
    [InlineData(typeof(ID<int>),    typeof(ID<>),    false)]
    [InlineData(typeof(ID<int>),    typeof(IC<>),    false)]
    // sub-type is a generic type definition, base-type is fully specified -> all should fail
    [InlineData(typeof(D<>),        typeof(D<int>),  false)]
    [InlineData(typeof(D<>),        typeof(ID<int>), false)]
    [InlineData(typeof(D<>),        typeof(C<int>),  false)]
    [InlineData(typeof(D<>),        typeof(IC<int>), false)]
    [InlineData(typeof(ID<>),       typeof(ID<int>), false)]
    [InlineData(typeof(ID<>),       typeof(IC<int>), false)]
    // fully specified generic types on both sides, type parameters do not match -> all should fail
    [InlineData(typeof(D<double>),  typeof(D<int>),  false)]
    [InlineData(typeof(D<double>),  typeof(ID<int>), false)]
    [InlineData(typeof(D<double>),  typeof(C<int>),  false)]
    [InlineData(typeof(D<double>),  typeof(IC<int>), false)]
    [InlineData(typeof(ID<double>), typeof(ID<int>), false)]
    [InlineData(typeof(ID<double>), typeof(IC<int>), false)]
    public void IsSubTypeOf_WithBaseType_ReturnsTrueOrFalse(Type type, Type baseType, bool expectedResult) {
      bool result = type.IsSubTypeOf(baseType);

      Assert.Equal(expectedResult, result);
    }
    [Theory]
    [InlineData(null,           null)]
    [InlineData(null,           typeof(object))]
    [InlineData(typeof(object), null)]
    public void IsSubTypeOf_WithNullParameter_ThrowsArgumentNullException(Type type, Type baseType) {
      var e = Assert.Throws<ArgumentNullException>(() => type.IsSubTypeOf(baseType));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    #endregion
  }
}
