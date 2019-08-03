#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;

namespace HEAL.Bricks {

  /// <summary>
  /// The ApplicationManager has a reference to the application manager singleton.
  /// </summary>
  public static class ApplicationManager {
    // the singleton instance is initialized to LightweightApplicationManager as long as no other application manager is registered    
    private static IApplicationManager appManager;

    /// <summary>
    /// Gets the application manager singleton.
    /// </summary>
    public static IApplicationManager Manager {
      get {
        if (appManager == null)
          appManager = new LightweightApplicationManager();
        return appManager;
      }
    }

    /// <summary>
    /// Registers a new application manager.
    /// </summary>
    /// <param name="manager"></param>
    internal static void RegisterApplicationManager(IApplicationManager manager) {
      if (appManager != null && !(appManager is LightweightApplicationManager)) throw new InvalidOperationException("The application manager has already been set.");
      else {
        appManager = manager;
      }
    }
  }
}
