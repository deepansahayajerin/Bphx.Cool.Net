using System;
using System.ComponentModel;

namespace Bphx.Cool.UI;

/// <summary>
/// Menu Interface Object.
/// </summary>
[Serializable]
public class UIMenu : UIControl
{
  /// <summary>
  /// A marked indicator.
  /// </summary>
  [DefaultValue(false)]
  [State("marked")]
  public bool Marked
  {
    get => marked;
    set
    {
      if(marked != value)
      {
        marked = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// A mark indicator.
  /// </summary>
  protected bool marked;
}
