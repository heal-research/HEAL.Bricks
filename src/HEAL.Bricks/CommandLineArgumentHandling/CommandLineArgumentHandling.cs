#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace HEAL.Bricks {
  public static class CommandLineArgumentHandling {
    public static ICommandLineArgument[] GetArguments(string[] args) {
      var arguments = new HashSet<ICommandLineArgument>();
      var exceptions = new List<Exception>();

      foreach (var entry in args) {
        var argument = ParseArgument(entry);
        if (argument != null && argument.Valid) arguments.Add(argument);
        else exceptions.Add(new ArgumentException(string.Format("The argument \"{0}\" is invalid.", entry)));
      }

      if (exceptions.Any()) throw new AggregateException("One or more arguments are invalid.", exceptions);
      return arguments.ToArray();
    }

    private static ICommandLineArgument ParseArgument(string entry) {
      var regex = new Regex(@"^/[A-Za-z]+(:\w[\w\s]*)?$");
      bool isFile = File.Exists(entry);
      if (!regex.IsMatch(entry) && !isFile) return null;
      if (!isFile) {
        entry = entry.Remove(0, 1);
        var parts = entry.Split(':');
        string key = parts[0].Trim();
        string value = parts.Length == 2 ? parts[1].Trim() : string.Empty;
        switch (key) {
          case StartArgument.TOKEN: return new StartArgument(value);
          case HideStarterArgument.TOKEN: return new HideStarterArgument(value);
          default: return null;
        }
      } else return new OpenArgument(entry);
    }
  }
}
