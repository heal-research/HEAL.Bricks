#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FluentValidation;

namespace HEAL.Bricks {
  public class SettingsValidator : AbstractValidator<ISettings> {
    public SettingsValidator() {
      RuleFor(settings => settings.Repositories).NotEmpty();
    }
  }
}
