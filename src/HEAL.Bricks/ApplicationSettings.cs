#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks {
  public class ApplicationSettings {
    public ApplicationInfo ApplicationInfo { get; }
    public bool AutoStart { get; set; }
    public Isolation Isolation { get; set; }

    public ApplicationSettings(ApplicationSettings settings) {
      ApplicationInfo = settings.ApplicationInfo;
      AutoStart = settings.AutoStart;
      Isolation = settings.Isolation;
    }
    internal ApplicationSettings(ApplicationInfo applicationInfo) {
      ApplicationInfo = applicationInfo;
      AutoStart = false;
      Isolation = applicationInfo.RecommendedIsolation;
    }
    internal ApplicationSettings(ApplicationInfo applicationInfo, ApplicationSettings oldSettings) {
      ApplicationInfo = applicationInfo;
      AutoStart = oldSettings.AutoStart;
      Isolation = oldSettings.Isolation;
    }

    public override bool Equals(object? obj) {
      if (obj is ApplicationSettings appSettings) {
        return appSettings.ApplicationInfo.Equals(ApplicationInfo) &&
               appSettings.AutoStart == AutoStart &&
               appSettings.Isolation == Isolation;
      } else {
        return base.Equals(obj);
      }
    }
    public override int GetHashCode() {
      return ApplicationInfo.GetHashCode();
    }
  }
}
