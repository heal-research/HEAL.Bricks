#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  public abstract class StreamChannel : Channel {
    protected Stream? inputStream = null, outputStream = null;

    public override void Open(out Task channelTerminated, CancellationToken cancellationToken = default) {
      Guard.Disposal(ObjectIsDisposed);
      Guard.Operation((inputStream == null) && (outputStream == null));
      channelTerminated = Task.CompletedTask;
    }
    public override void Close() {
      Guard.Operation(((inputStream != null) && (outputStream != null)) || ObjectIsDisposed);
      base.Close();
    }

    public override async Task SendMessageAsync(IMessage message, CancellationToken cancellationToken = default) {
      Guard.Disposal(ObjectIsDisposed);
      Guard.Argument(message, nameof(message)).NotNull();
      Guard.Operation((outputStream != null) && outputStream.CanWrite);

      await Task.Run(() => {
        cancellationToken.ThrowIfCancellationRequested();
        IFormatter formatter = new BinaryFormatter();
        formatter.Serialize(outputStream ?? throw new InvalidOperationException("Output stream is null"), message);
        //JsonSerializer.Serialize(outputStream ?? throw new InvalidOperationException("Output stream is null"), message, message.GetType());
        outputStream.Flush();
      }, cancellationToken).ConfigureAwait(false);
    }

    public override async Task<IMessage> ReceiveMessageAsync(CancellationToken cancellationToken = default) {
      Guard.Disposal(ObjectIsDisposed);
      Guard.Operation((inputStream != null) && inputStream.CanRead);

      return await Task.Run<IMessage>(() => {
        cancellationToken.ThrowIfCancellationRequested();
        IFormatter formatter = new BinaryFormatter();
        IMessage message = (IMessage)formatter.Deserialize(inputStream ?? throw new InvalidOperationException("Input stream is null"));
        //T? message = JsonSerializer.Deserialize<T>(inputStream ?? throw new InvalidOperationException("Input stream is null"));
        return message ?? throw new InvalidOperationException("Deserialized message is null");
      }, cancellationToken).ConfigureAwait(false);
    }

    protected override void DisposeMembers() {
      outputStream?.Dispose();
      outputStream = null;
      inputStream?.Dispose();
      inputStream = null;
      base.DisposeMembers();
    }
  }
}
