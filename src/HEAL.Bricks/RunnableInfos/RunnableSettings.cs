#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks {
  public class RunnableSettings {
    public Isolation Isolation { get; set; }
    public bool AutoStart { get; set; }

    public RunnableSettings() {
      Isolation = Isolation.None;
      AutoStart = false;
    }
    public RunnableSettings(RunnableSettings settings) {
      Isolation = settings.Isolation;
      AutoStart = settings.AutoStart;
    }
  }
}
