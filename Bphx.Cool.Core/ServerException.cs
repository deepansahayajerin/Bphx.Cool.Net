using System;

namespace Bphx.Cool;

/// <summary>
/// An exception specifies that the response returned from the server
/// contains a server failure.
/// </summary>
public class ServerException: Exception
{
  /// <summary>
  /// Default constructor. 
  /// Creates an ServerException instance.
  /// </summary>
  public ServerException()
  {
  }

  /// <summary>
  /// Creates an ServerException instance.
  /// </summary>
  /// <param name="message">a detail message.</param>
  public ServerException(string message): 
    base(message)
  {
  }

  /// <summary>
  /// Creates an ServerException instance.
  /// </summary>
  /// <param name="cause">
  /// a cause exception (which is saved for later retrieval).
  /// </param>
  public ServerException(Exception cause) : 
    base(cause.Message, cause)
  {
  }
}
