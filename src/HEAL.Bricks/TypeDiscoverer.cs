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
    public IEnumerable<Type> GetTypes(Type type, bool onlyInstantiable = true, bool excludeGenericTypeDefinitions = true) {
      if (type == null) throw new ArgumentNullException(nameof(type));

      return from assembly in AppDomain.CurrentDomain.GetAssemblies()
             from t in GetTypes(type, assembly, onlyInstantiable, excludeGenericTypeDefinitions)
             select t;
    }

    public IEnumerable<Type> GetTypes(IEnumerable<Type> types, bool onlyInstantiable = true, bool excludeGenericTypeDefinitions = true, bool assignableToAllTypes = true) {
      if (types == null) throw new ArgumentNullException(nameof(types));
      if (types.Count() == 0) throw new ArgumentException($"{nameof(types)} is empty.", nameof(types));
      if (types.Any(x => x == null)) throw new ArgumentException($"{nameof(types)} contains null elements.", nameof(types));

      return types.Select(t => GetTypes(t, onlyInstantiable, excludeGenericTypeDefinitions))
                  .Aggregate((a, b) => assignableToAllTypes ? a.Intersect(b) : a.Union(b));
    }

    public IEnumerable<Type> GetTypes(Type type, Assembly assembly, bool onlyInstantiable = true, bool excludeGenericTypeDefinitions = true) {
      if (type == null) throw new ArgumentNullException(nameof(type));
      if (assembly == null) throw new ArgumentNullException(nameof(assembly));

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
      if (types == null) throw new ArgumentNullException(nameof(types));
      if (types.Count() == 0) throw new ArgumentException($"{nameof(types)} is empty.", nameof(types));
      if (types.Any(x => x == null)) throw new ArgumentException($"{nameof(types)} contains null elements.", nameof(types));
      if (assembly == null) throw new ArgumentNullException(nameof(assembly));

      return types.Select(t => GetTypes(t, assembly, onlyInstantiable, excludeGenericTypeDefinitions))
                  .Aggregate((a, b) => assignableToAllTypes ? a.Intersect(b) : a.Union(b));
    }

    public IEnumerable<T> GetInstances<T>() where T : class => GetInstances<T>(null);
    public IEnumerable<T> GetInstances<T>(params object[] args) where T : class => GetInstances(typeof(T), args).Cast<T>();
    public IEnumerable<object> GetInstances(Type type) => GetInstances(type, null);
    public IEnumerable<object> GetInstances(Type type, params object[] args) {
      if (type == null) throw new ArgumentNullException(nameof(type));

      List<object> instances = new List<object>();
      foreach (Type t in GetTypes(type)) {
        try {
          object instance = Activator.CreateInstance(t, args);
          if (instance != null) instances.Add(instance);
        }
        catch { }
      }
      return instances;
    }
  }
}
