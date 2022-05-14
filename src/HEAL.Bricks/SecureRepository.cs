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
  public class SecureRepository : Repository {
    public string Username { get; }
    public string Password { get; }

    [JsonConstructor]
    public SecureRepository(string source, string username, string password) : base(source) {
      Username = Guard.Argument(username, nameof(username)).NotNull();
      Password = Guard.Argument(password, nameof(password)).NotNull();
    }

    protected override SourceRepository CreateSourceRepository() {
      PackageSource packageSource = new(Source) {
        Credentials = PackageSourceCredential.FromUserInput(Source, Username, Password, storePasswordInClearText: true, validAuthenticationTypesText: string.Empty)
      };
      return NuGetRepository.CreateSource(NuGetRepository.Provider.GetCoreV3(), packageSource);
    }

    public override bool Equals(object? obj) {
      if (obj is SecureRepository secureRepository) {
        return base.Equals(obj) &&
               secureRepository.Username == Username &&
               secureRepository.Password == Password;
      } else {
        return base.Equals(obj);
      }
    }
    public override int GetHashCode() {
      return base.GetHashCode();
    }
  }
}
