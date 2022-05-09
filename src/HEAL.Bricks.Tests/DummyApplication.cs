#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Threading.Tasks;
using System.Threading;

namespace HEAL.Bricks.Tests {
  public class DummyApplication : Application {
    public override string Name => "DummyApplication";
    public override string Description => "This is a dummy application for testing.";
    public override ApplicationKind Kind => ApplicationKind.Console;
    public override Task StartAsync(string[] args, CancellationToken cancellationToken = default) {
      throw new NotImplementedException();
    }
  }
}
