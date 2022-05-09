#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System.Threading;
using System.Threading.Tasks;

namespace HEAL.Bricks {
  public interface IFunction<TInput, TOutput> : IRunnable {
    Task<TOutput> ExecAsync(TInput input, CancellationToken cancellationToken);
  }
}
