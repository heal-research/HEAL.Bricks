#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Xunit;
using System;

namespace HEAL.Bricks.Tests {
  [Trait("Category", "Unit")]
  public class MemoryChannelUnitTests {
    [Fact]
    public void Constructor_WithParameterIsNull_ThrowsArgumentNullException() {
      var e = Assert.Throws<ArgumentNullException>(() => new MemoryChannel(null));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }
  }
}
