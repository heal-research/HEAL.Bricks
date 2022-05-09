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
  public class BricksOptionsValidator : AbstractValidator<BricksOptions> {
    public static ValidationResult Check(BricksOptions options) {
      return new BricksOptionsValidator().Validate(options);
    }

    public BricksOptionsValidator() {
      RuleFor(options => options.Repositories).NotNull();
      RuleForEach(options => options.Repositories).NotEmpty().ChildRules(repository => repository.RuleFor(r => r.Source).NotEmpty());
      RuleFor(options => options.AppPath).NotEmpty().Must(p => Path.IsPathRooted(p)).WithMessage($"'{nameof(BricksOptions.AppPath)}' must be an absolute path.");
      RuleFor(options => options.PackagesPath).NotEmpty().Must(p => Path.IsPathRooted(p)).WithMessage($"'{nameof(BricksOptions.PackagesPath)}' must be an absolute path.");
      RuleFor(options => options.PackagesCachePath).NotEmpty().Must(p => Path.IsPathRooted(p)).WithMessage($"'{nameof(BricksOptions.PackagesCachePath)}' must be an absolute path.");
      RuleFor(options => options.DefaultIsolation).IsInEnum();
      RuleFor(options => options.DotnetCommand).NotEmpty();
      RuleFor(options => options.DockerCommand).NotEmpty();
      RuleFor(options => options.DefaultDockerImage).NotEmpty();
      RuleFor(options => options.StarterAssembly).NotEmpty().Must(p => !Path.IsPathRooted(p)).WithMessage($"'{nameof(BricksOptions.StarterAssembly)}' must be a relative path.");
    }
  }
}
