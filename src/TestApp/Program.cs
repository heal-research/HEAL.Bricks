using System;
using System.IO;
using System.Threading.Tasks;
using HEAL.Bricks;

namespace TestApp {
  class Program {
    static async Task Main(string[] args) {
      if ((args.Length == 1) && (args[0] == Runner.StartRunnerArgument)) {
        Runner.ReceiveAndExecuteRunner(Console.OpenStandardInput());
        return;
      }

      Settings settings = new Settings();
      settings.PluginTag = "HEALBricksPlugin";
      settings.Repositories.Add(@"C:\# Daten\NuGet");
      Directory.CreateDirectory(settings.PackagesPath);
      Directory.CreateDirectory(settings.PackagesCachePath);
      IPluginManager pluginManager = PluginManager.Create(settings);
      pluginManager.Initialize();

      DiscoverApplicationsRunner discoverApplicationsRunner = new DiscoverApplicationsRunner(pluginManager.Settings);
      ApplicationInfo[] applications = discoverApplicationsRunner.GetApplications();

      ConsoleApplicationRunner applicationRunner = new ConsoleApplicationRunner(pluginManager.Settings, applications[1]);
      await applicationRunner.RunAsync();
    }
  }
}
