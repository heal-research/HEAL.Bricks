using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    static async Task Main(string[] args) {
//      Application.SetHighDpiMode(HighDpiMode.SystemAware);
      System.Windows.Forms.Application.EnableVisualStyles();
      System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

      using (IChannel? channel = ProcessChannel.CreateFromCLIArguments(args)) {
        if (channel != null) {
          //System.Diagnostics.Debugger.Launch();
          await MessageHandler.Factory.ClientMessageHandler().ReceiveMessagesAsync(channel);
          return;
        }
      }

      BricksOptions options = BricksOptions.Default;
      StarterForm starterForm = new(options) {
        Text = "Choose your application",
        DefaultApplicationImageKey = "Cloud"
      };
      starterForm.LargeImageList.Images.Add("Cloud", FeatherIconsLarge.Cloud);

      System.Windows.Forms.Application.Run(starterForm);
    }
  }
}
