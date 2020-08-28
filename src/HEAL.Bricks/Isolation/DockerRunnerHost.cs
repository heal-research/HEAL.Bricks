#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  /// <summary>
  /// IRunner for isolation with docker.
  /// </summary>
  //public class DockerRunnerHost : RunnerHost {
  //  #region Constants
  //  private readonly static string Docker = "docker";
  //  private readonly static string ContainerStartup = "container run -i --rm ";
  //  private readonly static string MountingLinux = @"--mount type=bind,source=/c/Users/,target=/Users ";
  //  private readonly static string MountingWindows = @"--mount type=bind,source=C:\Users\,target=C:\Users\ ";
  //  private readonly static string LinuxImage = "." + Path.DirectorySeparatorChar + "DockerLinuxBuild" + Path.DirectorySeparatorChar + "Dockerfile";
  //  private readonly static string WindowsImage = "." + Path.DirectorySeparatorChar + "DockerWindowsBuild" + Path.DirectorySeparatorChar + "Dockerfile";
  //  private readonly static string Image = "heuristiclab33:latest";
  //  private readonly static string DockerExceptionMessage = "Docker is not running!";
  //  #endregion

  //  #region Constructors
  //  public DockerRunnerHost()
  //    : base(Docker, ContainerStartup + GetTargetOSMounting() + Image, null, null, null) {
  //  }
  //  #endregion

  //  #region Helper

  //  private static string GetTargetOSMounting() {
  //    Task<bool> win = Task.Run(() => BuildImage(WindowsImage));
  //    Task<bool> lin = Task.Run(() => BuildImage(LinuxImage));
  //    if (win.Result) return MountingWindows;
  //    else if (lin.Result) return MountingLinux;
  //    else throw new DockerException(DockerExceptionMessage);
  //  }

  //  private static bool BuildImage(string pathToDockerfile) {
  //    var process = new Process {
  //      StartInfo = new ProcessStartInfo {
  //        FileName = Docker,
  //        Arguments = "image build -t " + Image +
  //          " -f " + Path.GetFullPath(pathToDockerfile) +
  //          " .",
  //        UseShellExecute = false,
  //        RedirectStandardOutput = true,
  //        RedirectStandardInput = false,
  //        RedirectStandardError = true,
  //        CreateNoWindow = false
  //      },
  //      EnableRaisingEvents = false
  //    };
  //    process.Start();
  //    process.BeginOutputReadLine();
  //    process.BeginErrorReadLine();
  //    process.WaitForExit();
  //    return process.ExitCode == 0;
  //  }
  //  #endregion
  //}
}
