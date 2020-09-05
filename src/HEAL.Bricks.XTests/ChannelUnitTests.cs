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
  public class ChannelUnitTests {
    #region Open
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    public void Open_WhenProgramNotFound_ThrowsWin32Exception(Type channelType) {
      IChannel channel = CreateChannel(channelType, "program", "", null);

      Assert.Throws<Win32Exception>(() => channel.Open());
    }

    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public void Open_WhenChannelIsOpened_ThrowsInvalidOperationException(Type channelType) {
      IChannel channel = CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestChannelAsync);
      channel.Open();

      var e = Assert.Throws<InvalidOperationException>(() => channel.Open());
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public void Open_WhenChannelIsClosed_ThrowsObjectDisposedException(Type channelType) {
      IChannel channel = CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestChannelAsync);
      channel.Open();
      channel.Close();

      var e = Assert.Throws<ObjectDisposedException>(() => channel.Open());
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    #endregion

    #region Close
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public void Close_WhenChannelNotOpened_ThrowsInvalidOperationException(Type channelType) {
      IChannel channel = CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestChannelAsync);

      var e = Assert.Throws<InvalidOperationException>(() => channel.Close());
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public void Close_WhenChannelIsClosed(Type channelType) {
      IChannel channel = CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestChannelAsync);
      channel.Open();
      channel.Close();
      channel.Close();
    }
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public void Close_WhenClientIsResponsive(Type channelType) {
      IChannel channel = CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestChannelAsync);
      channel.Open();
      channel.Close();
    }
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public void Close_WhenClientIsNotResponsive(Type channelType) {
      IChannel channel = CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestNotResponsive", TestNotResponsiveAsync);
      channel.Open();
      channel.Close();
    }
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public void Close_WhenClientExited(Type channelType) {
      IChannel channel = CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestImmediateExit", TestImmediateExitAsync);
      channel.Open();
      channel.Close();
    }
    #endregion

    #region SendMessageAsync
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public async Task SendMessageAsync_WithNullParameter_ThrowsArgumentNullException(Type channelType) {
      IChannel channel = CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestChannelAsync);

      var e = await Assert.ThrowsAsync<ArgumentNullException>(() => channel.SendMessageAsync(null));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public async Task SendMessageAsync_WhenChannelNotOpened_ThrowsInvalidOperationException(Type channelType) {
      TextMessage message = new TextMessage("TestMessage");
      IChannel channel = CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestChannelAsync);

      var e = await Assert.ThrowsAsync<InvalidOperationException>(() => channel.SendMessageAsync(message));
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public async Task SendMessageAsync_WhenChannelIsClosed_ThrowsObjectDisposedException(Type channelType) {
      TextMessage message = new TextMessage("TestMessage");
      IChannel channel = CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestChannelAsync);
      channel.Open();
      channel.Close();

      var e = await Assert.ThrowsAsync<ObjectDisposedException>(() => channel.SendMessageAsync(message));
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    #endregion

    #region ReceiveMessageAsync
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public async Task ReceiveMessageAsync_WhenChannelNotOpened_ThrowsInvalidOperationException(Type channelType) {
      IChannel channel = CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestChannelAsync);

      var e = await Assert.ThrowsAsync<InvalidOperationException>(() => channel.ReceiveMessageAsync());
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public async Task ReceiveMessageAsync_WhenChannelIsClosed_ThrowsObjectDisposedException(Type channelType) {
      IChannel channel = CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestChannelAsync);
      channel.Open();
      channel.Close();

      var e = await Assert.ThrowsAsync<ObjectDisposedException>(() => channel.ReceiveMessageAsync());
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    #endregion

    #region Send/ReceiveMessageAsync
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public async Task SendAndReceiveMessageAsync_WithMessage_ReturnsMessage(Type channelType) {
      TextMessage request = new TextMessage("TestMessage");
      TextMessage response;

      using (IChannel channel = CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestChannelAsync)) {
        channel.Open();
        await channel.SendMessageAsync(request);
        response = await channel.ReceiveMessageAsync<TextMessage>();
      }

      Assert.Equal(request.Data, response.Data);
    }
    #endregion

    #region Helpers
    private static IChannel CreateChannel(Type channelType, string programPath, string arguments, Func<IChannel, Task> testCode) {
      if (typeof(ProcessChannel).IsAssignableFrom(channelType)) {
        return Activator.CreateInstance(channelType, programPath, arguments) as IChannel;
      }
      else if (typeof(MemoryChannel).IsAssignableFrom(channelType)) {
        return Activator.CreateInstance(channelType, new Action<MemoryChannel>(async channel => await testCode(channel))) as MemoryChannel;
      }
      else {
        return Activator.CreateInstance(channelType) as IChannel;
      }
    }
    private static async Task TestChannelAsync(IChannel channel) {
      IMessage message = await channel.ReceiveMessageAsync();
      while (!(message is CancelRunnerMessage)) {
        await channel.SendMessageAsync(message);
        message = await channel.ReceiveMessageAsync();
      }
    }
    private static Task TestNotResponsiveAsync(IChannel channel) {
      while (true) { }
    }
    private static Task TestImmediateExitAsync(IChannel channel) {
      return Task.CompletedTask;
    }
    #endregion
  }
}
