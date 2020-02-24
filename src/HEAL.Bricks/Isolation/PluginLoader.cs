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
using System.Security;
using System.Text;

namespace HEAL.Bricks {
  public class PluginLoader : IPluginLoader {

    /// <summary>
    /// Helper class for depedency metadata.
    /// </summary>
    private class PluginDependency {
      public string Name { get; private set; }
      public Version Version { get; private set; }
      public PluginDependency(string name, Version version) {
        this.Name = name;
        this.Version = version;
      }
    }

    #region Vars
    private IList<IPlugin> plugins = new List<IPlugin>();
    private IList<IApplication> applications = new List<IApplication>();
    private IAssemblyLoader assemblyLoader = null;
    private Dictionary<PluginDescription, IEnumerable<PluginDependency>> pluginDependencies = new Dictionary<PluginDescription, IEnumerable<PluginDependency>>();
    #endregion

    #region Properties
    public IEnumerable<IPlugin> Plugins => plugins;

    public IEnumerable<IApplication> Applications => applications;
    #endregion

    #region Constructors
    public PluginLoader(IAssemblyLoader assemblyLoader) {
      this.assemblyLoader = assemblyLoader;
    }
    #endregion

    #region Discover Methods
    public void LoadPlugins(IEnumerable<AssemblyInfo> assemblyInfos) {
      assemblyLoader.LoadAssemblies(assemblyInfos);
      DiscoverPlugins();
      DiscoverApplications();
    }
    /// <summary>
    /// Discovers all types of IPlugin of all loaded assemblies. Saves the found plugins in a list.
    /// </summary>
    private void DiscoverPlugins() {
      // search plugins out of all types
      string curDir = Directory.GetCurrentDirectory(); // save current working directory
      foreach (var type in assemblyLoader.Types) {
        if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface && !type.HasElementType) {
          // to set the working directory to the assembly location, this is necessary for assemblies which load data in the OnLoad method by their own.
          // example: HeuristicLabMathJaxPlugin
          Directory.SetCurrentDirectory(type.Assembly.Location.Replace(Path.GetFileName(type.Assembly.Location), ""));
          IPlugin p = (IPlugin)Activator.CreateInstance(type);
          p.OnLoad();
          plugins.Add(p);
        }
      }
      Directory.SetCurrentDirectory(curDir); // set working directory to its base value
    }

    /// <summary>
    /// Discovers all types of IApplication of all loaded assemblies. Saves the found applications in a list.
    /// </summary>
    private void DiscoverApplications() {
      foreach (Type type in assemblyLoader.Types) {
        if (typeof(IApplication).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface && !type.HasElementType) {
          IApplication app = (IApplication)Activator.CreateInstance(type);
          applications.Add(app);
        }
      }
    }
    #endregion
       
    public IEnumerable<AssemblyInfo> Validate(string basePath) {
      IEnumerable<Assembly> assemblies = assemblyLoader.LoadAssemblies(basePath);

      IList<PluginDescription> pluginDescriptions = GatherPluginDescriptions(assemblies, basePath);
      CheckPluginFiles(pluginDescriptions, basePath);

      // check if all plugin assemblies can be loaded
      CheckPluginAssemblies(assemblies, pluginDescriptions);

      // a full list of plugin descriptions is available now we can build the dependency tree
      BuildDependencyTree(pluginDescriptions);

      // check for dependency cycles
      CheckPluginDependencyCycles(pluginDescriptions);

      // 1st time recursively check if all necessary plugins are available and not disabled
      // disable plugins with missing or disabled dependencies 
      // to prevent that plugins with missing dependencies are loaded into the execution context
      // in the next step
      CheckPluginDependencies(pluginDescriptions);

      // build assemblyInfo list: 
      // 1.) iterate through pluginDescriptions and pick all not disabled plugins
      // 2.) iterate through the assemblyLocations, saved in a description
      // 3.) iterate thorugh all loaded assemblies 
      // 4.) if the location of an assemblies and a pluginDescriptions match -> create new AssemblyInfo and add it to a list
      IList<AssemblyInfo> list = new List<AssemblyInfo>();
      foreach (var desc in pluginDescriptions) {
        if (desc.PluginState != PluginState.Disabled) {
          foreach (var loc in desc.AssemblyLocations) {
            foreach (var asm in assemblies) {
              if (string.Equals(Path.GetFullPath(asm.Location), Path.GetFullPath(loc), StringComparison.CurrentCultureIgnoreCase))
                list.Add(new AssemblyInfo() { Path = new UniPath(asm.Location), Name = asm.FullName });
            }
          }
        }
      }

      return list;
    }

