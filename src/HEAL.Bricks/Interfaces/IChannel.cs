#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks {
  public interface IChannel : IDisposable {
    void Open(out Task channelTerminated, CancellationToken cancellationToken = default);
    void Close();

    Task SendMessageAsync(string command, CancellationToken cancellationToken = default);
    Task SendMessageAsync<T>(string command, T payload, CancellationToken cancellationToken = default);
    Task SendMessageAsync(IMessage message, CancellationToken cancellationToken = default);

    Task ReceiveMessageAsync(string command, CancellationToken cancellationToken = default);
    Task<T> ReceiveMessageAsync<T>(string command, CancellationToken cancellationToken = default);
    Task<IMessage> ReceiveMessageAsync(CancellationToken cancellationToken = default);
  }
}
