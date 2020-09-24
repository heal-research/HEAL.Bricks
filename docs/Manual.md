# HEAL.Bricks Manual
HEAL.Bricks is a package framework for .NET applications based on [NuGet](https://nuget.org). It allows you to download, manage, update, and execute packages at runtime to extend your application. HEAL.Bricks also offers isolation by executing packages in separate processes or Docker containers.


# Example: StringFormatter Console Application
In this example we implement an application which reads a string from the console, offers different formatters to format the string, and write the formatted string back to the console. Formatters are provided by packages, so everyone can easily extend the application by providing additional packages.

First, we create an interface for string formatters. This interface has to be implemented to provide a formatter.
```csharp
namespace HEAL.StringFormatter {
  public interface IStringFormatter {
    string Format(string input);
  }
}
```
As each package needs this interface to provide a new string formatter, we publish this interface in a new NuGet package named `HEAL.StringFormatter.Interfaces`.

Next, we implement a HEAL.Bricks console application. Each HEAL.Bricks application has to implement the `IApplication` interface, which provides basic information about the application and a `RunAsync` method which is called by HEAL.Bricks to execute the application.

```csharp
using HEAL.Bricks;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.StringFormatter.ConsoleApp {
  class Application : IApplication {
    public string Name => "HEAL.StringFormatter.ConsoleApp";
    public string Description => "Console application of the HEAL String Formatter.";
    public ApplicationKind Kind => ApplicationKind.Console;

    public async Task RunAsync(ICommandLineArgument[] args, CancellationToken cancellationToken) {
      await Task.Run(() => {
        ITypeDiscoverer typeDiscoverer = TypeDiscoverer.Create();
        IStringFormatter[] formatters = typeDiscoverer.GetInstances<IStringFormatter>()
                                        .OrderBy(x => x.GetType().Name).ToArray();

        string input = ReadString();
        while (!string.IsNullOrEmpty(input)) {
          IStringFormatter formatter = ChooseFormatter(formatters);
          Console.Write("output: ");
          Console.WriteLine(formatter?.Format(input) ?? "--- none ---");
          Console.WriteLine();
          input = ReadString();
        }
      }, cancellationToken);
    }

    private string ReadString() {
      Console.Write("string > ");
      return Console.ReadLine();
    }

    private IStringFormatter ChooseFormatter(IStringFormatter[] formatters) {
      if (formatters.Length == 0) {
        Console.WriteLine("No formatters available.");
        return null;
      } else {
        Console.WriteLine("Available formatters:");
        for (int i = 0; i < formatters.Length; i++) {
          Console.WriteLine($"[{i + 1}] {formatters[i].GetType().Name}");
        }
        int index = -1;
        while (index < 1 || index > formatters.Length) {
          Console.Write("formatter > ");
          int.TryParse(Console.ReadLine(), out index);
        }
        return formatters[index - 1];
      }
    }
  }
}
```
