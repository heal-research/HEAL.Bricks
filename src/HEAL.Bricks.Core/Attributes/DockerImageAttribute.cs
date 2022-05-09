#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;

namespace HEAL.Bricks {
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
  public class DockerImageAttribute : Attribute {
    public string Image { get; private set; }
    public DockerImageAttribute(string image) {
      if (image == null) throw new ArgumentNullException(nameof(image));
      if (string.IsNullOrWhiteSpace(image)) throw new ArgumentException($"{nameof(image)} must not be empty.", nameof(image));
      Image = image;
    }
  }
}
