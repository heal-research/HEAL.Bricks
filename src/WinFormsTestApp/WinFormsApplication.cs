using HEAL.Bricks;
using HEAL.Bricks.UI.WindowsForms;

namespace WinFormsTestApp {
  class WinFormsApplication : WindowsFormsApplication {
    public override string Name => "WinFormsApplication";
    public override string Description => "Simple WinForms GUI application.";

    public WinFormsApplication() : base() { }
    public WinFormsApplication(IChannel channel) : base(channel) { }

    public override Form CreateMainForm() {
      return new ApplicationForm {
        Text = "WinFormsApplication"
      };
    }
  }
}
