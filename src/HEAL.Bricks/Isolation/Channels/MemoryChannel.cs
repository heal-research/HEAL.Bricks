#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  public class MemoryChannel : IChannel {
    protected BlockingCollection<IMessage> inputQueue = null, outputQueue = null;
    private readonly Action<MemoryChannel> clientCode;

    public MemoryChannel(Action<MemoryChannel> clientCode) {
      Guard.Argument(clientCode, nameof(clientCode)).NotNull();
      this.clientCode = clientCode;
    }
    protected MemoryChannel() { }

    public virtual void Open() {
      Guard.Disposal(ObjectIsDisposed);
      Guard.Operation((inputQueue == null) && (outputQueue == null));

      inputQueue = new BlockingCollection<IMessage>();
      outputQueue = new BlockingCollection<IMessage>();

      MemoryChannel clientChannel = new MemoryChannel {
        inputQueue = outputQueue,
        outputQueue = inputQueue
      };
      Task.Run(() => clientCode(clientChannel));
    }
    public void Close() {
      Guard.Operation(((inputQueue != null) && (outputQueue != null)) || ObjectIsDisposed);
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    public virtual Task SendMessageAsync(IMessage message, CancellationToken cancellationToken = default) {
      Guard.Disposal(ObjectIsDisposed);
      Guard.Argument(message, nameof(message)).NotNull();
      Guard.Operation(outputQueue != null);
      outputQueue.Add(message, cancellationToken);
      return Task.CompletedTask;
    }

    public Task<IMessage> ReceiveMessageAsync(CancellationToken cancellationToken = default) => ReceiveMessageAsync<IMessage>(cancellationToken);
    public Task<T> ReceiveMessageAsync<T>(CancellationToken cancellationToken = default) where T : class, IMessage {
      Guard.Disposal(ObjectIsDisposed);
      Guard.Operation(inputQueue != null);
      return Task.FromResult(inputQueue.Take(cancellationToken) as T);
    }

    protected virtual void DisposeMembers() {
      inputQueue?.CompleteAdding();
      outputQueue?.CompleteAdding();
      inputQueue?.Dispose();
      outputQueue?.Dispose();
      inputQueue = null;
      outputQueue = null;
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
