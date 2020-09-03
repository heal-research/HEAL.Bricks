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
  public class DiscoverApplicationsRunnerUnitTests {
    [Fact]
    public async Task GetApplicationsAsync_ReturnsApplications() {
      PackageLoadInfo[] packageLoadInfos = new[] {
        PackageLoadInfo.CreateForTests("a", "1.0.0", Path.Combine(GetWorkingDir(), "HEAL.Bricks.XTests.dll"))
      };
      IApplication expectedApplication = new DummyApplication();
      IChannel channel = new AnonymousPipesProcessChannel(Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestRunner");
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
    #endregion
  }
}
