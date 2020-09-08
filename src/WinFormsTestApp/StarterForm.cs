using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HEAL.Bricks;

namespace WinFormsTestApp {
  public partial class StarterForm : Form {
    private IApplicationManager am;

    public StarterForm() {
      InitializeComponent();
    }

    private async void StarterForm_Load(object sender, EventArgs e) {
      Settings settings = new Settings() {
        Isolation = Isolation.AnonymousPipes
      };
      settings.Repositories.Add(@"C:\# Daten\NuGet");
      Directory.CreateDirectory(settings.PackagesPath);
      Directory.CreateDirectory(settings.PackagesCachePath);
      am = await ApplicationManager.CreateAsync(settings);

      foreach (ApplicationInfo application in am.InstalledApplications) {
        appsListView.Items.Add(new ListViewItem(application.ToString()) { Tag = application });
      }



    }

    private async void startButton_Click(object sender, EventArgs e) {
      ApplicationInfo app = appsListView.SelectedItems[0].Tag as ApplicationInfo;
      await am.RunAsync(app);
    }
  }
}
