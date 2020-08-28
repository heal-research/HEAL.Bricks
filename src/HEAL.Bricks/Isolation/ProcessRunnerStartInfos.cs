#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.IO;
using System.Reflection;

namespace HEAL.Bricks {
  [Serializable]
  public class ProcessRunnerStartInfo : IProcessRunnerStartInfo {
    private string programPath;
    public virtual string ProgramPath {
      get { return programPath; }
      set {
        if (value == null) throw new ArgumentNullException(nameof(ProgramPath));
        if (value == "") throw new ArgumentException($"{nameof(ProgramPath)} is empty", nameof(programPath));
        programPath = value;
      }
    }
    public virtual string Arguments { get; set; }
    public virtual CommunicationMode CommunicationMode { get; set; }
    // TODO: reconsider if it is really a good idea to provide using alternative user accounts
    //       maybe we do not need this, as we provide other ways of isolation (e.g. docker)
    public virtual string UserName { get; set; }
    public virtual string UserDomain { get; set; }
    // TODO: to discuss
    // we do not use SecureString to store a user password here, as SecureString is not recommended anymore (see https://github.com/dotnet/platform-compat/blob/master/docs/DE0001.md)
    // we expect that the used user account has strongly restricted privileges, therefore storing the password in clear text is not that critical (???)
    public virtual string UserPassword { get; set; }

    public ProcessRunnerStartInfo(string programPath, string arguments = null, CommunicationMode communicationMode = CommunicationMode.AnonymousPipes) {
      ProgramPath = programPath;
      Arguments = arguments;
      CommunicationMode = communicationMode;
    }
  }

  [Serializable]
  public class GenericProgramStartInfo : ProcessRunnerStartInfo {
    public override string ProgramPath { get => base.ProgramPath; set => SetProgramPath(value); }
    public string Program { get => Path.GetFileName(ProgramPath); }

    public GenericProgramStartInfo(string program, string arguments = null, CommunicationMode communicationMode = CommunicationMode.AnonymousPipes) : base(program, arguments, communicationMode) { }

    private void SetProgramPath(string path) {
      if (path == null) throw new ArgumentNullException(nameof(ProgramPath));
      if (path == "") throw new ArgumentException($"{nameof(ProgramPath)} is empty", nameof(ProgramPath));

      base.ProgramPath = path;
      if (!Path.IsPathRooted(base.ProgramPath)) {
        string appDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        base.ProgramPath = Path.Combine(appDir, base.ProgramPath);
      }
    }
  }

  [Serializable]
  public class NetCoreAssemblyStartInfo : ProcessRunnerStartInfo {
    public NetCoreAssemblyStartInfo(string assembly, string arguments = null, CommunicationMode communicationMode = CommunicationMode.AnonymousPipes) : base("dotnet", arguments, communicationMode) {
      if (assembly == null) throw new ArgumentNullException(nameof(assembly));
      if (assembly == "") throw new ArgumentException($"{nameof(assembly)} is empty", nameof(assembly));

      if (!assembly.EndsWith(".dll")) assembly += ".dll";
      assembly = "\"" + assembly + "\"";
      Arguments = assembly + (!string.IsNullOrEmpty(Arguments) ? " " + Arguments : "");
    }
  }

  [Serializable]
  public class NetCoreEntryAssemblyStartInfo : ProcessRunnerStartInfo {
    public NetCoreEntryAssemblyStartInfo(CommunicationMode communicationMode = CommunicationMode.AnonymousPipes) : base("dotnet", "\"" + Assembly.GetEntryAssembly().Location + "\"", communicationMode) { }
  }
}
