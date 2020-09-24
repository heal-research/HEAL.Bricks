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

namespace HEAL.Bricks.XTests {
  [Trait("Category", "Unit")]
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
    [Theory]
    [InlineData(Constants.netCoreApp31FrameworkName)]
    [InlineData(Constants.netFramework472FrameworkName)]
    [InlineData(Constants.netStandard20FrameworkName)]
    public void CreateForTests_WithFrameworkName_ReturnsInstanceWhereFrameworkIsCorrect(string frameworkName) {
      NuGetConnector nuGetConnector = NuGetConnector.CreateForTests(frameworkName, Enumerable.Empty<string>(), logger);

      Assert.Equal(frameworkName, nuGetConnector.CurrentFramework.DotNetFrameworkName);
    }

    [Theory]
    [InlineData("a", "1.0.0")]  // package without dependencies
    [InlineData("b", "1.0.0")]  // duplicated package
    [InlineData("c", "1.0.0")]  // package with one dependency
    [InlineData("d", "1.0.0")]  // package with multiple dependencies
    [InlineData("x", "1.0.0")]  // unknown package
    [InlineData("a", "2.0.0")]  // unknown version
    public async Task GetRemotePackageAsync_WithPackageIdAndVersion_ReturnsRemotePackageInfoOrNull(string packageId, string version) {
      var packages = new[] {
        new[] {
          Mock.CreatePackage("b", "1.0.0", ("a", "1.0.0"))
        },
        new[] {
          Mock.CreatePackage("a", "1.0.0"),
          Mock.CreatePackage("b", "1.0.0", ("a", "1.0.0")),
          Mock.CreatePackage("c", "1.0.0", ("a", "1.0.0")),
          Mock.CreatePackage("d", "1.0.0", ("b", "1.0.0"), ("c", "1.0.0"))
        }
      };
      var repositories = packages.Select((x, i) => Mock.CreateSourceRepositoryMock($"PS{i}", x));
      var expectedPackage = packages.SelectMany(x => x).Where(x => (x.Id.Id == packageId) && (x.Id.Version.ToString() == version)).FirstOrDefault();
      NuGetConnector nuGetConnector = NuGetConnector.CreateForTests(Constants.netCoreApp31FrameworkName, repositories, logger);

      RemotePackageInfo result = await nuGetConnector.GetRemotePackageAsync(packageId, version, default);

      if (expectedPackage.Id == null) {
        Assert.Null(result);
      } else {
        Assert.Equal(expectedPackage.Id.Id, result.Id);
        Assert.Equal(expectedPackage.Id.Version.ToString(), result.Version.ToString());

        Assert.Equal(expectedPackage.Dependencies.Length, result.Dependencies.Count());
        Assert.All(expectedPackage.Dependencies.Zip(result.Dependencies), x => {
          Assert.Equal(x.First.Id, x.Second.Id);
          Assert.Equal(x.First.VersionRange.MinVersion?.ToString(), x.Second.VersionRange.MinVersion?.ToString());
          Assert.Equal(x.First.VersionRange.MaxVersion?.ToString(), x.Second.VersionRange.MaxVersion?.ToString());
        });
      }
    }

    [Theory]
    [InlineData("a", false, new[] { "1.0.0" } )]
    [InlineData("a", true,  new[] { "1.0.0" } )]
    [InlineData("b", false, new[] { "1.0.0", "2.0.0" } )]
    [InlineData("b", true,  new[] { "1.0.0-alpha", "1.0.0", "2.0.0", "3.0.0-alpha" } )]
    [InlineData("x", false, new string[0] )]
    [InlineData("x", true,  new string[0] )]
    public async Task GetRemotePackagesAsync_WithPackageIdAndIncludePreReleases_ReturnsRemotePackageInfos(string packageId, bool includePreReleases, string[] expectedVersions) {
      var packages = new[] {
        Mock.CreatePackage("a", "1.0.0"),
        Mock.CreatePackage("b", "1.0.0-alpha", ("a", "1.0.0")),
        Mock.CreatePackage("b", "1.0.0",       ("a", "1.0.0")),
        Mock.CreatePackage("b", "2.0.0",       ("a", "1.0.0")),
        Mock.CreatePackage("b", "3.0.0-alpha", ("a", "1.0.0")),
      };
      var repository = Mock.CreateSourceRepositoryMock("PS", packages);
      NuGetConnector nuGetConnector = NuGetConnector.CreateForTests(Constants.netCoreApp31FrameworkName, new[] { repository }, logger);

      var result = await nuGetConnector.GetRemotePackagesAsync(packageId, includePreReleases, default);

      if (!packages.Any(x => x.Id.Id == packageId)) {
        Assert.Empty(result);
      }
      else {
        Assert.All(result, x => Assert.Equal(packageId, x.Id));
        Assert.Equal(expectedVersions, result.Select(x => x.Version.ToString()));
      }
    }

