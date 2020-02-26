#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace HEAL.Bricks {
//  [Serializable]
//  public class DiscoverPluginsRunner : ProcessRunner {
//    public ISettings Settings { get; }

//    public DiscoverPluginsRunner(ISettings settings) {
//      Settings = settings;
//    }
    
//    protected override void Execute() {
////      Console.WriteLine("DiscoverPluginsRunner Execute STARTED");
//      IPluginManager pluginManager = PluginManager.Create(Settings);
//      pluginManager.Initialize();
//      pluginManager.InstallMissingDependenciesAsync().Wait();
//      pluginManager.InstallPackageUpdatesAsync().Wait();
//      pluginManager.LoadPackageAssemblies();

//      ITypeDiscoverer typeDiscoverer = TypeDiscoverer.Create();
//      IEnumerable<Type> pluginTypes = typeDiscoverer.GetTypes(typeof(IPlugin));
//      DiscoveredPluginsRunnerMessage message = new DiscoveredPluginsRunnerMessage(pluginTypes.Select(x => x.FullName));
////      Console.WriteLine("DiscoverPluginsRunner discovered plugins:");
////      foreach (var t in pluginTypes)
////        Console.WriteLine(t.FullName);
//      RunnerMessage.WriteToStream(message, Console.OpenStandardOutput());
////      Console.WriteLine("DiscoverPluginsRunner Message WRITTEN");
//    }

//    protected override void OnRunnerMessage(RunnerMessage message) {
//      if (message is PauseRunnerMessage)
//        ;
//      else if (message is ResumeRunnerMessage)
//        ;
//      else if (message is CancelRunnerMessage)
//        ;
//    }
//  }
}
