using System;

namespace Bphx.Cool.Events;

/// <summary>
/// Defines a deactivate window event.
/// </summary>
[Serializable]
public class DeactivatedEvent: Event
{
  /// <summary>
  /// An event type name.
  /// </summary>
  public const string EventType = "DEACTIVATED";

  /// <summary>
  /// Creates an instance of DeactivateEvent window event.
  /// </summary>
  /// <param name="procedure">A <see cref="IProcedure"/> instance.</param>
  /// <param name="type">An event type.</param>
  /// <param name="window">
  /// a window or dialog name, whose event is to be processed.
  /// </param>
  /// <param name="eventObject">
  /// An optional argument that represents client side event properties.
  /// </param>
  public DeactivatedEvent(
    IProcedure procedure,
    string window,
    EventObject eventObject = null) :
    base(procedure, EventType, window, null, eventObject)
  {
  }

  /// <summary>
  /// Deactivates the current window if it was active.
  /// </summary>
  /// <param name="context">A <see cref="IContext"/> instance.</param>
  /// <returns>
  /// true when event is run and application logic can be called.
  /// </returns>
  public override bool Run(IContext context) 
  {
    var name = Window;
    var procedure = Procedure;
    var window = procedure.ActiveWindow;
      
    if ((window != null) && (name == window.Name))
    {
      procedure.ActiveWindow = null;

      return true;
    }

    return false;
  }
}
