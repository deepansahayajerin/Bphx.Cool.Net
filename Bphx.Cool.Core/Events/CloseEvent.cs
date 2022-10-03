using System;
using System.Linq;
using System.Collections.Generic;

using Bphx.Cool.UI;

namespace Bphx.Cool.Events;

/// <summary>
/// Defines a close event for window mode.
/// </summary>
[Serializable]
public class CloseEvent: Event
{
  /// <summary>
  /// An event type name.
  /// </summary>
  public const string EventType = "CLOSE";

  /// <summary>
  /// Creates an instance of CLOSE event.
  /// </summary>
  /// <param name="procedure">A <see cref="IProcedure"/> instance.</param>
  /// <param name="window">
  /// a window or dialog name, which to be closed.
  /// </param>
  /// <param name="eventObject">
  /// An optional argument that represents client side event properties.
  /// </param>
  public CloseEvent(
    IProcedure procedure, 
    string window,
    EventObject eventObject = null) : 
    base(procedure, EventType, window, null, eventObject)
  {
  }

  /// <summary>
  /// A <see cref="Global"/> instance that might be used to restore
  /// procedure's global after the event handler call.
  /// </summary>
  public Global Global { get; set; }

  /// <summary>
  /// Put into event queue DEACTIVATE and CLOSE events.
  /// </summary>
  /// <param name="dialog">An <see cref="IDialogManager"/> instance.</param>
  /// <param name="queue">An event queue.</param>
  /// <param name="index">An index in the queue to put event.</param>
  public override void Prepare(
    IDialogManager dialog, 
    IList<Event> queue, 
    int index)
  {
    var name = Window;
    var procedure = Procedure;
    var window = procedure.GetWindow(dialog, name, false);

    if (window != null)
    {
      // If a procedure is on execute first then work as if
      // the procedure is window-less.
      if ((procedure.ExecutionState == ExecutionState.Initial) &&
        (name == procedure.PrimaryWindow))
      {
        var windowless = true;
        var hasOpen = true;

        foreach(var item in dialog.SessionManager.Events)
        {
          if (!item.Canceled && (item.Procedure == procedure))
          {
            if ((item is OpenEvent) && (item.Window == name))
            {
              hasOpen = true;

              continue;
            }

            windowless = false;

            break;
          }
        }

        if (windowless)
        {
          procedure.PrimaryWindow = null;

          if (hasOpen)
          {
            foreach(var item in dialog.SessionManager.Events)
            {
              if (!item.Canceled &&
                (item.Procedure == procedure) &&
                (item is OpenEvent) &&
                (item.Window == name))
              {
                dialog.SessionManager.CancelEvent(item, true);
              }
            }
          }
        }
      }
      else
      {
        closeBeforeOpened = window.WindowState != WindowState.Opened;

        var anEvent = new DeactivatedEvent(Procedure, Window);

        anEvent.Prepare(dialog, queue, index);
        base.Prepare(dialog, queue, index);
      }
    }
  }

  /// <summary>
  /// Closes all related windows and dialogs, resets an active window,
  /// and removes the specified window view from the list of opened windows.
  /// </summary>
  /// <param name="context">A <see cref="IContext"/> instance.</param>
  /// <returns>
  /// true when event is run and application logic can be called.
  /// </returns>
  public override bool Run(IContext context)
  {
    var name = Window;
    var procedure = Procedure;
    var windows = procedure.Windows;
    var window = windows.Where(item => item.Name == name).FirstOrDefault();

    if ((window == null) || (window.WindowState == WindowState.Closed))
    {
      return false;
    }

    var dialog = context.Dialog;
    var state = dialog.SessionManager;
    var events = state.Events;
    var hasWindows = false;
    var primaryWindow = window.PrimaryWindow;

    foreach(var other in windows)
    {
      if ((other != window) && (other.WindowState != WindowState.Closed))
      {
        hasWindows = true;

        if (!primaryWindow)
        {
          break;
        }

        // Close this primary window and all its children, if any.
        events.Add(new CloseEvent(procedure, other.Name));
      }
    }

    if (window == procedure.ActiveWindow)
    {
      procedure.ActiveWindow = null;
    }

    if (primaryWindow)
    {
      procedure.Attributes["bphx:primaryWindowClosed"] = true;
    }

    // Remove the window view from the list of opened windows
    windows.Remove(window);
    procedure.Dirty = true;

    if (!hasWindows && 
      ((procedure.PrimaryWindow == null) ||
      (procedure.Attributes["bphx:primaryWindowClosed"] as bool? == true)))
    {
      // Allow a modeless window with LINKs to hang until LINKs are closed.
      if (!window.Modal && 
        (procedure.CalledCount > 0) &&
        (closeBeforeOpened || !state.CanAcceptRequest(procedure)))
      {
        procedure.ExecutionState = ExecutionState.Closing;
      }
      else
      { 
        dialog.CloseProcedure(procedure, false);

        if (!procedure.IsComplete())
        {
          // Return window into a window list; it will be removed later.
          windows.Add(window);

          // Queue this event again.
          base.Prepare(dialog, events, 0);

          return false;
        }
      }
    }

    return true;
  }

  /// <summary>
  /// Executes an after run logic, if any.
  /// </summary>
  /// <param name="context">a context instance.</param>
  /// <param name="run"><see cref="Run(IContext)"/> outcome.</param>
  public override void AfterRun(IContext context, bool run)
  {
    if (run && (Global != null))
    {
      Procedure.Global.Assign(Global);
    }
  }

  /// <summary>
  /// Indicates whether the Close event has been queued before 
  /// Open event handler has completed.
  /// </summary>
  private bool closeBeforeOpened;
}