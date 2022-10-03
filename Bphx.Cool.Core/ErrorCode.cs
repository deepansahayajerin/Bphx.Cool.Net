namespace Bphx.Cool;

/// <summary>
/// Defines database vendor independent error codes.
/// </summary>
public enum ErrorCode
{
  /// <summary>
  /// An operation was performed successfully.
  /// </summary>
  Successful,

  /// <summary>
  /// Data not found.
  /// </summary>
  NotFound,

  /// <summary>
  /// Not unique identifier was used.
  /// </summary>
  NotUnique,

  /// <summary>
  /// Value already exists.
  /// </summary>
  AlreadyExists,

  /// <summary>
  /// Invalid permitted value.
  /// </summary>
  PermittedValueViolation,

  /// <summary>
  /// Operation timeout.
  /// </summary>
  Timeout,

  /// <summary>
  /// Other database error.
  /// </summary>
  DatabaseError,

  /// <summary>
  /// Unknown error.
  /// </summary>
  Error,

  /// <summary>
  /// Request is accepted.
  /// </summary>
  Accepted,

  /// <summary>
  /// Request is not accepted.
  /// </summary>
  NotAccepted,

  /// <summary>
  /// Async request is pending.
  /// </summary>
  Pending,

  /// <summary>
  /// Invalid async request id.
  /// </summary>
  InvalidRequestId,

  /// <summary>
  /// Error during execution of async action.
  /// </summary>
  ServerError,

  /// <summary>
  /// Communication error during execution of async action.
  /// </summary>
  CommunicationError
}
