#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.Collections.Generic;

namespace HEAL.Bricks {
  public interface ISettings {
    string PackageTag { get; }
    IEnumerable<string> Repositories { get; }
    string AppPath { get; }
    string PackagesPath { get; }
    string PackagesCachePath { get; }
  }
}
