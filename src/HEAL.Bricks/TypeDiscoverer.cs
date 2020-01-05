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

namespace HEAL.Bricks {
  public sealed class TypeDiscoverer : ITypeDiscoverer {
    public static ITypeDiscoverer Create() {
      return new TypeDiscoverer();
    }

    public IEnumerable<Type> GetTypes(Type type, bool onlyInstantiable = true, bool excludeGenericTypeDefinitions = true) {
      return from assembly in AppDomain.CurrentDomain.GetAssemblies()
             from t in GetTypes(type, assembly, onlyInstantiable, excludeGenericTypeDefinitions)
             select t;
    }

    public IEnumerable<Type> GetTypes(IEnumerable<Type> types, bool onlyInstantiable = true, bool excludeGenericTypeDefinitions = true, bool assignableToAllTypes = true) {
      return types.Select(t => GetTypes(t, onlyInstantiable, excludeGenericTypeDefinitions))
                  .Aggregate((a, b) => assignableToAllTypes ? a.Intersect(b) : a.Union(b));
    }

    public IEnumerable<Type> GetTypes(Type type, Assembly assembly, bool onlyInstantiable = true, bool excludeGenericTypeDefinitions = true) {
      try {
        return assembly.GetTypes().Where(t => !t.IsNonDiscoverableType())
                                  .Select(t => t.BuildType(type))
                                  .Where(t => t != null)
                                  .Where(t => t.IsSubTypeOf(type))
                                  .Where(t => !(onlyInstantiable && (t.IsAbstract || t.IsInterface || t.HasElementType)))
                                  .Where(t => !(excludeGenericTypeDefinitions && t.IsGenericTypeDefinition));
      }
      catch (Exception ex) when (ex is TypeLoadException || ex is ReflectionTypeLoadException) {
        return Enumerable.Empty<Type>();
      }
    }

    public IEnumerable<Type> GetTypes(IEnumerable<Type> types, Assembly assembly, bool onlyInstantiable = true, bool excludeGenericTypeDefinitions = true, bool assignableToAllTypes = true) {
      return types.Select(t => GetTypes(t, assembly, onlyInstantiable, excludeGenericTypeDefinitions))
                  .Aggregate((a, b) => assignableToAllTypes ? a.Intersect(b) : a.Union(b));
    }
    public IEnumerable<T> GetInstances<T>() where T : class {
      foreach (Type t in GetTypes(typeof(T))) {
        T instance = default;
        try {
          instance = Activator.CreateInstance(t) as T;
        }
        catch { }
        if (instance != null) yield return instance;
      }
    }
  }
}
