using System;
using System.Data.Common;

namespace Bphx.Cool.Data;

/// <summary>
/// Resolve error codes.
/// </summary>
public interface IErrorCodeResolver
{
  /// <summary>
  /// Resolves an exception in vendor independent error.
  /// </summary>
  /// <param name="e">an exception to resolve.</param>
  /// <returns>a vendor independent error as an ErrorCode instance.</returns>
  /// <see cref="ErrorCode"/>
  ErrorCode Resolve(Exception e)
  {
    if (e == null)
    {
      return ErrorCode.Successful;
    }

    if (e is ProhibitedValueException)
    {
      return ErrorCode.PermittedValueViolation;
    }

    if (e is RequestNotAcceptedException)
    {
      return ErrorCode.NotAccepted;
    }

    if (e is InvalidRequestIdException)
    {
      return ErrorCode.InvalidRequestId;
    }

    if (e is ServerException)
    {
      return ErrorCode.ServerError;
    }


    if (e is not DbException sqlError)
    {
      sqlError = e.InnerException as DbException;

      if (sqlError == null)
      {
        return ErrorCode.Error;
      }
    }

    return IsError(sqlError, ErrorCode.AlreadyExists) ?
      ErrorCode.AlreadyExists :
      IsError(sqlError, ErrorCode.PermittedValueViolation) ?
        ErrorCode.PermittedValueViolation :
      IsError(sqlError, ErrorCode.Timeout) ? ErrorCode.Timeout :
      IsError(sqlError, ErrorCode.DatabaseError) ? ErrorCode.DatabaseError :
        ErrorCode.Error;
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
  bool IsError(DbException exception, ErrorCode errorCode);
}
