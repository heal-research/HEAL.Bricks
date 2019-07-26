#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;

namespace HEAL.Bricks {
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
  public sealed class ContactInformationAttribute : Attribute {
    public string Name { get; }
    public string EMail { get; }

    public ContactInformationAttribute(string name, string email) {
      #region Parameter Validation
      if (string.IsNullOrWhiteSpace(name)) {
        throw (name == null) ? new ArgumentNullException(nameof(name)) :
                               new ArgumentException("ContactInformationAttribute name must not be empty or all whitespace.", nameof(name));
      }
      if (string.IsNullOrWhiteSpace(email)) {
        throw (email == null) ? new ArgumentNullException(nameof(email)) :
                                new ArgumentException("ContactInformationAttribute email must not be empty or all whitespace.", nameof(email));
      }
      #endregion

      Name = name;
      EMail = email;
    }
  }
}
