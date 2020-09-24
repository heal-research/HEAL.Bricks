#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Runtime.InteropServices;

namespace HEAL.Bricks.Tests {
  public enum Platform {
    Unknown,
    Windows,
    Linux,
    OSX,
    FreeBSD
  }

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
    public const string netCoreApp11FrameworkName = @".NETCoreApp,Version=v1.1";
    public const string netCoreApp20FrameworkName = @".NETCoreApp,Version=v2.0";
    public const string netCoreApp21FrameworkName = @".NETCoreApp,Version=v2.1";
    public const string netCoreApp22FrameworkName = @".NETCoreApp,Version=v2.2";
    public const string netCoreApp30FrameworkName = @".NETCoreApp,Version=v3.0";
    public const string netCoreApp31FrameworkName = @".NETCoreApp,Version=v3.1";
    public const string netStandard10FrameworkName = @".NETStandard,Version=v1.0";
    public const string netStandard11FrameworkName = @".NETStandard,Version=v1.1";
    public const string netStandard12FrameworkName = @".NETStandard,Version=v1.2";
    public const string netStandard13FrameworkName = @".NETStandard,Version=v1.3";
    public const string netStandard14FrameworkName = @".NETStandard,Version=v1.4";
    public const string netStandard15FrameworkName = @".NETStandard,Version=v1.5";
    public const string netStandard16FrameworkName = @".NETStandard,Version=v1.6";
    public const string netStandard20FrameworkName = @".NETStandard,Version=v2.0";
    public const string netStandard21FrameworkName = @".NETStandard,Version=v2.1";
    public const string netFramework35FrameworkName = @".NETFramework,Version=v3.5";
    public const string netFramework45FrameworkName = @".NETFramework,Version=v4.5";
    public const string netFramework451FrameworkName = @".NETFramework,Version=v4.5.1";
    public const string netFramework452FrameworkName = @".NETFramework,Version=v4.5.2";
    public const string netFramework46FrameworkName = @".NETFramework,Version=v4.6";
    public const string netFramework461FrameworkName = @".NETFramework,Version=v4.6.1";
    public const string netFramework462FrameworkName = @".NETFramework,Version=v4.6.2";
    public const string netFramework47FrameworkName = @".NETFramework,Version=v4.7";
    public const string netFramework471FrameworkName = @".NETFramework,Version=v4.7.1";
    public const string netFramework472FrameworkName = @".NETFramework,Version=v4.7.2";
    public const string netFramework48FrameworkName = @".NETFramework,Version=v4.8";
  }
}
