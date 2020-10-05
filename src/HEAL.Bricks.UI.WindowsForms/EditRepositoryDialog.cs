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

    public EditRepositoryDialog() {
      InitializeComponent();
    }
    public EditRepositoryDialog(string repository) : this() {
      repositoryTextBox.Text = repository;
      Text = "Edit Package Source";
    }
  }
}