    [Theory]
    [InlineData("a", 0, 6, true,  new[] { "a", "ab", "abc", "abcd", "abcde", "abcdef" }, new[] { "PS0", "PS0", "PS0", "PS1", "PS1", "PS1" } )]
    [InlineData("a", 0, 6, false, new[] { "a", "ab", "abcd", "abcde" }, new[] { "PS0", "PS0", "PS1", "PS1" })]
    [InlineData("a", 1, 2, true,  new[] { "ab", "abc", "abcde", "abcdef" }, new[] { "PS0", "PS0", "PS1", "PS1" })]
    [InlineData("a", 1, 2, false, new[] { "ab", "abcde" }, new[] { "PS0", "PS1" })]
    [InlineData("f", 0, 4, true,  new[] { "abcdef" }, new[] { "PS1" } )]
    [InlineData("x", 0, 4, true,  new string[0], new string[0] )]
    public async Task SearchRemotePackagesAsync_WithPackageIdAndIncludePreReleases_ReturnsRemotePackageInfos(string searchString, int skip, int take, bool includePreReleases, string[] expectedPackages, string[] expectedSources) {
      var packages = new[] {
        new[] {
          Mock.CreatePackage("a",      "1.0.0"),
          Mock.CreatePackage("ab",     "1.0.0"),
          Mock.CreatePackage("abc",    "1.0.0-alpha")
        },
        new[] {
          Mock.CreatePackage("abcd",   "1.0.0"),
          Mock.CreatePackage("abcde",  "1.0.0"),
          Mock.CreatePackage("abcdef", "1.0.0-alpha")
        }
      };
      var repositories = packages.Select((x, i) => Mock.CreateSourceRepositoryMock($"PS{i}", x));
      NuGetConnector nuGetConnector = NuGetConnector.CreateForTests(Constants.netCoreApp31FrameworkName, repositories, logger);

      var result = await nuGetConnector.SearchRemotePackagesAsync(searchString, skip, take, includePreReleases, default);

      if (!packages.SelectMany(x => x).Any(x => x.Id.Id.Contains(searchString))) {
        Assert.Empty(result);
      }
      else {
        Assert.Equal(expectedPackages.Zip(expectedSources).Select(x => (x.Second, x.First)), result.Select(x => (x.Repository, x.Package.Id) ));
      }
    }

    [Theory]
    [InlineData("b", "1.0.0-alpha", new[] { "a" }, new[] { "1.0.0-alpha" } )]
    [InlineData("b", "1.0.0",       new[] { "a" }, new[] { "1.0.0" })]
    [InlineData("b", "2.0.0",       new[] { "a" }, new[] { "2.0.0-alpha" })]
    [InlineData("b", "3.0.0-alpha", new[] { "a" }, new[] { "2.0.0" })]
    [InlineData("a", "1.0.0",       new string[0], new string[0])]
    public async Task GetMissingDependenciesAsync_WithLocalPackages_ReturnsRemotePackageInfos(string localPackageId, string localVersion, string[] expectedDependencies, string[] expectedVersions) {
      var packages = new[] {
        Mock.CreatePackage("a", "1.0.0-alpha"),
        Mock.CreatePackage("a", "1.0.0"),
        Mock.CreatePackage("a", "2.0.0-alpha"),
        Mock.CreatePackage("a", "2.0.0"),
        Mock.CreatePackage("b", "1.0.0-alpha", ("a", "1.0.0-alpha")),
        Mock.CreatePackage("b", "1.0.0",       ("a", "1.0.0")),
        Mock.CreatePackage("b", "2.0.0",       ("a", "2.0.0-alpha")),
        Mock.CreatePackage("b", "3.0.0-alpha", ("a", "2.0.0")),
      };
      var localPackage = LocalPackageInfo.CreateForTests(localPackageId, localVersion);
      var repository = Mock.CreateSourceRepositoryMock("PS", packages);
      NuGetConnector nuGetConnector = NuGetConnector.CreateForTests(Constants.netCoreApp31FrameworkName, new[] { repository }, logger);

      var result = await nuGetConnector.GetMissingDependenciesAsync(new[] { localPackage }, default);

      Assert.Equal(expectedDependencies.Length, result.Count());
      Assert.All(result.Zip(expectedDependencies.Zip(expectedVersions)), x => {
        Assert.Equal(x.Second.First, x.First.Id);
        Assert.Equal(x.Second.Second, x.First.Version.ToString());
      });
    }

    [Theory]
    [InlineData("a", "1.0.0-alpha", false, "2.0.0")]
    [InlineData("a", "1.0.0-alpha", true,  "3.0.0-alpha")]
    [InlineData("a", "2.0.0",       false, null)]
    [InlineData("a", "2.0.0",       true,  "3.0.0-alpha")]
    [InlineData("a", "3.0.0-alpha", false, null)]
    [InlineData("a", "3.0.0-alpha", true,  null)]
    public async Task GetPackageUpdatesAsync_WithLocalPackages_ReturnsRemotePackageInfos(string localPackageId, string localVersion, bool includePreReleases, string expectedVersion) {
      var packages = new[] {
        Mock.CreatePackage("a", "1.0.0-alpha"),
        Mock.CreatePackage("a", "1.0.0"),
        Mock.CreatePackage("a", "2.0.0-alpha"),
        Mock.CreatePackage("a", "2.0.0"),
        Mock.CreatePackage("a", "3.0.0-alpha"),
      };
      var localPackage = LocalPackageInfo.CreateForTests(localPackageId, localVersion);
      var repository = Mock.CreateSourceRepositoryMock("PS", packages);
      NuGetConnector nuGetConnector = NuGetConnector.CreateForTests(Constants.netCoreApp31FrameworkName, new[] { repository }, logger);

      var result = await nuGetConnector.GetPackageUpdatesAsync(new[] { localPackage }, includePreReleases, default);

      Assert.Equal(expectedVersion, result.SingleOrDefault()?.Version.ToString());
    }
  }
}
