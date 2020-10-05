using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using HEAL.Bricks;
using HEAL.Bricks.UI.WindowsForms;

namespace WinFormsTestApp {
  static class Program {
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args) {
//      Application.SetHighDpiMode(HighDpiMode.SystemAware);
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      using (IChannel channel = ProcessChannel.CreateFromCLIArguments(args)) {
        if (channel != null) {
          Runner.ReceiveAndExecuteAsync(channel).Wait();
          return;
        }
      }

      Settings settings = new Settings {
        Isolation = Isolation.AnonymousPipes
      };
      settings.Repositories.Add(@"C:\# Daten\NuGet");

      StarterForm starterForm = new StarterForm(settings) {
        Text = "Choose your application",
        DefaultApplicationImageKey = "Cloud"
      };
      starterForm.LargeImageList.Images.Add("Cloud", FeatherIconsLarge.Cloud);

      Application.Run(starterForm);
    }
  }
}
