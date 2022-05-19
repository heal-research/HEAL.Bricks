#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks {
  public interface IMessage {
    string Command { get; }
    string Payload { get; }

    public T DeserializePayload<T>();
  }
}
