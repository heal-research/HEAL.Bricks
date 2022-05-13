#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using NuGet.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  internal class NuGetLogger : LoggerBase {
    public static ILogger NoLogger => NullLogger.Instance;

    private readonly List<string> log = new();

    public NuGetLogger(LogLevel verbosityLevel = LogLevel.Verbose) : base(verbosityLevel) { }

    public string[] GetLog() {
      return log.ToArray();
    }
    public void Clear() {
      log.Clear();
    }

    public override void Log(ILogMessage message) {
      if (DisplayMessage(message.Level)) {
        log.Add($"[{message.Time}] {message.Level}: {message.FormatWithCode()}");
      }
    }
    public override async Task LogAsync(ILogMessage message) {
      await Task.Run(() => Log(message));
    }

    public override void LogInformationSummary(string data) {
      Log(LogLevel.Information, $"[Summary] {data}");
    }
  }
}
