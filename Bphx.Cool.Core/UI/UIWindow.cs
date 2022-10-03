using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

using Bphx.Cool.Xml;
using Bphx.Cool.Events;

using static Bphx.Cool.Functions;

namespace Bphx.Cool.UI;

/// <summary>
/// Window Interface Object
/// </summary>
[Serializable]
public class UIWindow: UIControl
{
  /// <summary>
  /// Owner <see cref="IProcedure"/> instance.
  /// </summary>
  public IProcedure Procedure { get; set; }

  /// <summary>
  /// A value indicating whether this is a primary window . 
  /// true if this is a primary window, and false otherwise.
  /// </summary>
  [DefaultValue(false)]
  [State("primaryWindow")]
  public bool PrimaryWindow { get; set; }

  /// <summary>
  /// A value indicating whether this is a modal window . 
  /// true if this is a modal window, and false otherwise.
  /// </summary>
  [DefaultValue(false)]
  [State("modal")]
  public bool Modal { get; set; }

  /// <summary>
  /// A lock index.
  /// </summary>
  [State("lock")]
  public int Lock { get; set; }

  /// <summary>
  /// A digest indicator.
  /// </summary>
  [DefaultValue(false)]
  [State("digest")]
  public bool Digest { get; set; }

  /// <summary>
  /// A window state.
  /// </summary>
  [State("windowState")]
  public WindowState WindowState { get; set; }

  /// <summary>
  /// A visible state.
  /// </summary>
  public override bool Visible
  {
    get => (WindowState == WindowState.Opened) && VisibleValue;
    set
    {
      var context = Application?.Context;

      if (value && (WindowState == WindowState.Closed) && (context != null))
      {
        var eventObject = new EventObject
        {
          Type = OpenEvent.EventType,
          Window = Name
        };

        var e = Event.Create(Procedure, eventObject);

        e.Cancelable = false;
        context.QueueEvent(e);
      }

      VisibleValue = value;
    }
  }

  /// <summary>
  /// Marks default state of the control.
  /// </summary>
  public override void SetDefault()
  {
    isDefaultValue = true;
    isDefaultForegroundColor = true;
    isDefaultBackgroundColor = true;
  }

  /// <summary>
  /// Display mode of a Window bitmap background. 
  /// Valid return values are "Centered", "Tiled" or "Scaled". 
  /// </summary>
  [State("bitmapDisplayMode")]
  public string BitmapDisplayMode { get; set; }

  /// <summary>
  /// A value indicating the Window's maximized state.
  /// </summary>
  [DefaultValue(false)]
  [State("maximized")]
  public bool Maximized { get; set; }

  /// <summary>
  /// A value indicating the Window's minimized state.
  /// </summary>
  [DefaultValue(false)]
  [State("minimized")]
  public bool Minimized { get; set; }

  /// <summary>
  /// Indicates whether to display message box for an 
  /// informational exit state message.
  /// </summary>
  [DefaultValue(false)]
  public bool DisplayExitStateMessage { get; set; }

  /// <summary>
  /// An error message snapshot.
  /// </summary>
  [State("errmsg")]
  public string Errmsg { get; set; }

  /// <summary>
  /// Gets and sets exit state message type.
  /// </summary>
  [DefaultValue(MessageType.None)]
  [State("messageType")]
  public MessageType MessageType { get; set; }

  /// <summary>
  /// An object name.
  /// </summary>
  public override string Caption
  {
    get => base.Caption;
    set
    {
      if (!Equal(value, caption))
      {
        base.Caption = TrimEnd(value);
        isDefaultCaption = false;
      }
    }
  }

