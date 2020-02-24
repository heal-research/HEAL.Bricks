#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
//using HEAL.Attic;

namespace HEAL.Bricks {
//  [StorableType("8911C8DC-FDA0-4CF6-A0CD-C67E72094D62")]
  public class UniPath {

    #region Vars
//    [Storable]
    private string fullPath = "";
    #endregion

    #region Constructors
//    [StorableConstructor]
//    private UniPath(StorableConstructorFlag _) { }
    public UniPath(string path) {
      fullPath = Path.GetFullPath(path);
    }
    #endregion

    private bool IsWindowsAbsolutePath(string path) => Regex.IsMatch(path, @"^[A-Z]:");

    private string Rebuild(char split, string startStr, string seperator) {
      string[] splits = fullPath.Split(split);
      string tmp = startStr;
      int i = 1;
      while (i < (splits.Length - 1)) {
        tmp += splits[i] + seperator;
        ++i;
      }
      tmp += splits[i];
      return tmp;
    }

    public override string ToString() {
      bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
      if (isWindows) {
        if (IsWindowsAbsolutePath(fullPath))
          return fullPath;
        else return Rebuild('/', @"C:\", @"\");
      } else {
        if (IsWindowsAbsolutePath(fullPath))
          return Rebuild('\\', "/", "/");
        else return fullPath;
      }
    }
  }
}
