#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;

namespace HEAL.Bricks {
  public interface IApplicationManager {
    IEnumerable<IPluginDescription> Plugins { get; }
    IEnumerable<IApplicationDescription> Applications { get; }

    IEnumerable<T> GetInstances<T>() where T : class;
    IEnumerable<T> GetInstances<T>(params object[] args) where T : class;
    IEnumerable<object> GetInstances(Type type);
    IEnumerable<object> GetInstances(Type type, params object[] args);

    Type GetType(string typeName);

    IEnumerable<Type> GetTypes(Type type, bool onlyInstantiable = true, bool includeGenericTypeDefinitions = false);
    IEnumerable<Type> GetTypes(IEnumerable<Type> types, bool onlyInstantiable = true, bool includeGenericTypeDefinitions = false, bool assignableToAllTypes = true);
    IEnumerable<Type> GetTypes(Type type, IPluginDescription plugin, bool onlyInstantiable = true, bool includeGenericTypeDefinitions = false);
    IEnumerable<Type> GetTypes(IEnumerable<Type> types, IPluginDescription plugin, bool onlyInstantiable = true, bool includeGenericTypeDefinitions = false, bool assignableToAllTypes = true);
    IEnumerable<Type> GetTypes(Type type, Assembly assembly, bool onlyInstantiable = true, bool includeGenericTypeDefinitions = false);
    IEnumerable<Type> GetTypes(IEnumerable<Type> types, Assembly assembly, bool onlyInstantiable = true, bool includeGenericTypeDefinitions = false, bool assignableToAllTypes = true);

    IPluginDescription GetDeclaringPlugin(Type type);
  }
}
