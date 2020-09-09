#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.IO;

namespace HEAL.Bricks.XTests {
  public class RandomTempDirectoryFixture : IDisposable {
    public string Directory { get; }

    public RandomTempDirectoryFixture() {
      Directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
      System.IO.Directory.CreateDirectory(Directory);
    }

    public void Dispose() {
      System.IO.Directory.Delete(Directory, recursive: true);
    }

    public string CreateRandomSubdirectory() {
      string directory = Path.Combine(Directory, Path.GetRandomFileName());
      System.IO.Directory.CreateDirectory(directory);
      return directory;
    }
  }
}
