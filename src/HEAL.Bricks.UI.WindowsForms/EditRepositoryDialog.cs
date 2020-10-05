#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace HEAL.Bricks.UI.WindowsForms {
  public partial class EditRepositoryDialog : Form {
    public string Repository => repositoryTextBox.Text.Trim();
    public string Username => usernameTextBox.Text;
    public string Password => passwordTextBox.Text;

    public EditRepositoryDialog() {
      InitializeComponent();
    }
    public EditRepositoryDialog(string repository, string username, string password) : this() {
      repositoryTextBox.Text = repository;
      usernameTextBox.Text = username;
      passwordTextBox.Text = password;
      Text = "Edit Package Source";
    }
  }
}
