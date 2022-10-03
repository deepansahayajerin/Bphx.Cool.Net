using System;

namespace Bphx.Cool.Events;

/// <summary>
/// Defines an event that's fired on content changed.
/// </summary>
[Serializable]
public class CommandEvent: Event
{
  /// <summary>
  /// An event type name.
  /// </summary>
  public const string EventType = "COMMAND";

  /// <summary>
  /// Creates a <see cref="CommandEvent"/> instance.
  /// </summary>
  /// <param name="procedure">A <see cref="IProcedure"/> instance.</param>
  /// <param name="command">A component name, the event originator.</param>
  /// <param name="eventObject">
  /// An optional argument that represents client side event properties.
  /// </param>
  public CommandEvent(
    IProcedure procedure,
    string command,
    EventObject eventObject = null) :
    base(procedure, EventType, null, null, eventObject) => 
    Command = command;

  /// <summary>
  /// A command value.
  /// </summary>
  public string Command { get; }
}
