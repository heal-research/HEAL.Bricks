#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace HEAL.Bricks {
  [Serializable]
  public class RunnerMessage : IRunnerMessage {
    private static readonly IFormatter serializer = new BinaryFormatter();

    public static T ReadFromStream<T>(Stream stream) where T : IRunnerMessage {
      return (T)serializer.Deserialize(stream);
    }
    public static IRunnerMessage ReadFromStream(Stream stream) {
      return ReadFromStream<IRunnerMessage>(stream);
    }
    public static void WriteToStream(IRunnerMessage message, Stream stream) {
      serializer.Serialize(stream, message);
      stream.Flush();
    }
  }

  [Serializable]
  public class RunnerDataMessage<T> : RunnerMessage {
    public T Data { get; set; }
    public RunnerDataMessage() => Data = default;
    public RunnerDataMessage(T data) => Data = data;
  }

  [Serializable]
  public class StartRunnerMessage : RunnerDataMessage<IRunner> {
    public StartRunnerMessage(IRunner runner) : base(runner) { }
  }

  [Serializable]
  public class PauseRunnerMessage : RunnerMessage { }

  [Serializable]
  public class ResumeRunnerMessage : RunnerMessage { }

  [Serializable]
  public class CancelRunnerMessage : RunnerMessage { }

  [Serializable]
  public class RunnerStartedMessage : RunnerMessage { }

  [Serializable]
  public class RunnerExceptionMessage : RunnerDataMessage<Exception> {
    public RunnerExceptionMessage(Exception exception) : base(exception) { }
  }

  [Serializable]
  public class RunnerTextMessage : RunnerDataMessage<string> {
    public RunnerTextMessage(string text) : base(text) { }
  }

  [Serializable]
  public class DiscoveredApplicationsMessage : RunnerDataMessage<ApplicationInfo[]> {
    public DiscoveredApplicationsMessage(ApplicationInfo[] applicationInfos) : base(applicationInfos) { }
  }
}
