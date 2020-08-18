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
using NuGet.Versioning;
using System.Reflection.Emit;
using Dawn;

namespace HEAL.Bricks {
  public static class GuardExtensions {
    public static ref readonly Guard.ArgumentInfo<string> ValidNuGetVersionString(in this Guard.ArgumentInfo<string> argument) {
      if (argument.HasValue() && (argument.Value.Length > 0)) {
        if (!NuGetVersion.TryParse(argument.Value, out _)) {
          throw Guard.Fail(new ArgumentException($"{argument.Name} is not a valid NuGet version.", argument.Name));
        }
      }
      return ref argument;
    }
  }
}
