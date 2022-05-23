#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.Text.Json;

namespace HEAL.Bricks {
  [Serializable]
  public sealed class Message : IMessage {
    public static class Commands {
      public const string LoadPackages        = nameof(LoadPackages);
      public const string PackagesLoaded      = nameof(PackagesLoaded);
      public const string DiscoverRunnables   = nameof(DiscoverRunnables);
      public const string RunnablesDiscovered = nameof(RunnablesDiscovered);
      public const string RunRunnable         = nameof(RunRunnable);
      public const string Log                 = nameof(Log);
      public const string Terminate           = nameof(Terminate);
    }

    public static IMessage Create(string command) {
      if (command == null) throw new ArgumentNullException(nameof(command));
      if (string.IsNullOrWhiteSpace(command)) throw new ArgumentException($"Argument {nameof(command)} must not be empty.", nameof(command));

      return new Message {
        Command = command,
        Payload = string.Empty
      };
    }
    public static IMessage Create<T>(string command, T payload) {
      if (command == null) throw new ArgumentNullException(nameof(command));
      if (string.IsNullOrWhiteSpace(command)) throw new ArgumentException($"Argument {nameof(command)} must not be empty.", nameof(command));

      return new Message {
        Command = command,
        Payload = payload == null ? string.Empty : JsonSerializer.Serialize(payload)
      };
    }

    public string Command { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;

    public T DeserializePayload<T>() {
      return JsonSerializer.Deserialize<T>(Payload) ?? throw new InvalidOperationException($"Cannot deserialize payload of message '{Command}'.");
    }

    public override string ToString() => Command;
  }
}
