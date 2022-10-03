using System;

namespace Bphx.Cool;

/// <summary>
///   <para>Defines a command view.</para>
///   <para>
///    Determines command states enabled/disabled, marked/unmarked, and
///    optionaly autoflow indicator.
///   </para>
/// </summary>
[Serializable]
public class CommandView : INamed
{
  /// <summary>
  /// Gets and sets command name.
  /// </summary>
  public string Name { get; set; }

  /// <summary>
  /// Gets or sets optional auto flow indicator.
  /// </summary>
  public bool Autoflow { get; set; }

  /// <summary>
  /// Returns the CommandView name for debug purposes.
  /// </summary>
  /// <returns>A command name.</returns>
  public override string ToString() => Name;
}
