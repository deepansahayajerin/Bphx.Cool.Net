using Bphx.Cool.Xml;

using static Bphx.Cool.Functions;

namespace Bphx.Cool;

/// <summary>
/// Interface to provide application resources.
/// </summary>
public interface IResources
{
  /// <summary>
  /// Gets an <see cref="NavigationRule"/> instance for a procedure name.
  /// </summary>
  /// <param name="name">A procedure name.</param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <returns>An <see cref="NavigationRule"/> instance</returns>
  ActionInfo GetActionInfo(string name, int resourceID);

  /// <summary>
  /// Gets a procedure name by a transaction code. 
  /// </summary>
  /// <param name="transactionCode">A transaction code.</param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <returns>
  /// A procedure name, or null if there is no procedure for a specified 
  /// transaction name.
  /// </returns>
  string GetProcedureByTransactionCode(
    string transactionCode,
    int resourceID);

  /// <summary>
  /// Gets <see cref="ExitState"/> instance by name.
  /// </summary>
  /// <param name="exitState">An exit state name.</param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <returns>
  /// An <see cref="ExitState"/> instance, or null if no exitstate 
  /// is available for the name.
  /// </returns>
  ExitState GetExitState(string exitState, int resourceID);

  /// <summary>
  /// Gets <see cref="ExitState"/> instance by ID.
  /// </summary>
  /// <param name="exitState">An exit state ID.</param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <returns>
  /// An <see cref="ExitState"/> instance, or null if no exitstate 
  /// is available for the ID.
  /// </returns>
  ExitState GetExitState(int exitStateID, int resourceID);

  /// <summary>
  /// Gets a message for an exit state.
  /// </summary>
  /// <param name="name">An exit state name.</param>
  /// <param name="dialect">Optional, a dialect name.</param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <returns>An exit state message.</returns>
  string GetExitStateMessage(string name, string dialect, int resourceID);
}

/// <summary>
/// Extensions API for <see cref="IResources"/> interface.
/// </summary>
public static class ResourceExtensions
{
  /// <summary>
  /// Gets <see cref="ExitState"/> instance by name.
  /// If no <see cref="ExitState"/> instance is available 
  /// for non empty exit state name then a dummy 
  /// <see cref="ExitState"/> instance created and warning 
  /// is written to a log.
  /// </summary>
  /// <param name="resources">An <see cref="IResources"/> instance.</param>
  /// <param name="exitState">An exit state name.</param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <returns>
  /// An <see cref="ExitState"/> instance, or null if no exitstate 
  /// is available for the name.
  /// </returns>
  public static ExitState GetCheckedExitState(
    this IResources resources,
    string exitState,
    int resourceID)
  {
    if (IsEmpty(exitState))
    {
      return null;
    }

    var instance = resources.GetExitState(exitState, resourceID);

    if ((instance == null) && !IsEmpty(exitState))
    {
      System.Diagnostics.Trace.Write(
        "No exit state definition is found for " + exitState,
        "Resources");

      instance = new() { Name = exitState };
    }

    return instance;
  }

  /// <summary>
  /// Gets <see cref="ExitState"/> instance by ID.
  /// If no <see cref="ExitState"/> instance is available then a dummy
  /// <see cref="ExitState"/> instance created and warning is written 
  /// to a log.
  /// </summary>
  /// <param name="resources">An <see cref="IResources"/> instance.</param>
  /// <param name="exitState">An exit state ID.</param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <returns>
  /// An <see cref="ExitState"/> instance, or null if no exitstate 
  /// is available for the ID.
  /// </returns>
  public static ExitState GetCheckedExitState(
    this IResources resources,
    int exitStateID,
    int resourceID)
  {
    if (exitStateID == 0)
    {
      return null;
    }

    var instance = resources.GetExitState(exitStateID, resourceID);

    if (instance == null)
    {
      System.Diagnostics.Trace.Write(
        "No exit state definition is found for " + exitStateID,
        "Resources");

      instance = new() { Id = exitStateID };
    }

    return instance;
  }
}
