#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.Collections.Generic;
using Moq;
using NuGet.Packaging;
using NuGet.Versioning;

namespace HEAL.Bricks.XTests {
  public static class Mock {
    public static Mock<T> Of<T>() where T : class {
      return new Mock<T>();
    }

    public static T Object<T>(this Mock<T> mock) where T : class {
      return mock.Object;
    }

    internal static Mock<INuGetConnector> GetLocalPackages(this Mock<INuGetConnector> mock, string packagesPathToVerify, string bricksPackageTagToVerify, params LocalPackageInfo[] packages) {
      mock.Setup(m => m.GetLocalPackages(It.Is<string>(s => s == packagesPathToVerify), It.Is<string>(s => s == bricksPackageTagToVerify))).Returns(packages);
      return mock;
    }
    internal static Mock<INuGetConnector> GetLocalPackages(this Mock<INuGetConnector> mock, params LocalPackageInfo[] packages) {
      mock.Setup(m => m.GetLocalPackages(It.IsAny<string>(), It.IsAny<string>())).Returns(packages);
      return mock;
    }
  }
}
