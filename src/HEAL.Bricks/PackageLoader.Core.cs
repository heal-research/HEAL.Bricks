#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

#if NETCOREAPP3_1

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace HEAL.Bricks {
  public partial class PackageLoader {
    public bool SupportsLoadingUnmanagedAssemblies => true;

    partial void Load(IEnumerable<PackageLoadInfo> packages) {
      Func<Assembly, string, IntPtr> resolveUnmanagedAssemblyHandler = (assembly, library) => {
        string libraryPath = GetUnmanagedAssemblyPath(packages, library);
        return libraryPath != null ? NativeLibrary.Load(libraryPath) : IntPtr.Zero;
      };
      AssemblyLoadContext.Default.ResolvingUnmanagedDll += resolveUnmanagedAssemblyHandler;

      LoadManagedAssemblies(packages, out IEnumerable<(string assembly, Exception exception)> errors);
      AssemblyLoadContext.Default.ResolvingUnmanagedDll -= resolveUnmanagedAssemblyHandler;

      ThrowExceptionOnError(errors);
    }

    private string GetUnmanagedAssemblyPath(IEnumerable<PackageLoadInfo> packages, string library) {
      IEnumerable<string> files = packages.SelectMany(x => x.Files).Where(x => x.Contains(library));

      if (files.Count() > 1) files = ChooseUnmanagedAssembly(files);
      
      string packagesPath = PreparePackagesPath(packages);
      files = files.Select(x => Path.Combine(packagesPath, x));
      return files.SingleOrDefault();
    }

    private IEnumerable<string> ChooseUnmanagedAssembly(IEnumerable<string> files) {
      IEnumerable<string> filtered;

      if (Environment.Is64BitProcess) {
        filtered = files.Where(x => x.Contains("64"));
        if (filtered.Count() == 1) return filtered;
        if (filtered.Count() == 0) filtered = files;
      } else {
        filtered = files.Where(x => x.Contains("86"));
        if (filtered.Count() == 1) return filtered;
        if (filtered.Count() == 0) {
          filtered = files.Where(x => x.Contains("32"));
          if (filtered.Count() == 1) return filtered;
          if (filtered.Count() == 0) filtered = files;
        }
      }

      filtered = filtered.Where(x => x.Contains(GetCurrentOS()));
      if (filtered.Count() == 1) return filtered;
      if (filtered.Count() == 0) filtered = files;

      return filtered;
    }

    private string GetCurrentOS() {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
        return "win";
      } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
        return "linux";
      } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
        return "osx";
      } else {
        throw new PlatformNotSupportedException($"Platform {RuntimeInformation.OSDescription} is not supported.");
      }
    }
  }
}

#endif