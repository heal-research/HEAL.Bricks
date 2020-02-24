#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using HEAL.Attic;

namespace HEAL.Bricks {
  [StorableType("612F98AF-E254-4C5E-BD41-75B4F1D9B96D")]
  public class ApplicationRunner : Runner {
    /// <summary>
    /// Arguments for the StartApplication.
    /// </summary>
    [Storable]
    public ICommandLineArgument[] Args { get; set; }

    /// <summary>
    /// The application which should run in child process.
    /// </summary>
    public IApplication StartApplication {
      get {
        lock (locker) {
          if (application == null)
            application = (IApplication)new ProtoBufSerializer().Deserialize(serializedStartApplication);
          return application;
        }
      }
      set {
        lock (locker) {
          serializedStartApplication = new ProtoBufSerializer().Serialize(value);
          application = value;
        }
      }
    }
    // Encapsulated application is necessary, because it is not possible to 
    // instantly deserialize the application, before all assemblies are loaded.
    [Storable]
    private byte[] serializedStartApplication = new byte[0];

    // cache application to prevent new instances every get call of StartApplication
    private IApplication application;
    private object locker = new object();

    protected override void Execute() {
      StartApplication.Run(Args);
    }

    protected override void OnRunnerMessage(RunnerMessage message) {
      if (message is PauseRunnerMessage)
        StartApplication.OnPause();
      else if (message is ResumeRunnerMessage)
        StartApplication.OnResume();
      else if (message is CancelRunnerMessage)
        StartApplication.OnCancel();
    }
  }
}
