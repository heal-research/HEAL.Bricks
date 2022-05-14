#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;

namespace HEAL.Bricks {
  [Serializable]
  public class ApplicationInfo : RunnableInfo {
    public ApplicationKind Kind { get; }

    public ApplicationInfo(IApplication application) : base(application) {
      Kind = application.Kind;
    }
  }
}
