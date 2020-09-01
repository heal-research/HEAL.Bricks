#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Threading.Tasks;
using System.Threading;

namespace HEAL.Bricks.XTests {
  public class DummyApplication : IApplication {
    public string Name => "DummyApplication";
    public string Description => "This is a dummy application for testing.";
    public ApplicationKind Kind => ApplicationKind.Console;
    public Task RunAsync(ICommandLineArgument[] args, CancellationToken cancellationToken = default) {
      throw new NotImplementedException();
    }
  }
}
