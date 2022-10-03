using System;

namespace Bphx.Cool;

/// <summary>
/// An exception specifies that request identifier does not correspond to 
/// an outstanding request.
/// </summary>
public class InvalidRequestIdException : Exception
{
  /// <summary>
  /// Creates an InvalidRequestIdException instance.
  /// </summary>
  public InvalidRequestIdException()
  {
  }

  /// <summary>
  /// Creates an InvalidRequestIdException instance.
  /// </summary>
  /// <param name="message">a detail message.</param>
  public InvalidRequestIdException(String message) : base(message)
  {
  }

  /// <summary>
  /// Creates an InvalidRequestIdException instance.
  /// </summary>
  /// <param name="message">a detail message.</param>
  /// <param name="cause">
  /// a cause (which is saved for later retrieval by the InnerException property.
  /// </param>
  public InvalidRequestIdException(String message, Exception cause) :
    base(message, cause)
  {
  }
}
