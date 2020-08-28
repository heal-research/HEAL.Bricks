﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HEAL.Bricks;

namespace WinFormsTestApp {
  public partial class StarterForm : Form {
    private Settings settings;
    private IPackageManager pm;
    private ApplicationInfo[] applications;

    public StarterForm() {
      InitializeComponent();
    }

    private async void StarterForm_Load(object sender, EventArgs e) {
      settings = new Settings() {
        PackageTag = "HEALBricksPlugin"
      };
      settings.Repositories.Add(@"C:\# Daten\NuGet");
      Directory.CreateDirectory(settings.PackagesPath);
      Directory.CreateDirectory(settings.PackagesCachePath);
      pm = PackageManager.Create(settings);

      DiscoverApplicationsRunner discoverApplicationsRunner = new DiscoverApplicationsRunner(pm.Settings);
      applications = await discoverApplicationsRunner.GetApplicationsAsync();

      foreach (ApplicationInfo application in applications) {
        appsListView.Items.Add(new ListViewItem(application.ToString()) { Tag = application });
      }



    }

    private async void startButton_Click(object sender, EventArgs e) {
      ApplicationInfo app = appsListView.SelectedItems[0].Tag as ApplicationInfo;
      ApplicationRunner appRunner = new ApplicationRunner(settings, app);
      await appRunner.RunAsync();
    }
  }
}