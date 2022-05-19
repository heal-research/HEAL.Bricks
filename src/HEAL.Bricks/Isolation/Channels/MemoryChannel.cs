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
  public class MemoryChannel : Channel {
    private Channel<IMessage>? channel = null;
    protected ChannelReader<IMessage>? input = null;
    protected ChannelWriter<IMessage>? output = null;
    private readonly Action<MemoryChannel, CancellationToken>? clientCode;
    private CancellationTokenSource? clientCTS;

    public MemoryChannel(Action<MemoryChannel, CancellationToken> clientCode) {
      Guard.Argument(clientCode, nameof(clientCode)).NotNull();
      this.clientCode = clientCode;
    }
    protected MemoryChannel(ChannelReader<IMessage> input) {
      // used to create a new channel on the client-side
      channel = System.Threading.Channels.Channel.CreateUnbounded<IMessage>(new UnboundedChannelOptions {
        SingleReader = true,
        SingleWriter = true
      });
      output = channel.Writer;
      this.input = input;
    }

    public override void Open(out Task channelTerminated, CancellationToken cancellationToken = default) {
      Guard.Disposal(ObjectIsDisposed);
      Guard.Operation((channel == null) && (input == null) && (output == null));

      channel = System.Threading.Channels.Channel.CreateUnbounded<IMessage>(new UnboundedChannelOptions {
        SingleReader = true,
        SingleWriter = true
      });
      output = channel.Writer;
      MemoryChannel client = new(channel.Reader);
      input = client.channel?.Reader;

      clientCTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      CancellationToken clientToken = clientCTS.Token;

      channelTerminated = Task.Run(() => { clientCode?.Invoke(client, clientToken); }, clientToken);
    }
    public override void Close() {
      Guard.Operation(((channel != null) && (output != null)) || ObjectIsDisposed);
      base.Close();
    }

    public override async Task SendMessageAsync(IMessage message, CancellationToken cancellationToken = default) {
      Guard.Disposal(ObjectIsDisposed);
      Guard.Argument(message, nameof(message)).NotNull();
      Guard.Operation(output != null);

      await (output?.WriteAsync(message, cancellationToken) ?? ValueTask.CompletedTask);
    }

    public override async Task<IMessage> ReceiveMessageAsync(CancellationToken cancellationToken = default) {
      Guard.Disposal(ObjectIsDisposed);
      Guard.Operation(input != null);

      return await (input?.ReadAsync(cancellationToken).ConfigureAwait(false) ?? throw new InvalidOperationException("input is null"));
    }

    protected override void DisposeMembers() {
      clientCTS?.Cancel();
      clientCTS?.Dispose();
      output?.Complete();
      output = null;
      input = null;
      channel = null;
      base.DisposeMembers();
    }
  }
}
