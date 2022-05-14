#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Microsoft.Extensions.Logging;
using NuGet.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  internal class NuGetLogger : LoggerBase {
    public static NuGet.Common.ILogger NoLogger => NullLogger.Instance;

    private readonly Microsoft.Extensions.Logging.ILogger logger;

    public NuGetLogger(Microsoft.Extensions.Logging.ILogger logger) : base(NuGet.Common.LogLevel.Verbose) {
      this.logger = logger;
    }

    [SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "Log messages are forwarded and not created.")]
    public override void Log(ILogMessage message) {
      if (DisplayMessage(message.Level)) {
        logger.Log(ConvertLogLevel(message.Level), message.FormatWithCode());
      }
    }
    public override async Task LogAsync(ILogMessage message) {
      await Task.Run(() => Log(message));
    }

    public override void LogInformationSummary(string data) {
      Log(NuGet.Common.LogLevel.Information, $"[Summary] {data}");
    }

    private static Microsoft.Extensions.Logging.LogLevel ConvertLogLevel(NuGet.Common.LogLevel level) {
      return level switch {
        NuGet.Common.LogLevel.Debug       => Microsoft.Extensions.Logging.LogLevel.Debug,
        NuGet.Common.LogLevel.Verbose     => Microsoft.Extensions.Logging.LogLevel.Debug,
        NuGet.Common.LogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
        NuGet.Common.LogLevel.Minimal     => Microsoft.Extensions.Logging.LogLevel.Information,
        NuGet.Common.LogLevel.Warning     => Microsoft.Extensions.Logging.LogLevel.Warning,
        NuGet.Common.LogLevel.Error       => Microsoft.Extensions.Logging.LogLevel.Error,
        _ => throw new InvalidOperationException("unknown log level.")
      };
    }
  }
}
