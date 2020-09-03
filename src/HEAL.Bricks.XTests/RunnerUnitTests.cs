#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.Reflection;
using System.IO;

namespace HEAL.Bricks.XTests {
  public class RunnerUnitTests {
    [Fact]
    public async Task TestAnonPipes() {
      var channel = new AnonymousPipesProcessChannel(Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestRunner");
      var runner = new DemoRunner();
      await runner.RunAsync(channel);
    }
    [Fact]
    public async Task TestStdInOut() {
      var channel = new StdInOutProcessChannel(Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestRunner");
      var runner = new DemoRunner();
      await runner.RunAsync(channel);
    }
  }
}
