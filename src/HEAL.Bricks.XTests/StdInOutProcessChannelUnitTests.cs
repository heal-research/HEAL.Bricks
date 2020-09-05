#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Xunit;
using System;

namespace HEAL.Bricks.XTests {
  public class StdInOutProcessChannelUnitTests {
    [Theory]
    [InlineData(null,  typeof(ArgumentNullException))]
    [InlineData("",    typeof(ArgumentException))]
    [InlineData("   ", typeof(ArgumentException))]
    public void Constructor_WithParameterIsNullOrEmptyOrWhiteSpace_ThrowsArgumentException(string programPath, Type expectedExceptionType) {
      var e = Assert.Throws(expectedExceptionType, () => new StdInOutProcessChannel(programPath));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty((e as ArgumentException).ParamName));
    }
  }
}
