using System;
using System.Collections.Generic;

namespace Bphx.Cool.UI;

/// <summary>
/// A base class for UIListBox, UIDropdownList and others.
/// </summary>
[Serializable]
public class UIListBoxBase : UIInputControl
{
  /// <summary>
  /// Collection of <see cref="UIListItem"/> instances.
  /// </summary>
  public UICollection<UIListItem> ListItems
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
  /// List box items
  /// </summary>
  public IList<UIListItem> Items
  {
    get => ListItems.List;
    set
    {
      var listItems = new UICollection<UIListItem>();

      listItems.List.AddRange(value);

      ListItems = listItems;
      SetDirty();
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
      collection.SetOwner(window);
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
    if(string.Compare(name, "ListItems", true) == 0)
    {
      return ListItems;
    }
    else if(string.Compare(name, "ListRowItems", true) == 0)
    {
      return new UICollection<UIObject>();
    }
    else if(string.Compare(name, "ListRowItem", true) == 0)
    {
      return null;
    }
    else
    {
      return base.Invoke(name, args);
    }
  }

  private UICollection<UIListItem> collection;
}
