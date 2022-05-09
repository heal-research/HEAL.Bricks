#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.IO;
using Xunit;

namespace HEAL.Bricks.Tests {
  [Trait("Category", "Unit")]
  public class BricksOptionsUnitTests {
    #region PublicNuGetRepository
    [Fact]
    public void PublicNuGetRepository_IsNotNullOrWhiteSpace() {
      Repository publicNuGetRepository = BricksOptions.PublicNuGetRepository;

      Assert.NotNull(publicNuGetRepository);
    }
    #endregion

    #region Ctor, Default
    [Fact]
    public void Default_ReturnsValidInstance() {
      BricksOptions options = BricksOptions.Default;

      Assert.NotNull(options);
      var validation = BricksOptionsValidator.Check(options);
      Assert.True(validation.IsValid, validation.ToString());
    }
    [Fact]
    public void Constructor_CreatesValidInstance() {
      BricksOptions options = new BricksOptions();

      Assert.NotNull(options);
      var validation = BricksOptionsValidator.Check(options);
      Assert.True(validation.IsValid, validation.ToString());
    }
    #endregion

    #region Repositories
    [Fact]
    public void Repositories_AddRepository_Succeeds() {
      Repository repository = new Repository("repository");
      BricksOptions options = new BricksOptions();

      options.Repositories.Add(repository);

      Assert.Contains(repository, options.Repositories);
    }
    #endregion

