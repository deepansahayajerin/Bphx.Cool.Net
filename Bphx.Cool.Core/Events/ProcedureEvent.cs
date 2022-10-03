using System;

namespace Bphx.Cool.Events;

/// <summary>
/// Defines procedure event.
/// </summary>
[Serializable]
public class ProcedureEvent: Event
{
  /// <summary>
  /// Creates a <see cref="ProcedureEvent"/> instance.
  /// </summary>
  /// <param name="procedure">A <see cref="IProcedure"/> instance.</param>
  /// <param name="type">An event type.</param>
  /// <param name="eventObject">
  /// An optional argument that represents client side event properties.
  /// </param>
  /// <param name="onRun">Optional on run handler.</param>
  public ProcedureEvent(
    IProcedure procedure,
    string type,
    EventObject eventObject = null,
    Func<IContext, bool> onRun = null) :
    base(procedure, type, null, null, eventObject) => 
    this.onRun = onRun;

  /// <summary>
  ///   <para>Executes a logic of event itself.</para>
  ///   <para>Note that this contrasts with application logic.</para>
  /// </summary>
  /// <param name="context">A <see cref="IContext"/> instance.</param>
  /// <returns>
  /// true when event is run and application logic can be called.
  /// </returns>
  public override bool Run(IContext context) => 
    base.Run(context) && (onRun?.Invoke(context) != false);

  /// <summary>
  /// Optional run handler.
  /// </summary>
  [NonSerialized]
  private readonly Func<IContext, bool> onRun;
}