    #region oldCode
    /// <summary>
    /// Checks if all plugin assemblies can be loaded. If an assembly can't be loaded the plugin is disabled.
    /// </summary>
    /// <param name="pluginDescriptions"></param>
    private void CheckPluginAssemblies(IEnumerable<Assembly> assemblies, IEnumerable<PluginDescription> pluginDescriptions) {
      foreach (var desc in pluginDescriptions.Where(x => x.PluginState != PluginState.Disabled)) {
        try {
          var missingAssemblies = new List<string>();
          foreach (var asmLocation in desc.AssemblyLocations) {
            // the assembly must have been loaded in ReflectionOnlyDlls
            // so we simply determine the name of the assembly and try to find it in the cache of loaded assemblies
            var asmName = AssemblyName.GetAssemblyName(asmLocation);
            if (!assemblies.Select(x => x.GetName().FullName).Contains(asmName.FullName)) {
              missingAssemblies.Add(asmName.FullName);
            }
          }
          if (missingAssemblies.Count > 0) {
            StringBuilder errorStrBuiler = new StringBuilder();
            errorStrBuiler.AppendLine("Missing assemblies:");
            foreach (string missingAsm in missingAssemblies) {
              errorStrBuiler.AppendLine(missingAsm);
            }
            desc.Disable(errorStrBuiler.ToString());
          }
        } catch (BadImageFormatException ex) {
          // disable the plugin
          desc.Disable("Problem while loading plugin assemblies:" + Environment.NewLine + "BadImageFormatException: " + ex.Message);
        } catch (FileNotFoundException ex) {
          // disable the plugin
          desc.Disable("Problem while loading plugin assemblies:" + Environment.NewLine + "FileNotFoundException: " + ex.Message);
        } catch (FileLoadException ex) {
          // disable the plugin
          desc.Disable("Problem while loading plugin assemblies:" + Environment.NewLine + "FileLoadException: " + ex.Message);
        } catch (ArgumentException ex) {
          // disable the plugin
          desc.Disable("Problem while loading plugin assemblies:" + Environment.NewLine + "ArgumentException: " + ex.Message);
        } catch (SecurityException ex) {
          // disable the plugin
          desc.Disable("Problem while loading plugin assemblies:" + Environment.NewLine + "SecurityException: " + ex.Message);
        }
      }
    }


    // find all types implementing IPlugin in the reflectionOnlyAssemblies and create a list of plugin descriptions
    // the dependencies in the plugin descriptions are not yet set correctly because we need to create
    // the full list of all plugin descriptions first
    private IList<PluginDescription> GatherPluginDescriptions(IEnumerable<Assembly> assemblies, string basePath) {
      List<PluginDescription> pluginDescriptions = new List<PluginDescription>();
      foreach (Assembly assembly in assemblies) {
        // GetExportedTypes throws FileNotFoundException when a referenced assembly
        // of the current assembly is missing.
        try {
          // if there is a type that implements IPlugin
          // use AssemblyQualifiedName to compare the types because we can't directly 
          // compare ReflectionOnly types and execution types

          var assemblyPluginDescriptions = from t in assembly.GetExportedTypes()
                                           where !t.IsAbstract && t.GetInterfaces()
                                           .Any(x => x.AssemblyQualifiedName == typeof(IPlugin).AssemblyQualifiedName)
                                           select GetPluginDescription(t, basePath);
          pluginDescriptions.AddRange(assemblyPluginDescriptions);
        }
        // ignore exceptions. Just don't yield a plugin description when an exception is thrown
        catch (FileNotFoundException) {
        } catch (FileLoadException) {
        } catch (InvalidPluginException) {
        } catch (TypeLoadException) {
        } catch (MissingMemberException) {
        }
      }
      return pluginDescriptions;
    }

