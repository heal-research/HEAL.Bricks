using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using HEAL.Bricks;

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

      if (Runner.ParseArguments(args, out CommunicationMode communicationMode, out string inputConnection, out string outputConnection)) {
        Runner.ReceiveAndExecuteRunnerAsync(communicationMode, inputConnection, outputConnection).Wait();
        return;
      }

      Application.Run(new StarterForm());
    }
  }
}
