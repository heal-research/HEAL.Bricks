#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System;

namespace HEAL.Bricks.Tests {
  [Trait("Category", "Unit")]
  public class DiscoverApplicationsRunnerUnitTests {
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel), false)]
    [InlineData(typeof(StdInOutProcessChannel),       false)]
    public async Task GetApplicationsAsync_ReturnsApplications(Type channelType, bool startDebugger) {
      PackageLoadInfo[] packageLoadInfos = new[] {
        PackageLoadInfo.CreateForTests("a", "1.0.0", TestHelpers.GetWorkingDir(), "HEAL.Bricks.Tests.dll")
      };
      IApplication expectedApplication = new DummyApplication();
      IChannel channel = TestHelpers.CreateChannel(channelType, Constants.DotnetExePath, $"HEAL.Bricks.Tests.BricksRunner.dll --TestRunner {(startDebugger ? "--Debug" : "")}", TestHelpers.TestRunnerAsync);
      DiscoverApplicationsRunner discoverApplicationsRunner = new(packageLoadInfos);

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

    [Theory]
    [InlineData(typeof(MemoryChannel))]
    public async Task GetApplicationsAsync_WithMemoryChannel_ReturnsApplications(Type channelType) {
      IApplication expectedApplication = new DummyApplication();
      IChannel channel = TestHelpers.CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestRunner", TestHelpers.TestRunnerAsync);
      DiscoverApplicationsRunner discoverApplicationsRunner = new(Enumerable.Empty<PackageLoadInfo>());

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

    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    public async Task GetApplicationsAsync_WhenNoApplicationsAvailable_ReturnsEmpty(Type channelType) {
      IChannel channel = TestHelpers.CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestRunner", TestHelpers.TestRunnerAsync);
      DiscoverApplicationsRunner discoverApplicationsRunner = new(Enumerable.Empty<PackageLoadInfo>());

      ApplicationInfo[] result = await discoverApplicationsRunner.GetApplicationsAsync(channel);

      Assert.Empty(result);
    }
  }
}
