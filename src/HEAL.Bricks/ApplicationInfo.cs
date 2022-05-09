#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Reflection;

namespace HEAL.Bricks {
  [Serializable]
  public class ApplicationInfo {
    public string Name { get; }
    public string Version { get; }
    public string Description { get; }
    [JsonConverter(typeof(StringEnumConverter))]
    public ApplicationKind Kind { get; }
    [JsonConverter(typeof(StringEnumConverter))]
    public Isolation RecommendedIsolation { get; }
    public string DockerImage { get; }
    public string TypeName { get; }

    [JsonConstructor]
    private ApplicationInfo(string name, string version, string description, ApplicationKind kind, string dockerImage, string typeName) {
      Name = Guard.Argument(name, nameof(name)).NotNull().NotEmpty().NotWhiteSpace();
      Version = Guard.Argument(version, nameof(version)).NotNull().NotEmpty().NotWhiteSpace();
      Description = Guard.Argument(description, nameof(description)).NotNull();
      Kind = kind;
      DockerImage = dockerImage;
      TypeName = Guard.Argument(typeName, nameof(typeName)).NotNull().NotEmpty().NotWhiteSpace();
    }
    internal ApplicationInfo(IApplication application) :
      this(application.Name,
           application.GetType().Assembly.GetName().Version.ToString(),
           application.Description,
           application.Kind,
           application.GetType().GetCustomAttribute<DockerImageAttribute>(inherit: false)?.Image,
           application.GetType().FullName
      ) { }

    public override bool Equals(object obj) {
      if (obj is ApplicationInfo appInfo) {
        return appInfo.TypeName == TypeName &&
               appInfo.Version == Version;
      }
      else {
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