  /// <summary>
  /// Gets a collection of UIControl instances.
  /// </summary>
  public IList<UIControl> Controls
  {
    get
    {
      return controls;
    }
    set
    {
      collections.Clear();
      controlsByName.Clear();
      controls.Clear();
      ControlsWithDefaultValues.Clear();
      ControlsWithCommands.Clear();

      if (value != null)
      {
        controls.AddRange(value);
      }

      CreateCollection<UIButton>();
      CreateCollection<UICheckBox>();
      CreateCollection<UIDropDownList>();
      CreateCollection<UIEnterableDropDownList>();
      CreateCollection<UIEnterableListBox>();
      CreateCollection<UIField>();
      CreateCollection<UIGroupBox>();
      CreateCollection<UIListBox>();
      CreateCollection<UILiteral>();
      CreateCollection<UIMenu>();
      CreateCollection<UIOleArea>();
      //CreateCollection<UIOLEControl>();
      CreateCollection<UIPicture>();
      CreateCollection<UIRadioButtonGroup>();

      foreach(var control in controls)
      {
        var name = control.Name;
        var collection = GetCollection(controls.GetType());

        if (collection != null)
        {
          collection.List.Add(control);
        }

        control.SetOwner(this);

        if (!IsEmpty(control.DefaultValue) && !IsEmpty(control.Binding))
        {
          ControlsWithDefaultValues.Add(control);
        }

        if (!IsEmpty(control.Command))
        {
          if (ControlsWithCommands.TryGetValue(control.Command, out var list))
          {
            list.Add(control);
          }
          else
          {
            ControlsWithCommands.Add(control.Command, new() { control });
          }
        }

        if (IsEmpty(name))
        {
          continue;
        }

        controlsByName[name] = control;
      }
    }
  }

  /// <summary>
  /// A map of controls by name.
  /// </summary>
  [State("controls")]
  public IDictionary<string, UIControl> ControlsByName => controlsByName;

  /// <summary>
  /// Buttons of the window.
  /// </summary>
  public UICollection<UIButton> Buttons => GetCollection<UIButton>();

  /// <summary>
  /// CheckBoxes of the window.
  /// </summary>
  public UICollection<UICheckBox> Checkboxes => GetCollection<UICheckBox>();

  /// <summary>
  /// Menus of the window.
  /// </summary>
  public UICollection<UIMenu> Menus => GetCollection<UIMenu>();

  /// <summary>
  /// DropDownLists of the window.
  /// </summary>
  public UICollection<UIDropDownList> DropDownLists =>
    GetCollection<UIDropDownList>();

  /// <summary>
  /// EnterableDropDownLists of the window.
  /// </summary>
  public UICollection<UIEnterableDropDownList> EnterableDropDownLists =>
    GetCollection<UIEnterableDropDownList>();

  /// <summary>
  /// EnterableListBoxes of the window.
  /// </summary>
  public UICollection<UIEnterableListBox> EnterableListBoxes =>
    GetCollection<UIEnterableListBox>();

  /// <summary>
  /// Fields of the window.
  /// </summary>
  public UICollection<UIField> Fields => GetCollection<UIField>();

  /// <summary>
  /// GroupBoxes of the window.
  /// </summary>
  public UICollection<UIGroupBox> GroupBoxes => GetCollection<UIGroupBox>();

  /// <summary>
  /// ListBoxes of the window.
  /// </summary>
  public UICollection<UIListBox> ListBoxes => GetCollection<UIListBox>();

  /// <summary>
  /// Literals of the window.
  /// </summary>
  public UICollection<UILiteral> Literals => GetCollection<UILiteral>();

  /// <summary>
  /// OLE areas of the window.
  /// </summary>
  public UICollection<UIOleArea> OleAreas => GetCollection<UIOleArea>();

  /// <summary>
  /// OLE Controls of the window.
  /// </summary>
  public UICollection<UIControl> OleControls => GetCollection<UIControl>();

  /// <summary>
  /// Pictures of the window.
  /// </summary>
  public UICollection<UIPicture> Pictures => GetCollection<UIPicture>();

  /// <summary>
  /// Radio button groups of the window.
  /// </summary>
  public UICollection<UIRadioButtonGroup> RadioButtonGroups =>
    GetCollection<UIRadioButtonGroup>();

  /// <summary>
  /// A status bar.
  /// </summary>
  public UIStatusBar StatusBar =>
    Controls.OfType<UIStatusBar>().FirstOrDefault();

  /// <summary>
  /// Controls that support bindings, have a binding and default values.
  /// </summary>
  public List<UIControl> ControlsWithDefaultValues { get; } = new();

  /// <summary>
  /// Controls groupped per commands.
  /// </summary>
  public Dictionary<string, List<UIControl>> ControlsWithCommands { get; } = new();

  /// <summary>
  /// <see cref="UIObject"/> handles cached by the window.
  /// </summary>
  public Dictionary<int, UIObject> Handles { get; } = new();

  /// <summary>
  /// A tool bar.
  /// </summary>
  public UIToolBar ToolBar => Controls.OfType<UIToolBar>().FirstOrDefault();

