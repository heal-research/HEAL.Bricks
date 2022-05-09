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
    static readonly string settingsPath =
      Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "HEAL.Bricks.settings.json");

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args) {
//      Application.SetHighDpiMode(HighDpiMode.SystemAware);
      System.Windows.Forms.Application.EnableVisualStyles();
      System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

      using (IChannel channel = ProcessChannel.CreateFromCLIArguments(args)) {
        if (channel != null) {
          Runner.ReceiveAndExecuteAsync(channel).Wait();
          return;
        }
      }

      StringWriter writer = new StringWriter();
      Console.SetOut(writer);
      Console.WriteLine("Hello World");
      Console.SetIn(new StringReader("Message!!!"));


      BricksOptions options = BricksOptions.Default;
      //if (File.Exists(settingsPath)) {
      //  options = BricksOptions.Load<Settings>(settingsPath);
      //}
      //else {
      //  options = Settings.Default;
      //  options.DefaultIsolation = Isolation.AnonymousPipes;
      //  options.Repositories.Add(new Repository(@"C:\# Daten\NuGet"));
      //}

      StarterForm starterForm = new StarterForm(options) {
        Text = "Choose your application",
        DefaultApplicationImageKey = "Cloud"
      };
      starterForm.LargeImageList.Images.Add("Cloud", FeatherIconsLarge.Cloud);

      System.Windows.Forms.Application.Run(starterForm);
      string s = writer.ToString();
    }
  }
}
