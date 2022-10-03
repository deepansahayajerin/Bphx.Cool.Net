using System;

namespace Bphx.Cool;

/// <summary>
/// This exception terminates execution of a procedure step.
/// </summary>
public class AbortTransactionException : Exception
{
  /// <summary>
  /// Default constructor. 
  /// Creates an AbortTransactionException instance.
  /// </summary>
  public AbortTransactionException()
  {
  }

  /// <summary>
  /// Creates an AbortTransactionException instance.
  /// </summary>
  /// <param name="message">a detail message.</param>
  public AbortTransactionException(string message) :
    base(message)
  {
  }

  /// <summary>
  /// Creates an AbortTransactionException instance.
  /// </summary>
  /// <param name="cause">
  /// a cause exception (which is saved for later retrieval).
  /// </param>
  public AbortTransactionException(Exception cause) :
    base(cause.Message, cause)
  {
  }

  /// <summary>
  /// Creates an AbortTransactionException instance.
  /// </summary>
  /// <param name="message">a detail message.</param>
  /// <param name="cause">
  /// a cause exception (which is saved for later retrieval).
  /// </param>
  public AbortTransactionException(string message, Exception cause) :
    base(message, cause)
  {
  }
}
