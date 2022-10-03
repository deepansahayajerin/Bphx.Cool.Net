using System;

namespace Bphx.Cool.UI;

/// <summary>
/// An Application type.
/// </summary>
[Serializable]
public class UIApplication: UIObject
{
  /// <summary>
  /// A reference to a <see cref="ISessionManager"/> instance.
  /// </summary>
  public ISessionManager SessionManager { get; set; }

  /// <summary>
  /// A reference to a <see cref="IContext"/> instance.
  /// </summary>
  public IContext Context { get; set; }

  /// <summary>
  /// Returns new handle id.
  /// </summary>
  /// <returns>A new handle id value.</returns>
  public int NewHandle() => ++lastHandle;

  /// <summary>
  /// Last object handle.
  /// </summary>
  private int lastHandle;
}
