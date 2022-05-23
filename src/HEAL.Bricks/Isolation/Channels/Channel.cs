#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;

namespace HEAL.Bricks {
  public abstract class Channel : IChannel {

    public abstract void Open(out Task channelTerminated, CancellationToken cancellationToken = default);
    public virtual void Close() {
      Dispose(disposing: true);
    }

    public async Task SendMessageAsync(string command, CancellationToken cancellationToken = default) => await SendMessageAsync(Message.Create(command), cancellationToken);
    public async Task SendMessageAsync<T>(string command, T payload, CancellationToken cancellationToken = default) => await SendMessageAsync(Message.Create(command, payload), cancellationToken);
    public abstract Task SendMessageAsync(IMessage message, CancellationToken cancellationToken = default);

    public async Task ReceiveMessageAsync(string command, CancellationToken cancellationToken = default) {
      Guard.Argument(command, nameof(command)).NotNull().NotEmpty().NotWhiteSpace();

      IMessage message = await ReceiveMessageAsync(cancellationToken);
      Guard.Operation(command.Equals(message.Command), $"Wrong message received (expected command '{command}', received command '{message.Command}').");
    }
    public async Task<T> ReceiveMessageAsync<T>(string command, CancellationToken cancellationToken = default) {
      Guard.Argument(command, nameof(command)).NotNull().NotEmpty().NotWhiteSpace();

      IMessage message = await ReceiveMessageAsync(cancellationToken);
      Guard.Operation(command.Equals(message.Command), $"Wrong message received (expected command '{command}', received command '{message.Command}').");
      return message.DeserializePayload<T>();
    }
    public abstract Task<IMessage> ReceiveMessageAsync(CancellationToken cancellationToken = default);

    protected virtual void DisposeMembers() { }

    #region Dispose
    protected bool ObjectIsDisposed { get; private set; } = false;
    protected virtual void Dispose(bool disposing) {
      if (!ObjectIsDisposed) {
        if (disposing) {
          DisposeMembers();
        }
        ObjectIsDisposed = true;
      }
    }
    void IDisposable.Dispose() {
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
    #endregion
  }
}
