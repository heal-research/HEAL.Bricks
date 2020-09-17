#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

#if NET472

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace HEAL.Bricks {
  public partial class PackageLoader {
    public bool SupportsLoadingUnmanagedAssemblies => true;

    [DllImport("kernel32", SetLastError = true)]
    static extern IntPtr LoadLibrary(string file);

    partial void Load(IEnumerable<PackageLoadInfo> packages) {
      LoadManagedAssemblies(packages, out IEnumerable<(string assembly, Exception exception)> errors);

      List<(string assembly, Exception exception)> loadingFailed = new List<(string assembly, Exception exception)>();
      foreach (string unmanagedAssembly in GetUnmanagedAssemblies(packages)) {
        try {
          IntPtr ptr = LoadLibrary(unmanagedAssembly);
          if (ptr == IntPtr.Zero) {
            int error = Marshal.GetLastWin32Error();
            Win32Exception exception = new Win32Exception(error);
            exception.Data.Add("LastWin32Error", error);
            throw exception;
          }
        }
        catch (Exception exception) {
          loadingFailed.Add((unmanagedAssembly, exception));
        }
      }
      errors = errors.Concat(loadingFailed);

      ThrowExceptionOnError(errors);
    }

    private IEnumerable<string> GetUnmanagedAssemblies(IEnumerable<PackageLoadInfo> packages) {
      IEnumerable<string> assemblies = packages.SelectMany(x => x.Files).Where(x => x.Contains("runtime") && x.EndsWith(".dll"));

      if (Environment.Is64BitProcess) {
        assemblies = assemblies.Where(x => x.Contains("x64"));
      } else {
        assemblies = assemblies.Where(x => !x.Contains("x64"));
      }

      string packagesPath = PreparePackagesPath(packages);
      return assemblies.Select(x => Path.Combine(packagesPath, x));
    }
  }
}

#endif