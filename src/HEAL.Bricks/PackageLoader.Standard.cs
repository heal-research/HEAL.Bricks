#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

#if NETSTANDARD2_0

using System;
using System.Collections.Generic;
using System.Reflection;

namespace HEAL.Bricks {
  public partial class PackageLoader {
    public bool SupportsLoadingUnmanagedAssemblies => false;

    partial void Load(IEnumerable<PackageLoadInfo> packages) {
      LoadManagedAssemblies(packages, out IEnumerable<(string assembly, Exception exception)> errors);
      ThrowExceptionOnError(errors);
    }
  }
}

#endif