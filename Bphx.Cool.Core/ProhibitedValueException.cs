using System;

namespace Bphx.Cool;

/// <summary>
/// This exception is thrown when prohibited value is set.
/// </summary>
public class ProhibitedValueException: ArgumentException
{
  /// <summary>
  /// Constructs a ProhibitedValueException instance.
  /// </summary>
  public ProhibitedValueException()
  {
  }

  /// <summary>
  /// Constructs a ProhibitedValueException instance.
  /// </summary>
  /// <param name="message">a detail message.</param>
  /// <param name="cause">a cause exception.</param>
  public ProhibitedValueException(string message, Exception cause) :
    base(message, cause)
  {
  }

  /// <summary>
  /// Constructs a ProhibitedValueException instance.
  /// </summary>
  /// <param name="message">a detail message.</param>
  public ProhibitedValueException(string message) :
    base(message)
  {
  }

  /// <summary>
  /// Constructs a ProhibitedValueException instance.
  /// </summary>
  /// <param name="cause">a cause exception.</param>
  public ProhibitedValueException(Exception cause) :
    base(cause.Message, cause)
  {
  }
}
