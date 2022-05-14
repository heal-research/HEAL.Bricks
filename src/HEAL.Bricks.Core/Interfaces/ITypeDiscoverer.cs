#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.Reflection;

namespace HEAL.Bricks {
  public interface ITypeDiscoverer {
    IEnumerable<Type> GetTypes(Type type, bool onlyInstantiable = true, bool excludeGenericTypeDefinitions = true);
    IEnumerable<Type> GetTypes(IEnumerable<Type> types, bool onlyInstantiable = true, bool excludeGenericTypeDefinitions = true, bool assignableToAllTypes = true);
    IEnumerable<Type> GetTypes(Type type, Assembly assembly, bool onlyInstantiable = true, bool excludeGenericTypeDefinitions = true);
    IEnumerable<Type> GetTypes(IEnumerable<Type> types, Assembly assembly, bool onlyInstantiable = true, bool excludeGenericTypeDefinitions = true, bool assignableToAllTypes = true);

    IEnumerable<T> GetInstances<T>() where T : class;
    IEnumerable<T> GetInstances<T>(params object[] args) where T : class;
    IEnumerable<object> GetInstances(Type type);
    IEnumerable<object> GetInstances(Type type, params object[] args);
  }
}
