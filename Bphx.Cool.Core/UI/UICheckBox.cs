using System;

namespace Bphx.Cool.UI;

/// <summary>
/// CheckBox Interface Object.
/// </summary>
[Serializable]
public class UICheckBox : UIInputControl
{
  /// <summary>
  /// Current status of the CheckBox. 
  /// </summary>
  public bool Selected
  {
    get { return string.Compare(Value, "on", true) == 0; }
    set { Value = value ? "on" : "off"; }
  }
}
