#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks.UI.WindowsForms {
  public abstract class WindowsFormsApplication : Application {
    public override ApplicationKind Kind => ApplicationKind.GUI;

    protected WindowsFormsApplication() : base() { }
    protected WindowsFormsApplication(IChannel channel) : base(channel) { }

    public abstract Form CreateMainForm();

    public override Task RunAsync(string[] args, CancellationToken cancellationToken = default) {
      Form mainForm = CreateMainForm();
      cancellationToken.Register(() => {
        if (mainForm.InvokeRequired) {
          mainForm.Invoke(() => { mainForm.Close(); });
        } else {
          mainForm.Close();
        }
      });
      System.Windows.Forms.Application.Run(mainForm);
      return Task.CompletedTask;
    }
  }
}
