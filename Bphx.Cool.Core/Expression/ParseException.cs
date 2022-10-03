using System;

namespace Bphx.Cool.Expression;

/// <summary>
/// Signals that an error has been reached unexpectedly
/// while parsing.
/// </summary>
public class ParseException : Exception
{
  /// <summary>
  /// Constructs a ParseException with the specified detail message and 
  /// offset. A detail message is a String that describes this particular 
  /// exception.
  /// </summary>
  /// <param name="s">a detail message.</param>
  /// <param name="errorOffset">
  /// the position where the error is found while parsing.
  /// </param>
  public ParseException(string s, int errorOffset): base(s) => 
    ErrorOffset = errorOffset;

  /// <summary>
  /// Returns the position where the error was found.
  /// </summary>
  public int ErrorOffset { get; }
}
