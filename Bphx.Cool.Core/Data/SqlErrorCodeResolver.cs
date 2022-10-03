using System.Data.Common;

namespace Bphx.Cool.Data;

/// <summary>
/// Implementation of IErrorCodeResolver interface for Microsoft SQL Server.
/// </summary>
public class SqlErrorCodeResolver: IErrorCodeResolver
{
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
    var sqlErrorCode = exception.ErrorCode;

    if (errorCode == ErrorCode.AlreadyExists)
    {
      return sqlErrorCode switch
      {
        2601 or 2627 => true,
        _ => false
      };
    }
    else if (errorCode == ErrorCode.PermittedValueViolation)
    {
      return sqlErrorCode switch
      {
        547 or 548 or 550 or 8166 or 11011 => true,
        _ => false
      };
    }
    else if (errorCode == ErrorCode.Timeout)
    {
      return sqlErrorCode switch
      {
        8645 or 8675 or 17830 or 35256 or 41149 or 41165 => true,
        _ => false
      };
    }
    else if (errorCode == ErrorCode.DatabaseError)
    {
      return sqlErrorCode switch
      {
        102 or 142 or 156 or 235 or
        293 or 319 or 325 or 336 or
        1018 or 1035 or 8173 or 46508 => false,
        _ => true
      };
    }
    else
    {
      return false;
    }
  }
}
