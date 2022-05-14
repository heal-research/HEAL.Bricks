#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;

namespace HEAL.Bricks {
  [Serializable]
  public abstract class RunnableInfo {
    public string Name { get; }
    public string Description { get; }
    public string Version { get; }
    public string DockerImage { get; }
    public string TypeName { get; }
    public bool AutoStart { get; }

    protected RunnableInfo(IRunnable runnable) {
      Guard.Argument(runnable, nameof(runnable)).NotNull()
                                                .Member(r => r.Name, n => n.NotNull().NotEmpty().NotWhiteSpace())
                                                .Member(r => r.Description, d => d.NotNull())
                                                .Member(r => r.Version, v => v.NotNull().NotEmpty().NotWhiteSpace())
                                                .Member(r => r.DockerImage, d => d.NotNull());
      Name = runnable.Name;
      Description = runnable.Description;
      Version = runnable.Version;
      DockerImage = runnable.DockerImage;
      TypeName = runnable.GetType().FullName ?? throw new InvalidOperationException("Cannot get type name of runnable.");
      AutoStart = runnable.AutoStart;
    }

    public override bool Equals(object? obj) {
      if (obj is RunnableInfo info) {
        return info.TypeName == TypeName &&
               info.Version == Version;
      } else {
        return base.Equals(obj);
      }
    }
    public override int GetHashCode() {
      return string.Concat(TypeName, Version).GetHashCode();
    }

    public override string ToString() {
      return Name + " " + Version;
    }
  }
}
