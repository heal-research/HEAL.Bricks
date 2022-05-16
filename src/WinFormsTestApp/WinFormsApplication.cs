using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HEAL.Bricks;

namespace WinFormsTestApp {
  class WinFormsApplication : HEAL.Bricks.Application {
    public override string Name => "WinFormsApplication";

    public override string Description => "Simple WinForms GUI application.";

    public override ApplicationKind Kind => ApplicationKind.GUI;

    public override Task StartAsync(string[] args, CancellationToken cancellationToken = default) {
      var form = new ApplicationForm {
        Text = "WinFormsApplication"
      };
      cancellationToken.Register(() => { form.Close(); });
      System.Windows.Forms.Application.Run(form);
      return Task.CompletedTask;
    }
  }
}
