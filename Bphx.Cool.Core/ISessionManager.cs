using System;
using System.Linq;

using System.Collections.Generic;

using Bphx.Cool.UI;
using Bphx.Cool.Events;
using System.IO;

namespace Bphx.Cool;

/// <summary>
/// <para>An application session.</para>
/// <para>
/// Original application, as well as converted is statefull, so this interface
/// encapsulated the application state. Main component of state is a list 
/// of <see cref="IProcedure"/> instances.
/// <para>
/// In addition <see cref="ISessionManager"/> keeps 
/// <see cref="IProfiler"/> instance,  in case of profiling mode is on, and 
/// supports generic map of attributes.
/// </para>
/// <para>
/// Except state <see cref="ISessionManager"/> exposes API to reset state, to
/// create <see cref="IProcedure"/> and <see cref="IContext"/> instances.
/// </para>
/// </summary>
public interface ISessionManager: IAttributes, IDisposable
{
  /// <summary>
  /// Session options.
  /// </summary>
  Dictionary<string, object> Options { get; }

  /// <summary>
  /// Session arguments.
  /// </summary>
  string[] Arguments { get; set; }

  /// <summary>
  /// Gets <see cref="UIApplication"/> instance.
  /// </summary>
  UIApplication Application { get; }
  
  /// <summary>
  /// A list of <see cref="IProcedure"/>s.
  /// </summary>
  List<IProcedure> Procedures { get; }

  /// <summary>
  /// A profiler instance.
  /// </summary>
  IProfiler Profiler { get; set; }

  /// <summary>
  /// A queue of <see cref="Event"/>s.
  /// </summary>
  List<Event> Events { get; }

  /// <summary>
  /// The current MessageBox instance, if available.
  /// </summary>
  MessageBox CurrentMessageBox { get; set; }

  /// <summary>
  /// Returns new procedure id.
  /// </summary>
  /// <returns>A new procedure id value.</returns>
  int NewProcedureId();
}

/// <summary>
/// Extension API for <see cref="ISessionManager"/>.
/// </summary>
public static class SessionManagerExtensions
{
  /// <summary>
  /// Copies a <see cref="ISessionManager"/>.
  /// </summary>
  /// <param name="session">A <see cref="ISessionManager"/> to copy.</param>
  /// <param name="environment">An <see cref="IEnvironment"/> instance.</param>
  /// <returns>A copy instance.</returns>
  public static ISessionManager Copy(
    this ISessionManager session, 
    IEnvironment environment)
  {
    var serializer = environment.Serializer;
    var stream = new MemoryStream();
    var profiler = session.Profiler;

    session.Profiler = null;

    try
    {
      serializer.Serilize(session, stream);
    }
    finally
    {
      session.Profiler = profiler;
    }

    stream.Position = 0;

    return serializer.Deserilize<ISessionManager>(stream);
  }

  /// <summary>
  /// Gets <see cref="IProcedure"/> by ID.
  /// </summary>
  /// <param name="state">An <see cref="ISessionManager"/> instance.</param>
  /// <param name="id">A procedure ID</param>
  /// <returns>
  /// A <see cref="IProcedure"/> instance, or null if no procedure is found.
  /// </returns>
  public static IProcedure GetProcedureByID(
    this ISessionManager state, int id) =>
    id == 0 ? null : 
      state.Procedures.FirstOrDefault(procedure => procedure.Id == id);

  /// <summary>
  /// Indicates whether a request to a specified procedure is possible.
  /// </summary>
  /// <param name="state">An <see cref="ISessionManager"/> instance.</param>
  /// <param name="procedure">An <see cref="IProcedure"/> to check.</param>
  /// <returns>
  /// true if the procedure can accept request, and false otherwise.
  /// </returns>
  public static bool CanAcceptRequest(
    this ISessionManager state,
    IProcedure procedure)
  {
    if (procedure.IsComplete())
    {
      return false;
    }

    if (procedure.Type == ProcedureType.Window)
    {
      foreach(var window in procedure.Windows)
      {
        if ((window.WindowState == WindowState.Opened) && window.Modal)
        {
          foreach(var called in state.Procedures)
          {
            if ((procedure != called) &&
              (called.Type == ProcedureType.Window) &&
              procedure.IsCaller(called))
            {
              foreach(var otherWindow in called.Windows)
              {
                if ((otherWindow.WindowState == WindowState.Opened) &&
                  otherWindow.Modal)
                {
                  return false;
                }
              }
            }
          }

          return true;
        }
      }

      var root = procedure.GetRoot();

      foreach(var other in state.Procedures)
      {
        if ((other != procedure) &&
          (other.Type == ProcedureType.Window) &&
          !other.IsCaller(procedure) &&
          (root == other.GetRoot()))
        {
          foreach(var window in other.Windows)
          {
            if ((window.WindowState == WindowState.Opened) && window.Modal)
            {
              return false;
            }
          }
        }
      }

      return true;
    }
    else
    {
      return procedure.CalledCount == 0;
    }
  }

  /// <summary>
  /// Gets a procedure, if any that can accept a request.
  /// </summary>
  /// <param name="state">An <see cref="ISessionManager"/> instance.</param>
  /// <returns>A <see cref="IProcedure"/> instance or <c>null</c>.</returns>
  public static IProcedure GetProcedureThatCanAcceptRequest(
    this ISessionManager state)
  {
    var procedures = state.Procedures;

    for(var i = procedures.Count; i-- > 0;)
    {
      var procedure = procedures[i];

      if(state.CanAcceptRequest(procedure))
      {
        return procedure;
      }
    }

    return null;
  }

  /// <summary>
  /// Cancel, if possible, events that belong to a procedure.
  /// </summary>
  /// <param name="state">An <see cref="ISessionManager"/> instance.</param>
  /// <param name="procedure">A procedure whose events to cancel.</param>
  public static void CancelEvents(
    this ISessionManager state,
    IProcedure procedure)
  {
    foreach(var item in state.Events)
    {
      if (item.Procedure == procedure)
      {
        state.CancelEvent(item);
      }
    }
  }

  /// <summary>
  /// Cancel an event, if possible.
  /// </summary>
  /// <param name="state">An <see cref="ISessionManager"/> instance.</param>
  /// <param name="anEvent">An event to cance.</param>
  /// <param name="force">
  /// <code>true</code> to force cancel, and <code>false</code> otherwise.
  /// </param>
  /// <returns>A <see cref="IProcedure"/> instance or <c>null</c>.</returns>
  public static void CancelEvent(
    this ISessionManager state, 
    Event anEvent, 
    bool force = false)
  {
    if ((anEvent != null) && 
      (force || anEvent.Cancelable) && 
      !anEvent.Canceled)
    {
      anEvent.Canceled = true;
      state.Profiler.Value("trace", "cancelEvent", anEvent.EventObject);
    }
  }
}
