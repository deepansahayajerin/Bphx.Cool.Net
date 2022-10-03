using System;
using System.Collections.Generic;

using Bphx.Cool.Events;

namespace Bphx.Cool.UI;

/// <summary>
/// OLE TabsetControl Object.
/// </summary>
[Serializable]
public class UITabsetControl : UIOleControl
{
  /// <summary>
  /// Gets the number of tabs to display in the Tabset control.
  /// </summary>
  public int TabCount => TabControls.Count;

  /// <summary>
  /// Gets collection of <see cref="UITabControl"/> instances.
  /// </summary>
  public UICollection<UITabControl> TabControls
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
  /// Gets and sets a list of tab controls.
  /// </summary>
  [State("Tabs")]
  public List<UITabControl> Tabs
  {
    get => TabControls.List;
    set
    {
      var tabControls = new UICollection<UITabControl>();

      tabControls.List.AddRange(value);

      TabControls = tabControls;
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

    TabControls.SetOwner(window);
  }

  /// <summary>
  /// Gets and sets the control's value, 
  /// which is an alias of activeTab property.
  /// </summary>
  public override string Value
  {
    get => ActiveTab.ToString();
    set => SetActiveTab(Convert.ToInt32(value), false);
  }

  /// <summary>
  /// Gets and sets an index of the active page.
  /// </summary>
  public int ActiveTab
  {
    get => activeTab;
    set => SetActiveTab(value, true);
  }

  /// <summary>
  /// Gets and sets an index of the current tab.
  /// </summary>
  public int Tab
  {
    get => tab;
    set
    {
      if(tab != value)
      {
        tab = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// A state of the current tab.
  /// </summary>
  public int TabState
  {
    get
    {
      var state = 0;
      var tab = Tabs[Tab];

      if(!tab.Visible)
      {
        state += 1;
      }

      if(!tab.Enabled)
      {
        state += 2;
      }

      return state;
    }
    set
    {
      var tab = Tabs[Tab];

      tab.Visible = ((value & 1) == 0);
      tab.Enabled = ((value & 2) == 0);
    }
  }

  /// <summary>
  /// A caption of the current tab.
  /// </summary>
  public string TabCaption
  {
    get
    {
      return Tabs[Tab]?.Caption;
    }
    set
    {
      var tab = Tabs[Tab];

      if(tab != null)
      {
        tab.Caption = value;
      }
    }
  }

  /// <summary>
  /// Sets active tab, and triggers event on change, if requested.
  /// </summary>
  /// <param name="value">A value to set.</param>
  /// <param name="triggerEvent">
  /// Indicates whether to trigger event on change.
  /// </param>
  public void SetActiveTab(int value, bool triggerEvent)
  {
    if(activeTab != value)
    {
      activeTab = value;
      SetDirty();

      if(triggerEvent)
      {
        var window = Owner;
        var procedure = window?.Procedure;
        var context = Application?.Context;

        if(context != null)
        {
          var eventObject = new EventObject
          {
            Type = "TabPageActivate",
            Window = window.Name,
            Component = Name,
            Attributes =
              {
                { "TabToActivate", value },
                { "PageToActivate", value },
              }
          };

          context.QueueEvent(Event.Create(procedure, eventObject));
        }
      }
    }
  }

  /// <summary>
  /// Invokes a method of <see cref="UITabsetControl"/> instance.
  /// </summary>
  /// <param name="name">A method name.</param>
  /// <param name="args">Method's arguments.</param>
  /// <returns>Result type.</returns>
  public override object Invoke(string name, params object[] args)
  {
    if((string.Compare(name, "Tabs", true) == 0) &&
      (args != null) && (args.Length == 1))
    {
      var index = Convert.ToInt32(args[0]);

      return TabControls.List[index];
    }

    return base.Invoke(name, args);
  }

  private UICollection<UITabControl> collection;

  /// <summary>
  /// Gets and sets an index of the active page.
  /// </summary>
  private int activeTab;

  /// <summary>
  /// Gets and sets an index of the current tab.
  /// </summary>
  private int tab;
}
