#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.IO;
using Xunit;

namespace HEAL.Bricks.XTests {
  public class SettingsUnitTests {
    #region PublicNuGetRepository
    [Fact]
    public void PublicNuGetRepository_IsNotNullOrWhiteSpace() {
      string publicNuGetRepository = Settings.PublicNuGetRepository;

      Assert.False(string.IsNullOrWhiteSpace(publicNuGetRepository));
    }
    #endregion

    #region Ctor, Default
    [Fact]
    public void Default_ReturnsValidInstance() {
      ISettings settings = Settings.Default;

      Assert.NotNull(settings);
      var validation = SettingsValidator.Check(settings);
      Assert.True(validation.IsValid, validation.ToString());
    }
    [Fact]
    public void Default_ModifyDefaultInstance_Succeeds() {
      string path = Path.GetTempPath();

      Settings.Default.PackagesPath = path;

      Assert.Equal(Settings.Default.PackagesPath, path);
    }
    [Fact]
    public void Constructor_CreatesValidInstance() {
      ISettings settings = new Settings();

      Assert.NotNull(settings);
      var validation = SettingsValidator.Check(settings);
      Assert.True(validation.IsValid, validation.ToString());
    }
    #endregion

    #region PackageTag
    [Fact]
    public void PackageTag_SetString_Succeeds() {
      string tag = "MyTestPackageTag";
      Settings settings = new Settings();

      settings.PackageTag = tag;

      Assert.Equal(tag, settings.PackageTag);
    }
    [Fact]
    public void PackageTag_SetNull_ThrowsArgumentNullException() {
      Settings settings = new Settings();

      var e = Assert.Throws<ArgumentNullException>(() => settings.PackageTag = null);
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void PackageTag_SetEmptyString_ThrowsArgumentException(string tag) {
      Settings settings = new Settings();

      var e = Assert.Throws<ArgumentException>(() => settings.PackageTag = tag);
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    #endregion

    #region Repositories
    [Fact]
    public void Repositories_AddRepository_Succeeds() {
      string repository = "repository";
      Settings settings = new Settings();

      settings.Repositories.Add(repository);

      Assert.Contains(repository, settings.Repositories);
    }
    #endregion

    #region PackagesPath
    [Theory]
    [InlineData("C:/absolute/path")]
    [InlineData("relative/path")]
    public void PackagesPath_SetAbsoluteOrRelativePath_Succeeds(string path) {
      Settings settings = new Settings();

      settings.PackagesPath = path;

      Assert.EndsWith(path, settings.PackagesPath);
      Assert.True(Path.IsPathRooted(settings.PackagesPath));
    }
    [Fact]
    public void PackagesPath_SetNull_ThrowsArgumentNullException() {
      Settings settings = new Settings();

      var e = Assert.Throws<ArgumentNullException>(() => settings.PackagesPath = null);
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void PackagesPath_SetEmptyString_ThrowsArgumentException(string path) {
      Settings settings = new Settings();

      var e = Assert.Throws<ArgumentException>(() => settings.PackagesPath = path);
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    #endregion

    #region PackagesCachePath
    [Theory]
    [InlineData("C:/absolute/path")]
    [InlineData("relative/path")]
    public void PackagesCachePath_SetAbsoluteOrRelativePath_Succeeds(string path) {
      Settings settings = new Settings();

      settings.PackagesCachePath = path;

      Assert.EndsWith(path, settings.PackagesCachePath);
      Assert.True(Path.IsPathRooted(settings.PackagesCachePath));
    }
    [Fact]
    public void PackagesCachePath_SetNull_ThrowsArgumentNullException() {
      Settings settings = new Settings();

      var e = Assert.Throws<ArgumentNullException>(() => settings.PackagesCachePath = null);
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void PackagesCachePath_SetEmptyString_ThrowsArgumentException(string path) {
      Settings settings = new Settings();

      var e = Assert.Throws<ArgumentException>(() => settings.PackagesCachePath = path);
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    #endregion

    #region SetAppPath
    [Fact]
    public void SetAppPath_WithAbsolutePath_Succeeds() {
      string path = Path.GetTempPath();
      Settings settings = new Settings();

      settings.SetAppPath(path);

      Assert.Equal(path, settings.AppPath);
    }
    [Fact]
    public void SetAppPath_WithNull_ThrowsArgumentNullException() {
      Settings settings = new Settings();

      var e = Assert.Throws<ArgumentNullException>(() => settings.SetAppPath(null));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("relative/path")]
    public void SetAppPath_WithEmptyOrRelativePath_ThrowsArgumentException(string path) {
      Settings settings = new Settings();

      var e = Assert.Throws<ArgumentException>(() => settings.SetAppPath(path));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    #endregion
  }
}
