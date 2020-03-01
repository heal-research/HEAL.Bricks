#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Linq;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  [Serializable]
  public class ApplicationRunner : ProcessRunner {
    public ISettings Settings { get; }
    public ApplicationInfo ApplicationInfo { get; }
    public ICommandLineArgument[] Arguments { get; }

    public ApplicationRunner(ISettings settings, ApplicationInfo applicationInfo, ICommandLineArgument[] arguments = null, IProcessRunnerStartInfo startInfo = null) : base(startInfo ?? new NetCoreEntryAssemblyStartInfo()) {
      Settings = settings;
      ApplicationInfo = applicationInfo;
      Arguments = arguments;
    }

    protected override void Process() {
      IPluginManager pluginManager = PluginManager.Create(Settings);
      pluginManager.Initialize();

      if (pluginManager.Status != PluginManagerStatus.OK) {
        SendException(new InvalidOperationException($"{nameof(PluginManager)}.{nameof(pluginManager.Status)} is not {nameof(PluginManagerStatus.OK)}."));
      }

      pluginManager.LoadPackageAssemblies();

      ITypeDiscoverer typeDiscoverer = TypeDiscoverer.Create();
      Type applicationType = typeDiscoverer.GetTypes(typeof(IApplication)).Where(x => x.FullName == ApplicationInfo.TypeName).SingleOrDefault();
      
      if (applicationType == null) {
        SendException(new InvalidOperationException($"Cannot find application {ApplicationInfo.Name}."));
      }

      IApplication application = Activator.CreateInstance(applicationType) as IApplication;
      application.Run(Arguments);
    }

    public void WriteToApplicationConsole(string value) {
      process.StandardInput.WriteLine(value);
    }
    public string ReadFromApplicationConsole() {
      return process.StandardOutput.ReadLine();
    }
    public async Task<string> ReadFromApplicationConsoleAsync() {
      return await process.StandardOutput.ReadLineAsync();
    }
  }
}
