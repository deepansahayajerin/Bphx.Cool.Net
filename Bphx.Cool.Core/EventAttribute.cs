using System;

namespace Bphx.Cool;

/// <summary>
/// An attribute defining an event handler.
/// </summary>
[AttributeUsage(
  AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field,
  Inherited = true, 
  AllowMultiple = true)]
public class EventAttribute : System.Attribute
{
  /// <summary>
  /// An event type.
  /// </summary>
  public string Type { get; set; }

  /// <summary>
  /// A component name, whose event is to be processed.
  /// </summary>
  public string Component { get; set; } = "";

  /// <summary>
  /// A window or dialog name, the component belongs to.
  /// </summary>
  public string Window { get; set; } = "";

  /// <summary>
  /// An optional command to pass with event.
  /// </summary>
  public string Command { get; set; } = "";

  /// <summary>
  /// Indicates whether to copy import to export before the event run.
  /// Returns true in case when copy import to export is required, 
  /// and false otherwise. Default value is true.
  /// </summary>
  public bool CopyImportToExport { get; set; } = true;
}
