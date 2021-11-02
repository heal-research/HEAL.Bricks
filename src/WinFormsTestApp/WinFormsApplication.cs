using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HEAL.Bricks;

namespace WinFormsTestApp {
  class WinFormsApplication : IApplication {
    public string Name => "WinFormsApplication";

    public string Description => "Simple WinForms GUI application.";

    public ApplicationKind Kind => ApplicationKind.GUI;

    public Task RunAsync(string args, CancellationToken cancellationToken = default) {
      var form = new ApplicationForm();
      cancellationToken.Register(() => { form.Close(); });
      Application.Run(form);
      return Task.CompletedTask;
    }
  }
}
