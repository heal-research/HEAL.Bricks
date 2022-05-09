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
using System.Text;

namespace HEAL.Bricks {
  public class DockerCLI {
    public string DockerCommand { get; }
    protected DockerCLI(string dockerCommand) {
      if (dockerCommand == null) throw new ArgumentNullException(nameof(dockerCommand));
      if (string.IsNullOrWhiteSpace(dockerCommand)) throw new ArgumentException($"{nameof(dockerCommand)} must not be empty.", nameof(dockerCommand));
      DockerCommand = dockerCommand;
    }
    public static DockerCLI BuildCommand(string dockerCommand = "docker") {
      return new DockerCLI(dockerCommand);
    }

    public override string ToString() {
      return (DockerCommand + " " + BuildArguments()).Trim();
    }

    public string BuildArguments() {
      StringBuilder sb = new StringBuilder();

      sb.Append("run");
      if (remove) sb.Append(" --rm");
      sb.Append(detached ? " -d" : " -i");
      if (terminal) sb.Append(" -t");
      if (name != null) sb.Append(" --name" + name);

      foreach (var (hostPort, containerPort) in exposedPorts) {
        sb.Append($" -p {hostPort}:{containerPort}");
      }

      foreach (var (hostPath, containerPath, writeable) in mountedPaths) {
        sb.Append($" --mount type=bind,source=\"{hostPath}\",target=\"{(windowsContainer ? "c:/" : "/") + containerPath}\"{(!writeable ? ",readonly" : "")}");
      }

      foreach (var (variable, value) in environmentVariables) {
        sb.Append($" -e \"{variable}\"{(value != null ? $"=\"{value}\"" : "")}");
      }

      sb.Append($" -w {(windowsContainer ? "c:/" : "/") + workDir}");
      sb.Append(image);
      sb.Append(command);
      sb.Append(arguments);

      return sb.ToString().Trim();
    }

    protected string image = null;
    public DockerCLI Image(string image) {
      if (image == null) throw new ArgumentNullException(nameof(image));
      if (string.IsNullOrWhiteSpace(image)) throw new ArgumentException($"{nameof(image)} must not be empty.", nameof(image));
      if (this.image != null) throw new InvalidOperationException($"{nameof(image)} is already defined.");
      this.image = " " + image;
      return this;
    }

    protected string name = null;
    public DockerCLI Name(string name) {
      if (name == null) throw new ArgumentNullException(nameof(name));
      if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException($"{nameof(name)} must not be empty.", nameof(name));
      if (this.name != null) throw new InvalidOperationException($"{nameof(name)} is already defined.");
      this.name = " " + name;
      return this;
    }

    protected bool remove = false;
    public DockerCLI Remove(bool remove = true) {
      this.remove = remove;
      return this;
    }

    protected bool detached = false;
    public DockerCLI Detached(bool detached = true) {
      this.detached = detached;
      return this;
    }

    protected bool terminal = false;
    public DockerCLI Terminal(bool terminal = true) {
      this.terminal = terminal;
      return this;
    }

    protected List<(string hostPort, string containerPort)> exposedPorts = new List<(string, string)>();
    public DockerCLI ExposePort(string hostPort, string containerPort) {
      if (hostPort == null) throw new ArgumentNullException(nameof(hostPort));
      if (string.IsNullOrWhiteSpace(hostPort)) throw new ArgumentException($"{nameof(hostPort)} must not be empty.", nameof(hostPort));
      if (containerPort == null) throw new ArgumentNullException(nameof(containerPort));
      if (string.IsNullOrWhiteSpace(containerPort)) throw new ArgumentException($"{nameof(containerPort)} must not be empty.", nameof(containerPort));
      if (exposedPorts.Any(x => (x.hostPort == hostPort) || (x.containerPort == containerPort))) throw new InvalidOperationException($"{nameof(hostPort)} or {nameof(containerPort)} is already exposed.");
      exposedPorts.Add((hostPort, containerPort));
      return this;
    }

    protected List<(string hostPath, string containerPath, bool writable)> mountedPaths = new List<(string, string, bool)>();
    public DockerCLI MountPath(string hostPath, string containerPath, bool writable = false) {
      if (hostPath == null) throw new ArgumentNullException(nameof(hostPath));
      if (string.IsNullOrWhiteSpace(hostPath)) throw new ArgumentException($"{nameof(hostPath)} must not be empty.", nameof(hostPath));
      if (!Path.IsPathRooted(hostPath)) throw new ArgumentException($"{nameof(hostPath)} must be an absolute path.", nameof(hostPath));
      if (containerPath == null) throw new ArgumentNullException(nameof(containerPath));
      if (string.IsNullOrWhiteSpace(containerPath)) throw new ArgumentException($"{nameof(containerPath)} must not be empty.", nameof(containerPath));
      if (mountedPaths.Any(x => (x.hostPath == hostPath) || (x.containerPath == containerPath))) throw new InvalidOperationException($"{nameof(hostPath)} or {nameof(containerPath)} is already mounted.");
      mountedPaths.Add((hostPath, containerPath, writable));
      return this;
    }

    protected string workDir = null;
    public DockerCLI WorkDir(string workDir) {
      if (workDir == null) throw new ArgumentNullException(nameof(workDir));
      if (string.IsNullOrWhiteSpace(workDir)) throw new ArgumentException($"{nameof(workDir)} must not be empty.", nameof(workDir));
      if (this.workDir != null) throw new InvalidOperationException($"{nameof(workDir)} is already defined.");
      this.workDir = workDir;
      return this;
    }

    protected List<(string variable, string value)> environmentVariables = new List<(string, string)>();
    public DockerCLI EnvironmentVariable(string variable, string value) {
      if (variable == null) throw new ArgumentNullException(nameof(variable));
      if (string.IsNullOrWhiteSpace(variable)) throw new ArgumentException($"{nameof(variable)} must not be empty.", nameof(variable));
      if (environmentVariables.Any(x => x.variable == variable)) throw new InvalidOperationException($"{nameof(variable)} is already defined.");
      environmentVariables.Add((variable, value));
      return this;
    }

    protected bool windowsContainer = false;
    public DockerCLI WindowsContainer(bool windowsContainer = true) {
      this.windowsContainer = windowsContainer;
      return this;
    }

    protected string command = null;
    public DockerCLI Command(string command) {
      if (command == null) throw new ArgumentNullException(nameof(command));
      if (string.IsNullOrWhiteSpace(command)) throw new ArgumentException($"{nameof(command)} must not be empty.", nameof(command));
      if (this.command != null) throw new InvalidOperationException($"{nameof(command)} is already defined.");
      this.command = " " + command;
      return this;
    }

    protected string arguments = null;
    public DockerCLI CommandArgument(string argument) {
      if (argument != null) {
        if (string.IsNullOrWhiteSpace(argument)) throw new ArgumentException($"{nameof(argument)} must not be empty.", nameof(argument));
        if (command == null) throw new InvalidOperationException($"{nameof(command)} is not defined.");
        arguments += " " + argument;
      }
      return this;
    }
  }
}
