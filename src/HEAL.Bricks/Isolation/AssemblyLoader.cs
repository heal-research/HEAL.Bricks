#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace HEAL.Bricks {

  /// <summary>
  /// Class to load assemblies.
  /// </summary>
  public class AssemblyLoader : IAssemblyLoader {
    /// <summary>
    /// Comparer for AssemblyNames.
    /// </summary>
    private class AssemblyNameComparer : IEqualityComparer<AssemblyName> {
      private object concatProps(AssemblyName name) => name.FullName + name.Version.ToString();
      public bool Equals(AssemblyName lhs, AssemblyName rhs) => concatProps(lhs).Equals(concatProps(rhs));
      public int GetHashCode(AssemblyName obj) => concatProps(obj).GetHashCode();
    }

    #region Vars
    private readonly IList<Assembly> assemblies = new List<Assembly>();
    private readonly IList<Type> types = new List<Type>();
    #endregion

    #region Properties
    /// <summary>
    /// All loaded assemblies. Contains items only after a call of LoadAssemblies.
    /// </summary>
    public IEnumerable<Assembly> Assemblies => assemblies;

    /// <summary>
    /// All types found in all assemblies.
    /// </summary>
    public IEnumerable<Type> Types => types;
    #endregion

    #region Constructors
    public AssemblyLoader() {

      #region alternative code
      /*
      ScanTypesInBasePath();
      Types = GetTypes();
      */
      /*
      IEnumerable<Assembly> asms = CheckDependencies(GetLoadableAssemblies(GetAssembliesFromBasePath()));
      LoadAssemblies(asms);
      LoadTypes(asms);
      */
      //IEnumerable<Assembly> asms = GetLoadableAssemblies(GetAssembliesFromBasePath());
      //LoadAssemblies(asms);
      //LoadTypes(asms);
      #endregion
    }
    #endregion


    private IEnumerable<string> GetAssembliesFromBasePath(string basePath) {
      return Directory.GetFiles(basePath, "*.dll", SearchOption.AllDirectories)
        .Concat(Directory.GetFiles(basePath, "*.exe", SearchOption.AllDirectories));
    }

    #region alternative code
    /*
    // 2.
    private IEnumerable<Assembly> GetLoadableAssemblies(IEnumerable<string> paths) {
      IList<Assembly> assemblies = new List<Assembly>();      
      foreach (string path in paths) {
        try {
          var asm = Assembly.LoadFile(path);
          if (asm != null)
            assemblies.Add(asm);
          else
            Console.WriteLine($"cannot load assembly with path={path}");
        } catch (Exception e) {
          Console.WriteLine($"exception occured for assembly with path={path}, type of exception={e.GetType()}");
        }
      }
      
      return assemblies;
    }
    
    // 3.
    private IEnumerable<Assembly> CheckDependencies(IEnumerable<Assembly> assemblies) {
      IEqualityComparer<AssemblyName> comp = new AssemblyNameComparer();
      IDictionary<AssemblyName, Assembly> dict = new Dictionary<AssemblyName, Assembly>(comp);

      // fill dict
      foreach (Assembly a in assemblies)
        dict.Add(a.GetName(), a);

      Parallel.ForEach(new List<Assembly>(dict.Values), asm => {
        if (CheckDependencyRecursiveHelper(asm.GetName(), asm.GetReferencedAssemblies(), comp, dict))
          lock(dict)
            dict.Remove(asm.GetName());
      });
      return dict.Values;
    }

    private bool CheckDependencyRecursiveHelper(AssemblyName toCheck, IEnumerable<AssemblyName> refs, IEqualityComparer<AssemblyName> comp, IDictionary<AssemblyName, Assembly> dict) {
      foreach(AssemblyName name in refs) {
        if (comp.Equals(toCheck, name) || !dict.ContainsKey(name) || CheckDependencyRecursiveHelper(toCheck, dict[name].GetReferencedAssemblies(), comp, dict)) return true;
      }
      return false;
    }
    
    // 4.
    private void LoadAssemblies(IEnumerable<Assembly> assemblies) {

      foreach (Assembly asm in assemblies) {
        try {
          Assemblies.Add(alc.LoadFromAssemblyName(asm.GetName()));
        } catch(Exception e) { }
      }
    }
    */
    #endregion

    private void LoadTypes(IEnumerable<Assembly> assemblies) {
      foreach (Assembly asm in assemblies) {
        try {
          foreach (Type t in asm.GetExportedTypes()) {
            types.Add(t);
          }
        } catch (ReflectionTypeLoadException e) {
          // ReflectionTypeLoadException gets thrown if any class in a module cannot be loaded.
          try {
            foreach (Type t in e.Types) { // fetch the already loaded types, be careful some of them can be null
              if (t != null) {
                types.Add(t);
              }
            }
          } catch (BadImageFormatException) { }
        } catch (Exception) { // to catch every other exception
          //Tracing.Logger.Error(
          //  $"Exception occured while loading types of assembly {asm.FullName}! \n " +
          //  $"---- Stacktrace ---- \n {e}");
        }
      }
    }

    public IEnumerable<Assembly> LoadAssemblies(string basePath) {
      foreach (string path in GetAssembliesFromBasePath(basePath))
        LoadAssemblyFromPath(path);

      LoadTypes(this.Assemblies);
      return this.Assemblies;
    }

    public IEnumerable<Assembly> LoadAssemblies(IEnumerable<AssemblyInfo> assemblyInfos) {
      foreach (var info in assemblyInfos)
        LoadAssemblyFromPath(info.Path.ToString());

      LoadTypes(this.Assemblies);
      return this.Assemblies;
    }

    private void LoadAssemblyFromPath(string path) {
      try {
        var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(path); // loads assembly into default context
        if (asm != null) {
          assemblies.Add(asm);
        } // else
          //Tracing.Logger.Error($"Unnable to load assembly with path {path}!");
      } catch (Exception) { // to catch every exception occured by assembly loading.
        //Tracing.Logger.Error(
        //  $"Exception occured while loading assembly from path {path}! \n " +
        //  $"---- Stacktrace ---- \n {e}");
      }
    }
  }
}