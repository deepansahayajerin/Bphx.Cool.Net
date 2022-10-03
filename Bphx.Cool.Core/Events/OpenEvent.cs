using System;
using System.Collections.Generic;
using Bphx.Cool.UI;

namespace Bphx.Cool.Events;

/// <summary>
/// Defines an open window/dialog event.
/// </summary>
[Serializable]
public class OpenEvent: Event
{
  /// <summary>
  /// An event type name.
  /// </summary>
  public const string EventType = "OPEN";

  /// <summary>
  /// Creates an instance of am OPEN window event.
  /// </summary>
  /// <param name="procedure">A <see cref="IProcedure"/> instance.</param>
  /// <param name="window">
  /// A window or dialog name, whose event is to be processed.
  /// </param>
  /// <param name="eventObject">
  /// An optional argument that represents client side event properties.
  /// </param>
  public OpenEvent(
    IProcedure procedure,
    string window,
    EventObject eventObject = null): 
    base(procedure, EventType, window, null, eventObject)
  {
  }

  /// <summary>
  /// Prepares an event, puts it in the specified queue. 
  /// </summary>
  /// <param name="dialog">An <see cref="IDialogManager"/> instance.</param>
  /// <param name="queue">An event queue.</param>
  /// <param name="index">An index in the queue to put event.</param>
  public override void Prepare(
    IDialogManager dialog,
    IList<Event> queue, 
    int index)
  {
    if (Procedure.GetWindow(dialog, Window, false) != null)
    {
      base.Prepare(dialog, queue, index);
    }
  }

  /// <summary>
  /// Prepares to handle open event.
  /// Creates a window view and put it into list of opened windows.
  /// </summary>
  /// <param name="context">A <see cref="IContext"/> instance.</param>
  /// <returns>
  /// true when event is run and application logic can be called.
  /// </returns>
  public override bool Run(IContext context) 
  {
    // Put the current window into the list of openned windows.
    var name = Window;
    var window = Procedure.GetWindow(context.Dialog, name, false);

    if ((window == null) || (window.WindowState != WindowState.Closed))
    {
      return false;
    }

    window.WindowState = WindowState.Opening;
    Procedure.UpdateDefaultValues(window);

    return true;
  }

  /// <summary>
  /// Executes an after run logic, if any.
  /// </summary>
  /// <param name="context">a context instance.</param>
  /// <param name="run"><see cref="Run(IContext)"/> outcome.</param>
  public override void AfterRun(IContext context, bool run)
  {
    var name = Window;
    var dialog = context.Dialog;
    var window = Procedure.GetWindow(dialog, name, false);

    if (window?.WindowState == WindowState.Opening)
    {
      window.WindowState = WindowState.Opening;
		  context.QueueEvent(new ActivatedEvent(Procedure, Window, null));
    }
  }
}
