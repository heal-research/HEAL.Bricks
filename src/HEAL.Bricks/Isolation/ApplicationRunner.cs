#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  [Serializable]
  public sealed class ApplicationRunner : PackageManagerProcessRunner {
    public ApplicationInfo ApplicationInfo { get; }
    public ICommandLineArgument[] Arguments { get; }

    public ApplicationRunner(ISettings settings, ApplicationInfo applicationInfo, ICommandLineArgument[] arguments = null, IProcessRunnerStartInfo startInfo = null) : base(settings, startInfo ?? new NetCoreEntryAssemblyStartInfo()) {
      ApplicationInfo = applicationInfo;
      Arguments = arguments;
      if (applicationInfo.Kind == ApplicationKind.Console) {
        ProcessRunnerStartInfo.CommunicationMode = CommunicationMode.StdInOut;
      }
    }

    protected override async Task ExecuteOnClientAsync(IPackageManager packageManager, CancellationToken cancellationToken) {
      ITypeDiscoverer typeDiscoverer = TypeDiscoverer.Create();
      Type applicationType = typeDiscoverer.GetTypes(typeof(IApplication)).Where(x => x.FullName == ApplicationInfo.TypeName).SingleOrDefault();
      
      if (applicationType == null) {
        throw new InvalidOperationException($"Cannot find application {ApplicationInfo.Name}.");
      }

      IApplication application = Activator.CreateInstance(applicationType) as IApplication;
      await application.RunAsync(Arguments, cancellationToken);
    }

    protected override async Task ExecuteOnHostAsync(CancellationToken cancellationToken) {
      if (ApplicationInfo.Kind == ApplicationKind.Console) {
        Task reader = Task.Run(() => {
          int ch = process.StandardOutput.Read();
          while ((ch != -1) && !cancellationToken.IsCancellationRequested) {
            Console.Write((char)ch);
            ch = process.StandardOutput.Read();
          }
        }, cancellationToken);

        //// alternative code for writer -> polling from console
        //// TODO: decide which version to use
        //Task writer = Task.Run(() => {
        //  while (Status == RunnerStatus.Running) {
        //    if (Console.KeyAvailable) {
        //      string s = Console.ReadLine();
        //      process.StandardInput.WriteLine(s);
        //    } else {
        //      while (!Console.KeyAvailable && (Status == RunnerStatus.Running)) {
        //        Task.Delay(250).Wait();
        //      }
        //    }
        //  }
        //}, cancellationToken);

        Task writer = Task.Run(() => {
          string s = Console.ReadLine();
          while ((s != null) && !cancellationToken.IsCancellationRequested) {
            process.StandardInput.WriteLine(s);
            s = Console.ReadLine();
          }
        }, cancellationToken);

        await reader;

        // not needed if writer polls
        TextReader stdIn = Console.In;
        Console.SetIn(new StringReader(""));
        Console.WriteLine($"Application {ApplicationInfo.Name} terminated. Press ENTER to continue.");

        await writer;

        // not needed if writer polls
        Console.SetIn(stdIn);
      }
    }
  }
}
