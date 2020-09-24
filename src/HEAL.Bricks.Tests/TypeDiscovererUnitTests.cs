#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace HEAL.Bricks.Tests {
  [Trait("Category", "Unit")]
  public class TypeDiscovererUnitTests {
    #region Type Definitions
    public interface I1 { }
    public interface I2 {
      int Value { get; }
    }
    public class A : I1 { }
    public class B : I1 { }
    public class C<T> : I1 { }
    public class D : I2 {
      public int Value { get; set; }
      public D() { }
      public D(int value) { Value = value; }
    }
    public class E : I1, I2 {
      public int Value { get; set; }
      public E() { }
      public E(int value) { Value = value; }
    }
    #endregion

    #region Create
    [Fact]
    public void Create_ReturnsInstance() {
      ITypeDiscoverer td = TypeDiscoverer.Create();

      Assert.NotNull(td);
      Assert.Equal(typeof(TypeDiscoverer), td.GetType());
    }
    #endregion

    #region GetTypes
    [Theory]
    [InlineData(typeof(I1), true,  true,  new[] { typeof(A), typeof(B), typeof(E) })]
    [InlineData(typeof(I1), false, true,  new[] { typeof(I1), typeof(A), typeof(B), typeof(E)})]
    [InlineData(typeof(I1), true,  false, new[] { typeof(A), typeof(B), typeof(C<>), typeof(E)})]
    [InlineData(typeof(I1), false, false, new[] { typeof(I1), typeof(A), typeof(B), typeof(C<>), typeof(E) })]
    public void GetTypes_WithType_ReturnsSubTypes(Type type, bool onlyInstantiable, bool excludeGenericTypeDefinitions, Type[] expectedTypes) {
      ITypeDiscoverer td = TypeDiscoverer.Create();

      IEnumerable<Type> result = td.GetTypes(type, onlyInstantiable, excludeGenericTypeDefinitions);

      Assert.Equal(expectedTypes.OrderBy(x => x.Name), result.OrderBy(x => x.Name));
    }
    [Theory]
    [InlineData(new[] { typeof(I1), typeof(I2) }, true, true, true,  new[] { typeof(E) })]
    [InlineData(new[] { typeof(I1), typeof(I2) }, true, true, false, new[] { typeof(A), typeof(B), typeof(D), typeof(E) })]
    public void GetTypes_WithTypes_ReturnsSubTypes(Type[] types, bool onlyInstantiable, bool excludeGenericTypeDefinitions, bool assignableToAllTypes, Type[] expectedTypes) {
      ITypeDiscoverer td = TypeDiscoverer.Create();

      IEnumerable<Type> result = td.GetTypes(types, onlyInstantiable, excludeGenericTypeDefinitions, assignableToAllTypes);

      Assert.Equal(expectedTypes.OrderBy(x => x.Name), result.OrderBy(x => x.Name));
    }
    [Theory]
    [InlineData(typeof(I1), true,  true,  new[] { typeof(A), typeof(B), typeof(E) })]
    [InlineData(typeof(I1), false, true,  new[] { typeof(I1), typeof(A), typeof(B), typeof(E) })]
    [InlineData(typeof(I1), true,  false, new[] { typeof(A), typeof(B), typeof(C<>), typeof(E) })]
    [InlineData(typeof(I1), false, false, new[] { typeof(I1), typeof(A), typeof(B), typeof(C<>), typeof(E) })]
    public void GetTypes_WithTypeAndAssembly_ReturnsSubTypes(Type type, bool onlyInstantiable, bool excludeGenericTypeDefinitions, Type[] expectedTypes) {
      Assembly assembly = Assembly.GetExecutingAssembly();
      ITypeDiscoverer td = TypeDiscoverer.Create();

      IEnumerable<Type> result = td.GetTypes(type, assembly, onlyInstantiable, excludeGenericTypeDefinitions);

      Assert.Equal(expectedTypes.OrderBy(x => x.Name), result.OrderBy(x => x.Name));
    }
    [Theory]
    [InlineData(new[] { typeof(I1), typeof(I2) }, true, true, true,  new[] { typeof(E) })]
    [InlineData(new[] { typeof(I1), typeof(I2) }, true, true, false, new[] { typeof(A), typeof(B), typeof(D), typeof(E) })]
    public void GetTypes_WithTypesAndAssembly_ReturnsSubTypes(Type[] types, bool onlyInstantiable, bool excludeGenericTypeDefinitions, bool assignableToAllTypes, Type[] expectedTypes) {
      Assembly assembly = Assembly.GetExecutingAssembly();
      ITypeDiscoverer td = TypeDiscoverer.Create();

      IEnumerable<Type> result = td.GetTypes(types, assembly, onlyInstantiable, excludeGenericTypeDefinitions, assignableToAllTypes);

      Assert.Equal(expectedTypes.OrderBy(x => x.Name), result.OrderBy(x => x.Name));
    }
    [Fact]
    public void GetTypes_WithNullType_ThrowsArgumentNullException() {
      Type type = null;
      ITypeDiscoverer td = TypeDiscoverer.Create();

      var e = Assert.Throws<ArgumentNullException>(() => td.GetTypes(type));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Fact]
    public void GetTypes_WithNullTypeEnumerable_ThrowsArgumentNullException() {
      Type[] types = null;
      ITypeDiscoverer td = TypeDiscoverer.Create();

      var e = Assert.Throws<ArgumentNullException>(() => td.GetTypes(types));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData(null, null)]
    [InlineData(null, "ExecutingAssembly")]
    [InlineData(typeof(object), null)]
    public void GetTypes_WithNullTypeOrAssembly_ThrowsArgumentNullException(Type type, string assemblyName) {
      Assembly assembly = assemblyName == null ? null : Assembly.GetExecutingAssembly();
      ITypeDiscoverer td = TypeDiscoverer.Create();

      var e = Assert.Throws<ArgumentNullException>(() => td.GetTypes(type, assembly));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData(null, null)]
    [InlineData(null, "ExecutingAssembly")]
    [InlineData(new[] { typeof(object) }, null)]
    public void GetTypes_WithNullTypeEnumerableOrAssembly_ThrowsArgumentNullException(Type[] types, string assemblyName) {
      Assembly assembly = assemblyName == null ? null : Assembly.GetExecutingAssembly();
      ITypeDiscoverer td = TypeDiscoverer.Create();

      var e = Assert.Throws<ArgumentNullException>(() => td.GetTypes(types, assembly));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetTypes_WithTypesIsEmptyOrContainsNull_ThrowsArgumentException(bool typesIsEmpty) {
      Type[] types = typesIsEmpty ? new Type[0] : new Type[] { null };
      ITypeDiscoverer td = TypeDiscoverer.Create();

      var e = Assert.Throws<ArgumentException>(() => td.GetTypes(types));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetTypes_WithTypesIsEmptyOrContainsNullAndAssembly_ThrowsArgumentException(bool typesIsEmpty) {
      Type[] types = typesIsEmpty ? new Type[0] : new Type[] { null };
      Assembly assembly = Assembly.GetExecutingAssembly();
      ITypeDiscoverer td = TypeDiscoverer.Create();

      var e = Assert.Throws<ArgumentException>(() => td.GetTypes(types, assembly));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    #endregion

    #region GetInstances
    [Fact]
    public void GetInstancesOfType_ReturnsInstances() {
      Type[] expectedInstanceTypes = new[] { typeof(A), typeof(B), typeof(E) };
      ITypeDiscoverer td = TypeDiscoverer.Create();

      IEnumerable<I1> result = td.GetInstances<I1>();

      Assert.Equal(expectedInstanceTypes, result.Select(x => x.GetType()));
    }
    [Fact]
    public void GetInstancesOfType_WithArgs_ReturnsInstances() {
      int value = 42;
      Type[] expectedInstanceTypes = new[] { typeof(D), typeof(E) };
      ITypeDiscoverer td = TypeDiscoverer.Create();

      IEnumerable<I2> result = td.GetInstances<I2>(value);

      Assert.Equal(expectedInstanceTypes, result.Select(x => x.GetType()));
      Assert.All(result, x => Assert.Equal(value, x.Value));
    }
    [Fact]
    public void GetInstances_WithType_ReturnsInstances() {
      Type[] expectedInstanceTypes = new[] { typeof(A), typeof(B), typeof(E) };
      ITypeDiscoverer td = TypeDiscoverer.Create();

      IEnumerable<object> result = td.GetInstances(typeof(I1));

      Assert.Equal(expectedInstanceTypes, result.Select(x => x.GetType()));
    }
    [Fact]
    public void GetInstances_WithTypeAndArgs_ReturnsInstances() {
      int value = 42;
      Type[] expectedInstanceTypes = new[] { typeof(D), typeof(E) };
      ITypeDiscoverer td = TypeDiscoverer.Create();

      IEnumerable<object> result = td.GetInstances(typeof(I2), value);

      Assert.Equal(expectedInstanceTypes, result.Select(x => x.GetType()));
      Assert.All(result.Cast<I2>(), x => Assert.Equal(value, x.Value));
    }
    [Fact]
    public void GetInstances_WithNullParameter_ThrowsArgumentNullException() {
      Type type = null;
      ITypeDiscoverer td = TypeDiscoverer.Create();

      var e = Assert.Throws<ArgumentNullException>(() => td.GetInstances(type));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    #endregion
  }
}
