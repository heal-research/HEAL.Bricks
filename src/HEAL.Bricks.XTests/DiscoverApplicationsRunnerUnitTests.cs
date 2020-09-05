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
using System;
using System.Threading.Channels;

namespace HEAL.Bricks.XTests {
  public class DiscoverApplicationsRunnerUnitTests {
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public async Task GetApplicationsAsync_ReturnsApplications(Type channelType) {
      PackageLoadInfo[] packageLoadInfos = new[] {
        PackageLoadInfo.CreateForTests("a", "1.0.0", Path.Combine(GetWorkingDir(), "HEAL.Bricks.XTests.dll"))
      };
      IApplication expectedApplication = new DummyApplication();
      IChannel channel = CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestRunner");
      DiscoverApplicationsRunner discoverApplicationsRunner = new DiscoverApplicationsRunner(packageLoadInfos);

      ApplicationInfo[] result = await discoverApplicationsRunner.GetApplicationsAsync(channel);

      Assert.Collection(result,
        x => {
          Assert.Equal(expectedApplication.Name, x.Name);
          Assert.Equal(expectedApplication.Description, x.Description);
          Assert.Equal(expectedApplication.Kind, x.Kind);
          Assert.Equal(expectedApplication.GetType().FullName, x.TypeName);
        }
      );
    }

    [Fact]
    public async Task GetApplicationsAsync_WhenNoApplicationsAvailable_ReturnsEmpty() {
      IChannel channel = new AnonymousPipesProcessChannel(Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestRunner");
      DiscoverApplicationsRunner discoverApplicationsRunner = new DiscoverApplicationsRunner(Enumerable.Empty<PackageLoadInfo>());

      ApplicationInfo[] result = await discoverApplicationsRunner.GetApplicationsAsync(channel);

      Assert.Empty(result);
    }

    #region Helpers
    private static string GetWorkingDir() {
      return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }
    private static IChannel CreateChannel(Type channelType, string programPath, string arguments) {
      if (typeof(ProcessChannel).IsAssignableFrom(channelType)) {
        return Activator.CreateInstance(channelType, programPath, arguments) as IChannel;
      } else if (typeof(MemoryChannel).IsAssignableFrom(channelType)) {
        return Activator.CreateInstance(channelType, new Action<MemoryChannel>(async channel => await Runner.ReceiveAndExecuteAsync(channel))) as MemoryChannel;
      }
      else {
        return Activator.CreateInstance(channelType) as IChannel;
      }
    }
    #endregion
  }
}
