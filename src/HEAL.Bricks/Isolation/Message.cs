#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using System.Text.Json;

namespace HEAL.Bricks {
  [Serializable]
  public class Message : IMessage {
    public static class Commands {
      public const string LoadPackages        = nameof(LoadPackages);
      public const string PackagesLoaded      = nameof(PackagesLoaded);
      public const string DiscoverRunnables   = nameof(DiscoverRunnables);
      public const string RunnablesDiscovered = nameof(RunnablesDiscovered);
      public const string RunRunnable         = nameof(RunRunnable);
      public const string Log                 = nameof(Log);
      public const string Terminate           = nameof(Terminate);
    }

    public string Command { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;

    public T DeserializePayload<T>() {
      return JsonSerializer.Deserialize<T>(Payload) ?? throw new InvalidOperationException($"Cannot deserialize payload of message '{Command}'.");
    }

    public static class Factory {
      public static IMessage CustomMessage(string command) => new Message {
        Command = Guard.Argument(command, nameof(command)).NotNull().NotEmpty().NotWhiteSpace().Value,
        Payload = string.Empty
      };
      public static IMessage CustomMessage<T>(string command, T payload) => new Message {
        Command = Guard.Argument(command, nameof(command)).NotNull().NotEmpty().NotWhiteSpace().Value,
        Payload = payload == null ? string.Empty : JsonSerializer.Serialize(payload)
      };
        
      public static IMessage LoadPackages(IEnumerable<PackageLoadInfo> packages)      => CustomMessage(Commands.LoadPackages, packages);
      public static IMessage PackagesLoaded()                                         => CustomMessage(Commands.PackagesLoaded);
      public static IMessage DiscoverRunnables()                                      => CustomMessage(Commands.DiscoverRunnables);
      public static IMessage RunnablesDiscovered(IEnumerable<RunnableInfo> runnables) => CustomMessage(Commands.RunnablesDiscovered, runnables);
      public static IMessage RunRunnable(RunnableInfo runnable, string[] args)        => CustomMessage(Commands.RunRunnable, new Tuple<RunnableInfo, string[]>(runnable, args)); 
      public static IMessage Log(string message)                                      => CustomMessage(Commands.Log, message);
      public static IMessage Terminate()                                              => CustomMessage(Commands.Terminate);
    }
  }
}
