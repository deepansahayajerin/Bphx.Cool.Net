using System;
using System.ComponentModel;

namespace Bphx.Cool.UI;

/// <summary>
/// Field Interface Object.
/// </summary>
[Serializable]
public class UIField : UIInputControl
{
  /// <summary>
  /// Multi- or single- line indicator.
  /// </summary>
  [DefaultValue(false)]
  public bool Multiline
  {
    get => multiline;
    set
    {
      if(multiline != value)
      {
        multiline = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// Resets dirty state of the object.
  /// </summary>
  public override void ResetDirty()
  {
    SetAttribute("SelectionStart", null);
    SetAttribute("SelectionEnd", null);
  }

  /// <summary>
  /// Sets the selection.
  /// </summary>
  /// <param name="start">A selection start.</param>
  /// <param name="end">A selection end.</param>
  public virtual void SetSelection(int start, int end)
  {
    SetAttribute("SelectionStart", start);
    SetAttribute("SelectionEnd", end);
    Focus = true;
  }

  /// <summary>
  /// Invokes a method of <see cref="UIObject"/> instance.
  /// </summary>
  /// <param name="name">A method name.</param>
  /// <param name="args">Method's arguments.</param>
  /// <returns>Result type.</returns>
  public override object Invoke(string name, params object[] args)
  {
    if(string.Compare(name, "SetSelection", true) == 0)
    {
      if(args.Length == 2)
      {
        SetSelection(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]));
      }

      return null;
    }

    return base.Invoke(name, args);
  }

  /// <summary>
  /// Multi- or single- line indicator.
  /// </summary>
  private bool multiline;
}
