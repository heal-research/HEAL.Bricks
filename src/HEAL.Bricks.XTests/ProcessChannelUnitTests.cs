#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Xunit;
using System;

namespace HEAL.Bricks.XTests {
  public class ProcessChannelUnitTests {
    [Fact]
    public void ChannelTypeArgument_IsNotNullOrEmptyOrWhiteSpace() {
      Assert.False(string.IsNullOrWhiteSpace(ProcessChannel.ChannelTypeArgument));
    }
    [Fact]
    public void CreateFromCLIArguments_WithNullParameter_ThrowsArgumentNullException() {
      var e = Assert.Throws<ArgumentNullException>(() => ProcessChannel.CreateFromCLIArguments(null));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData("--ChannelType")]
    [InlineData("--ChannelType=")]
    [InlineData("--ChannelType=InvalidType")]
    [InlineData("--ChannelType=InvalidType", "--ChannelType=InvalidType")]
    public void CreateFromCLIArguments_WithInvalidChannelTypeArgument_ThrowsArgumentException(params string[] arguments) {
      var e = Assert.Throws<ArgumentException>(() => ProcessChannel.CreateFromCLIArguments(arguments));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
  }
}
