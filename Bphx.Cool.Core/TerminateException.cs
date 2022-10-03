using System;

namespace Bphx.Cool;

/// <summary>
/// An exception terminating current session.
/// </summary>
public class TerminateException: Exception
{
  /// <summary>
  /// Default constructor. 
  /// Creates an TerminateException instance.
  /// </summary>
  public TerminateException()
  {
  }

  /// <summary>
  /// Creates an TerminateException instance.
  /// </summary>
  /// <param name="message">a detail message.</param>
  public TerminateException(string message): 
    base(message)
  {
  }

  /// <summary>
  /// Creates an TerminateException instance.
  /// </summary>
  /// <param name="cause">
  /// a cause exception (which is saved for later retrieval).
  /// </param>
  public TerminateException(Exception cause) : 
    base(cause.Message, cause)
  {
  }
}
