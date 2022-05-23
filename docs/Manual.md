# HEAL.Bricks Manual
HEAL.Bricks is a package framework for .NET applications based on [NuGet](https://nuget.org). It allows you to download, manage, update, and execute packages at runtime to extend your application. HEAL.Bricks also offers isolation by executing packages in separate processes or Docker containers.


# Example: StringFormatter Console Application
In this example we implement an application which reads a string from the console, offers different formatters to format the string, and write the formatted string back to the console. Formatters are provided by packages, so everyone can easily extend the application by providing additional packages.

First, we create an interface for string formatters. This interface has to be implemented to provide a formatter.
```csharp
namespace HEAL.Bricks.Demo.StringFormatter {
  public interface IStringFormatter {
    string Format(string input);
  }
}
```
As each package needs this interface to provide a new string formatter, we publish this interface in a new NuGet package named `HEAL.Bricks.Demo.StringFormatter.Abstractions`.

Next, we implement a HEAL.Bricks.Demo.StringFormatter.ConsoleApp console application. Each HEAL.Bricks application has to implement the `IApplication` interface, which provides basic information about the application and a `RunAsync` method which is called by HEAL.Bricks to execute the application. HEAL.Bricks also provides the `Application` base class, from which new applications can be derived easily, which is done in the following:

```csharp
namespace HEAL.Bricks.Demo.StringFormatter.ConsoleApp {
  class App : Application {
    public override string Name => "HEAL.Bricks.Demo.StringFormatter";
    public override string Description => "Console application of the HEAL Bricks Demo String Formatter.";
    public override ApplicationKind Kind => ApplicationKind.Console;

    public override async Task RunAsync(string[] args, CancellationToken cancellationToken = default) {
      await Task.Run(() => {
        ITypeDiscoverer typeDiscoverer = new TypeDiscoverer();
        IStringFormatter[] formatters = typeDiscoverer.GetInstances<IStringFormatter>().OrderBy(x => x.GetType().Name).ToArray();

        string input = ReadString();
        while (!string.IsNullOrEmpty(input)) {
          IStringFormatter? formatter = ChooseFormatter(formatters);
          Console.Write("output: ");
          Console.WriteLine(formatter?.Format(input) ?? "--- none ---");
          Console.WriteLine();
          input = ReadString();
        }
      }, cancellationToken);
    }

    private static string ReadString() {
      Console.Write("string > ");
      return Console.ReadLine() ?? string.Empty;
    }

    private static IStringFormatter? ChooseFormatter(IStringFormatter[] formatters) {
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
          _ = int.TryParse(Console.ReadLine(), out index);
        }
        return formatters[index - 1];
      }
    }
  }
}
```
