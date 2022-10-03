using System;

namespace Bphx.Cool;

/// <summary>
/// An exception specifies the supporting runtime encounters an error 
/// attempting to initiate the asynchronous cooperative flow.
/// </summary>
public class RequestNotAcceptedException : Exception
{
  /// <summary>
  /// Default constructor. 
  /// Creates an RequestNotAcceptedException instance.
  /// </summary>
  public RequestNotAcceptedException()
  {
  }

  /// <summary>
  /// Creates an RequestNotAcceptedException instance.
  /// </summary>
  /// <param name="message">a detail message.</param>
  public RequestNotAcceptedException(string message) :
    base(message)
  {
  }

  /// <summary>
  /// Creates an RequestNotAcceptedException instance.
  /// </summary>
  /// <param name="cause">
  /// a cause exception (which is saved for later retrieval).
  /// </param>
  public RequestNotAcceptedException(Exception cause) :
    base(cause.Message, cause)
  {
  }
}
