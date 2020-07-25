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
  internal class NuGetLogger : ILogger {
    public static ILogger NoLogger => NullLogger.Instance;

    private readonly List<string> log = new List<string>();
    private readonly LogLevel minLevel;

    public NuGetLogger(LogLevel minLevel = LogLevel.Verbose) {
      this.minLevel = minLevel;
    }

    public string[] GetLog() {
      return log.ToArray();
    }
    public void Clear() {
      log.Clear();
    }

    public void Log(LogLevel level, string data) {
      if (level >= minLevel)
        log.Add($"[{DateTimeOffset.Now}] {level}: {data}");
    }
    public void Log(ILogMessage message) {
      if (message.Level >= minLevel)
        log.Add($"[{message.Time}] {message.Level}: {message.FormatWithCode()}");
    }
    public Task LogAsync(LogLevel level, string data) {
      return Task.Run(() => Log(level, data));
    }
    public Task LogAsync(ILogMessage message) {
      return Task.Run(() => Log(message));
    }
    public void LogDebug(string data) {
      Log(LogLevel.Debug, data);
    }
    public void LogError(string data) {
      Log(LogLevel.Error, data);
    }
    public void LogInformation(string data) {
      Log(LogLevel.Information, data);
    }
    public void LogInformationSummary(string data) {
      Log(LogLevel.Information, $"[Summary] {data}");
    }
    public void LogMinimal(string data) {
      Log(LogLevel.Minimal, data);
    }
    public void LogVerbose(string data) {
      Log(LogLevel.Verbose, data);
    }
    public void LogWarning(string data) {
      Log(LogLevel.Warning, data);
    }
  }
}
