using HEAL.Bricks;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Calculator {
  class Program {
    static async Task Main(string[] args) {
//      string nuGetRepository = "https://api.nuget.org/v3/index.json";
//      string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

//      IPluginManager pm = PluginManager.Create(nuGetRepository);

//      Console.WriteLine("List local plugins:");
//      foreach (IPluginInfo plugin in await pm.GetLocalPluginsAsync("HEALBricksPlugin")) {
//        Console.WriteLine(plugin.ToStringWithDependencies());
//      }
//      Console.WriteLine("Listing of local plugins finished.\n");

//      Console.WriteLine("List remote plugins:");
////      foreach (IPluginInfo plugin in await pm.ReadRemotePlugins("owner:HeuristicLab")) {
//      foreach (IPluginInfo plugin in await pm.GetRemotePluginsAsync("HtmlAgilityPack version:1.11.17")) {
//        Console.WriteLine(plugin.ToStringWithDependencies());
//      }
//      Console.WriteLine("Listing of remote plugins finished.\n");

//      Console.WriteLine("List local plugin dependencies:");
//      foreach (IPluginInfo plugin in await pm.GetLocalPluginDependenciesAsync("HEALBricksPlugin")) {
//        Console.WriteLine(plugin.ToString());
//      }
//      Console.WriteLine("Listing of local plugin dependencies finished.\n");

//      Console.WriteLine("Download remote plugin:");
//      bool result;
//      IPluginInfo pluginInfo;
//      pluginInfo = new PluginInfo("HEAL.Attic", "1.4.0");
//      result = await pm.DownloadPluginAsync(pluginInfo, appDir);
//      Console.WriteLine($"Download of plugin {pluginInfo.Name} ({pluginInfo.Version.ToString()}) was successful: {result}");
//      pluginInfo = new PluginInfo("SimSharp", "3.3.0");
//      result = await pm.DownloadPluginAsync(pluginInfo, appDir);
//      Console.WriteLine($"Download of plugin {pluginInfo.Name} ({pluginInfo.Version.ToString()}) was successful: {result}");
//      Console.WriteLine("Download of remote plugin finished.\n");

      Console.WriteLine("Loading assemblies:");
      NuGetAssemblyLoader.Load();
      Console.WriteLine("Assembly loading finished.\n");

      Console.WriteLine("Discovering IApplication:");
      ITypeDiscoverer td = TypeDiscoverer.Create();
      foreach (Type t in td.GetTypes(typeof(IApplication)))
        Console.WriteLine(t.FullName);
      Console.WriteLine();

      Console.WriteLine("Applications:");
      var apps = td.GetInstances<IApplication>();
      foreach (IApplication app in apps)
        Console.WriteLine($"{app.Name}: {app.Description}");
      Console.WriteLine();

      foreach (IApplication app in apps) {
        Console.WriteLine($"Running application {app.Name}:");
        app.Run(CommandLineArgumentHandling.GetArguments(args));
        Console.WriteLine($"Application {app.Name} terminated.");
        Console.WriteLine();
      }
    }
  }
}
