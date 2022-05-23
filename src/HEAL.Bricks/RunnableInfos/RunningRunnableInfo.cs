#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks {
  public class RunningRunnableInfo {
    public RunnableInfo RunnableInfo { get; }
    public IChannel Channel { get; }
    public IMessageHandler MessageHandler { get; }
    public CancellationTokenSource CancellationTokenSource { get; }
    public Task Terminated { get; internal set; }

    public RunningRunnableInfo(RunnableInfo runnableInfo, IChannel channel, IMessageHandler messageHandler, CancellationTokenSource cancellationTokenSource, Task terminated) {
      RunnableInfo = runnableInfo;
      Channel = channel;
      MessageHandler = messageHandler;
      CancellationTokenSource = cancellationTokenSource;
      Terminated = terminated;
    }
  }
}
