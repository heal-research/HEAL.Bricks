#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HEAL.Bricks.UI.WindowsForms {
  public partial class StarterForm : Form {
    private BricksOptions options;
    private IApplicationManager? appMan;

    public string DefaultApplicationImageKey { get; set; } = "Package";
    public ImageList SmallImageList => smallImageList;
    public ImageList LargeImageList => largeImageList;
    
    public StarterForm() : this(BricksOptions.Default) { }
    public StarterForm(BricksOptions options) {
      InitializeComponent();
      this.options = options;
    }

    private void EnableDisableControls() {
      startStopButton.Enabled = applicationsListView.SelectedItems.Count != 0;
      packageManagerButton.Enabled = appMan?.RunningRunnables.Count() == 0;
      settingsButton.Enabled = appMan?.RunningRunnables.Count() == 0;
      reloadButton.Enabled = appMan?.RunningRunnables.Count() == 0;

      if (startStopButton.Enabled) {
        RunnableInfo runnableInfo = (RunnableInfo)applicationsListView.SelectedItems[0].Tag;
        if (appMan?.RunningRunnables.Where(x => x.RunnableInfo == runnableInfo).Any() ?? false) {
          startStopButton.Text = "&Stop";
          startStopButton.ImageKey = "Stop";
        } else {
          startStopButton.Text = "&Start";
          startStopButton.ImageKey = "Play";
        }
      }
    }

    private async Task StartApplicationAsync(RunnableInfo runnableInfo) {
      RunningRunnableInfo? runningRunnableInfo = await (appMan?.RunAsync(runnableInfo) ?? throw new InvalidOperationException("Application manager is null."));
      EnableDisableControls();
      await (runningRunnableInfo?.Terminated ?? Task.CompletedTask);
      EnableDisableControls();
    }

    private async void LoadApplicationsOnLoad(object sender, EventArgs e) {
      await LoadApplicationsAsync();
    }

    private async void StartStopApplicationOnClick(object sender, EventArgs e) {
      IEnumerable<RunningRunnableInfo> runningRunnables = appMan?.RunningRunnables ?? Enumerable.Empty<RunningRunnableInfo>();
      if (applicationsListView.SelectedItems.Count != 0) {
        RunnableInfo runnableInfo = (RunnableInfo)applicationsListView.SelectedItems[0].Tag;
        if (runningRunnables.Where(x => x.RunnableInfo == runnableInfo).Any()) {
          foreach (CancellationTokenSource cts in runningRunnables.Where(x => x.RunnableInfo == runnableInfo).Select(x => x.CancellationTokenSource).ToArray()) {
            cts.Cancel();
          }
        } else {
          await StartApplicationAsync(runnableInfo);
        }
      }
    }

    private async void StartApplicationOnItemActivate(object sender, EventArgs e) {
      if (applicationsListView.SelectedItems.Count != 0) {
        await StartApplicationAsync((RunnableInfo)applicationsListView.SelectedItems[0].Tag);
      }
    }

    private async void LoadApplicationsOnClick(object sender, EventArgs e) {
      await LoadApplicationsAsync();
    }

    private async void ShowOptionsDialogOnClick(object sender, EventArgs e) {
      var dialog = new OptionsForm(options);
      if (dialog.ShowDialog(this) == DialogResult.OK) {
        options = dialog.Options;
        await LoadApplicationsAsync();
      }
    }

    private async void ShowPackageManagerOnClick(object sender, EventArgs e) {
      var dialog = new PackageManagerForm(appMan?.PackageManager);
      dialog.ShowDialog(this);
      await LoadApplicationsAsync();
    }

    private void EnableDisableControlsOnSelectedIndexChanged(object sender, EventArgs e) {
      EnableDisableControls();
    }

    private async Task LoadApplicationsAsync() {
      appMan = await ApplicationManager.CreateAsync(options);

      applicationsListView.Items.Clear();
      foreach (RunnableInfo runnable in appMan.InstalledRunnables) {
        applicationsListView.Items.Add(new ListViewItem {
          Text = runnable.ToString(),
          ImageKey = DefaultApplicationImageKey,
          Tag = runnable,
          ToolTipText = runnable.Description
        });
      }
      EnableDisableControls();
      if (applicationsListView.Items.Count > 0) applicationsListView.Items[0].Selected = true;
    }
  }
}
