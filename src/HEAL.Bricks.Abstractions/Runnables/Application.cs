#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks {
  public abstract class Application : Runnable, IApplication {
    public abstract ApplicationKind Kind { get; }

    protected Application() : base() { }
    protected Application(IChannel channel) : base(channel) { }

    public abstract Task RunAsync(string[] args, CancellationToken cancellationToken);

    public Task SendToHostAsync(string command, CancellationToken cancellationToken)
      => Channel?.SendMessageAsync(command, cancellationToken) ?? throw new InvalidOperationException("Channel is null.");
    public Task SendToHostAsync<T>(string command, T payload, CancellationToken cancellationToken)
      => Channel?.SendMessageAsync(command, payload, cancellationToken) ?? throw new InvalidOperationException("Channel is null.");
    public Task SendToHostAsync(IMessage message, CancellationToken cancellationToken)
      => Channel?.SendMessageAsync(message, cancellationToken) ?? throw new InvalidOperationException("Channel is null.");

    public Task ReceiveFromHostAsync(string command, CancellationToken cancellationToken)
      => Channel?.ReceiveMessageAsync(command, cancellationToken) ?? throw new InvalidOperationException("Channel is null.");
    public Task<T> ReceiveFromHostAsync<T>(string command, CancellationToken cancellationToken)
      => Channel?.ReceiveMessageAsync<T>(command, cancellationToken) ?? throw new InvalidOperationException("Channel is null.");
    public Task<IMessage> ReceiveFromHostAsync(CancellationToken cancellationToken)
      => Channel?.ReceiveMessageAsync(cancellationToken) ?? throw new InvalidOperationException("Channel is null.");
  }
}
