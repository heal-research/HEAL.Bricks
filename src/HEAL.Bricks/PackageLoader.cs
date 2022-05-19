#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace HEAL.Bricks {
  public static class PackageLoader {
    public static void LoadPackageAssemblies(PackageLoadInfo package) {
      Guard.Argument(package, nameof(package)).NotNull();

      LoadPackageAssemblies(Enumerable.Repeat(package, 1));
    }
    public static void LoadPackageAssemblies(IEnumerable<PackageLoadInfo> packages) {
      Guard.Argument(packages, nameof(packages)).NotNull().DoesNotContainNull();
      if (!packages.Any()) return;

      IntPtr resolveUnmanagedAssembly(Assembly assembly, string library) {
        string? libraryPath = GetUnmanagedAssemblyPath(packages, library);
        return libraryPath != null ? NativeLibrary.Load(libraryPath) : IntPtr.Zero;
      }

      AssemblyLoadContext.Default.ResolvingUnmanagedDll += resolveUnmanagedAssembly;
      LoadManagedAssemblies(packages, out IEnumerable<(string assembly, Exception exception)> errors);
      AssemblyLoadContext.Default.ResolvingUnmanagedDll -= resolveUnmanagedAssembly;

      if (errors.Any()) {
        throw new AggregateException("Package loading failed. See inner exception for details.",
                                     errors.Select(x => new InvalidOperationException($"Loading of assembly '{x.assembly}' failed.", x.exception)));
      }
    }

    private static void LoadManagedAssemblies(IEnumerable<PackageLoadInfo> packages, out IEnumerable<(string assembly, Exception exception)> errors) {
      List<(string assembly, Exception exception)> loadingFailed = new();
      string packagesPath = PreparePackagesPath(packages);

      foreach (string assembly in packages.SelectMany(x => x.AssemblyPaths).Select(x => Path.Combine(packagesPath, x))) {
        try {
          Assembly.LoadFrom(assembly);
        }
        catch (Exception exception) {
          loadingFailed.Add((assembly, exception));
        }
      }

      errors = loadingFailed;
    }

    private static string PreparePackagesPath(IEnumerable<PackageLoadInfo> packages) {
      string packagesPath = packages.First().PackagesPath;
      if (!Path.IsPathRooted(packagesPath)) {
        string basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? throw new InvalidOperationException("Cannot identify base path");
        packagesPath = Path.Combine(basePath, packagesPath);
      }

      if (!Directory.Exists(packagesPath)) {
        throw new InvalidOperationException($"Packages path '{packagesPath}' not found.");
      }
      return packagesPath;
    }

    private static string? GetUnmanagedAssemblyPath(IEnumerable<PackageLoadInfo> packages, string library) {
      IEnumerable<string> files = packages.SelectMany(x => x.Files).Where(x => x.Contains(library));

      if (files.Count() > 1) files = ChooseUnmanagedAssembly(files);

      string packagesPath = PreparePackagesPath(packages);
      files = files.Select(x => Path.Combine(packagesPath, x));
      return files.SingleOrDefault();
    }

    private static IEnumerable<string> ChooseUnmanagedAssembly(IEnumerable<string> files) {
      IEnumerable<string> filtered;

      if (Environment.Is64BitProcess) {
        filtered = files.Where(x => x.Contains("64"));
        if (filtered.Count() == 1) return filtered;
        if (!filtered.Any()) filtered = files;
      } else {
        filtered = files.Where(x => x.Contains("86"));
        if (filtered.Count() == 1) return filtered;
        if (!filtered.Any()) {
          filtered = files.Where(x => x.Contains("32"));
          if (filtered.Count() == 1) return filtered;
          if (!filtered.Any()) filtered = files;
        }
      }

      filtered = filtered.Where(x => x.Contains(GetCurrentOS()));
      if (filtered.Count() == 1) return filtered;
      if (!filtered.Any()) filtered = files;

      return filtered;
    }

    private static string GetCurrentOS() {
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