    // checks if all declared plugin files are actually available and disables plugins with missing files
    private void CheckPluginFiles(IEnumerable<PluginDescription> pluginDescriptions, string basePath) {
      foreach (PluginDescription desc in pluginDescriptions) {
        IEnumerable<string> missingFiles;
        if (ArePluginFilesMissing(desc, basePath, out missingFiles)) {
          StringBuilder errorStrBuilder = new StringBuilder();
          errorStrBuilder.AppendLine("Missing files:");
          foreach (string fileName in missingFiles) {
            errorStrBuilder.AppendLine(fileName);
          }
          desc.Disable(errorStrBuilder.ToString());
        }
      }
    }

    private bool ArePluginFilesMissing(PluginDescription pluginDescription, string basePath, out IEnumerable<string> missingFiles) {
      List<string> missing = new List<string>();
      foreach (string filename in pluginDescription.Files.Select(x => x.Name)) {
        if (!FileLiesInDirectory(basePath, filename) ||
          !File.Exists(filename)) {
          missing.Add(filename);
        }
      }
      missingFiles = missing;
      return missing.Count > 0;
    }

    private static bool FileLiesInDirectory(string dir, string fileName) {
      var basePath = Path.GetFullPath(dir);
      return Path.GetFullPath(fileName).StartsWith(basePath);
    }

    /// <summary>
    /// Extracts plugin information for this type.
    /// Reads plugin name, list and type of files and dependencies of the plugin. This information is necessary for
    /// plugin dependency checking before plugin activation.
    /// </summary>
    /// <param name="pluginType"></param>
    private PluginDescription GetPluginDescription(Type pluginType, string basePath) {

      string pluginName, pluginDescription, pluginVersion;
      string contactName, contactAddress;
      GetPluginMetaData(pluginType, out pluginName, out pluginDescription, out pluginVersion);
      GetPluginContactMetaData(pluginType, out contactName, out contactAddress);
      var pluginFiles = GetPluginFilesMetaData(pluginType, basePath);
      var pluginDependencies = GetPluginDependencyMetaData(pluginType);

      // minimal sanity check of the attribute values
      if (!string.IsNullOrEmpty(pluginName) &&
          pluginFiles.Count() > 0 &&                                 // at least one file
          pluginFiles.Any(f => f.Type == PluginFileType.Assembly)) { // at least one assembly
        // create a temporary PluginDescription that contains the attribute values
        PluginDescription info = new PluginDescription();
        info.Name = pluginName;
        info.Description = pluginDescription;
        info.Version = new Version(pluginVersion);
        info.ContactName = contactName;
        info.ContactEmail = contactAddress;
        info.LicenseText = ReadLicenseFiles(pluginFiles);
        info.AddFiles(pluginFiles);

        this.pluginDependencies[info] = pluginDependencies;
        return info;
      } else {
        throw new InvalidPluginException("Invalid metadata in plugin " + pluginType.ToString());
      }
    }

    private string ReadLicenseFiles(IEnumerable<PluginFile> pluginFiles) {
      // combine the contents of all plugin files 
      var licenseFiles = from file in pluginFiles
                         where file.Type == PluginFileType.License
                         select file;
      if (licenseFiles.Count() == 0) return string.Empty;
      StringBuilder licenseTextBuilder = new StringBuilder();
      licenseTextBuilder.AppendLine(File.ReadAllText(licenseFiles.First().Name));
      foreach (var licenseFile in licenseFiles.Skip(1)) {
        licenseTextBuilder.AppendLine().AppendLine(); // leave some empty space between multiple license files
        licenseTextBuilder.AppendLine(File.ReadAllText(licenseFile.Name));
      }
      return licenseTextBuilder.ToString();
    }

