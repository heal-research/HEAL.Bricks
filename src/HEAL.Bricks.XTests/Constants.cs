﻿#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Runtime.InteropServices;

namespace HEAL.Bricks.XTests {
  public class Constants {
    // OS platform and dotnet.exe paths
    public static Platform Platform {
      get {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return Platform.Windows;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return Platform.Linux;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return Platform.OSX;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)) return Platform.FreeBSD;
        return Platform.Unknown;
      }
    }

    public const string dotnetExeWindowsPath = @"C:\Program Files\dotnet\dotnet.exe";
    public const string dotnetExeLinuxPath = "/usr/bin/dotnet";
    public static string DotnetExePath {
      get {
        return Platform switch
        {
          Platform.Windows => Constants.dotnetExeWindowsPath,
          Platform.Linux => Constants.dotnetExeLinuxPath,
          _ => throw new PlatformNotSupportedException()
        };
      }
    }

    // .NET Framework names
    public const string netCoreApp10FrameworkName = @".NETCoreApp,Version=v1.0";
    public const string netCoreApp21FrameworkName = @".NETCoreApp,Version=v2.1";
    public const string netCoreApp31FrameworkName = @".NETCoreApp,Version=v3.1";
  }
}
