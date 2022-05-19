#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using System.Text.Json.Serialization;

namespace HEAL.Bricks {
  [Serializable]
  public class RunnableInfo {
    public string Name { get; }
    public string Description { get; }
    public string Version { get; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RunnableKind Kind { get; }
    public string DockerImage { get; }
    public string TypeName { get; }
    public bool AutoStart { get; }

    [JsonConstructor]
    public RunnableInfo(string name, string description, string version, RunnableKind kind, string dockerImage, string typeName, bool autoStart) {
      // required for JSON serialization and unit tests
      Name = Guard.Argument(name, nameof(name)).NotNull().NotEmpty().NotWhiteSpace().Value;
      Description = Guard.Argument(description, nameof(description)).NotNull().Value;
      Version = Guard.Argument(version, nameof(version)).NotNull().NotEmpty().NotWhiteSpace().Value;
      Kind = kind;
      DockerImage = Guard.Argument(dockerImage, nameof(dockerImage)).NotNull().Value;
      TypeName = Guard.Argument(typeName, nameof(typeName)).NotNull().NotEmpty().NotWhiteSpace().Value;
      AutoStart = autoStart;
    }
    public RunnableInfo(IRunnable runnable) {
      Guard.Argument(runnable, nameof(runnable)).NotNull()
                                                .Member(r => r.Name, n => n.NotNull().NotEmpty().NotWhiteSpace())
                                                .Member(r => r.Description, d => d.NotNull())
                                                .Member(r => r.Version, v => v.NotNull().NotEmpty().NotWhiteSpace())
                                                .Member(r => r.DockerImage, d => d.NotNull());
      Name = runnable.Name;
      Description = runnable.Description;
      Version = runnable.Version;
      Kind = runnable switch {
        IApplication app => (app.Kind == ApplicationKind.Console ? RunnableKind.ConsoleApplication : RunnableKind.GUIApplication),
        IService service => RunnableKind.Service,
        _ => throw new NotSupportedException($"Unknown runnable kind.")
      };
      DockerImage = runnable.DockerImage;
      TypeName = runnable.GetType().AssemblyQualifiedName ?? throw new InvalidOperationException("Cannot get type name of runnable.");
      AutoStart = runnable.AutoStart;
    }

    public IRunnable CreateRunnable() {
      Type runnableType = Type.GetType(TypeName) ?? throw new InvalidOperationException($"Cannot resolve runnable type name {TypeName}.");
      return (IRunnable)(Activator.CreateInstance(runnableType) ?? throw new InvalidOperationException($"Cannot create runnable of type {TypeName}."));
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
