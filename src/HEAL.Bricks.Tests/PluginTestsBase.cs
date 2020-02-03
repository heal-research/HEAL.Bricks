#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet.Common;
using NuGet.Packaging;
using System.IO;

namespace HEAL.Bricks.Tests {
  [TestClass]
  // HEAL.Bricks.PluginTypes package
  [DeploymentItem(Constants.pathBricksPluginTypes, Constants.localPackagesRelativePath)]
  [DeploymentItem(Constants.pathBricksPluginTypes, Constants.remoteOfficialRepositoryRelativePath)]
  [DeploymentItem(Constants.pathBricksPluginTypes, Constants.remoteDevRepositoryRelativePath)]
  // local plugins
  [DeploymentItem(Constants.pathPluginB_010, Constants.localPackagesRelativePath)]
  [DeploymentItem(Constants.pathPluginB_020_alpha1, Constants.localPackagesRelativePath)]
  // released plugins
  [DeploymentItem(Constants.pathPluginA_010_alpha1, Constants.remoteOfficialRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_010_alpha2, Constants.remoteOfficialRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_010, Constants.remoteOfficialRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_020_alpha1, Constants.remoteOfficialRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_020, Constants.remoteOfficialRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_021, Constants.remoteOfficialRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_010_alpha1, Constants.remoteOfficialRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_010_alpha2, Constants.remoteOfficialRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_010, Constants.remoteOfficialRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_020_alpha1, Constants.remoteOfficialRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_020, Constants.remoteOfficialRepositoryRelativePath)]
  // plugins in development
  [DeploymentItem(Constants.pathPluginA_020_alpha1, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_020, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_021, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_030_alpha1, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_030_beta1, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginA_030, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_020_alpha1, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_020, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_030_alpha1, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_030_alpha2, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_030, Constants.remoteDevRepositoryRelativePath)]
  [DeploymentItem(Constants.pathPluginB_031, Constants.remoteDevRepositoryRelativePath)]
  public abstract class PluginTestsBase {
    public TestContext TestContext { get; set; }
    protected string TestDeploymentPath {
      get { return TestContext.DeploymentDirectory; }
    }
    protected string UniqueTestId {
      get { return TestContext.FullyQualifiedTestClassName + "." + TestContext.TestName; }
    }
    protected string TestExecutionPath {
      get { return Path.Combine(TestContext.TestRunDirectory, UniqueTestId); }
    }

    [AssemblyInitialize]
    public static void UnpackLocalPackages(TestContext testContext) {
      string packagesPath = Path.Combine(testContext.DeploymentDirectory, Constants.localPackagesRelativePath);
      string packagesCachePath = Path.Combine(testContext.DeploymentDirectory, Constants.localPackagesCacheRelativePath);
      PackagePathResolver packagePathResolver = new PackagePathResolver(packagesPath);
      PackageExtractionContext packageExtractionContext = new PackageExtractionContext(PackageSaveMode.Defaultv3, XmlDocFileSaveMode.Skip, null, NullLogger.Instance);

      Directory.CreateDirectory(packagesPath);
      Directory.CreateDirectory(packagesCachePath);

      foreach (string package in Directory.GetFiles(packagesPath, "*.nupkg")) {
        PackageArchiveReader packageReader = new PackageArchiveReader(package);
        PackageExtractor.ExtractPackageAsync(package, packageReader, packagePathResolver, packageExtractionContext, default);
      }
    }

    [TestInitialize]
    public void DeployTestPackageSources() {
      string sourcePath = Path.Combine(TestDeploymentPath, Constants.testPackageSourcesRelativePath);
      string targetPath = Path.Combine(TestExecutionPath, Constants.testPackageSourcesRelativePath);
      CopyDirectory(sourcePath, targetPath);
      TestContext.WriteLine($"Deployed TestPackageSources to {targetPath}.");
    }

    private protected NuGetConnector CreateNuGetConnector(bool includePublicNuGetRepository = false) {
      Settings settings = new Settings();
      settings.SetAppPath(TestExecutionPath);
      settings.PluginTag = "HEALBricksPlugin";
      settings.PackagesPath = Constants.localPackagesRelativePath;
      settings.PackagesCachePath = Constants.localPackagesCacheRelativePath;
      settings.Repositories.Clear();
      settings.Repositories.Add(Path.Combine(settings.AppPath, Constants.remoteOfficialRepositoryRelativePath));
      settings.Repositories.Add(Path.Combine(settings.AppPath, Constants.remoteDevRepositoryRelativePath));
      if (includePublicNuGetRepository)
        settings.Repositories.Add(Constants.publicNuGetRepository);

      NuGetConnector nuGetConnector = new NuGetConnector(settings);
      nuGetConnector.EnableLogging(LogLevel.Debug);
      nuGetConnector.SetFrameworkForUnitTests(".NETCoreApp,Version=v3.1");
      return nuGetConnector;
    }

    private protected void WriteLogToTestContextAndClear(NuGetConnector nuGetConnector, string header = null) {
      string[] log = nuGetConnector.GetLog();
      if (log.Length > 0) {
        if (header != null) TestContext.WriteLine(header);
        TestContext.WriteLine("NuGetConnector Log:");
        foreach (string line in log)
          TestContext.WriteLine(line);
        TestContext.WriteLine("");
        nuGetConnector.ClearLog();
      }
    }

    protected void CopyDirectory(string sourcePath, string targetPath) {
      DirectoryInfo source = new DirectoryInfo(sourcePath);
      DirectoryInfo target = new DirectoryInfo(targetPath);

      Directory.CreateDirectory(target.FullName);
      foreach (FileInfo file in source.GetFiles())
        file.CopyTo(Path.Combine(target.FullName, file.Name), true);

      foreach (DirectoryInfo subdir in source.GetDirectories())
        CopyDirectory(subdir.FullName, Path.Combine(target.FullName, subdir.Name));
    }
  }
}
