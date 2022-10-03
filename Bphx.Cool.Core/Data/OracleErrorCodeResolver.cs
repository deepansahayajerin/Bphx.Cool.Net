using System;
using System.Data.Common;

namespace Bphx.Cool.Data;

/// <summary>
/// Implementation of IErrorCodeResolver interface for Oracle DB.
/// </summary>
public class OracleErrorCodeResolver: IErrorCodeResolver
{
  /// <summary>
  ///   <para>Resolves an exception in vendor independent error.</para>
  ///   <para>
  ///     See SQL states for Oracle at 
  ///     http://camden-www.rutgers.edu/help/Documentation/Oracle/server.815/a68023/err.htm
  ///   </para>
  ///   <para>and</para>
  ///   <para>
  ///     http://www.ora-code.com/code-6.html
  ///     for more details
  ///   </para>
  /// </summary>
  /// <param name="e">an exception to resolve.</param>
  /// <returns>a vendor independent error as a ErrorCode instance.</returns>
  /// <seealso cref="Bphx.Cool.IErrorCodeResolver"/>
  /// <seealso cref="Bphx.Cool.ErrorCode"/>
  public ErrorCode Resolve(Exception e)
  {
    if (e == null)
    {
      return ErrorCode.Successful;
    }

    if (e is ProhibitedValueException)
    {
      return ErrorCode.PermittedValueViolation;
    }

    if (e is not DbException sqlError)
    {
      // Checks a wrapped sql exception.
      sqlError = e.InnerException as DbException;
    }

    if (sqlError == null)
    {
      return ErrorCode.Error;
    }

    var sqlCode = sqlError.ErrorCode;
    var sqlState = e is DataException dataException ? 
      dataException.SqlState : sqlError.Message;

    if (sqlCode == 1)
    {
      return ErrorCode.AlreadyExists;
    }
    else if (sqlCode == 99)
    {
      return ErrorCode.Timeout;
    }
    else if (sqlState == null)
    {
      return ErrorCode.DatabaseError;
    }
    else if (sqlState.StartsWith("220"))
    {
      // data exception  
      if (!((sqlCode == 1426) ||
        ((sqlCode > 1799) && (sqlCode < 1900)) ||
        (sqlCode == 1411) ||
        (sqlCode == 1025) ||
        (sqlState.StartsWith("22015")) ||
        (sqlState.StartsWith("22026"))))
      {
        // see e.g. http://ora-01438.ora-code.com/
        return ErrorCode.DatabaseError;
      }

      return ErrorCode.PermittedValueViolation;
    }
    else
    {
      return ErrorCode.DatabaseError;
    }
  }

  /// <summary>
  /// Tests whether the exception represents a specified error code.
  /// </summary>
  /// <param name="exception">A <see cref="DbException"/> instance.</param>
  /// <param name="errorCode">A error code value.</param>
  /// <returns>
  /// <c>true</c> if the exception represents a specified error code, and
  /// <c>false</c> otherwise.
  /// </returns>
  public bool IsError(DbException exception, ErrorCode errorCode)
  {
    var sqlState = exception.SqlState;

    if (sqlState == null)
    {
      return false;
    }

    var sqlCode = exception.ErrorCode;

    if (errorCode == ErrorCode.AlreadyExists)
    {
      return sqlCode == 1;
    }
    else if (errorCode == ErrorCode.PermittedValueViolation)
    {
      if (sqlState.StartsWith("220"))
      {
        // data exception  
        if (!((sqlCode == 1426) ||
          ((sqlCode > 1799) && (sqlCode < 1900)) ||
          (sqlCode == 1411) ||
          (sqlCode == 1025) ||
          (sqlState == "22015") ||
          (sqlState == "22026")))
        {
          // see e.g. http://ora-01438.ora-code.com/
          return false;
        }

        return true;
      }
      else if (sqlState.StartsWith("230"))
      {
        return true;
      }

      return false;
    }
    else if (errorCode == ErrorCode.Timeout)
    {
      return sqlCode == 99;
    }
    else if (errorCode == ErrorCode.DatabaseError)
    {
      return true;
    }
    else
    {
      return false;
    }
  }
}
