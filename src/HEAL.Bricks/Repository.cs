#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using Newtonsoft.Json;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using NuGetRepository = NuGet.Protocol.Core.Types.Repository;

namespace HEAL.Bricks {
  public class Repository {
    public string Source { get; }

    [JsonConstructor]
    public Repository(string source) {
      Source = Guard.Argument(source, nameof(source)).NotNull().NotEmpty().NotWhiteSpace();
    }

    protected internal virtual SourceRepository CreateSourceRepository() {
      PackageSource packageSource = new(Source);
      return NuGetRepository.CreateSource(NuGetRepository.Provider.GetCoreV3(), packageSource);
    }

    public override bool Equals(object? obj) {
      if (obj is Repository repository) {
        return repository.Source == Source;
      } else {
        return base.Equals(obj);
      }
    }
    public override int GetHashCode() {
      return Source.GetHashCode();
    }
  }
}
