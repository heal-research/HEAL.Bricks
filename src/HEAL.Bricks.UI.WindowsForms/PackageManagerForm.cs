#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HEAL.Bricks.UI.WindowsForms {
  public partial class PackageManagerForm : Form {
    private readonly IPackageManager? packMan;
    private int searchSkipPackages = 0;
    private readonly int searchTakePackages = 10;
    private CancellationTokenSource? searchCTS;
    private readonly SemaphoreSlim searchSemaphore = new(1, 1);

    public PackageManagerForm() {
      InitializeComponent();
    }
    public PackageManagerForm(IPackageManager? packageManager) : this() {
      packMan = packageManager;
    }

    private void EnableDisableControls() {
      installButton.Enabled = browsePackagesListView.SelectedItems.Count > 0;
      removeButton.Enabled = installedPackagesListView.SelectedItems.Count > 0;
      updateButton.Enabled = updatePackagesListView.SelectedItems.Count > 0;
    }

    private void LoadInstalledPackages() {
      if (packMan != null) {
        installedPackagesListView.Items.Clear();
        foreach (LocalPackageInfo package in packMan.InstalledPackages) {
          installedPackagesListView.Items.Add(new ListViewItem(new[] {
            package.Id,
            package.Version.ToString(),
            package.Status.ToString()
          }) {
            Tag = package
          });
        }
        EnableDisableControls();
      }
    }

    private async Task LoadRemotePackagesAsync(CancellationToken cancellationToken) {
      if (packMan != null) {
        Cursor = Cursors.AppStarting;
        showMoreButton.Enabled = false;
        try {
          var searchResults = await packMan.SearchRemotePackagesAsync(searchTextBox.Text, searchSkipPackages, searchTakePackages, includePreReleases.Checked, cancellationToken);
          foreach (var (Repository, Package) in searchResults) {
            browsePackagesListView.Items.Add(new ListViewItem(new[] {
              Package.Id,
              Package.Version.ToString(),
              Repository
            }) {
              Tag = Package
            });
          }
          searchSkipPackages += searchResults.Count();
        }
        finally {
          EnableDisableControls();
          showMoreButton.Enabled = true;
          Cursor = Cursors.Default;
        }
      }
    }

    private async Task LoadUpdatesAsync() {
      if (packMan != null) {
        updatePackagesListView.Items.Clear();
        Cursor = Cursors.AppStarting;
        try {
          foreach (RemotePackageInfo package in await packMan.GetPackageUpdatesAsync(includePreReleases.Checked)) {
            updatePackagesListView.Items.Add(new ListViewItem(new[] {
              package.Id,
              packMan.InstalledPackages.Where(x => x.Id == package.Id).OrderByDescending(x => x.Version).FirstOrDefault()?.Version.ToString(),
              package.Version.ToString()
            }) {
              Tag = package
            });
          }
        }
        finally {
          updatesTabPage.Text = $"Updates{(updatePackagesListView.Items.Count > 0 ? $" ({updatePackagesListView.Items.Count})" : "")}";
          EnableDisableControls();
          Cursor = Cursors.Default;
        }
      }
    }

    private async Task InstallPackagesAsync(IEnumerable<RemotePackageInfo> packages) {
      if (packMan == null) throw new InvalidOperationException("Package manager is null.");
      Cursor = Cursors.AppStarting;
      await packMan.InstallRemotePackagesAsync(packages, installMissingDependencies: true);
      LoadInstalledPackages();
      EnableDisableControls();
      Cursor = Cursors.Default;
      await LoadUpdatesAsync();
    }

    private async Task RemovePackagesAsync(IEnumerable<LocalPackageInfo> packages) {
      if (packMan == null) throw new InvalidOperationException("Package manager is null.");
      Cursor = Cursors.WaitCursor;
      packMan?.RemoveInstalledPackages(packages);
      LoadInstalledPackages();
      EnableDisableControls();
      Cursor = Cursors.Default;
      await LoadUpdatesAsync();
    }

    private async void PrepareForm(object sender, EventArgs e) {
      LoadInstalledPackages();
      await LoadUpdatesAsync();
    }
    private async void InstallPackagesOnClick(object sender, EventArgs e) {
      List<RemotePackageInfo> packages = new();
      foreach (ListViewItem item in browsePackagesListView.SelectedItems) {
        packages.Add((RemotePackageInfo)item.Tag);
      }
      await InstallPackagesAsync(packages);
    }

    private async void UpdatePackagesOnClick(object sender, EventArgs e) {
      List<RemotePackageInfo> packages = new();
      foreach (ListViewItem item in updatePackagesListView.SelectedItems) {
        packages.Add((RemotePackageInfo)item.Tag);
      }
      await InstallPackagesAsync(packages);
    }

    private async void RemovePackagesOnClick(object sender, EventArgs e) {
      List<LocalPackageInfo> packages = new();
      foreach (ListViewItem item in installedPackagesListView.SelectedItems) {
        packages.Add((LocalPackageInfo)item.Tag);
      }
      await RemovePackagesAsync(packages);
    }

    private void UpdateControlsOnSelectedIndexChanged(object sender, EventArgs e) {
      EnableDisableControls();
      detailsTextBox.Text = string.Empty;
      if (installedPackagesListView.Visible && (installedPackagesListView.SelectedItems.Count == 1)) {
        LocalPackageInfo package = (LocalPackageInfo)installedPackagesListView.SelectedItems[0].Tag;
        detailsTextBox.Text = package.ToStringWithDependencies();
      } else if (browsePackagesListView.Visible && (browsePackagesListView.SelectedItems.Count == 1)) {
        RemotePackageInfo package = (RemotePackageInfo)browsePackagesListView.SelectedItems[0].Tag;
        detailsTextBox.Text = package.ToStringWithDependencies();
      } else if (updatePackagesListView.Visible && (updatePackagesListView.SelectedItems.Count == 1)) {
        RemotePackageInfo package = (RemotePackageInfo)updatePackagesListView.SelectedItems[0].Tag;
        detailsTextBox.Text = package.ToStringWithDependencies();
      }
    }

    private async void ReloadRemotePackages(object sender, EventArgs e) {
      searchCTS?.Cancel();
      using CancellationTokenSource cts = new();
      try {
        searchCTS = cts;
        await searchSemaphore.WaitAsync();
        browsePackagesListView.Items.Clear();
        searchSkipPackages = 0;
        await LoadRemotePackagesAsync(cts.Token);
      }
      catch { }
      finally {
        searchSemaphore.Release();
        if (searchCTS == cts) searchCTS = null;
      }
    }

    private async void LoadMoreRemotePackages(object sender, EventArgs e) {
      Cursor = Cursors.AppStarting;
      showMoreButton.Enabled = false;
      await LoadRemotePackagesAsync(default);
      showMoreButton.Enabled = true;
      Cursor = Cursors.Default;
    }
  }
}
