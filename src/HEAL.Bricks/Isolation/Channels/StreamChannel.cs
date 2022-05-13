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
  public abstract class StreamChannel : IChannel {
    protected Stream? inputStream = null, outputStream = null;

    public virtual void Open(out Task channelTerminated, CancellationToken cancellationToken = default) {
      Guard.Disposal(ObjectIsDisposed);
      Guard.Operation((inputStream == null) && (outputStream == null));
      channelTerminated = Task.CompletedTask;
    }
    public void Close() {
      Guard.Operation(((inputStream != null) && (outputStream != null)) || ObjectIsDisposed);
      Dispose(disposing: true);
    }

    public async Task SendMessageAsync<T>(T message, CancellationToken cancellationToken = default) where T : class, IMessage {
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
    public async Task<T> ReceiveMessageAsync<T>(CancellationToken cancellationToken = default) where T : class, IMessage {
      Guard.Disposal(ObjectIsDisposed);
      Guard.Operation((inputStream != null) && inputStream.CanRead);

      return await Task.Run<T>(() => {
        cancellationToken.ThrowIfCancellationRequested();
        IFormatter formatter = new BinaryFormatter();
        T message = (T)formatter.Deserialize(inputStream ?? throw new InvalidOperationException("Input stream is null"));
        //T? message = JsonSerializer.Deserialize<T>(inputStream ?? throw new InvalidOperationException("Input stream is null"));
        return message ?? throw new InvalidOperationException("Deserialized message is null");
      }, cancellationToken).ConfigureAwait(false);
    }

    protected virtual void DisposeMembers() {
      outputStream?.Dispose();
      outputStream = null;
      inputStream?.Dispose();
      inputStream = null;
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
