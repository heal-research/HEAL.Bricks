#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks.Tests {
  public class Constants {
    // deployment path
    public const string testPackageSourcesRelativePath = @"TestPackageSources";

    // package repositories
    public const string publicNuGetRepository = "https://api.nuget.org/v3/index.json";
    public const string localPackagesRelativePath = @"TestPackageSources\local";
    public const string localPackagesCacheRelativePath = @"TestPackageSources\cache";
    public const string remoteOfficialRepositoryRelativePath = @"TestPackageSources\remote";
    public const string remoteDevRepositoryRelativePath = @"TestPackageSources\remote_dev";

    // HEAL.Bricks package
    public const string pathBricksPluginTypes = @"TestPlugins\HEAL.Bricks.PluginTypes.0.1.0-alpha.9.nupkg";
    public const string nameBricksPluginTypes = "HEAL.Bricks.PluginTypes";
    public const string versionBricksPluginTypes = "0.1.0-alpha.9";
    public const string nameVersionBricksPluginTypes = nameBricksPluginTypes + "." + versionBricksPluginTypes;

    // System.Collections package
    public const string nameCollections = "System.Collections";
    public const string versionCollections = "4.3.0";
    public const string nameVersionCollections = nameCollections + "." + versionCollections;

    // nupkg-files of test plugins
    public const string pathPluginA_010_alpha1 = @"TestPlugins\HEAL.Bricks.Tests.PluginA.0.1.0-alpha.1.nupkg";
    public const string pathPluginA_010_alpha2 = @"TestPlugins\HEAL.Bricks.Tests.PluginA.0.1.0-alpha.2.nupkg";
    public const string pathPluginA_010        = @"TestPlugins\HEAL.Bricks.Tests.PluginA.0.1.0.nupkg";
    public const string pathPluginA_020_alpha1 = @"TestPlugins\HEAL.Bricks.Tests.PluginA.0.2.0-alpha.1.nupkg";
    public const string pathPluginA_020        = @"TestPlugins\HEAL.Bricks.Tests.PluginA.0.2.0.nupkg";
    public const string pathPluginA_021        = @"TestPlugins\HEAL.Bricks.Tests.PluginA.0.2.1.nupkg";
    public const string pathPluginA_030_alpha1 = @"TestPlugins\HEAL.Bricks.Tests.PluginA.0.3.0-alpha.1.nupkg";
    public const string pathPluginA_030_beta1  = @"TestPlugins\HEAL.Bricks.Tests.PluginA.0.3.0-beta.1.nupkg";
    public const string pathPluginA_030        = @"TestPlugins\HEAL.Bricks.Tests.PluginA.0.3.0.nupkg";
    public const string pathPluginB_010_alpha1 = @"TestPlugins\HEAL.Bricks.Tests.PluginB.0.1.0-alpha.1.nupkg";
    public const string pathPluginB_010_alpha2 = @"TestPlugins\HEAL.Bricks.Tests.PluginB.0.1.0-alpha.2.nupkg";
    public const string pathPluginB_010        = @"TestPlugins\HEAL.Bricks.Tests.PluginB.0.1.0.nupkg";
    public const string pathPluginB_020_alpha1 = @"TestPlugins\HEAL.Bricks.Tests.PluginB.0.2.0-alpha.1.nupkg";
    public const string pathPluginB_020        = @"TestPlugins\HEAL.Bricks.Tests.PluginB.0.2.0.nupkg";
    public const string pathPluginB_030_alpha1 = @"TestPlugins\HEAL.Bricks.Tests.PluginB.0.3.0-alpha.1.nupkg";
    public const string pathPluginB_030_alpha2 = @"TestPlugins\HEAL.Bricks.Tests.PluginB.0.3.0-alpha.2.nupkg";
    public const string pathPluginB_030        = @"TestPlugins\HEAL.Bricks.Tests.PluginB.0.3.0.nupkg";
    public const string pathPluginB_031        = @"TestPlugins\HEAL.Bricks.Tests.PluginB.0.3.1.nupkg";

    // names of test plugins
    public const string nameInvalid = "HEAL.Bricks.Tests.InvalidPluginName";
    public const string namePluginA = "HEAL.Bricks.Tests.PluginA";
    public const string namePluginB = "HEAL.Bricks.Tests.PluginB";

    // versions of test plugins
    public const string versionInvalid    = "abcdefg";
    public const string version000        = "0.0.0";
    public const string version010_alpha1 = "0.1.0-alpha.1";
    public const string version010_alpha2 = "0.1.0-alpha.2";
    public const string version010        = "0.1.0";
    public const string version020_alpha1 = "0.2.0-alpha.1";
    public const string version020        = "0.2.0";
    public const string version021        = "0.2.1";
    public const string version030_alpha1 = "0.3.0-alpha.1";
    public const string version030_alpha2 = "0.3.0-alpha.2";
    public const string version030_beta1  = "0.3.0-beta.1";
    public const string version030        = "0.3.0";
    public const string version031        = "0.3.1";
  }
}
