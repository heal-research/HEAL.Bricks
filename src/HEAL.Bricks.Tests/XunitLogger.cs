#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using NuGet.Common;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace HEAL.Bricks.Tests {
  public class XunitLogger : LoggerBase {
    public ITestOutputHelper Output { get; }

    public XunitLogger(ITestOutputHelper output) : base(LogLevel.Information) {
      Output = output;
    }

    public override void Log(ILogMessage message) {
      if (DisplayMessage(message.Level)) {
        Output.WriteLine($"[{message.Time}] {message.Level}: {message.FormatWithCode()}");
      }
    }
    public override Task LogAsync(ILogMessage message) {
      Log(message);
      return Task.CompletedTask;
    }

    public override void LogInformationSummary(string data) {
      Log(LogLevel.Information, $"[Summary] {data}");
    }
  }
}
