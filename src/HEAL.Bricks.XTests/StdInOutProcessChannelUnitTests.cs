#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.Threading.Tasks;
using Xunit;
using System;
using System.ComponentModel;

namespace HEAL.Bricks.XTests {
  public class StdInOutProcessChannelUnitTests {
    #region Constructor
    [Theory]
    [InlineData(null,  typeof(ArgumentNullException))]
    [InlineData("",    typeof(ArgumentException))]
    [InlineData("   ", typeof(ArgumentException))]
    public void Constructor_WithParameterIsNullOrEmptyOrWhiteSpace_ThrowsArgumentException(string programPath, Type expectedExceptionType) {
      var e = Assert.Throws(expectedExceptionType, () => new StdInOutProcessChannel(programPath));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty((e as ArgumentException).ParamName));
    }
    #endregion

    #region Open
    [Fact]
    public void Open_WithNotExistingProgram_ThrowsWin32Exception() {
      StdInOutProcessChannel channel = new StdInOutProcessChannel("program");

      Assert.Throws<Win32Exception>(() => channel.Open());
    }
    [Fact]
    public void Open_WhenChannelIsOpened_ThrowsInvalidOperationException() {
      StdInOutProcessChannel channel = new StdInOutProcessChannel(Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel");
      channel.Open();

      var e = Assert.Throws<InvalidOperationException>(() => channel.Open());
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    [Fact]
    public void Open_WhenChannelIsClosed_ThrowsInvalidOperationException() {
      StdInOutProcessChannel channel = new StdInOutProcessChannel(Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel");
      channel.Open();
      channel.Close();

      var e = Assert.Throws<InvalidOperationException>(() => channel.Open());
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    #endregion

    #region Close
    [Fact]
    public void Close_WhenChannelNotOpened_ThrowsInvalidOperationException() {
      StdInOutProcessChannel channel = new StdInOutProcessChannel(Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel");

      var e = Assert.Throws<InvalidOperationException>(() => channel.Close());
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    [Fact]
    public void Close_WhenChannelIsClosed_ThrowsInvalidOperationException() {
      StdInOutProcessChannel channel = new StdInOutProcessChannel(Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel");
      channel.Open();
      channel.Close();

      var e = Assert.Throws<InvalidOperationException>(() => channel.Close());
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    [Fact]
    public void Close_WhenClientIsResponsive() {
      StdInOutProcessChannel channel = new StdInOutProcessChannel(Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel");
      channel.Open();
      channel.Close();
    }
    [Fact]
    public void Close_WhenClientIsNotResponsive() {
      StdInOutProcessChannel channel = new StdInOutProcessChannel(Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestNotResponsive");
      channel.Open();
      channel.Close();
    }
    [Fact]
    public void Close_WhenClientExited() {
      StdInOutProcessChannel channel = new StdInOutProcessChannel(Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestImmediateExit");
      channel.Open();
      channel.Close();
    }
    #endregion

    #region SendMessageAsync
    [Fact]
    public async Task SendMessageAsync_WithNullParameter_ThrowsArgumentNullException() {
      StdInOutProcessChannel channel = new StdInOutProcessChannel("program");

      var e = await Assert.ThrowsAsync<ArgumentNullException>(() => channel.SendMessageAsync(null));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Fact]
    public async Task SendMessageAsync_WhenChannelNotOpened_ThrowsInvalidOperationException() {
      TextMessage message = new TextMessage("TestMessage");
      StdInOutProcessChannel channel = new StdInOutProcessChannel(Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel");

      var e = await Assert.ThrowsAsync<InvalidOperationException>(() => channel.SendMessageAsync(message));
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    [Fact]
    public async Task SendMessageAsync_WhenChannelIsClosed_ThrowsInvalidOperationException() {
      TextMessage message = new TextMessage("TestMessage");
      StdInOutProcessChannel channel = new StdInOutProcessChannel(Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel");
      channel.Open();
      channel.Close();

      var e = await Assert.ThrowsAsync<InvalidOperationException>(() => channel.SendMessageAsync(message));
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    #endregion

    #region ReceiveMessageAsync
    [Fact]
    public async Task ReceiveMessageAsync_WhenChannelNotOpened_ThrowsInvalidOperationException() {
      StdInOutProcessChannel channel = new StdInOutProcessChannel(Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel");

      var e = await Assert.ThrowsAsync<InvalidOperationException>(() => channel.ReceiveMessageAsync());
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    [Fact]
    public async Task ReceiveMessageAsync_WhenChannelIsClosed_ThrowsInvalidOperationException() {
      StdInOutProcessChannel channel = new StdInOutProcessChannel(Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel");
      channel.Open();
      channel.Close();

      var e = await Assert.ThrowsAsync<InvalidOperationException>(() => channel.ReceiveMessageAsync());
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    #endregion

    #region Send/ReceiveMessageAsync
    [Fact]
    public async Task SendAndReceiveMessageAsync_WithMessage_ReturnsMessage() {
      TextMessage request = new TextMessage("TestMessage");
      TextMessage response;

      using(StdInOutProcessChannel channel = new StdInOutProcessChannel(Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel")) {
        channel.Open();
        await channel.SendMessageAsync(request);
        response = await channel.ReceiveMessageAsync<TextMessage>();
      }

      Assert.Equal(request.Data, response.Data);
    }
    #endregion
  }
}
