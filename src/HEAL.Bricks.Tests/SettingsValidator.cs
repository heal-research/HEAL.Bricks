#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.IO;
using FluentValidation;
using FluentValidation.Results;

namespace HEAL.Bricks.Tests {
  public class SettingsValidator : AbstractValidator<ISettings> {
    public static ValidationResult Check(ISettings settings) {
      return new SettingsValidator().Validate(settings);
    }

    public SettingsValidator() {
      RuleFor(settings => settings.Repositories).NotEmpty();
      RuleForEach(settings => settings.Repositories).NotEmpty().ChildRules(repository => repository.RuleFor(r => r.Source).NotEmpty());
      RuleFor(settings => settings.AppPath).NotEmpty().Must(p => Path.IsPathRooted(p)).WithMessage($"'{nameof(ISettings.AppPath)}' must be an absolute path.");
      RuleFor(settings => settings.PackagesPath).NotEmpty().Must(p => Path.IsPathRooted(p)).WithMessage($"'{nameof(ISettings.PackagesPath)}' must be an absolute path.");
      RuleFor(settings => settings.PackagesCachePath).NotEmpty().Must(p => Path.IsPathRooted(p)).WithMessage($"'{nameof(ISettings.PackagesCachePath)}' must be an absolute path.");
      RuleFor(settings => settings.Isolation).IsInEnum();
      RuleFor(settings => settings.DotnetCommand).NotEmpty();
      RuleFor(settings => settings.DockerCommand).NotEmpty();
      RuleFor(settings => settings.DockerImage).NotEmpty();
      RuleFor(settings => settings.StarterAssembly).NotEmpty().Must(p => !Path.IsPathRooted(p)).WithMessage($"'{nameof(ISettings.StarterAssembly)}' must be a relative path.");
    }
  }
}
