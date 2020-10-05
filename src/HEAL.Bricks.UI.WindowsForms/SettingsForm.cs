#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using Dawn;
using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HEAL.Bricks.UI.WindowsForms {
  public partial class SettingsForm : Form {
    public Settings Settings => GetSettings();

    public SettingsForm() {
      InitializeComponent();
    }
    public SettingsForm(ISettings settings) : this() {
      Guard.Argument(settings, nameof(settings)).NotNull();
      appPathTextBox.Text = settings.AppPath;
      packagesPathTextBox.Text = settings.PackagesPath;
      selectPackagesPathButton.Tag = packagesPathTextBox;
      packagesCachePathTextBox.Text = settings.PackagesCachePath;
      selectPackagesCachePathButton.Tag = packagesCachePathTextBox;
      foreach (string repository in settings.Repositories) {
        repositoriesListBox.Items.Add(repository);
      }
      isolationComboBox.DataSource = Enum.GetValues(typeof(Isolation));
      isolationComboBox.SelectedItem = settings.Isolation;
      dotnetCommandTextBox.Text = settings.DotnetCommand;
      starterAssemblyTextBox.Text = settings.StarterAssembly;
      selectStarterAssemblyButton.Tag = starterAssemblyTextBox;
      dockerCommandTextBox.Text = settings.DockerCommand;
      dockerImageTextBox.Text = settings.DockerImage;
      useWindowsContainerCheckBox.Checked = settings.UseWindowsContainer;

      EnableDisableControls();
    }

    private Settings GetSettings() {
      Settings settings = new Settings {
        PackagesPath = packagesPathTextBox.Text.Trim(),
        PackagesCachePath = packagesCachePathTextBox.Text.Trim(),
        Isolation = (Isolation)isolationComboBox.SelectedItem,
        DotnetCommand = dotnetCommandTextBox.Text.Trim(),
        StarterAssembly = starterAssemblyTextBox.Text.Trim(),
        DockerCommand = dockerCommandTextBox.Text.Trim(),
        DockerImage = dockerImageTextBox.Text.Trim(),
        UseWindowsContainer = useWindowsContainerCheckBox.Checked
      };
      settings.Repositories.Clear();
      settings.Repositories.AddRange(repositoriesListBox.Items.Cast<string>());
      return settings;
    }

    private void EnableDisableControls() {
      int index = repositoriesListBox.SelectedIndex;
      removeRepositoryButton.Enabled = index != -1;
      editRepositoryButton.Enabled = index != -1;
      moveRepositoryUpButton.Enabled = index > 0;
      moveRepositoryDownButton.Enabled = (index != -1) && (index < repositoriesListBox.Items.Count - 1);
    }

    private void SelectFolderPathOnClick(object sender, EventArgs e) {
      if ((sender as Control)?.Tag is TextBox textBox) {
        folderBrowserDialog.SelectedPath = textBox.Text;
        if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK) {
          textBox.Text = folderBrowserDialog.SelectedPath;
        }
      }
    }

    private void SelectAssemblyPathOnClick(object sender, EventArgs e) {
      if ((sender as Control)?.Tag is TextBox textBox) {
        openFileDialog.InitialDirectory = appPathTextBox.Text;
        openFileDialog.FileName = textBox.Text;
        if (openFileDialog.ShowDialog(this) == DialogResult.OK) {
          textBox.Text = Path.GetFileName(openFileDialog.FileName);
        }
      }
    }

    private void TextBoxNotNullOrEmptyOrWhiteSpaceOnValidating(object sender, CancelEventArgs e) {
      TextBox textBox = sender as TextBox;
      if (string.IsNullOrWhiteSpace(textBox.Text)) {
        errorProvider.SetError(textBox, "Must not be empty.");
        e.Cancel = true;
      }
    }

    private void RemoveErrorOnValidated(object sender, EventArgs e) {
      Control control = sender as Control;
      errorProvider.SetError(control, string.Empty);
    }

    private void AddRepositoryOnClick(object sender, EventArgs e) {
      EditRepositoryDialog dialog = new EditRepositoryDialog();
      if (dialog.ShowDialog(this) == DialogResult.OK) {
        repositoriesListBox.Items.Add(dialog.Repository);
        EnableDisableControls();
      }
    }

    private void RemoveRepositoryOnClick(object sender, EventArgs e) {
      repositoriesListBox.Items.RemoveAt(repositoriesListBox.SelectedIndex);
    }

    private void EditRepositoryOnClick(object sender, EventArgs e) {
      EditRepositoryDialog dialog = new EditRepositoryDialog(repositoriesListBox.SelectedItem as string);
      if ((dialog.ShowDialog(this) == DialogResult.OK) && (!string.IsNullOrWhiteSpace(dialog.Repository))) {
        int index = repositoriesListBox.SelectedIndex;
        repositoriesListBox.Items.RemoveAt(index);
        repositoriesListBox.Items.Insert(index, dialog.Repository);
        repositoriesListBox.SelectedIndex = index;
      }
    }

    private void MoveRepositoryUpOnClick(object sender, EventArgs e) {
      int index = repositoriesListBox.SelectedIndex;
      object item = repositoriesListBox.SelectedItem;
      repositoriesListBox.Items.RemoveAt(index);
      repositoriesListBox.Items.Insert(index - 1, item);
      repositoriesListBox.SelectedIndex = index - 1;
    }

    private void MoveRepositoryDownOnClick(object sender, EventArgs e) {
      int index = repositoriesListBox.SelectedIndex;
      object item = repositoriesListBox.SelectedItem;
      repositoriesListBox.Items.RemoveAt(index);
      repositoriesListBox.Items.Insert(index + 1, item);
      repositoriesListBox.SelectedIndex = index + 1;
    }

    private void EnableDisableControlsOnSelectedIndexChanged(object sender, EventArgs e) {
      EnableDisableControls();
    }
  }
}
