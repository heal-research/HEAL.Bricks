#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Reflection;

namespace HEAL.Bricks {
  public static class AssemblyExtensions {
    //Based on the code from http://stackoverflow.com/a/7156425
    public static string GetCustomAttributeValue<T>(this Assembly assembly, string propertyName)
       where T : Attribute {
      if (assembly == null || string.IsNullOrEmpty(propertyName)) {
        throw new ArgumentException("Arguments are not allowed to be null.");
      }

      object[] attributes = assembly.GetCustomAttributes(typeof(T), false);
      if (attributes.Length == 0) {
        throw new InvalidOperationException(string.Format("No attributes of type {0} found in assembly {1}", typeof(T).Name,
          assembly.FullName));
      }

      var attribute = (T)attributes[0];
      var propertyInfo = attribute.GetType().GetProperty(propertyName);
      if (propertyInfo == null) {
        throw new InvalidOperationException(string.Format("No property {0} found in attribute {1}, assembly: {2}",
          propertyName, typeof(T).Name, assembly.FullName));
      }

      var value = propertyInfo.GetValue(attribute, null);
      return value.ToString();
    }

    public static string GetFileVersion(this Assembly assembly) {
      return GetCustomAttributeValue<AssemblyFileVersionAttribute>(assembly, "Version");
    }
    public static string GetCopyright(this Assembly assembly) {
      return GetCustomAttributeValue<AssemblyCopyrightAttribute>(assembly, "Copyright");
    }

    public static string GetProduct(this Assembly assembly) {
      return GetCustomAttributeValue<AssemblyProductAttribute>(assembly, "Product");
    }

  }
}
