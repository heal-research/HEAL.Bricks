#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("HEAL.Bricks.Runner-3.4, PublicKey=0024000004800000940000000602000000240000525341310004000001000100e3d38bc66a0dd8dd36f57285e34632ec04b3049866ab1e64cf230e95ffcbfbb90c437b4d11dfe74ba981f746274290bb03f3e636e139e685b501031dc6e0bc8409153f0c842721eb9e8e2a703c9e4d102283f3ddbdfab4078c08de51869715992a694d2f608d0fa865c9d17c06b8d6a9135004e982fd862cdb2277e4ad15a4a6")]
namespace HEAL.Bricks {
  [Serializable]
  public sealed class PluginDescription : IPluginDescription {
    private int nTimesLoaded;

    public string Name { get; internal set; }
    public string Description { get; internal set; }
    public Version Version { get; internal set; }
    public string ContactName { get; internal set; }
    public string ContactEmail { get; internal set; }
    public string LicenseText { get; internal set; }
    public PluginState PluginState { get; private set; }

    public string LoadingErrorInformation { get; private set; }

    private List<PluginFile> files = new List<PluginFile>();
    public IEnumerable<IPluginFile> Files => files;
    internal void AddFiles(IEnumerable<PluginFile> names) {
      files.AddRange(names);
    }

    private List<PluginDescription> dependencies = new List<PluginDescription>();
    internal IEnumerable<PluginDescription> Dependencies => dependencies;
    IEnumerable<IPluginDescription> IPluginDescription.Dependencies {
      get { return dependencies.Cast<IPluginDescription>(); }
    }
    internal void AddDependency(PluginDescription dependency) {
      dependencies.Add(dependency);
    }


    public IEnumerable<string> AssemblyLocations {
      get { return Files.Where(f => f.Type == PluginFileType.Assembly).Select(f => f.Name); }
    }

    private List<string> assemblyNames;
    public IEnumerable<string> AssemblyNames {
      get { return assemblyNames; }
      set { this.assemblyNames = new List<string>(value); }
    }

    internal PluginDescription() {
      PluginState = PluginState.Undefined;
    }

    internal void Disable(string loadingErrorInformation) {
      if (PluginState != PluginState.Undefined)
        throw new InvalidOperationException("Cannot disable a plugin in state " + PluginState);
      PluginState = PluginState.Disabled;
      LoadingErrorInformation = loadingErrorInformation;
    }

    internal void Enable() {
      if (PluginState != PluginState.Undefined)
        throw new InvalidOperationException("Cannot enable a plugin in state " + PluginState);
      PluginState = PluginState.Enabled;
    }

    internal void Load() {
      if (!(PluginState == PluginState.Enabled || PluginState == PluginState.Loaded))
        throw new InvalidOperationException("Cannot load a plugin in state " + PluginState);
      PluginState = PluginState.Loaded;
      nTimesLoaded++;
    }

    internal void Unload() {
      if (PluginState != PluginState.Loaded)
        throw new InvalidOperationException("Cannot unload a plugin in state " + PluginState);
      nTimesLoaded--;
      if (nTimesLoaded == 0) PluginState = PluginState.Enabled;
    }


    public override string ToString() {
      return Name + " " + Version;
    }

    public override bool Equals(object obj) {
      PluginDescription other = obj as PluginDescription;
      if (other == null) return false;

      return other.Name == this.Name && other.Version == this.Version;
    }
    public override int GetHashCode() {
      if (Version != null) {
        return Name.GetHashCode() + Version.GetHashCode();
      } else return Name.GetHashCode();
    }
  }
}
