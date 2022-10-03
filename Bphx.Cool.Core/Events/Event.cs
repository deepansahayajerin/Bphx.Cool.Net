using System;
using System.Collections.Generic;

using Bphx.Cool.UI;

using static Bphx.Cool.Functions;

namespace Bphx.Cool.Events;

/// <summary>
/// Defines a base event.
/// </summary>
[Serializable]
public class Event
{
  /// <summary>
  /// Gets event key.
  /// </summary>
  /// <param name="type">An event type.</param>
  /// <param name="window">A window name.</param>
  /// <param name="component">A component name.</param>
  /// <returns>An event key.</returns>
  public static string GetKey(string type, string window, string component)
  {
    type = type.Replace(' ', '-').Replace('.', '-');

    return IsEmpty(component) ?
      window + "." + type :
      window + "." + component + "." + type;
  }

  /// <summary>
  /// Creates an instance of the window event.
  /// </summary>
  /// <param name="procedure">A <see cref="IProcedure"/> instance.</param>
  /// <param name="eventObject">
  /// An argument that represents client side event properties.
  /// </param>
  /// <returns>An WindowEvent instance.</returns>
  public static Event Create(IProcedure procedure, EventObject eventObject)
  {
    if (eventObject == null)
    {
      throw new ArgumentNullException(nameof(eventObject));
    }

    var type = eventObject.Type?.Trim().ToUpper();

    if (IsEmpty(type))
    {
      throw new ArgumentException("Invalid event type", nameof(eventObject));
    }

    var window = eventObject.Window;
    var component = eventObject.Component;
    Event anEvent;
    var cancellable = true;

    // create an event instance
    switch(type)
    {
      case OpenEvent.EventType:
      {
        eventObject.Window = window;
        eventObject.Component = null;
        anEvent = new OpenEvent(procedure, window, eventObject);

        break;
      }
      case CloseEvent.EventType:
      {
        eventObject.Window = window;
        eventObject.Component = null;
        anEvent = new CloseEvent(procedure, window, eventObject);

        break;
      }
      case ActivatedEvent.EventType:
      {
        eventObject.Window = window;
        eventObject.Component = null;
        anEvent = new ActivatedEvent(procedure, window, eventObject);

        break;
      }
      case DeactivatedEvent.EventType:
      {
        eventObject.Window = window;
        eventObject.Component = null;
        anEvent = new DeactivatedEvent(procedure, window, eventObject);

        break;
      }
      case CommandEvent.EventType:
      {
        anEvent = 
          new CommandEvent(procedure, eventObject.Command, eventObject);

        break;
      }
      case "CHANGED":
      case "GAINFOCUS":
      case "LOSEFOCUS":
      {
        cancellable = false;

        goto default;
      }
      default:
      {
        // a common event
        anEvent = new Event(procedure, type, window, component, eventObject);

        break;
      }
    }

    anEvent.Cancelable = cancellable;

    return anEvent;
  }

  /// <summary>
  /// Creates an instance of the window event.
  /// </summary>
  /// <param name="procedure">A <see cref="IProcedure"/> instance.</param>
  /// <param name="type">An event type.</param>
  /// <param name="window">A window or dialog box name.</param>
  /// <param name="component">
  /// A component name, whose event is to be processed.
  /// </param>
  /// <param name="eventObject">
  /// An optional argument that represents client side event properties.
  /// </param>
  public Event(
    IProcedure procedure,
    string type,
    string window,
    string component = null,
    EventObject eventObject = null)
  {
    type = type == null ? "" : 
      type.TrimEnd().Replace(' ', '-').Replace('.', '-');

    if (window == null)
    {
      window = "";
    }

    if (component == null)
    {
      component = "";
    }

    Procedure = procedure ?? 
      throw new ArgumentNullException(nameof(procedure));
    Type = type;
    Window = window;
    Component = component;
    EventObject = eventObject ??
      new()
      {
        Type = type,
        Window = window,
        Component = component
      };
  }

  /// <summary>
  /// Prepares an event, puts it in the specified queue. 
  /// </summary>
  /// <param name="dialog">An <see cref="IDialogManager"/> instance.</param>
  /// <param name="queue">An event queue.</param>
  /// <param name="index">An index in the queue to put event.</param>
  public virtual void Prepare(
    IDialogManager dialog,
    IList<Event> queue,
    int index) => 
    queue.Insert(index, this);

  /// <summary>
  ///   <para>Executes a logic of event itself.</para>
  ///   <para>Note that this contrasts with application logic.</para>
  /// </summary>
  /// <param name="context">A <see cref="IContext"/> instance.</param>
  /// <returns>
  /// true when event is run and application logic can be called.
  /// </returns>
  public virtual bool Run(IContext context)
  {
    var emptyWindow = IsEmpty(Window);
    var window = emptyWindow ? null : 
      Procedure.GetWindow(context.Dialog, Window, false);

    if (Cancelable &&
      !emptyWindow &&
      ((window == null) || (window.WindowState == WindowState.Closed)))
    {
      return false;
    }

    var control = window == null ? null :
      IsEmpty(Component) ? window : window.GetControl(Component);

    Control = control;

    return (control == null) || control.Run(context, this);
  }

  /// <summary>
  /// Executes an after run logic, if any.
  /// </summary>
  /// <param name="context">a context instance.</param>
  /// <param name="run"><see cref="Run(IContext)"/> outcome.</param>
  public virtual void AfterRun(IContext context, bool run)
  {
    Control?.AfterRun(context, this, run);
  }

  /// <summary>
  /// A <see cref="IProcedure"/> instance.
  /// </summary>
  public IProcedure Procedure { get; }

  /// <summary>
  /// Gets event type.
  /// </summary>
  public string Type { get; }

  /// <summary>
  /// Gets window or dialog box name.
  /// </summary>
  public string Window { get; }

  /// <summary>
  /// Gets a component name.
  /// </summary>
  public string Component { get; }

  /// <summary>
  /// Gets an event object, if any.
  /// </summary>
  public EventObject EventObject { get; }

  /// <summary>
  /// Canceled indicator.
  /// </summary>
  public bool Canceled { get; set; }

  /// <summary>
  /// Cancelable indicator.
  /// </summary>
  public bool Cancelable { get; set; }

  /// <summary>
  /// Indicates that event is queued by the client.
  /// </summary>
  public bool Client { get; set; }

  /// <summary>
  /// Control associated with the event.
  /// </summary>
  public UIControl Control { get; set; }

  /// <summary>
  /// Returns the event's identifier.
  /// </summary>
  public string Key { get => GetKey(Type, Window, Component); }
}
