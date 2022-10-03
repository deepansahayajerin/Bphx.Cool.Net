using System;
using System.Data.Common;

namespace Bphx.Cool.Data;
 
/// <summary>
/// A data exception.
/// </summary>
public class DataException : DbException
{
  /// <summary>
  /// Creates a <see cref="DataException"/> instance.
  /// </summary>
  /// <param name="message">An error message.</param>
  /// <param name="sqlState">A SQL state.</param>
  public DataException(string message, string sqlState) :
    base(message) => 
    SqlState = sqlState;

  /// <summary>
  /// Creates a <see cref="DataException"/> instance.
  /// </summary>
  /// <param name="message">An error message.</param>
  /// <param name="sqlState">A SQL state.</param>
  /// <param name="sqlCode">An SQL code.</param>
  public DataException(string message, string sqlState, int sqlCode) :
    base(message, sqlCode) =>
    SqlState = sqlState;

  /// <summary>
  /// Creates a <see cref="DataException"/> instance.
  /// </summary>
  /// <param name="message">An error message.</param>
  /// <param name="sqlState">A SQL state.</param>
  /// <param name="innerException">Optional inner exception.</param>
  public DataException(
    string message,
    string sqlState,
    Exception innerException) :
    base(message, innerException) => 
    SqlState = sqlState;

  /// <summary>
  /// An SQL state value.
  /// </summary>
  public override string SqlState { get; }
}