    private static IEnumerable<PluginDependency> GetPluginDependencyMetaData(Type pluginType) {
      // get all attributes of type PluginDependency
      var dependencyAttributes = pluginType.GetCustomAttributes<PluginDependencyAttribute>();
      /*from attr in CustomAttributeData.GetCustomAttributes(pluginType)
                               where IsAttributeDataForType(attr, typeof(PluginDependencyAttribute))
                               select attr;*/
      foreach (var dependencyAttr in dependencyAttributes) {
        string name = (string)dependencyAttr.Dependency; //ConstructorArguments[0].Value;
        Version version = new Version("0.0.0.0"); // default version
        // check if version is given for now
        // later when the constructor of PluginDependencyAttribute with only one argument has been removed
        // this conditional can be removed as well
        if (dependencyAttr.Version != null) {
          try {
            version = dependencyAttr.Version;//new Version((string)dependencyAttr.ConstructorArguments[1].Value); // might throw FormatException
          } catch (FormatException ex) {
            throw new InvalidPluginException("Invalid version format of dependency " + name + " in plugin " + pluginType.ToString(), ex);
          }
        }
        yield return new PluginDependency(name, version);
      }
    }

    private static void GetPluginContactMetaData(Type pluginType, out string contactName, out string contactAddress) {
      // get attribute of type ContactInformation if there is any
      var contactInfoAttribute = (from attr in CustomAttributeData.GetCustomAttributes(pluginType)
                                  where IsAttributeDataForType(attr, typeof(ContactInformationAttribute))
                                  select attr).SingleOrDefault();

      if (contactInfoAttribute != null) {
        contactName = (string)contactInfoAttribute.ConstructorArguments[0].Value;
        contactAddress = (string)contactInfoAttribute.ConstructorArguments[1].Value;
      } else {
        contactName = string.Empty;
        contactAddress = string.Empty;
      }
    }

    // not static because we need the BasePath property
    private IEnumerable<PluginFile> GetPluginFilesMetaData(Type pluginType, string basePath) {
      // get all attributes of type PluginFileAttribute
      var pluginFileAttributes = from attr in CustomAttributeData.GetCustomAttributes(pluginType)
                                 where IsAttributeDataForType(attr, typeof(PluginFileAttribute))
                                 select attr;
      foreach (var pluginFileAttribute in pluginFileAttributes) {
        string pluginFileName = (string)pluginFileAttribute.ConstructorArguments[0].Value;
        PluginFileType fileType = (PluginFileType)pluginFileAttribute.ConstructorArguments[1].Value;
        yield return new PluginFile(Path.GetFullPath(Path.Combine(basePath, pluginFileName)), fileType);
      }
    }

    private static void GetPluginMetaData(Type pluginType, out string pluginName, out string pluginDescription, out string pluginVersion) {
      // there must be a single attribute of type PluginAttribute
      var pluginMetaDataAttr = (from attr in CustomAttributeData.GetCustomAttributes(pluginType)
                                where IsAttributeDataForType(attr, typeof(PluginAttribute))
                                select attr).Single();

      pluginName = (string)pluginMetaDataAttr.ConstructorArguments[0].Value;

      // default description and version
      pluginVersion = "0.0.0.0";
      pluginDescription = string.Empty;
      if (pluginMetaDataAttr.ConstructorArguments.Count() == 2) {
        // if two arguments are given the second argument is the version
        pluginVersion = (string)pluginMetaDataAttr.ConstructorArguments[1].Value;
      } else if (pluginMetaDataAttr.ConstructorArguments.Count() == 3) {
        // if three arguments are given the second argument is the description and the third is the version
        pluginDescription = (string)pluginMetaDataAttr.ConstructorArguments[1].Value;
        pluginVersion = (string)pluginMetaDataAttr.ConstructorArguments[2].Value;
      }
    }


    private static bool IsAttributeDataForType(CustomAttributeData attributeData, Type attributeType) {
      return attributeData.Constructor.DeclaringType.AssemblyQualifiedName == attributeType.AssemblyQualifiedName;
    }