  /// <summary>
  /// A focus state of the control.
  /// </summary>
  public override bool Focus
  {
    get => (Procedure != null) &&
      (Procedure.ActiveWindow == this) &&
      (Procedure == Application?.SessionManager.
        GetProcedureThatCanAcceptRequest());
    set
    {
      if (value && !Focus)
      {
        var context = Application?.Context;

        if (context != null)
        {
          var eventObject = new EventObject
          {
            Type = ActivatedEvent.EventType,
            Window = Name
          };

          var e = Event.Create(Procedure, eventObject);

          e.Cancelable = false;
          context.QueueEvent(e);
        }
      }
    }
  }

  /// <summary>
  /// Focused control name.
  /// </summary>
  [State("focused")]
  public string Focused
  {
    get => Digest ? focusedControlName : FocusedControl?.Name;
    set
    {
      if (Digest)
      {
        focusedControlName = value;
      }
      else
      {
        FocusedControl = IsEmpty(value) ? null : GetControl(value);
      }
    }
  }

  /// <summary>
  /// Focused control.
  /// </summary>
  public UIControl FocusedControl
  {
    get
    {
      return focusedControl;
    }
    set
    {
      var focused = focusedControl;

      if (focused != value)
      {
        if ((value != null) && (value.Owner != this))
        {
          return;
        }

        focusedControl = null;

        if (focused != null)
        {
          focused.Focus = false;
        }

        focusedControl = value;

        if (value != null)
        {
          value.Focus = true;
        }
      }
    }
  }

  /// <summary>
  /// A value indicating whether the Window is resizable or not.
  /// </summary>
  [DefaultValue(false)]
  [State("resizable")]
  public bool Resizable { get; set; }

  /// <summary>
  /// Initial window position.
  /// </summary>
  [DefaultValue(InitialPosition.Designed)]
  [State("position")]
  public InitialPosition Position { get; set; }

  /// <summary>
  /// A height of the control.
  /// </summary>
  public override int? Height
  {
    get => base.Height;
    set { }
  }

  /// <summary>
  /// A width of the control.
  /// </summary>
  public override int? Width
  {
    get => base.Width;
    set { }
  }

  /// <summary>
  /// Dirty indicator.
  /// </summary>
  public bool Dirty { get; private set; }

  /// <summary>
  /// Sets dirty indicator of the window.
  /// </summary>
  public override void SetDirty()
  {
    if (lockDirty)
    {
      return;
    }

    Dirty = true;

    if (Procedure != null)
    {
      Procedure.Dirty = true;
    }
  }

  /// <summary>
  /// Resets dirty state of the window.
  /// </summary>
  public override void ResetDirty()
  {
    if (lockDirty)
    {
      return;
    }

    foreach(var control in controls)
    {
      control.ResetDirty();
    }

    base.ResetDirty();
  }

  /// <summary>
  /// Locks dirty state.
  /// </summary>
  public override void LockDirty() => lockDirty = true;

  /// <summary>
  /// Unlocks dirty state.
  /// </summary>
  public override void UnlockDirty() => lockDirty = false;

  /// <summary>
  /// Gets a control by name.
  /// </summary>
  /// <param name="name">the control's name.</param>
  /// <returns>
  /// UIControl instance or null.
  /// </returns>
  public UIControl GetControl(String name)
  {
    return GetControl<UIControl>(name) ?? null;
  }

  /// <summary>
  /// Gets typed control by the name.
  /// </summary>
  /// <typeparam name="T">A control type.</typeparam>
  /// <param name="name">A control name.</param>
  /// <returns>
  /// A control instance, or null if no control is available.
  /// </returns>
  public T GetControl<T>(string name)
    where T : UIControl, new()
  {
    if (string.IsNullOrEmpty(name))
    {
      return null;
    }

    name = name.Trim();

    var p = name.IndexOf('.');

    if (p >= 0)
    {
      name = name[(p + 1)..];
    }

    var control = name == Name ? this : controlsByName.Get(name);

    return (T)control;
  }

  /// <summary>
  /// Gets <see cref="UIButton"/> instance by name.
  /// </summary>
  /// <param name="name">A control name.</param>
  /// <returns><see cref="UIButton"/> instance or null.</returns>
  public UIButton GetButton(string name)
  {
    return GetControl<UIButton>(name);
  }

  /// <summary>
  /// Gets <see cref="UICheckBox"/> instance by name.
  /// </summary>
  /// <param name="name">A control name.</param>
  /// <returns><see cref="UICheckBox"/> instance or null.</returns>
  public UICheckBox GetCheckBox(string name)
  {
    return GetControl<UICheckBox>(name);
  }

