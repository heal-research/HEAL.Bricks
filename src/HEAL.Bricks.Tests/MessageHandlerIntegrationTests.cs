#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System;
using System.Collections.Generic;

namespace HEAL.Bricks.Tests {
  [Trait("Category", "Integration")]
  public class MessageHandlerIntegrationTests {
    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel), false)]
    [InlineData(typeof(StdInOutProcessChannel),       false)]
    [InlineData(typeof(MemoryChannel),                false)]
    public async Task DiscoverRunnablesAsync_ReturnsRunnables(Type channelType, bool startDebugger) {
      PackageLoadInfo[] packageLoadInfos = new[] {
        PackageLoadInfo.CreateForTests("a", "1.0.0", TestHelpers.GetWorkingDir(), "HEAL.Bricks.Tests.dll")
      };
      IApplication expectedApplication = new DummyApplication();
      IChannel channel = TestHelpers.CreateChannel(channelType, Constants.DotnetExePath, $"HEAL.Bricks.Tests.BricksRunner.dll --TestMessageHandler {(startDebugger ? "--Debug" : "")}", TestHelpers.TestMessageHandler);
      channel.Open(out Task clientTerminated);

      await channel.SendMessageAsync(Message.Factory.LoadPackages(packageLoadInfos));
      await channel.ReceiveMessageAsync(Message.Commands.PackagesLoaded);
      await channel.SendMessageAsync(Message.Factory.DiscoverRunnables());
      RunnableInfo[] result = (await channel.ReceiveMessageAsync<IEnumerable<RunnableInfo>>(Message.Commands.RunnablesDiscovered)).ToArray();
      await channel.SendMessageAsync(Message.Factory.Terminate());
      await (clientTerminated);

      Assert.Collection(result,
        x => {
          Assert.Equal(expectedApplication.Name, x.Name);
          Assert.Equal(expectedApplication.Description, x.Description);
          Assert.Equal(expectedApplication.GetType().AssemblyQualifiedName, x.TypeName);
        }
      );
    }

    [Theory]
    [InlineData(typeof(MemoryChannel))]
    public async Task GetApplicationsAsync_WithMemoryChannel_ReturnsApplications(Type channelType) {
      IApplication expectedApplication = new DummyApplication();
      IChannel channel = TestHelpers.CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestMessageHandler", TestHelpers.TestMessageHandler);
      channel.Open(out Task clientTerminated);

      await channel.SendMessageAsync(Message.Factory.DiscoverRunnables());
      RunnableInfo[] result = (await channel.ReceiveMessageAsync<IEnumerable<RunnableInfo>>(Message.Commands.RunnablesDiscovered)).ToArray();
      await channel.SendMessageAsync(Message.Factory.Terminate());
      await (clientTerminated);

      Assert.Collection(result,
        x => {
          Assert.Equal(expectedApplication.Name, x.Name);
          Assert.Equal(expectedApplication.Description, x.Description);
          Assert.Equal(expectedApplication.GetType().AssemblyQualifiedName, x.TypeName);
        }
      );
    }

    [Theory]
    [InlineData(typeof(AnonymousPipesProcessChannel))]
    [InlineData(typeof(StdInOutProcessChannel))]
    public async Task GetApplicationsAsync_WhenNoApplicationsAvailable_ReturnsEmpty(Type channelType) {
      IChannel channel = TestHelpers.CreateChannel(channelType, Constants.DotnetExePath, "HEAL.Bricks.Tests.BricksRunner.dll --TestMessageHandler", TestHelpers.TestMessageHandler);
      channel.Open(out Task clientTerminated);

      await channel.SendMessageAsync(Message.Factory.LoadPackages(Enumerable.Empty<PackageLoadInfo>()));
      await channel.ReceiveMessageAsync(Message.Commands.PackagesLoaded);
      await channel.SendMessageAsync(Message.Factory.DiscoverRunnables());
      RunnableInfo[] result = (await channel.ReceiveMessageAsync<IEnumerable<RunnableInfo>>(Message.Commands.RunnablesDiscovered)).ToArray();
      await channel.SendMessageAsync(Message.Factory.Terminate());
      await (clientTerminated);

      Assert.Empty(result);
    }
  }
}
