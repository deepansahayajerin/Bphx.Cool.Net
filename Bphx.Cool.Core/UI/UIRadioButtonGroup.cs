using System;

namespace Bphx.Cool.UI;

/// <summary>
/// RadioButtonGroup Interface Object.
/// </summary>
[Serializable]
public class UIRadioButtonGroup : UIControl
{
  /// <summary>
  /// Indicates whether the control supports binding.
  /// </summary>
  public override bool SupportsBinding => true;

  /// <summary>
  /// Radio buttons.
  /// </summary>
  public UICollection<UIRadioButton> RadioButtons
  {
    get
    {
      if(collection == null)
      {
        collection = new();
        collection.SetOwner(Owner);
      }

      return collection;
    }
    set
    {
      if(collection != value)
      {
        if(collection != null)
        {
          collection.SetOwner(null);
        }

        collection = value;

        if(value != null)
        {
          value.SetOwner(Owner);
        }

        SetDirty();
      }
    }
  }

  /// <summary>
  /// Sets the owner window.
  /// </summary>
  /// <param name="window">An owner window.</param>
  public override void SetOwner(UIWindow window)
  {
    base.SetOwner(window);

    if(collection != null)
    {
      collection.SetOwner(Owner);
    }
  }

  /// <summary>
  /// Invokes a method of <see cref="UIObject"/> instance.
  /// </summary>
  /// <param name="name">A method name.</param>
  /// <param name="args">Method's arguments.</param>
  /// <returns>Result type.</returns>
  public override object Invoke(string name, params object[] args)
  {
    if(string.Compare(name, "RadioButtons", true) == 0)
    {
      return RadioButtons;
    }

    return base.Invoke(name, args);
  }

  private UICollection<UIRadioButton> collection;
}
