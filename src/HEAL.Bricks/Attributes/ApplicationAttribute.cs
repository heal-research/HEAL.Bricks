#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;

namespace HEAL.Bricks {
  [AttributeUsage(AttributeTargets.Class)]
  public class ApplicationAttribute : Attribute {
    public string Name { get; }
    public string Description { get; }

    public ApplicationAttribute(string name, string description = "") {
      #region Parameter Validation
      if (string.IsNullOrWhiteSpace(name)) {
        throw (name == null) ? new ArgumentNullException(nameof(name)) :
                               new ArgumentException($"{nameof(ApplicationAttribute)}.{nameof(Name)} must not be empty or all whitespace.", nameof(name));
      }
      if (description == null) throw new ArgumentNullException(nameof(description));
      #endregion

      Name = name;
      Description = description;
    }
  }
}
