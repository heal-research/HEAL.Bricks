#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using NuGet.Frameworks;
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.Versioning;

namespace HEAL.Bricks {
  public sealed class NuGetAssemblyLoader {
    public static void Load() {
      string applicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      foreach (string packagePath in Directory.GetFiles(applicationPath, "*.nupkg")) {
        string symbolsPackagePath = Path.ChangeExtension(packagePath, "snupkg");

        using (PackageArchiveReader packageReader = new PackageArchiveReader(packagePath)) {
          NuspecReader nuspecReader = new NuspecReader(packageReader.GetNuspec());
          Console.WriteLine($"Loading assemblies from package {nuspecReader.GetIdentity()}:");

          var files = NuGetFrameworkUtility.GetNearest(packageReader.GetReferenceItems(), GetCurrentFramework()).Items;
          foreach (string dllFile in files) {
            try {
              using (Stream assemblyStream = packageReader.GetStream(dllFile))
              using (MemoryStream decompressedAssemblyStream = new MemoryStream()) {
                assemblyStream.CopyTo(decompressedAssemblyStream);
                decompressedAssemblyStream.Position = 0;

                if (File.Exists(symbolsPackagePath)) {
                  string symbolsFile = Path.ChangeExtension(dllFile, "pdb");
                  using (PackageArchiveReader symbolsPackageReader = new PackageArchiveReader(symbolsPackagePath))
                  using (Stream symbolsStream = symbolsPackageReader.GetStream(symbolsFile))
                  using (MemoryStream decompressedSymbolsStream = new MemoryStream()) {
                    symbolsStream.CopyTo(decompressedSymbolsStream);
                    decompressedSymbolsStream.Position = 0;
                    Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(decompressedAssemblyStream, decompressedSymbolsStream);
                    Console.WriteLine($"Loaded assembly and symbols {assembly.FullName}.");
                  }
                } else {
                  Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(decompressedAssemblyStream);
                  Console.WriteLine($"Loaded assembly {assembly.FullName}.");
                }
              }
            }
            catch (Exception e) {
              Console.WriteLine($"Loading assembly {dllFile} failed: {e.Message}");
            }
          }
        }
        Console.WriteLine();
      }
    }

    private static NuGetFramework GetCurrentFramework() {
      string frameworkName = Assembly.GetEntryAssembly().GetCustomAttribute<TargetFrameworkAttribute>().FrameworkName;
      return NuGetFramework.ParseFrameworkName(frameworkName, DefaultFrameworkNameProvider.Instance);
    }
  }
}
