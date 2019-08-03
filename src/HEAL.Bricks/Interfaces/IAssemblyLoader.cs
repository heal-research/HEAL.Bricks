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
  public interface IAssemblyLoader {
    IEnumerable<Assembly> Assemblies { get; }
    IEnumerable<Type> Types { get; }

    IEnumerable<Assembly> LoadAssemblies(string basePath);
    IEnumerable<Assembly> LoadAssemblies(IEnumerable<AssemblyInfo> assemblyInfos);
  }
}
