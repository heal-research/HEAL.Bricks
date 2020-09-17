#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;

namespace HEAL.Bricks {
  public class DockerChannel : StdInOutProcessChannel {
    private static string CreateDockerArguments(string dockerImage, bool useWindowsContainer, string hostDirectory, string programPath, string arguments) {
      Guard.Argument(dockerImage, nameof(dockerImage)).NotNull().NotEmpty().NotWhiteSpace();
      Guard.Argument(hostDirectory, nameof(hostDirectory)).NotNull().NotEmpty().NotWhiteSpace().AbsolutePath();
      Guard.Argument(programPath, nameof(programPath)).NotNull().NotEmpty().NotWhiteSpace();

      return  "run --rm -i " +
             $"--mount type=bind,source=\"{hostDirectory}\",target={(useWindowsContainer ? "c:/" : "/mnt/")}bricks,readonly " +
             $"-w {(useWindowsContainer ? "c:/" : "/mnt/")}bricks " +
             $"{dockerImage} " +
             $"{programPath} {arguments ?? ""}";
    }

    public DockerChannel(string dockerCommand, string dockerImage, bool useWindowsContainer, string hostDirectory, string programPath, string arguments = null)
      : base(dockerCommand, CreateDockerArguments(dockerImage, useWindowsContainer, hostDirectory, programPath, arguments)) { }
    protected DockerChannel() {
      // used to create a new channel on the client-side
    }
  }
}
