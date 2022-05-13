#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using System;

namespace HEAL.Bricks {
  [Serializable]
  public class Message : IMessage { }

  [Serializable]
  public class DataMessage<T> : Message {
    public T Data { get; }
    public DataMessage(T data) => Data = data;
  }

  [Serializable]
  public class StartRunnerMessage : DataMessage<Runner> {
    public StartRunnerMessage(Runner runner) : base(runner) {
      Guard.Argument(runner, nameof(runner)).NotNull();
    }
  }

  [Serializable]
  public class SuspendRunnerMessage : Message { }

  [Serializable]
  public class ResumeRunnerMessage : Message { }

  [Serializable]
  public class CancelMessage : Message { }

  [Serializable]
  public class RunnerStartedMessage : Message { }

  [Serializable]
  public class RunnerStoppedMessage : Message { }

  [Serializable]
  public class ExceptionMessage : DataMessage<Exception> {
    public ExceptionMessage(Exception exception) : base(exception) {
      Guard.Argument(exception, nameof(exception)).NotNull();
    }
  }

  [Serializable]
  public class TextMessage : DataMessage<string> {
    public TextMessage(string text) : base(text) {
      Guard.Argument(text, nameof(text)).NotNull();
    }
  }

  [Serializable]
  public class DiscoveredApplicationsMessage : DataMessage<ApplicationInfo[]> {
    public DiscoveredApplicationsMessage(ApplicationInfo[] applicationInfos) : base(applicationInfos) {
      Guard.Argument(applicationInfos, nameof(applicationInfos)).NotNull();
    }
  }
}
