using System;
using System.ComponentModel;

namespace Bphx.Cool.UI;

/// <summary>
/// OLEControl Interface Object.
/// </summary>
[Serializable]
public class UIOleControl : UIControl
{
  /// <summary>
  /// The indicator that the control was designed as the default button. 
  /// </summary>
  [DefaultValue(false)]
  public bool DisplayAsDefaultButton
  {
    get => displayAsDefaultButton;
    set
    {
      if(displayAsDefaultButton != value)
      {
        displayAsDefaultButton = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// <para>Text alignement within control.</para>
  /// <para>Valid values are:
  /// <list type="bullet">
  ///   <item><c>"Default"</c></item>
  ///   <item><c>"Left"</c></item>
  ///   <item><c>"Right"</c></item>
  ///   <item><c>"Centered"</c></item>
  ///   <item><c>"None"</c></item>
  /// </para>
  /// </summary>
  public string TextAlign
  {
    get => textAlign;
    set
    {
      if(textAlign != value)
      {
        textAlign = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// A border display state of the control.
  /// </summary>
  [DefaultValue(false)]
  public bool Border
  {
    get => border;
    set
    {
      if(border != value)
      {
        border = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// Display as default button indicator.
  /// </summary>
  protected bool displayAsDefaultButton;

  /// <summary>
  /// Text align.
  /// </summary>
  protected string textAlign;

  /// <summary>
  /// Border indicator.
  /// </summary>
  protected bool border;
}
