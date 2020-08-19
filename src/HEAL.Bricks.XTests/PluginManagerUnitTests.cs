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
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Xunit;
using Xunit.Abstractions;

namespace HEAL.Bricks.XTests {
  public class PluginManagerUnitTests {
    private readonly ITestOutputHelper output;
    private readonly ILogger logger;

    public PluginManagerUnitTests(ITestOutputHelper output) {
      this.output = output;
      logger = new XunitLogger(output);
    }

    #region Create
    [Fact]
    public void Create_WithSettingsWhereRepositoriesIsEmpty_ThrowsArgumentException() {
      Settings settings = new Settings();
      settings.Repositories.Clear();

      var e = Assert.Throws<ArgumentException>(() => PluginManager.Create(settings));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithSettingsWhereRepositoriesContainsNullOrEmpty_ThrowsArgumentException(string repository) {
      Settings settings = new Settings();
      settings.Repositories.Add(repository);

      var e = Assert.Throws<ArgumentException>(() => PluginManager.Create(settings));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    #endregion

    //[Fact]
    //public void Constructor_Succeeds() {
    //  INuGetConnector nuGetConnector = CreateNuGetConnectorMock();

    //  PluginManager pm = new PluginManager(Settings.Default, nuGetConnector);

    //  Assert.NotNull(pm);
    //}

    //internal static INuGetConnector CreateNuGetConnectorMock() {
    //  var nuGetConnector = new Mock<NuGetConnector>();
    //  nuGetConnector.Setup(n => n.GetInstalledPackages(It.IsAny<string>())).Returns(_ => Enumerable.Repeat(CreatePackageFolderReaderMock(), 1));


    //  return nuGetConnector.Object;
    //}

    //internal static PackageFolderReader CreatePackageFolderReaderMock() {
    //  var packageFolderReader = new Mock<PackageFolderReader>();
    //  packageFolderReader.Setup(n => n.GetInstalledPackages(It.IsAny<string>())).Returns();


    //  return packageFolderReader.Object;
    //}


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
  }
}
