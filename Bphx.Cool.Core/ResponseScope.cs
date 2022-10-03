namespace Bphx.Cool;

/// <summary>
/// Defines the scope of an asynchronous response.
/// </summary>
public enum ResponseScope
{
  /// <summary>
  /// Limits the scope to the initiating procedure step.
  /// </summary>
  Procedure,
    
  /// <summary>
  /// Indicates that the response is considered global to all 
  /// procedure steps executing in the process.
  /// </summary>
  Global,

  /// <summary>
  /// Indicates that the application is not interested in 
  /// retrieving the corresponding response of the request. 
  /// Additionally, no error conditions are reported to the client. 
  /// Once accepted, no indication of how the request was 
  /// processed is returned to the requesting client application.
  /// </summary>
  NoResponse
}