#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace HEAL.Bricks.Tests {
  [TestClass]
  public class ProcessRunnerStartInfosTests {
    [TestMethod]
    public void TestProcessRunnerStartInfo() {
      ProcessRunnerStartInfo processRunnerStartInfo;
      string programPath = "programPath";
      string arguments = "arguments";
      string userName = "userName";
      string userDomain = "userDomain";
      string userPassword = "userPassword";
      ArgumentNullException argumentNullException;
      ArgumentException argumentException;

      processRunnerStartInfo = new ProcessRunnerStartInfo(programPath);
      Assert.AreEqual(programPath, processRunnerStartInfo.ProgramPath);
      Assert.IsNull(processRunnerStartInfo.Arguments);
      Assert.IsNull(processRunnerStartInfo.UserName);
      Assert.IsNull(processRunnerStartInfo.UserDomain);
      Assert.IsNull(processRunnerStartInfo.UserPassword);

      processRunnerStartInfo.Arguments = arguments;
      processRunnerStartInfo.UserName = userName;
      processRunnerStartInfo.UserDomain = userDomain;
      processRunnerStartInfo.UserPassword = userPassword;
      Assert.AreEqual(arguments, processRunnerStartInfo.Arguments);
      Assert.AreEqual(userName, processRunnerStartInfo.UserName);
      Assert.AreEqual(userDomain, processRunnerStartInfo.UserDomain);
      Assert.AreEqual(userPassword, processRunnerStartInfo.UserPassword);

      processRunnerStartInfo = new ProcessRunnerStartInfo(programPath, arguments);
      Assert.AreEqual(programPath, processRunnerStartInfo.ProgramPath);
      Assert.AreEqual(arguments, processRunnerStartInfo.Arguments);
      Assert.IsNull(processRunnerStartInfo.UserName);
      Assert.IsNull(processRunnerStartInfo.UserDomain);
      Assert.IsNull(processRunnerStartInfo.UserPassword);

      argumentNullException = Assert.ThrowsException<ArgumentNullException>(() => processRunnerStartInfo.ProgramPath = null);
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));
      argumentNullException = Assert.ThrowsException<ArgumentNullException>(() => new ProcessRunnerStartInfo(null));
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));
      argumentException = Assert.ThrowsException<ArgumentException>(() => processRunnerStartInfo.ProgramPath = "");
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));
      argumentException = Assert.ThrowsException<ArgumentException>(() => new ProcessRunnerStartInfo(""));
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));
    }

    [TestMethod]
    public void GenericProgramStartInfo() {
      string appDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

      GenericProgramStartInfo genericProgramStartInfo;
      string dir;
      string program;
      string expectedProgram, expectedProgramPath;
      ArgumentNullException argumentNullException;
      ArgumentException argumentException;

      program = "program";
      expectedProgram = "program";
      expectedProgramPath = Path.Combine(appDir, expectedProgram);
      genericProgramStartInfo = new GenericProgramStartInfo(program);
      Assert.AreEqual(expectedProgram, genericProgramStartInfo.Program);
      Assert.AreEqual(expectedProgramPath, genericProgramStartInfo.ProgramPath);

      program = Path.Combine("dir", "program");
      expectedProgram = "program";
      expectedProgramPath = Path.Combine(appDir, "dir", expectedProgram);
      genericProgramStartInfo = new GenericProgramStartInfo(program);
      Assert.AreEqual(expectedProgram, genericProgramStartInfo.Program);
      Assert.AreEqual(expectedProgramPath, genericProgramStartInfo.ProgramPath);

      dir = Path.GetTempPath();
      program = Path.Combine(dir, "program");
      expectedProgram = "program";
      expectedProgramPath = Path.Combine(dir, expectedProgram);
      genericProgramStartInfo = new GenericProgramStartInfo(program);
      Assert.AreEqual(expectedProgram, genericProgramStartInfo.Program);
      Assert.AreEqual(expectedProgramPath, genericProgramStartInfo.ProgramPath);

      argumentNullException = Assert.ThrowsException<ArgumentNullException>(() => genericProgramStartInfo.ProgramPath = null);
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));
      argumentNullException = Assert.ThrowsException<ArgumentNullException>(() => new GenericProgramStartInfo(null));
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));
      argumentException = Assert.ThrowsException<ArgumentException>(() => genericProgramStartInfo.ProgramPath = "");
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));
      argumentException = Assert.ThrowsException<ArgumentException>(() => new GenericProgramStartInfo(""));
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));
    }

    [TestMethod]
    public void NetCoreAssemblyStartInfo() {
      NetCoreAssemblyStartInfo netCoreAssemblyStartInfo;
      string assembly = "assembly";
      string assemblyDll = "assembly.dll";
      string programPath = "dotnet";
      string arguments = "\"assembly.dll\"";
      ArgumentNullException argumentNullException;
      ArgumentException argumentException;

      netCoreAssemblyStartInfo = new NetCoreAssemblyStartInfo(assembly);
      Assert.AreEqual(programPath, netCoreAssemblyStartInfo.ProgramPath);
      Assert.AreEqual(arguments, netCoreAssemblyStartInfo.Arguments);

      netCoreAssemblyStartInfo = new NetCoreAssemblyStartInfo(assemblyDll);
      Assert.AreEqual(programPath, netCoreAssemblyStartInfo.ProgramPath);
      Assert.AreEqual(arguments, netCoreAssemblyStartInfo.Arguments);

      argumentNullException = Assert.ThrowsException<ArgumentNullException>(() => new NetCoreAssemblyStartInfo(null));
      Assert.IsFalse(string.IsNullOrEmpty(argumentNullException.ParamName));
      argumentException = Assert.ThrowsException<ArgumentException>(() => new NetCoreAssemblyStartInfo(""));
      Assert.IsFalse(string.IsNullOrEmpty(argumentException.ParamName));
    }

    [TestMethod]
    public void NetCoreEntryAssemblyStartInfo() {
      NetCoreEntryAssemblyStartInfo netCoreEntryAssemblyStartInfo;
      string programPath = "dotnet";
      string arguments = $"\"{Assembly.GetEntryAssembly().Location}\" --StartRunner";

      netCoreEntryAssemblyStartInfo = new NetCoreEntryAssemblyStartInfo();
      Assert.AreEqual(programPath, netCoreEntryAssemblyStartInfo.ProgramPath);
      Assert.AreEqual(arguments, netCoreEntryAssemblyStartInfo.Arguments);
    }
  }
}
