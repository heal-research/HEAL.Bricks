#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  [Serializable]
  public sealed class ApplicationRunner : PackageLoaderRunner {
    public ApplicationInfo ApplicationInfo { get; }
    public string[] Arguments { get; }

    public ApplicationRunner(IEnumerable<PackageLoadInfo> packages, ApplicationInfo applicationInfo, string[]? args = null) : base(packages) {
      ApplicationInfo = Guard.Argument(applicationInfo, nameof(applicationInfo)).NotNull();
      Arguments = args ?? Array.Empty<string>();
    }

    protected override async Task ExecuteOnClientAsync(IChannel channel, CancellationToken cancellationToken) {
      await base.ExecuteOnClientAsync(channel, cancellationToken);
      ITypeDiscoverer typeDiscoverer = new TypeDiscoverer();
      Type applicationType = typeDiscoverer.GetTypes(typeof(IApplication))
                                           .Where(x => x.FullName == ApplicationInfo.TypeName)
                                           .SingleOrDefault() ?? throw new InvalidOperationException($"Cannot find application {ApplicationInfo.Name}.");

      IApplication application = (IApplication?)Activator.CreateInstance(applicationType) ?? throw new InvalidOperationException($"Cannot create application {ApplicationInfo.Name}.");
      await application.StartAsync(Arguments, cancellationToken);
    }

    protected override async Task ExecuteOnHostAsync(IChannel channel, CancellationToken cancellationToken) {
      if (ApplicationInfo.Kind == ApplicationKind.Console) {
        if (channel is MemoryChannel) return;

        ProcessChannel processChannel = Guard.Argument(channel, nameof(channel)).Cast<ProcessChannel>();
        Task reader = Task.Run(() => {
          int ch = processChannel.StandardOutput.Read();
          while ((ch != -1) && !cancellationToken.IsCancellationRequested) {
            Console.Write((char)ch);
            ch = processChannel.StandardOutput.Read();
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
          string? s = Console.ReadLine();
          while ((s != null) && !cancellationToken.IsCancellationRequested) {
            processChannel.StandardInput.WriteLine(s);
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
