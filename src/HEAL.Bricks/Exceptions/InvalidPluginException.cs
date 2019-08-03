#region License Information
/*
 * This file is part of HEAL.Bricks which is licensed under the MIT license.
 * See the LICENSE file in the project root for more information.
 */
#endregion

using System;
using System.Runtime.Serialization;

namespace HEAL.Bricks {
  /// <summary>
  /// Exception class for invalid plugins.
  /// </summary>
  [Serializable]
  public sealed class InvalidPluginException : Exception {
    /// <summary>
    /// Initializes a new InvalidPluginException
    /// </summary>
    public InvalidPluginException() : base() { }
    /// <summary>
    /// Initializes a new InvalidPluginException with an error message.
    /// </summary>
    /// <param name="message">The exception message</param>
    public InvalidPluginException(string message) : base(message) { }
    /// <summary>
    /// Initializes a new InvalidPluginException with an error message and an inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The original exception.</param>
    public InvalidPluginException(string message, Exception innerException) : base(message, innerException) { }
    /// <summary>
    /// Constructor for serialization.
    /// </summary>
    /// <param name="info">The serialization info.</param>
    /// <param name="contex">The serialization context.</param>
    private InvalidPluginException(SerializationInfo info, StreamingContext contex) : base(info, contex) { }
  }
}
