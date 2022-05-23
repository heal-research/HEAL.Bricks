// See https://aka.ms/new-console-template for more information
using HEAL.Bricks;

using (IChannel? channel = ProcessChannel.CreateFromCLIArguments(args)) {
  if (channel != null) {
    //System.Diagnostics.Debugger.Launch();
    await MessageHandler.Factory.ClientMessageHandler().ReceiveMessagesAsync(channel);
    return;
  }
}

BricksOptions options = BricksOptions.Default;
options.DefaultIsolation = Isolation.AnonymousPipes;
options.Repositories.Add(new Repository(@"C:\00-Daten\NuGet"));
Directory.CreateDirectory(Path.Combine(options.AppPath, options.PackagesPath));
Directory.CreateDirectory(options.PackagesCachePath);
IApplicationManager am = ApplicationManager.Create(options);

if (!am.InstalledRunnables.Any()) {
  Console.WriteLine("No applications or services found.");
  return;
}

int index;
do {
  index = 1;
  foreach (var app in am.InstalledRunnables) {
    Console.WriteLine($"[{index}] {app}");
    index++;
  }
  Console.Write("application > ");
  index = int.TryParse(Console.ReadLine(), out index) ? index - 1 : -1;

  if (index != -1) {
    await am.RunAsync(am.InstalledRunnables.ElementAt(index));
  }
} while (index != -1);

Console.WriteLine("Done.");
