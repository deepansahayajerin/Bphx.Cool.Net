using System;

namespace Bphx.Cool;
  
/// <summary>
/// An exception terminating execution of a procedure. 
/// A navifation flow manager rolls back opened transaction, and
/// restarts the procedure.
/// </summary>
public class RetryTransactionException: Exception
{
  /// <summary>
  /// Default constructor. 
  /// Creates an RetryTransactionException instance.
  /// </summary>
  public RetryTransactionException()
  {
  }

  /// <summary>
  /// Creates an RetryTransactionException instance.
  /// </summary>
  /// <param name="message">a detail message.</param>
  public RetryTransactionException(String message): 
    base(message)
  {
  }

  /// <summary>
  /// Creates an RetryTransactionException instance.
  /// </summary>
  /// <param name="cause">
  /// a cause exception (which is saved for later retrieval).
  /// </param>
  public RetryTransactionException(Exception cause):
    base(cause.Message, cause)
  {
  }
}