  /// <summary>
  /// Gets <see cref="UIMenu"/> instance by name.
  /// </summary>
  /// <param name="name">A control name.</param>
  /// <returns><see cref="UIMenu"/> instance or null.</returns>
  public UIMenu GetMenu(string name)
  {
    return GetControl<UIMenu>(name);
  }

  /// <summary>
  /// Gets <see cref="UIDropDownList"/> instance by name.
  /// </summary>
  /// <param name="name">A control name.</param>
  /// <returns><see cref="UIDropDownList"/> instance or null.</returns>
  public UIDropDownList GetDropDownList(string name)
  {
    return GetControl<UIDropDownList>(name);
  }

  /// <summary>
  /// Gets <see cref="UIEnterableDropDownList"/> instance by name.
  /// </summary>
  /// <param name="name">A control name.</param>
  /// <returns>
  /// <see cref="UIEnterableDropDownList"/> instance or null.
  /// </returns>
  public UIEnterableDropDownList GetEnterableDropDownList(string name)
  {
    return GetControl<UIEnterableDropDownList>(name);
  }

  /// <summary>
  /// Gets <see cref="UIEnterableListBox"/> instance by name.
  /// </summary>
  /// <param name="name">A control name.</param>
  /// <returns><see cref="UIEnterableListBox"/> instance or null.</returns>
  public UIEnterableListBox GetEnterableListBox(string name)
  {
    return GetControl<UIEnterableListBox>(name);
  }

  /// <summary>
  /// Gets <see cref="UIField"/> instance by name.
  /// </summary>
  /// <param name="name">A control name.</param>
  /// <returns><see cref="UIField"/> instance or null.</returns>
  public UIField GetField(string name)
  {
    return GetControl<UIField>(name);
  }

  /// <summary>
  /// Gets <see cref="UIGroupBox"/> instance by name.
  /// </summary>
  /// <param name="name">A control name.</param>
  /// <returns><see cref="UIGroupBox"/> instance or null.</returns>
  public UIGroupBox GetGroupBox(string name)
  {
    return GetControl<UIGroupBox>(name);
  }

  /// <summary>
  /// Gets <see cref="UIListBox"/> instance by name.
  /// </summary>
  /// <param name="name">A control name.</param>
  /// <returns><see cref="UIListBox"/> instance or null.</returns>
  public UIListBox GetListBox(string name)
  {
    return GetControl<UIListBox>(name);
  }

  /// <summary>
  /// Gets <see cref="UILiteral"/> instance by name.
  /// </summary>
  /// <param name="name">A control name.</param>
  /// <returns><see cref="UILiteral"/> instance or null.</returns>
  public UILiteral GetLiteral(string name)
  {
    return GetControl<UILiteral>(name);
  }

  /// <summary>
  /// Gets <see cref="UIOleArea"/> instance by name.
  /// </summary>
  /// <param name="name">A control name.</param>
  /// <returns><see cref="UIOleArea"/> instance or null.</returns>
  public UIOleArea GetOleArea(string name)
  {
    return GetControl<UIOleArea>(name);
  }

  /// <summary>
  /// Gets <see cref="UIOleControl"/> instance by name.
  /// </summary>
  /// <param name="name">A control name.</param>
  /// <returns><see cref="UIOleControl"/> instance or null.</returns>
  public UIControl GetOleControl(string name)
  {
    return GetControl<UIControl>(name);
  }

  /// <summary>
  /// Gets <see cref="UIPicture"/> instance by name.
  /// </summary>
  /// <param name="name">A control name.</param>
  /// <returns><see cref="UIPicture"/> instance or null.</returns>
  public UIPicture GetPicture(string name)
  {
    return GetControl<UIPicture>(name);
  }

  /// <summary>
  /// Gets <see cref="UIRadioButtonGroup"/> instance by name.
  /// </summary>
  /// <param name="name">A control name.</param>
  /// <returns><see cref="UIRadioButtonGroup"/> instance or null.</returns>
  public UIRadioButtonGroup GetRadioButtonGroup(string name)
  {
    return GetControl<UIRadioButtonGroup>(name);
  }

