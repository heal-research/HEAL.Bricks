#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.IO;
using HEAL.Attic;

namespace HEAL.Bricks {

  [StorableType(StorableMemberSelection.MarkedOnly, "081DC79E-1E57-49A2-AB25-D9AC42A40911")]
  public class RunnerMessage {
    private static ProtoBufSerializer serializer = new ProtoBufSerializer();
    public static RunnerMessage ReadMessageFromStream(Stream stream) {
      byte[] buf = new byte[4];
      stream.Read(buf, 0, buf.Length);
      int bufSize = BitConverter.ToInt32(buf, 0);
      buf = new byte[bufSize];
      stream.Read(buf, 0, buf.Length);
      return (RunnerMessage)serializer.Deserialize(buf);
    }

    [Storable]
    public DateTime SendTime { get; set; }
  }


  [StorableType(StorableMemberSelection.AllProperties, "84771CF3-46CA-4D0B-96D1-7F2E8EAC50B7")]
  public class PauseRunnerMessage : RunnerMessage { }


  [StorableType(StorableMemberSelection.AllProperties, "FE2AEE66-E2A2-4DE1-8E60-C116F084D2AA")]
  public class ResumeRunnerMessage : RunnerMessage { }


  [StorableType(StorableMemberSelection.AllProperties, "2C836B74-C28B-4F4A-9645-E1ED5F4415A7")]
  public class CancelRunnerMessage : RunnerMessage { }


  [StorableType(StorableMemberSelection.AllProperties, "2D8D1D8F-58D4-4D92-BB15-62DF55DC0459")]
  public class TransportRunnerMessage : RunnerMessage {
    public IRunner Runner { get; set; }
    public TransportRunnerMessage(IRunner runner) => Runner = runner;

    [StorableConstructor]
    private TransportRunnerMessage(StorableConstructorFlag _) { }
  }

}
