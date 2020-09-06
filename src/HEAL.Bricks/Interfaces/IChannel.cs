#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  public interface IChannel : IDisposable {
    void Open(out Task channelTerminated, CancellationToken cancellationToken = default);
    void Close();

    Task SendMessageAsync(IMessage message, CancellationToken cancellationToken = default);
    Task<IMessage> ReceiveMessageAsync(CancellationToken cancellationToken = default);
    Task<T> ReceiveMessageAsync<T>(CancellationToken cancellationToken = default) where T : class, IMessage;
  }
}
