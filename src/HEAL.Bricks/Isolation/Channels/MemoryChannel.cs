#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  public class MemoryChannel : IChannel {
    private Channel<IMessage> channel = null;
    protected ChannelReader<IMessage> input = null;
    protected ChannelWriter<IMessage> output = null;
    private readonly Action<MemoryChannel, CancellationToken> clientCode;
    private CancellationTokenSource clientCTS;

    public MemoryChannel(Action<MemoryChannel, CancellationToken> clientCode) {
      Guard.Argument(clientCode, nameof(clientCode)).NotNull();
      this.clientCode = clientCode;
    }
    protected MemoryChannel(ChannelReader<IMessage> input) {
      // used to create a new channel on the client-side
      channel = Channel.CreateUnbounded<IMessage>(new UnboundedChannelOptions {
        SingleReader = true,
        SingleWriter = true
      });
      output = channel.Writer;
      this.input = input;
    }

    public virtual void Open(out Task channelTerminated, CancellationToken cancellationToken = default) {
      Guard.Disposal(ObjectIsDisposed);
      Guard.Operation((channel == null) && (input == null) && (output == null));

      channel = Channel.CreateUnbounded<IMessage>(new UnboundedChannelOptions {
        SingleReader = true,
        SingleWriter = true
      });
      output = channel.Writer;
      MemoryChannel client = new MemoryChannel(channel.Reader);
      input = client.channel.Reader;

      clientCTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      CancellationToken clientToken = clientCTS.Token;

      channelTerminated = Task.Run(() => { clientCode(client, clientToken); }, clientToken);
    }
    public void Close() {
      Guard.Operation(((channel != null) && (output != null)) || ObjectIsDisposed);
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    public async Task SendMessageAsync(IMessage message, CancellationToken cancellationToken = default) {
      Guard.Disposal(ObjectIsDisposed);
      Guard.Argument(message, nameof(message)).NotNull();
      Guard.Operation(output != null);

      await output.WriteAsync(message, cancellationToken);
    }

    public Task<IMessage> ReceiveMessageAsync(CancellationToken cancellationToken = default) => ReceiveMessageAsync<IMessage>(cancellationToken);
    public async Task<T> ReceiveMessageAsync<T>(CancellationToken cancellationToken = default) where T : class, IMessage {
      Guard.Disposal(ObjectIsDisposed);
      Guard.Operation(input != null);

      return await input.ReadAsync(cancellationToken).ConfigureAwait(false) as T;
    }

    protected virtual void DisposeMembers() {
      clientCTS?.Cancel();
      clientCTS?.Dispose();
      output?.Complete();
      output = null;
      input = null;
      channel = null;
    }

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
