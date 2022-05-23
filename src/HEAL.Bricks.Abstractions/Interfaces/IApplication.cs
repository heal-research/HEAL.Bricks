#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks {
  public interface IApplication : IRunnable {
    ApplicationKind Kind { get; }

    Task RunAsync(string[] args, CancellationToken cancellationToken);

    Task SendToHostAsync(string command, CancellationToken cancellationToken);
    Task SendToHostAsync<T>(string command, T payload, CancellationToken cancellationToken);
    Task SendToHostAsync(IMessage message, CancellationToken cancellationToken);

    Task ReceiveFromHostAsync(string command, CancellationToken cancellationToken);
    Task<T> ReceiveFromHostAsync<T>(string command, CancellationToken cancellationToken);
    Task<IMessage> ReceiveFromHostAsync(CancellationToken cancellationToken);
  }
}
