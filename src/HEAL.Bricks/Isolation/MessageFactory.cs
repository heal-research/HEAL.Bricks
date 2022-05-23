#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

namespace HEAL.Bricks {
  internal static class MessageFactory {
    public static IMessage CustomMessage(string command) => Message.Create(command);
    public static IMessage CustomMessage<T>(string command, T payload) => Message.Create(command, payload);
    public static IMessage LoadPackages(IEnumerable<PackageLoadInfo> packages) => CustomMessage(Message.Commands.LoadPackages, packages);
    public static IMessage PackagesLoaded() => CustomMessage(Message.Commands.PackagesLoaded);
    public static IMessage DiscoverRunnables() => CustomMessage(Message.Commands.DiscoverRunnables);
    public static IMessage RunnablesDiscovered(IEnumerable<RunnableInfo> runnables) => CustomMessage(Message.Commands.RunnablesDiscovered, runnables);
    public static IMessage RunRunnable(RunnableInfo runnable, string[] args) => CustomMessage(Message.Commands.RunRunnable, new Tuple<RunnableInfo, string[]>(runnable, args));
    public static IMessage Log(string message) => CustomMessage(Message.Commands.Log, message);
    public static IMessage Terminate() => CustomMessage(Message.Commands.Terminate);
  }
}
