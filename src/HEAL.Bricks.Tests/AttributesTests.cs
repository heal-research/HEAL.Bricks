#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using HEAL.Bricks.Attributes;

namespace HEAL.Bricks.Tests {
  [TestClass]
  public class AttributesTests {
    #region Helpers
    private void AssertArgumentNullException(Action action, string expectedParamName) {
      Assert.AreEqual(expectedParamName, Assert.ThrowsException<ArgumentNullException>(action).ParamName);
    }
    private void AssertArgumentException(Action action, string expectedParamName) {
      Assert.AreEqual(expectedParamName, Assert.ThrowsException<ArgumentException>(action).ParamName);
    }
    #endregion

    [TestMethod]
    public void ApplicationAttributeTest() {
      ApplicationAttribute a;
      a = new ApplicationAttribute("name");
      Assert.AreEqual("name", a.Name);
      Assert.AreEqual(string.Empty, a.Description);

      a = new ApplicationAttribute("name", "desc");
      Assert.AreEqual("name", a.Name);
      Assert.AreEqual("desc", a.Description);

      AssertArgumentNullException(() => new ApplicationAttribute(null), "name");
      AssertArgumentException(() => new ApplicationAttribute(""), "name");
      AssertArgumentException(() => new ApplicationAttribute("   "), "name");
      AssertArgumentNullException(() => new ApplicationAttribute(null, "desc"), "name");
      AssertArgumentException(() => new ApplicationAttribute("", "desc"), "name");
      AssertArgumentException(() => new ApplicationAttribute("   ", "desc"), "name");
      AssertArgumentNullException(() => new ApplicationAttribute(null, null), "name");
      AssertArgumentException(() => new ApplicationAttribute("", null), "name");
      AssertArgumentException(() => new ApplicationAttribute("   ", null), "name");
      AssertArgumentNullException(() => new ApplicationAttribute("name", null), "description");
    }

    [TestMethod]
    public void ContactInformationAttributeTest() {
      ContactInformationAttribute a;
      a = new ContactInformationAttribute("name", "email");
      Assert.AreEqual("name", a.Name);
      Assert.AreEqual("email", a.EMail);

      AssertArgumentNullException(() => new ContactInformationAttribute(null, "email"), "name");
      AssertArgumentException(() => new ContactInformationAttribute("", "email"), "name");
      AssertArgumentException(() => new ContactInformationAttribute("   ", "email"), "name");
      AssertArgumentNullException(() => new ContactInformationAttribute(null, null), "name");
      AssertArgumentException(() => new ContactInformationAttribute("", null), "name");
      AssertArgumentException(() => new ContactInformationAttribute("   ", null), "name");
      AssertArgumentNullException(() => new ContactInformationAttribute("name", null), "email");
      AssertArgumentNullException(() => new ContactInformationAttribute(null, ""), "name");
      AssertArgumentException(() => new ContactInformationAttribute("", ""), "name");
      AssertArgumentException(() => new ContactInformationAttribute("   ", ""), "name");
      AssertArgumentException(() => new ContactInformationAttribute("name", ""), "email");
      AssertArgumentNullException(() => new ContactInformationAttribute(null, "   "), "name");
      AssertArgumentException(() => new ContactInformationAttribute("", "   "), "name");
      AssertArgumentException(() => new ContactInformationAttribute("   ", "   "), "name");
      AssertArgumentException(() => new ContactInformationAttribute("name", "   "), "email");
    }

    [TestMethod]
    public void PluginAttributeTest() {
      PluginAttribute a;
      a = new PluginAttribute("name", "1.2.3.4");
      Assert.AreEqual("name", a.Name);
      Assert.AreEqual(string.Empty, a.Description);
      Assert.AreEqual(new Version(1, 2, 3, 4), a.Version);

      a = new PluginAttribute("name", "desc", "1.2.3.4");
      Assert.AreEqual("name", a.Name);
      Assert.AreEqual("desc", a.Description);
      Assert.AreEqual(new Version(1, 2, 3, 4), a.Version);

      AssertArgumentNullException(() => new ApplicationAttribute(null), "name");
      AssertArgumentException(() => new ApplicationAttribute(""), "name");
      AssertArgumentException(() => new ApplicationAttribute("   "), "name");
      AssertArgumentNullException(() => new ApplicationAttribute(null, "desc"), "name");
      AssertArgumentException(() => new ApplicationAttribute("", "desc"), "name");
      AssertArgumentException(() => new ApplicationAttribute("   ", "desc"), "name");
      AssertArgumentNullException(() => new ApplicationAttribute(null, null), "name");
      AssertArgumentException(() => new ApplicationAttribute("", null), "name");
      AssertArgumentException(() => new ApplicationAttribute("   ", null), "name");
      AssertArgumentNullException(() => new ApplicationAttribute("name", null), "description");
    }
  }
}