    // builds a dependency tree of all plugin descriptions
    // searches matching plugin descriptions based on the list of dependency names for each plugin
    // and sets the dependencies in the plugin descriptions
    private void BuildDependencyTree(IEnumerable<PluginDescription> pluginDescriptions) {
      foreach (var desc in pluginDescriptions.Where(x => x.PluginState != PluginState.Disabled)) {
        var missingDependencies = new List<PluginDependency>();
        foreach (var dependency in pluginDependencies[desc]) {
          var matchingDescriptions = from availablePlugin in pluginDescriptions
                                     where availablePlugin.PluginState != PluginState.Disabled
                                     where availablePlugin.Name == dependency.Name
                                     where IsCompatiblePluginVersion(availablePlugin.Version, dependency.Version)
                                     select availablePlugin;
          if (matchingDescriptions.Count() > 0) {
            desc.AddDependency(matchingDescriptions.First());
          } else {
            missingDependencies.Add(dependency);
          }
        }
        // no plugin description that matches the dependencies are available => plugin is disabled
        if (missingDependencies.Count > 0) {
          StringBuilder errorStrBuilder = new StringBuilder();
          errorStrBuilder.AppendLine("Missing dependencies:");
          foreach (var missingDep in missingDependencies) {
            errorStrBuilder.AppendLine(missingDep.Name + " " + missingDep.Version);
          }
          desc.Disable(errorStrBuilder.ToString());
        }
      }
    }

    /// <summary>
    /// Checks if version <paramref name="available"/> is compatible to version <paramref name="requested"/>.
    /// Note: the compatibility relation is not bijective.
    /// Compatibility rules:
    ///  * major and minor number must be the same
    ///  * build and revision number of <paramref name="available"/> must be larger or equal to <paramref name="requested"/>.
    /// </summary>
    /// <param name="available">The available version which should be compared to <paramref name="requested"/>.</param>
    /// <param name="requested">The requested version that must be matched.</param>
    /// <returns></returns>
    private static bool IsCompatiblePluginVersion(Version available, Version requested) {
      // this condition must be removed after all plugins have been updated to declare plugin and dependency versions
      if (
        (requested.Major == 0 && requested.Minor == 0) ||
        (available.Major == 0 && available.Minor == 0)) return true;
      return
        available.Major == requested.Major &&
        available.Minor == requested.Minor &&
        available.Build >= requested.Build &&
        available.Revision >= requested.Revision;
    }

    private void CheckPluginDependencyCycles(IEnumerable<PluginDescription> pluginDescriptions) {
      foreach (var plugin in pluginDescriptions) {
        // if the plugin is not disabled check if there are cycles
        if (plugin.PluginState != PluginState.Disabled && HasCycleInDependencies(plugin, plugin.Dependencies)) {
          plugin.Disable("Dependency graph has a cycle.");
        }
      }
    }

    private bool HasCycleInDependencies(PluginDescription plugin, IEnumerable<PluginDescription> pluginDependencies) {
      foreach (var dep in pluginDependencies) {
        // if one of the dependencies is the original plugin we found a cycle and can return
        // if the dependency is already disabled we can ignore the cycle detection because we will disable the plugin anyway
        // if following one of the dependencies recursively leads to a cycle then we also return
        if (dep == plugin || dep.PluginState == PluginState.Disabled || HasCycleInDependencies(plugin, dep.Dependencies)) return true;
      }
      // no cycle found and none of the direct and indirect dependencies is disabled
      return false;
    }

    private void CheckPluginDependencies(IEnumerable<PluginDescription> pluginDescriptions) {
      foreach (PluginDescription pluginDescription in pluginDescriptions.Where(x => x.PluginState != PluginState.Disabled)) {
        List<PluginDescription> disabledPlugins = new List<PluginDescription>();
        if (IsAnyDependencyDisabled(pluginDescription, disabledPlugins)) {
          StringBuilder errorStrBuilder = new StringBuilder();
          errorStrBuilder.AppendLine("Dependencies are disabled:");
          foreach (var disabledPlugin in disabledPlugins) {
            errorStrBuilder.AppendLine(disabledPlugin.Name + " " + disabledPlugin.Version);
          }
          pluginDescription.Disable(errorStrBuilder.ToString());
        }
      }
    }

    private bool IsAnyDependencyDisabled(PluginDescription descr, List<PluginDescription> disabledPlugins) {
      if (descr.PluginState == PluginState.Disabled) {
        disabledPlugins.Add(descr);
        return true;
      }
      foreach (PluginDescription dependency in descr.Dependencies) {
        IsAnyDependencyDisabled(dependency, disabledPlugins);
      }
      return disabledPlugins.Count > 0;
    }
  }
  #endregion
}
