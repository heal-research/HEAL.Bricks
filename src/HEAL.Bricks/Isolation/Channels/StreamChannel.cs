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
using System.Text;
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

      StringBuilder json = new(JsonSerializer.Serialize(message, message.GetType()));
      using StreamWriter writer = new(outputStream!, leaveOpen: true);
      await writer.WriteLineAsync(json, cancellationToken).ConfigureAwait(false);
    }

    public override async Task<IMessage> ReceiveMessageAsync(CancellationToken cancellationToken = default) {
      Guard.Disposal(ObjectIsDisposed);
      Guard.Operation((inputStream != null) && inputStream.CanRead);

      using StreamReader reader = new(inputStream!, leaveOpen: true);
      string json = await reader.ReadLineAsync().WaitAsync(cancellationToken).ConfigureAwait(false) ?? string.Empty;
      return JsonSerializer.Deserialize<Message>(json) ?? throw new InvalidOperationException("Deserialized message is null");
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
