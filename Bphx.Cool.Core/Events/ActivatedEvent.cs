using System;
using System.Collections.Generic;

using static Bphx.Cool.Functions;

namespace Bphx.Cool.Events;

/// <summary>
/// Defines an activate window event.
/// </summary>
[Serializable]
public class ActivatedEvent: Event
{
  /// <summary>
  /// An event type name.
  /// </summary>
  public const string EventType = "ACTIVATED";

  /// <summary>
  /// Creates an ActivateEvent instance.
  /// </summary>
  /// <param name="procedure">A <see cref="IProcedure"/> instance.</param>
  /// <param name="window">a window or a dialog name to be activated.</param>
  /// <param name="eventObject">
  /// An optional argument that represents client side event properties.
  /// </param>
  public ActivatedEvent(
    IProcedure procedure,
    string window,
    EventObject eventObject = null): 
    base(procedure, EventType, window, null, eventObject)
  {
  }

  /// <summary>
  /// Put into event queue DEACTIVATE and CLOSE events.
  /// </summary>
  /// <param name="dialog">An <see cref="IDialogManager"/> instance.</param>
  /// <param name="queue">an event queue.</param>
  /// <param name="index">An index in the queue to put event.</param>
  public override void Prepare(
    IDialogManager dialog,
    IList<Event> queue, 
    int index)
  {
    base.Prepare(dialog, queue, index);

    // Cancel other queued Activated events.
    foreach(var item in queue)
    {
      if ((item != this) && (item is ActivatedEvent))
      {
        dialog.SessionManager.CancelEvent(item, true);
      }
    }
  }

  /// <summary>Sets an active window.</summary>
  /// <param name="context">A <see cref="IContext"/> instance.</param>
  /// <returns>
  /// true when event is run and application logic can be called.
  /// </returns>
  public override bool Run(IContext context) 
  {
    var dialog = context.Dialog;
    var state = dialog.SessionManager;
    var procedure = Procedure;
    var procedures = state.Procedures;
    var index = procedures.IndexOf(procedure);
    var name = Window;
    var window = procedure.GetWindow(dialog, name, false);

    if (window == null)
    {
      return false;
    }

    var activeWindow = procedure.ActiveWindow;

    if (window.Lock == 0)
    {
      window.Lock = activeWindow == null ? 1 : 
        window.Modal ? activeWindow.Lock + 1 : 
        activeWindow.Lock;
    }
    else
    {
      if (window.Lock < activeWindow?.Lock)
      {
        return false;
      }
    }

    if(state.CanAcceptRequest(procedure))
    {
      CircularMove(
        procedures,
        index,
        procedures.Count,
        procedures.Count - 1);

      procedure.ActiveWindow = window;

      return true;
    }
    else
    {
      var end = index;

      while(end + 1 < procedures.Count)
      {
        var other = procedures[end + 1];

        if ((other.Type == ProcedureType.Window) &&
          state.CanAcceptRequest(other))
        {
          break;
        }

        ++end;
      }

      CircularMove(procedures, index, end + 1, end);

      return false;
    }
  }
}
