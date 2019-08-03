#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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

    #region ApplicationAttribute
    [TestMethod]
    public void ApplicationAttributeCtorTest() {
      var a = new ApplicationAttribute("name");
      Assert.AreEqual("name", a.Name);
      Assert.AreEqual(string.Empty, a.Description);
      a = new ApplicationAttribute("name", "desc");
      Assert.AreEqual("name", a.Name);
      Assert.AreEqual("desc", a.Description);
      // test invalid ctor params
      AssertArgumentNullException(() => new ApplicationAttribute(null), "name");
      AssertArgumentException(() => new ApplicationAttribute(""), "name");
      AssertArgumentException(() => new ApplicationAttribute("   "), "name");
      AssertArgumentNullException(() => new ApplicationAttribute("name", null), "description");
    }
    #endregion

    #region ContactInformationAttribute
    [TestMethod]
    public void ContactInformationAttributeCtorTest() {
      var a = new ContactInformationAttribute("name", "email");
      Assert.AreEqual("name", a.Name);
      Assert.AreEqual("email", a.EMail);
      // test invalid ctor params
      AssertArgumentNullException(() => new ContactInformationAttribute(null, "email"), "name");
      AssertArgumentException(() => new ContactInformationAttribute("", "email"), "name");
      AssertArgumentException(() => new ContactInformationAttribute("   ", "email"), "name");
      AssertArgumentNullException(() => new ContactInformationAttribute("name", null), "email");
      AssertArgumentException(() => new ContactInformationAttribute("name", ""), "email");
      AssertArgumentException(() => new ContactInformationAttribute("name", "   "), "email");
    }
    #endregion

    #region PluginAttribute
    [TestMethod]
    public void PluginAttributeCtorTest() {
      var a = new PluginAttribute("name", "1.2.3.4");
      Assert.AreEqual("name", a.Name);
      Assert.AreEqual(new Version(1, 2, 3, 4), a.Version);
      Assert.AreEqual(string.Empty, a.Description);
      a = new PluginAttribute("name", "1.2.3.4", "desc");
      Assert.AreEqual("name", a.Name);
      Assert.AreEqual(new Version(1, 2, 3, 4), a.Version);
      Assert.AreEqual("desc", a.Description);
      // test invalid ctor params
      AssertArgumentNullException(() => new PluginAttribute(null, "1.2.3.4"), "name");
      AssertArgumentException(() => new PluginAttribute("", "1.2.3.4"), "name");
      AssertArgumentException(() => new PluginAttribute("   ", "1.2.3.4"), "name");
      AssertArgumentNullException(() => new PluginAttribute("name", null), "version");
      AssertArgumentException(() => new PluginAttribute("name", ""), "version");
      AssertArgumentException(() => new PluginAttribute("name", "   "), "version");
      AssertArgumentException(() => new PluginAttribute("name", "1"), "version");
      AssertArgumentException(() => new PluginAttribute("name", "1.2.3.4.5"), "version");
      AssertArgumentException(() => new PluginAttribute("name", "1.2.alpha"), "version");
      AssertArgumentNullException(() => new PluginAttribute("name", "1.2.3.4", null), "description");
    }
    #endregion

    #region PluginDependencyAttribute
    [TestMethod]
    public void PluginDependencyAttributeCtorTest() {
      var a = new PluginDependencyAttribute("dependency", "1.2.3.4");
      Assert.AreEqual("dependency", a.Dependency);
      Assert.AreEqual(new Version(1, 2, 3, 4), a.Version);
      // test invalid ctor params
      AssertArgumentNullException(() => new PluginDependencyAttribute(null, "1.2.3.4"), "dependency");
      AssertArgumentException(() => new PluginDependencyAttribute("", "1.2.3.4"), "dependency");
      AssertArgumentException(() => new PluginDependencyAttribute("   ", "1.2.3.4"), "dependency");
      AssertArgumentNullException(() => new PluginDependencyAttribute("dependency", null), "version");
      AssertArgumentException(() => new PluginDependencyAttribute("dependency", ""), "version");
      AssertArgumentException(() => new PluginDependencyAttribute("dependency", "   "), "version");
      AssertArgumentException(() => new PluginDependencyAttribute("dependency", "1"), "version");
      AssertArgumentException(() => new PluginDependencyAttribute("dependency", "1.2.3.4.5"), "version");
      AssertArgumentException(() => new PluginDependencyAttribute("dependency", "1.2.alpha"), "version");
    }
    #endregion

    #region PluginFileAttribute
    [TestMethod]
    public void PluginFileAttributeCtorTest() {
      var a = new PluginFileAttribute("name");
      Assert.AreEqual("name", a.Name);
      Assert.AreEqual(PluginFileType.Assembly, a.Type);
      a = new PluginFileAttribute("name", PluginFileType.License);
      Assert.AreEqual("name", a.Name);
      Assert.AreEqual(PluginFileType.License, a.Type);
      // test invalid ctor params
      AssertArgumentNullException(() => new PluginFileAttribute(null), "name");
      AssertArgumentException(() => new PluginFileAttribute(""), "name");
      AssertArgumentException(() => new PluginFileAttribute("   "), "name");
    }
    #endregion
  }
}
