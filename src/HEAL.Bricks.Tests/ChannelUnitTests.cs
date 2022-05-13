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
using System.Threading;

namespace HEAL.Bricks.Tests {
  [Trait("Category", "Unit")]
  public class ChannelUnitTests {
    #region Open
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    public void Open_WhenProgramNotFound_ThrowsWin32Exception(Type channelType) {
      IChannel channel = TestHelpers.CreateChannel(channelType, "program", "", null);

      Assert.Throws<Win32Exception>(() => channel.Open(out Task clientTerminated));
    }
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public void Open_WhenChannelIsOpened_ThrowsInvalidOperationException(Type channelType) {
      IChannel channel = TestHelpers.CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestHelpers.TestChannelAsync);
      channel.Open(out Task clientTerminated);

      var e = Assert.Throws<InvalidOperationException>(() => channel.Open(out Task clientTerminated));
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public void Open_WhenChannelIsClosed_ThrowsObjectDisposedException(Type channelType) {
      IChannel channel = TestHelpers.CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestHelpers.TestChannelAsync);
      channel.Open(out Task clientTerminated);
      channel.Close();
      try { clientTerminated.Wait(); } catch { }

      var e = Assert.Throws<ObjectDisposedException>(() => channel.Open(out Task clientTerminated));
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public void Open_WithCancellation_ThrowsAggregateException(Type channelType) {
      IChannel channel = TestHelpers.CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestHelpers.TestChannelAsync);
      CancellationTokenSource cts = new();
      channel.Open(out Task clientTerminated, cts.Token);
      cts.Cancel();

      var e = Assert.Throws<AggregateException>(() => clientTerminated.Wait());
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    #endregion

    #region Close
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public void Close_WhenChannelNotOpened_ThrowsInvalidOperationException(Type channelType) {
      IChannel channel = TestHelpers.CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestHelpers.TestChannelAsync);

      var e = Assert.Throws<InvalidOperationException>(() => channel.Close());
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public void Close_WhenChannelIsClosed(Type channelType) {
      IChannel channel = TestHelpers.CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestHelpers.TestChannelAsync);
      channel.Open(out Task clientTerminated);
      channel.Close();
      try { clientTerminated.Wait(); } catch { }
      
      channel.Close();
    }
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public void Close_WhenClientIsRunning_ThrowsAggregateException(Type channelType) {
      IChannel channel = TestHelpers.CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestHelpers.TestChannelAsync);
      channel.Open(out Task clientTerminated);
      
      channel.Close();

      var e = Assert.Throws<AggregateException>(() => clientTerminated.Wait());
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public void Close_WhenClientExited(Type channelType) {
      IChannel channel = TestHelpers.CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestHelpers.TestChannelAsync);
      channel.Open(out Task clientTerminated);
      channel.SendMessageAsync(new CancelMessage()).Wait();
      clientTerminated.Wait();

      channel.Close();
    }
    #endregion

    #region SendMessageAsync
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public async Task SendMessageAsync_WithNullParameter_ThrowsArgumentNullException(Type channelType) {
      IChannel channel = TestHelpers.CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestHelpers.TestChannelAsync);

      var e = await Assert.ThrowsAsync<ArgumentNullException>(() => channel.SendMessageAsync<IMessage>(null!));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public async Task SendMessageAsync_WhenChannelNotOpened_ThrowsInvalidOperationException(Type channelType) {
      TextMessage message = new("TestMessage");
      IChannel channel = TestHelpers.CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestHelpers.TestChannelAsync);

      var e = await Assert.ThrowsAsync<InvalidOperationException>(() => channel.SendMessageAsync(message));
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public async Task SendMessageAsync_WhenChannelIsClosed_ThrowsObjectDisposedException(Type channelType) {
      TextMessage message = new("TestMessage");
      IChannel channel = TestHelpers.CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestHelpers.TestChannelAsync);
      channel.Open(out Task clientTerminated);
      channel.Close();
      try { await clientTerminated; } catch { }

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
      IChannel channel = TestHelpers.CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestHelpers.TestChannelAsync);

      var e = await Assert.ThrowsAsync<InvalidOperationException>(() => channel.ReceiveMessageAsync<IMessage>());
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public async Task ReceiveMessageAsync_WhenChannelIsClosed_ThrowsObjectDisposedException(Type channelType) {
      IChannel channel = TestHelpers.CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestHelpers.TestChannelAsync);
      channel.Open(out Task clientTerminated);
      channel.Close();
      try { await clientTerminated; } catch { }

      var e = await Assert.ThrowsAsync<ObjectDisposedException>(() => channel.ReceiveMessageAsync<IMessage>());
      Assert.False(string.IsNullOrEmpty(e.Message));
    }
    #endregion

    #region Send/ReceiveMessageAsync
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    [InlineData(typeof(MemoryChannel))]
    public async Task SendAndReceiveMessageAsync_WithMessage_ReturnsMessage(Type channelType) {
      TextMessage request = new("TestMessage");
      TextMessage response;

      using (IChannel channel = TestHelpers.CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestChannel", TestHelpers.TestChannelAsync)) {
        channel.Open(out Task clientTerminated);
        await channel.SendMessageAsync(request);
        response = await channel.ReceiveMessageAsync<TextMessage>();
        await channel.SendMessageAsync(new CancelMessage());
        await clientTerminated;
      }

      Assert.Equal(request.Data, response.Data);
    }
    #endregion
  }
}
