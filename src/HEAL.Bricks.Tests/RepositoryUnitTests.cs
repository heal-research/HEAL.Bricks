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
  public class RepositoryUnitTests {
    [Fact]
    public void Constructor_WithValidSource_CreatesValidInstance() {
      string source = "source";

      Repository repository = new(source);
      Assert.Equal(source, repository.Source);
    }

    [Fact]
    public void Constructor_WithSourceIsNull_ThrowsArgumentNullException() {
      var e = Assert.Throws<ArgumentNullException>(() => new Repository(null!));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithSourceIsEmpty_ThrowsArgumentException(string source) {
      var e = Assert.Throws<ArgumentException>(() => new Repository(source));
      Assert.False(string.IsNullOrEmpty(e.Message));
      Assert.False(string.IsNullOrEmpty(e.ParamName));
    }

    [Fact]
    public void Equals_WithSameSource_ReturnsTrue() {
      Repository rep = new("source");
      Repository otherRep = new("source");

      bool equal = rep.Equals(otherRep);
      Assert.True(equal);
    }

    [Fact]
    public void Equals_WithDifferentSource_ReturnsFalse() {
      Repository rep = new("source");
      Repository otherRep = new("other_source");

      bool equal = rep.Equals(otherRep);
      Assert.False(equal);
    }
  }
}