    #region PackagesPath
    [Theory]
    [InlineData("C:/absolute/path")]
    [InlineData("relative/path")]
    public void PackagesPath_SetAbsoluteOrRelativePath_Succeeds(string path) {
      BricksOptions options = new BricksOptions();

      options.PackagesPath = path;

      Assert.EndsWith(path, options.PackagesPath);
      Assert.True(Path.IsPathRooted(options.PackagesPath));
    }
    [Fact]
    public void PackagesPath_SetNull_ThrowsArgumentNullException() {
      BricksOptions options = new BricksOptions();

      var e = Assert.Throws<ArgumentNullException>(() => options.PackagesPath = null);
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void PackagesPath_SetEmptyString_ThrowsArgumentException(string path) {
      BricksOptions options = new BricksOptions();

      var e = Assert.Throws<ArgumentException>(() => options.PackagesPath = path);
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    #endregion

    #region PackagesCachePath
    [Theory]
    [InlineData("C:/absolute/path")]
    [InlineData("relative/path")]
    public void PackagesCachePath_SetAbsoluteOrRelativePath_Succeeds(string path) {
      BricksOptions options = new BricksOptions();

      options.PackagesCachePath = path;

      Assert.EndsWith(path, options.PackagesCachePath);
      Assert.True(Path.IsPathRooted(options.PackagesCachePath));
    }
    [Fact]
    public void PackagesCachePath_SetNull_ThrowsArgumentNullException() {
      BricksOptions options = new BricksOptions();

      var e = Assert.Throws<ArgumentNullException>(() => options.PackagesCachePath = null);
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void PackagesCachePath_SetEmptyString_ThrowsArgumentException(string path) {
      BricksOptions options = new BricksOptions();

      var e = Assert.Throws<ArgumentException>(() => options.PackagesCachePath = path);
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    #endregion

    #region Isolation
    [Fact]
    public void Isolation_SetValue_Succeeds() {
      Isolation isolation = Isolation.Docker;
      BricksOptions options = new BricksOptions();

      options.DefaultIsolation = isolation;

      Assert.Equal(isolation, options.DefaultIsolation);
    }
    #endregion

    #region DotnetCommand
    [Fact]
    public void DotnetCommand_SetString_Succeeds() {
      string path = "path/to/dotnet.exe";
      BricksOptions options = new BricksOptions();

      options.DotnetCommand = path;

      Assert.Equal(path, options.DotnetCommand);
    }
    [Fact]
    public void DotnetCommand_SetNull_ThrowsArgumentNullException() {
      BricksOptions options = new BricksOptions();

      var e = Assert.Throws<ArgumentNullException>(() => options.DotnetCommand = null);
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void DotnetCommand_SetEmptyString_ThrowsArgumentException(string command) {
      BricksOptions options = new BricksOptions();

      var e = Assert.Throws<ArgumentException>(() => options.DotnetCommand = command);
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    #endregion

    #region DockerCommand
    [Fact]
    public void DockerCommand_SetString_Succeeds() {
      string path = "path/to/docker.exe";
      BricksOptions options = new BricksOptions();

      options.DockerCommand = path;

      Assert.Equal(path, options.DockerCommand);
    }
    [Fact]
    public void DockerCommand_SetNull_ThrowsArgumentNullException() {
      BricksOptions options = new BricksOptions();

      var e = Assert.Throws<ArgumentNullException>(() => options.DockerCommand = null);
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void DockerCommand_SetEmptyString_ThrowsArgumentException(string command) {
      BricksOptions options = new BricksOptions();

      var e = Assert.Throws<ArgumentException>(() => options.DockerCommand = command);
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    #endregion

    #region DockerImage
    [Fact]
    public void DockerImage_SetString_Succeeds() {
      string image = "my.dockerimage.com/url/image:1.0";
      BricksOptions options = new BricksOptions();

      options.DefaultDockerImage = image;

      Assert.Equal(image, options.DefaultDockerImage);
    }
    [Fact]
    public void DockerImage_SetNull_ThrowsArgumentNullException() {
      BricksOptions options = new BricksOptions();

      var e = Assert.Throws<ArgumentNullException>(() => options.DefaultDockerImage = null);
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void DockerImage_SetEmptyString_ThrowsArgumentException(string command) {
      BricksOptions options = new BricksOptions();

      var e = Assert.Throws<ArgumentException>(() => options.DefaultDockerImage = command);
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    #endregion

    #region StarterAssembly
    [Fact]
    public void StarterAssembly_SetRelativePath_Succeeds() {
      string path = "relative/path";
      BricksOptions options = new BricksOptions();

      options.StarterAssembly = path;

      Assert.Equal(path, options.StarterAssembly);
    }
    [Fact]
    public void StarterAssembly_SetNull_ThrowsArgumentNullException() {
      BricksOptions options = new BricksOptions();

      var e = Assert.Throws<ArgumentNullException>(() => options.StarterAssembly = null);
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void StarterAssembly_SetEmptyString_ThrowsArgumentException(string path) {
      BricksOptions options = new BricksOptions();

      var e = Assert.Throws<ArgumentException>(() => options.StarterAssembly = path);
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Fact]
    public void StarterAssembly_SetAbsolutePath_ThrowsArgumentException() {
      string path = Constants.Platform switch {
        Platform.Windows => "C:/absolute/path",
        Platform.Linux =>   "/absolute/path",
        Platform.OSX =>     "/absolute/path",
        Platform.FreeBSD => "/absolute/path",
        _ => throw new NotSupportedException($"Platform {Constants.Platform} is not supported.")
      };
      BricksOptions options = new BricksOptions();

      var e = Assert.Throws<ArgumentException>(() => options.StarterAssembly = path);
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    #endregion

    #region SetAppPath
    [Fact]
    public void SetAppPath_WithAbsolutePath_Succeeds() {
      string path = Path.GetTempPath();
      BricksOptions options = new BricksOptions();

      options.SetAppPath(path);

      Assert.Equal(path, options.AppPath);
    }
    [Fact]
    public void SetAppPath_WithNull_ThrowsArgumentNullException() {
      BricksOptions options = new BricksOptions();

      var e = Assert.Throws<ArgumentNullException>(() => options.SetAppPath(null));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("relative/path")]
    public void SetAppPath_WithEmptyOrRelativePath_ThrowsArgumentException(string path) {
      BricksOptions options = new BricksOptions();

      var e = Assert.Throws<ArgumentException>(() => options.SetAppPath(path));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    #endregion
  }
}
