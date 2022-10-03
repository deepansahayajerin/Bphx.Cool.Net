using System;

namespace Bphx.Cool.UI;

/// <summary>
/// OLE TabControl Object.
/// </summary>
[Serializable]
public class UITabControl : UIOleControl
{
  /// <summary>
  /// The tab index.
  /// </summary>
  public int Index
  {
    get => index;
    set
    {
      if(index != value)
      {
        index = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// The tab index.
  /// </summary>
  private int index;
}
