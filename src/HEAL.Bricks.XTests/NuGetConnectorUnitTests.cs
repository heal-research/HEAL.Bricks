#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.Linq;
using System.Threading.Tasks;
using NuGet.Common;
using Xunit;
using Xunit.Abstractions;
using NuGet.Protocol.Core.Types;

namespace HEAL.Bricks.XTests {
  public class NuGetConnectorUnitTests {
    private readonly ITestOutputHelper output;
    private readonly ILogger logger;

    public NuGetConnectorUnitTests(ITestOutputHelper output) {
      this.output = output;
      logger = new XunitLogger(output);
    }

    [Fact]
    public void Constructor_ReturnsInstanceWhereFrameworkIsCorrect() {
      string expectedCurrentFrameworkName = Constants.netCoreApp21FrameworkName;

      NuGetConnector nuGetConnector = new NuGetConnector(Enumerable.Empty<string>(), logger);

      Assert.Equal(expectedCurrentFrameworkName, nuGetConnector.CurrentFramework.DotNetFrameworkName);
    }
    [Fact]
    public void CreateForTests_WithFrameworkName_ReturnsInstanceWhereFrameworkIsCorrect() {
      string currentFrameworkName = Constants.netCoreApp31FrameworkName;

      NuGetConnector nuGetConnector = NuGetConnector.CreateForTests(currentFrameworkName, Enumerable.Empty<string>(), logger);

      Assert.Equal(currentFrameworkName, nuGetConnector.CurrentFramework.DotNetFrameworkName);
    }

    [Fact]
    public async Task GetPackageAsync_WithPackageIdentity_ReturnsPackage() {
      var a = Mock.CreatePackage("a", "1.0.0");
      var repository = Mock.CreateSourceRepositoryMock(a);
      NuGetConnector nuGetConnector = NuGetConnector.CreateForTests(Constants.netCoreApp31FrameworkName, new[] { repository }, logger);

      IPackageSearchMetadata result = await nuGetConnector.GetPackageAsync(a.Id, default);

      Assert.Equal(a.Id, result.Identity);
    }
    [Fact]
    public async Task GetPackageAsync_WithUnknownPackage_ReturnsNull() {
      var a = Mock.CreatePackage("a", "1.0.0");
      var repository = Mock.CreateSourceRepositoryMock(a);
      var missingId = Mock.CreatePackageIdentity("unknown", "1.0.0");
      NuGetConnector nuGetConnector = NuGetConnector.CreateForTests(Constants.netCoreApp31FrameworkName, new[] { repository }, logger);

      IPackageSearchMetadata result = await nuGetConnector.GetPackageAsync(missingId, default);

      Assert.Null(result);
    }
    [Fact]
    public async Task GetPackageAsync_WithDuplicatedPackage_ReturnsPackage() {
      var a = Mock.CreatePackage("a", "1.0.0");
      var duplicate = Mock.CreatePackage("duplicate", "1.0.0");
      var repository1 = Mock.CreateSourceRepositoryMock(a, duplicate);
      var repository2 = Mock.CreateSourceRepositoryMock(duplicate);
      NuGetConnector nuGetConnector = NuGetConnector.CreateForTests(Constants.netCoreApp31FrameworkName, new[] { repository1, repository2 }, logger);

      IPackageSearchMetadata result = await nuGetConnector.GetPackageAsync(duplicate.Id, default);

      Assert.Equal(duplicate.Id, result.Identity);
    }

    [Fact]
    public async Task GetRemotePackageAsync_WithNameAndVersion_ReturnsPackage() {
      var a = Mock.CreatePackage("a", "1.0.0");
      var b = Mock.CreatePackage("b", "1.0.0");
      var c = Mock.CreatePackage("c", "1.0.0", ("a", "1.0.0"), ("b", "1.0.0"));
      var repository = Mock.CreateSourceRepositoryMock(a, b, c);
      NuGetConnector nuGetConnector = NuGetConnector.CreateForTests(Constants.netCoreApp31FrameworkName, new[] { repository }, logger);

      RemotePackageInfo result = await nuGetConnector.GetRemotePackageAsync("c", "1.0.0", default);

      Assert.Equal(c.Id, result.packageIdentity);
      Assert.Equal(c.Dependencies.OrderBy(x => x.Id), result.Dependencies.OrderBy(x => x.Id).Select(x => x.nuGetPackageDependency));
    }
    [Fact]
    public async Task GetRemotePackageAsync_WithDuplicatedPackage_ReturnsPackage() {
      var a = Mock.CreatePackage("a", "1.0.0");
      var duplicate = Mock.CreatePackage("duplicate", "1.0.0");
      var repository1 = Mock.CreateSourceRepositoryMock(a, duplicate);
      var repository2 = Mock.CreateSourceRepositoryMock(duplicate);
      NuGetConnector nuGetConnector = NuGetConnector.CreateForTests(Constants.netCoreApp31FrameworkName, new[] { repository1, repository2 }, logger);

      RemotePackageInfo result = await nuGetConnector.GetRemotePackageAsync("duplicate", "1.0.0", default);

      Assert.Equal(duplicate.Id, result.packageIdentity);
    }
    [Fact]
    public async Task GetRemotePackageAsync_WithUnknownPackage_ReturnsNull() {
      var a = Mock.CreatePackage("a", "1.0.0");
      var repository = Mock.CreateSourceRepositoryMock(a);
      NuGetConnector nuGetConnector = NuGetConnector.CreateForTests(Constants.netCoreApp31FrameworkName, new[] { repository }, logger);

      RemotePackageInfo result = await nuGetConnector.GetRemotePackageAsync("unknown", "1.0.0", default);

      Assert.Null(result);
    }
  }
}
