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
    private IApplicationManager appMan;
    private readonly Dictionary<ApplicationInfo, List<CancellationTokenSource>> appCTS = new();
    private int runningApplications = 0;

    public string DefaultApplicationImageKey { get; set; } = "Package";
    public ImageList SmallImageList => this.smallImageList;
    public ImageList LargeImageList => this.largeImageList;
    
    public StarterForm() {
      InitializeComponent();
      options = new BricksOptions() {
        DefaultIsolation = Isolation.AnonymousPipes
      };
    }
    public StarterForm(BricksOptions options) : this() {
      this.options = options;
    }

    private void EnableDisableControls() {
      startStopButton.Enabled = applicationsListView.SelectedItems.Count != 0;
      packageManagerButton.Enabled = runningApplications == 0;
      settingsButton.Enabled = runningApplications == 0;
      reloadButton.Enabled = runningApplications == 0;

      if (startStopButton.Enabled) {
        ApplicationInfo appInfo = applicationsListView.SelectedItems[0].Tag as ApplicationInfo;
        if (appCTS[appInfo].Count == 0) {
          startStopButton.Text = "&Start";
          startStopButton.ImageKey = "Play";
        }
        else {
          startStopButton.Text = "&Stop";
          startStopButton.ImageKey = "Stop";
        }
      }
    }

    private async Task StartApplicationAsync(ApplicationInfo appInfo) {
      using CancellationTokenSource cts = new();
      appCTS[appInfo].Add(cts);
      runningApplications++;
      EnableDisableControls();
      try {
        await appMan.RunAsync(appInfo, cancellationToken: cts.Token);
      }
      catch (Exception) {
        if (!cts.IsCancellationRequested) throw;
      }
      finally {
        appCTS[appInfo].Remove(cts);
        runningApplications--;
        EnableDisableControls();
      }
    }

    private async void LoadApplicationsOnLoad(object sender, EventArgs e) {
      await LoadApplicationsAsync();
    }

    private async void StartStopApplicationOnClick(object sender, EventArgs e) {
      if (applicationsListView.SelectedItems.Count != 0) {
        ApplicationInfo appInfo = applicationsListView.SelectedItems[0].Tag as ApplicationInfo;
        if (appCTS[appInfo].Count > 0) {
          foreach (CancellationTokenSource cts in appCTS[appInfo].ToArray()) {
            cts.Cancel();
          }
        } else {
          await StartApplicationAsync(appInfo);
        }
      }
    }

    private async void StartApplicationOnItemActivate(object sender, EventArgs e) {
      if (applicationsListView.SelectedItems.Count != 0) {
        await StartApplicationAsync(applicationsListView.SelectedItems[0].Tag as ApplicationInfo);
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
      var dialog = new PackageManagerForm(appMan.PackageManager);
      dialog.ShowDialog(this);
      await LoadApplicationsAsync();
    }

    private void EnableDisableControlsOnSelectedIndexChanged(object sender, EventArgs e) {
      EnableDisableControls();
    }

    private async Task LoadApplicationsAsync() {
      appMan = await ApplicationManager.CreateAsync(options);

      applicationsListView.Items.Clear();
      appCTS.Clear();
      foreach (ApplicationInfo application in appMan.InstalledApplications) {
        applicationsListView.Items.Add(new ListViewItem {
          Text = application.ToString(),
          ImageKey = DefaultApplicationImageKey,
          Tag = application,
          ToolTipText = application.Description
        });
        appCTS.Add(application, new List<CancellationTokenSource>());
      }
      EnableDisableControls();
      if (applicationsListView.Items.Count > 0) applicationsListView.Items[0].Selected = true;
    }
  }
}
