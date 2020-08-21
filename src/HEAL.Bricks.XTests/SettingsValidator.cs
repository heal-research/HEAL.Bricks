#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.IO;
using FluentValidation;
using FluentValidation.Results;

namespace HEAL.Bricks.XTests {
  public class SettingsValidator : AbstractValidator<ISettings> {
    public static ValidationResult Check(ISettings settings) {
      return new SettingsValidator().Validate(settings);
    }

    public SettingsValidator() {
      RuleFor(settings => settings.PackageTag).NotEmpty();
      RuleFor(settings => settings.Repositories).NotEmpty();
      RuleForEach(settings => settings.Repositories).NotEmpty();
      RuleFor(settings => settings.AppPath).NotEmpty().Must(p => Path.IsPathRooted(p)).WithMessage($"'{nameof(ISettings.AppPath)}' must be an absolute path.");
      RuleFor(settings => settings.PackagesPath).NotEmpty().Must(p => Path.IsPathRooted(p)).WithMessage($"'{nameof(ISettings.PackagesPath)}' must be an absolute path.");
      RuleFor(settings => settings.PackagesCachePath).NotEmpty().Must(p => Path.IsPathRooted(p)).WithMessage($"'{nameof(ISettings.PackagesCachePath)}' must be an absolute path.");
    }
  }
}
