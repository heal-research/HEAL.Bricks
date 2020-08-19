#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Xunit;
using Xunit.Abstractions;

namespace HEAL.Bricks.XTests {
  public class NuGetConnectorUnitTests {
    private readonly ITestOutputHelper output;
    private readonly ILogger logger;

    public NuGetConnectorUnitTests(ITestOutputHelper output) {
      this.output = output;
      logger = new XunitLogger(output);
    }

    [Fact]
    public void Constructor_CurrentFrameworkCorrect() {
      string expectedCurrentFrameworkName = Constants.netCoreApp21FrameworkName;

      NuGetConnector nuGetConnector = new NuGetConnector(Enumerable.Empty<string>(), logger);

      Assert.Equal(expectedCurrentFrameworkName, nuGetConnector.CurrentFramework.DotNetFrameworkName);
    }
    [Fact]
    public void CreateForTests_CurrentFrameworkCorrect() {
      string currentFrameworkName = Constants.netCoreApp31FrameworkName;

      NuGetConnector nuGetConnector = NuGetConnector.CreateForTests(currentFrameworkName, Enumerable.Empty<string>(), logger);

      Assert.Equal(currentFrameworkName, nuGetConnector.CurrentFramework.DotNetFrameworkName);
    }

    public static IEnumerable<PackageIdentity> CreatePackageIdentities(params (string Package, string Version)[] packages) {
      return packages.Select(p => new PackageIdentity(p.Package, new NuGetVersion(p.Version)));
    }
    public static IEnumerable<IPackageSearchMetadata> CreatePackageSearchMetadata(params (string Package, string Version)[] packages) {
      return CreatePackageIdentities(packages).Select(id => PackageSearchMetadataBuilder.FromIdentity(id).Build());
    }
    public static SourceRepository CreatePackageSearchMetadataSourceRepositoryMock(IEnumerable<IPackageSearchMetadata> packages) {
      var resource = new Mock<PackageMetadataResource>();
      resource.Setup(r => r.GetMetadataAsync(It.IsAny<PackageIdentity>(), It.IsAny<SourceCacheContext>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()))
        .Returns<PackageIdentity, SourceCacheContext, ILogger, CancellationToken>((id, cache, logger, token) => Task.FromResult(packages.Where(p => p.Identity.Equals(id)).FirstOrDefault()));

      var repository = new Mock<SourceRepository>();
      repository.Setup(r => r.GetResourceAsync<PackageMetadataResource>(It.IsAny<CancellationToken>())).Returns(Task.FromResult(resource.Object));

      return repository.Object;
    }

    [Fact]
    public void GetPackageAsync_PackageFound() {
      var packages = CreatePackageSearchMetadata(("package", "1.0.0"));
      var repository = CreatePackageSearchMetadataSourceRepositoryMock(packages);
      NuGetConnector nuGetConnector = NuGetConnector.CreateForTests(Constants.netCoreApp31FrameworkName, new[] { repository }, logger);

      var result = nuGetConnector.GetPackageAsync(packages.First().Identity, default).Result;

      Assert.Equal(packages.First().Identity, result.Identity);
    }
    [Fact]
    public void GetPackageAsync_PackageNotFound() {
      var packages = CreatePackageSearchMetadata(("package", "1.0.0"));
      var repository = CreatePackageSearchMetadataSourceRepositoryMock(packages);
      var missingId = CreatePackageIdentities(("unknown", "1.0.0"));
      NuGetConnector nuGetConnector = NuGetConnector.CreateForTests(Constants.netCoreApp31FrameworkName, new[] { repository }, logger);

      var result = nuGetConnector.GetPackageAsync(missingId.First(), default).Result;

      Assert.Null(result);
    }
    [Fact]
    public void GetPackageAsync_DublicatesRemoved() {
      var packages1 = CreatePackageSearchMetadata(("package", "1.0.0"), ("duplicate", "1.0.0"));
      var repository1 = CreatePackageSearchMetadataSourceRepositoryMock(packages1);
      var packages2 = CreatePackageSearchMetadata(("duplicate", "1.0.0"));
      var repository2 = CreatePackageSearchMetadataSourceRepositoryMock(packages2);
      NuGetConnector nuGetConnector = NuGetConnector.CreateForTests(Constants.netCoreApp31FrameworkName, new[] { repository1, repository2 }, logger);

      var result = nuGetConnector.GetPackageAsync(packages2.First().Identity, default).Result;

      Assert.Equal(packages2.First().Identity, result.Identity);
    }



  }
}
