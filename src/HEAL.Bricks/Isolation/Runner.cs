#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HEAL.Attic;

namespace HEAL.Bricks {
  [StorableType(StorableMemberSelection.MarkedOnly, "D93CBE04-9847-417A-AAB5-7FBCA6A32247")]
  public abstract class Runner : IRunner {

    #region Vars
    private Thread listener;
    #endregion

    #region Properties
    [Storable]
    public IEnumerable<AssemblyInfo> AssembliesToLoad { get; set; }

    internal RunnerHost Host { get; set; }
    #endregion

    public void Pause() {
      var message = new PauseRunnerMessage();
      if (Host != null) Host.Send(message);
      else OnRunnerMessage(message);
    }

    public void Resume() {
      var message = new ResumeRunnerMessage();
      if (Host != null) Host.Send(message);
      else OnRunnerMessage(message);
    }

    public void Cancel() {
      var message = new CancelRunnerMessage();
      if (Host != null) Host.Send(message);
      else OnRunnerMessage(message);
    }

    public void Run() {
      IPluginLoader loader = PluginLoaderFactory.Create();
      loader.LoadPlugins(AssembliesToLoad);
      Task t = Task.Run((Action)Execute);
      StartListener();
      t.Wait();
    }

    protected abstract void Execute();
    protected abstract void OnRunnerMessage(RunnerMessage message);

    #region Helper

    private void StartListener() {
      listener = new Thread(() => {
        Stream stdin = Console.OpenStandardInput();
        DateTime lastMessageRecieved = DateTime.MinValue;
        while (true) {
          RunnerMessage message = RunnerMessage.ReadMessageFromStream(stdin);
          if (DateTime.Compare(lastMessageRecieved, message.SendTime) < 0) {
            OnRunnerMessage(message);
            lastMessageRecieved = message.SendTime;
          }
        }
      });
      listener.IsBackground = true;
      listener.Start();
    }
    #endregion
  }
}
