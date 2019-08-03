#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.Collections.Generic;

namespace HEAL.Bricks {
  public interface IRunner {
    IEnumerable<AssemblyInfo> AssembliesToLoad { get; set; }

    void Run();
    void Pause();
    void Resume();
    void Cancel();
  }
}