  /// <summary>
  /// Invokes a method of <see cref="UIObject"/> instance.
  /// </summary>
  /// <param name="name">A method name.</param>
  /// <param name="args">Method's arguments.</param>
  /// <returns>Result type.</returns>
  public override object Invoke(string name, params object[] args)
  {
    if (name != null)
    {
      switch(name.ToLower())
      {
        case "buttons":
        {
          return Buttons;
        }
        case "checkboxes":
        {
          return Checkboxes;
        }
        case "dropdownlists":
        {
          return DropDownLists;
        }
        case "enterabledropdownlists":
        {
          return EnterableDropDownLists;
        }
        case "enterablelistboxes":
        {
          return EnterableListBoxes;
        }
        case "fields":
        {
          return Fields;
        }
        case "groupboxes":
        {
          return GroupBoxes;
        }
        case "listboxes":
        {
          return ListBoxes;
        }
        case "literals":
        {
          return Literals;
        }
        case "menus":
        {
          return Menus;
        }
        case "oleareas":
        {
          return OleAreas;
        }
        case "olecontrols":
        {
          return OleControls;
        }
        case "pictures":
        {
          return Pictures;
        }
        case "radiobuttongroups":
        {
          return RadioButtonGroups;
        }

        case "button":
        {
          return GetButton(Convert.ToString(args[0]));
        }
        case "checkboxe":
        {
          return GetCheckBox(Convert.ToString(args[0]));
        }
        case "dropdownlist":
        {
          return GetDropDownList(Convert.ToString(args[0]));
        }
        case "enterabledropdownlist":
        {
          return GetEnterableDropDownList(
            Convert.ToString(args[0]));
        }
        case "enterablelistbox":
        {
          return GetEnterableListBox(Convert.ToString(args[0]));
        }
        case "field":
        {
          return GetField(Convert.ToString(args[0]));
        }
        case "groupbox":
        {
          return GetGroupBox(Convert.ToString(args[0]));
        }
        case "listbox":
        {
          return GetListBox(Convert.ToString(args[0]));
        }
        case "literal":
        {
          return GetLiteral(Convert.ToString(args[0]));
        }
        case "menu":
        {
          return GetMenu(Convert.ToString(args[0]));
        }
        case "olearea":
        {
          return GetOleArea(Convert.ToString(args[0]));
        }
        case "olecontrol":
        {
          return GetOleControl(Convert.ToString(args[0]));
        }
        case "picture":
        {
          return GetPicture(Convert.ToString(args[0]));
        }
        case "radiobuttongroup":
        {
          return GetRadioButtonGroup(Convert.ToString(args[0]));
        }
      }
    }

    return base.Invoke(name, args);
  }

  /// <summary>
  /// Creates and appends a type collection of UI controls to the
  /// map of all typed collections.
  /// </summary>
  /// <typeparam name="T">An element type.</typeparam>
  protected UICollection<T> CreateCollection<T>()
    where T : UIObject
  {
    var collection = new UICollection<T>();

    collection.SetOwner(this);
    collections[typeof(T)] = collection;

    return collection;
  }

  /// <summary>
  /// Retrieves a typed UI collection.
  /// </summary>
  /// <typeparam name="T">A collection type.</typeparam>
  /// <returns>Typed UI collection.</returns>
  protected UICollection<T> GetCollection<T>()
    where T : UIObject
  {
    return (UICollection<T>)GetCollection(typeof(T));
  }

  /// <summary>
  /// Retrieves a typed UI collection.
  /// </summary>
  /// <typeparam name="T">A collection type.</typeparam>
  /// <returns>Typed UI collection.</returns>
  protected UICollection<T> GetOrCreateCollection<T>()
    where T : UIObject
  {
    return GetCollection<T>() ?? CreateCollection<T>();
  }

  /// <summary>
  /// Retieves a UI collection.
  /// </summary>
  /// <param name="type">A collection type.</param>
  /// <returns>A UI collection.</returns>
  protected IUICollection GetCollection(Type type)
  {
    return collections.Get(type);
  }

  /// <summary>
  /// A list of controls.
  /// </summary>
  private readonly List<UIControl> controls = new();

  /// <summary>
  /// A map of controls by name.
  /// </summary>
  private readonly Dictionary<string, UIControl> controlsByName = new();

  /// <summary>
  /// A map of collections.
  /// </summary>
  protected readonly Dictionary<Type, IUICollection> collections = new();

  /// <summary>
  /// Focused control.
  /// </summary>
  private UIControl focusedControl;

  /// <summary>
  /// Focused control in case of digest.
  /// </summary>
  private string focusedControlName;

  /// <summary>
  /// LockDirty indicator.
  /// </summary>
  private bool lockDirty;
}
