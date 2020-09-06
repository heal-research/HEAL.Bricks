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
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  public abstract class StreamChannel : IChannel {
    protected Stream inputStream = null, outputStream = null;

    public virtual void Open(out Task channelTerminated, CancellationToken cancellationToken = default) {
      Guard.Disposal(ObjectIsDisposed);
      Guard.Operation((inputStream == null) && (outputStream == null));
      channelTerminated = null;
    }
    public void Close() {
      Guard.Operation(((inputStream != null) && (outputStream != null)) || ObjectIsDisposed);
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    public async Task SendMessageAsync(IMessage message, CancellationToken cancellationToken = default) {
      Guard.Disposal(ObjectIsDisposed);
      Guard.Argument(message, nameof(message)).NotNull();
      Guard.Operation((outputStream != null) && outputStream.CanWrite);

      await Task.Run(() => {
        cancellationToken.ThrowIfCancellationRequested();
        IFormatter serializer = new BinaryFormatter();
        serializer.Serialize(outputStream, message);
        outputStream.Flush();
      }, cancellationToken).ConfigureAwait(false);
    }
    public Task<IMessage> ReceiveMessageAsync(CancellationToken cancellationToken = default) => ReceiveMessageAsync<IMessage>(cancellationToken);
    public async Task<T> ReceiveMessageAsync<T>(CancellationToken cancellationToken = default) where T : class, IMessage {
      Guard.Disposal(ObjectIsDisposed);
      Guard.Operation((inputStream != null) && inputStream.CanRead);

      return await Task.Run<T>(() => {
        cancellationToken.ThrowIfCancellationRequested();
        IFormatter serializer = new BinaryFormatter();
        object message = serializer.Deserialize(inputStream);
        T t = (T)message;
        return t;
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
