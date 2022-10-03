using System;

namespace Bphx.Cool;

/// <summary>
/// A class representing state shared by all screen fields.
/// </summary>
[Serializable]
public class ScreenFields
{
  /// <summary>
  /// Focused screen field, if any.
  /// </summary>
  public ScreenField Focused { get; set; }
}
