#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Linq;
using System.Reflection;

namespace HEAL.Bricks {
  internal static class TypeExtensions {
    internal static bool IsNonDiscoverableType(this Type type) {
      if (type == null) throw new ArgumentNullException(nameof(type));

      return type.GetCustomAttribute<NonDiscoverableTypeAttribute>(inherit: false) != null;
    }

    /// <summary>
    /// Constructs a concrete type from a given proto type.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="protoType"></param>
    /// <returns>The constructed type, a generic type definition or null, if a type construction is not possible</returns>
    /// <remarks>This method does not work with nested generic types</remarks>
    internal static Type BuildType(this Type type, Type protoType) {
      if (type == null) throw new ArgumentNullException(nameof(type));
      if (protoType == null) throw new ArgumentNullException(nameof(protoType));

      if (!type.IsGenericTypeDefinition) return type;
      if (protoType.IsGenericTypeDefinition) return type;
      if (!protoType.IsGenericType) return type;

      var typeGenericArguments = type.GetGenericArguments();
      var protoTypeGenericArguments = protoType.GetGenericArguments();
      if (typeGenericArguments.Length != protoTypeGenericArguments.Length) return null;

      for (int i = 0; i < typeGenericArguments.Length; i++) {
        var typeGenericArgument = typeGenericArguments[i];
        var protoTypeGenericArgument = protoTypeGenericArguments[i];

        //check class contraint on generic type parameter 
        if (typeGenericArgument.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
          if (!protoTypeGenericArgument.IsClass && !protoTypeGenericArgument.IsInterface && !protoType.IsArray) return null;

        //check default constructor constraint on generic type parameter 
        if (typeGenericArgument.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
          if (!protoTypeGenericArgument.IsValueType && protoTypeGenericArgument.GetConstructor(Type.EmptyTypes) == null) return null;

        //check type restrictions on generic type parameter
        foreach (var constraint in typeGenericArgument.GetGenericParameterConstraints())
          if (!constraint.IsAssignableFrom(protoTypeGenericArgument)) return null;
      }

      try {
        return type.MakeGenericType(protoTypeGenericArguments);
      }
      catch (Exception) {
        return null;
      }
    }

    internal static bool IsSubTypeOf(this Type type, Type baseType) {
      if (type == null) throw new ArgumentNullException(nameof(type));
      if (baseType == null) throw new ArgumentNullException(nameof(baseType));

      if (baseType.IsAssignableFrom(type)) return true;
      if (!baseType.IsGenericType) return false;

      if (RecursiveCheckGenericTypes(baseType, type)) return true;
      foreach (Type genericInterfaceOfSubType in type.GetInterfaces().Where(i => i.IsGenericType)) {
        if (baseType.CheckGenericTypes(genericInterfaceOfSubType)) return true;
      }

      return false;
    }

    private static bool RecursiveCheckGenericTypes(Type baseType, Type subType) {
      if (!baseType.IsGenericType) return false;
      if (!subType.IsGenericType) return false;
      if (baseType.CheckGenericTypes(subType)) return true;
      if (subType.BaseType == null) return false;

      return RecursiveCheckGenericTypes(baseType, subType.BaseType);
    }

    private static bool CheckGenericTypes(this Type baseType, Type subType) {
      Type baseTypeGenericTypeDefinition = baseType.GetGenericTypeDefinition();
      Type subTypeGenericTypeDefinition = subType.GetGenericTypeDefinition();
      if (baseTypeGenericTypeDefinition != subTypeGenericTypeDefinition) return false;
      Type[] baseTypeGenericArguments = baseType.GetGenericArguments();
      Type[] subTypeGenericArguments = subType.GetGenericArguments();

      for (int i = 0; i < baseTypeGenericArguments.Length; i++) {
        Type baseTypeGenericArgument = baseTypeGenericArguments[i];
        Type subTypeGenericArgument = subTypeGenericArguments[i];

        if (baseTypeGenericArgument.IsGenericParameter ^ subTypeGenericArgument.IsGenericParameter) return false;
        if (baseTypeGenericArgument == subTypeGenericArgument) continue;
        if (!baseTypeGenericArgument.IsGenericParameter && !subTypeGenericArgument.IsGenericParameter) return false;

        if (baseTypeGenericArgument.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) &&
            !subTypeGenericArgument.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint)) return false;
        if (baseTypeGenericArgument.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) &&
            !subTypeGenericArgument.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint)) return false;
        if (baseTypeGenericArgument.GenericParameterAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint) &&
            !subTypeGenericArgument.GenericParameterAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint)) return false;

        foreach (Type baseTypeGenericParameterConstraint in baseTypeGenericArgument.GetGenericParameterConstraints()) {
          if (!subTypeGenericArgument.GetGenericParameterConstraints().Any(t => baseTypeGenericParameterConstraint.IsAssignableFrom(t))) return false;
        }
      }
      return true;
    }
  }
}
