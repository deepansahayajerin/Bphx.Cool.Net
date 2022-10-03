using System;
using System.ComponentModel;

using Bphx.Cool.Events;

using static Bphx.Cool.Functions;

namespace Bphx.Cool.UI;

/// <summary>
/// A base class for all <see cref="UIControl"/> descendants.
/// </summary>
[Serializable]
public class UIControl : UIObject
{
  /// <summary>
  /// Control's value.
  /// </summary>
  public virtual string Value
  {
    get => value;
    set
    {
      if (value != this.value)
      {
        this.value = value;
        isDefaultValue = false;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// Value for the xml serialization.
  /// </summary>
  [State("value")]
  public virtual string Value_Xml
  {
    get
    {
      if (isDefaultValue)
      {
        return null;
      }

      var value = Value;

      return (value == null) || (value.Length > 0) ? value : " ";
    }
    set => Value = value;
  }

  /// <summary>
  /// A background RGB value. 
  /// </summary>
  public int? BackgroundColor
  {
    get => backgroundColor ?? (ReadOnly ? -1 : Disabled ? -2 : null);
    set
    {
      if (backgroundColor != value)
      {
        backgroundColor = value;
        isDefaultBackgroundColor = false;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// A background RGB value. 
  /// </summary>
  [State("backgroundColor")]
  public int? BackgroundColor_Xml
  {
    get => isDefaultBackgroundColor ? null : BackgroundColor;
    set => BackgroundColor = value;
  }


  /// <summary>
  /// A bitmap background.
  /// </summary>
  [State("bitmapBackground")]
  public string BitmapBackground
  {
    get => bitmapBackground;
    set
    {
      if (bitmapBackground != value)
      {
        bitmapBackground = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// An object name.
  /// </summary>
  public virtual string Caption
  {
    get => caption;
    set
    {
      if (!Equal(value, caption))
      {
        caption = TrimEnd(value);
        isDefaultCaption = false;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// Caption for the xml serialization.
  /// </summary>
  [State("caption")]
  public virtual string Caption_Xml
  {
    get
    {
      if (isDefaultCaption)
      {
        return null;
      }

      var value = Caption;

      return (value == null) || (value.Length > 0) ? value : " ";
    }
    set => Caption = value;
  }

  /// <summary>
  /// A command associated with the control.
  /// </summary>
  public string Command { get; set; }

  /// <summary>
  /// Disabled indicator.
  /// </summary>
  [DefaultValue(false)]
  [State("disabled")]
  public bool Disabled
  {
    get => disabled;
    set
    {
      if (disabled != value)
      {
        disabled = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// Disable state.
  /// </summary>
  [State("disabledState", Read = false, Write = true)]
  public bool DisabledState { get; set; }

  /// <summary>
  /// Enabled state.
  /// </summary>
  public bool Enabled
  {
    get => !Disabled && !DisabledState;
    set
    {
      Disabled = !value;
      DisabledState = false;
    }
  }

  /// <summary>
  /// A focus state of the control.
  /// </summary>
  public virtual bool Focus
  {
    get
    {
      return focused;
    }
    set
    {
      if (focused != value)
      {
        focused = value;

        if (Owner != null)
        {
          if (value)
          {
            Owner.FocusedControl = this;
          }
          else if (Owner.FocusedControl == this)
          {
            Owner.FocusedControl = null;
          }
          // No more cases.
        }
      }
    }
  }

  /// <summary>
  /// A size of the font.
  /// </summary>
  [State("fontSize")]
  public int? FontSize
  {
    get => fontSize;
    set
    {
      if (fontSize != value)
      {
        fontSize = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// A font style. 
  /// Valid values is a sequence of following values separated by the commas:
  /// 
  /// <list type="bullet">
  ///   <item>Bold</item>
  ///   <item>Not Bold</item>
  ///   <item>Underline</item>
  ///   <item>Not Underline</item>
  ///   <item>Strikeout</item>
  ///   <item>Not Strikeout</item>
  ///   <item>Standard</item>
  /// </list>
  /// 
  /// "Standard" has the same meaning as "Not Bold". 
  /// </summary>
  [State("fontStype")]
  public string FontStyle
  {
    get => fontStyle;
    set
    {
      if (fontStyle != value)
      {
        fontStyle = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// A font type, such as "Helvetica".
  /// </summary>
  [State("fontType")]
  public string FontType
  {
    get => fontType;
    set
    {
      if (fontType != value)
      {
        fontType = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// A foreground RGB value. 
  /// </summary>
  public int? ForegroundColor
  {
    get => foregroundColor;
    set
    {
      if (foregroundColor != value)
      {
        foregroundColor = value;
        isDefaultForegroundColor = false;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// A foreground RGB value. 
  /// </summary>
  [State("foregroundColor")]
  public int? ForegroundColor_Xml
  {
    get => isDefaultForegroundColor ? null : foregroundColor;
    set => ForegroundColor = value;
  }

  /// <summary>
  /// A height of the control.
  /// </summary>
  [State("height")]
  public string HeightValue
  {
    get => height;
    set
    {
      if (height != value)
      {
        height = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// A height of the control.
  /// </summary>
  public virtual int? Height
  {
    get => GetLengthValue(height);
    set => HeightValue = value == null ? null : value + "px";
  }

  /// <summary>
  /// A leftmost location of the control.
  /// </summary>
  [State("left")]
  public string LeftValue
  {
    get => left;
    set
    {
      if (left != value)
      {
        left = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// A leftmost location of the control.
  /// </summary>
  public int? Left
  {
    get => GetLengthValue(left);
    set => LeftValue = value == null ? null : value + "px";
  }

  /// <summary>
  /// A read only state.
  /// </summary>
  [DefaultValue(false)]
  [State("readOnly")]
  public bool ReadOnly
  {
    get => readOnly;
    set
    {
      if (readOnly != value)
      {
        readOnly = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// A visible state.
  /// </summary>
  public virtual bool Visible
  {
    get => (Owner?.Visible != false) && VisibleValue;
    set => VisibleValue = value;
  }

  /// <summary>
  /// A visible state.
  /// </summary>
  [DefaultValue(true)]
  [State("visible")]
  public bool VisibleValue
  {
    get => !hidden;
    set
    {
      if (hidden == value)
      {
        hidden = !value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// A width of the control.
  /// </summary>
  [State("width")]
  public string WidthValue
  {
    get => width;
    set
    {
      if (width != value)
      {
        width = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// A width of the control.
  /// </summary>
  public virtual int? Width
  {
    get => GetLengthValue(width);
    set => WidthValue = value == null ? null : value + "px";
  }

  /// <summary>
  /// A topmost location of the control.
  /// </summary>
  [State("top")]
  public string TopValue
  {
    get => top;
    set
    {
      if (top != value)
      {
        top = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// A topmost location of the control.
  /// </summary>
  public int? Top
  {
    get => GetLengthValue(top);
    set => TopValue = value == null ? null : value + "px";
  }

  /// <summary>
  /// Sets the owner window.
  /// </summary>
  /// <param name="window">An owner window.</param>
  public override void SetOwner(UIWindow window)
  {
    var focus = Focus;

    Focus = false;
    base.SetOwner(window);
    Focus = focus;
  }

  /// <summary>
  /// Marks default state of the control.
  /// </summary>
  public virtual void SetDefault()
  {
    isDefaultValue = true;
    isDefaultCaption = true;
    isDefaultForegroundColor = true;
    isDefaultBackgroundColor = true;
  }

  /// <summary>
  /// Indicates whether the control supports binding.
  /// </summary>
  public virtual bool SupportsBinding => false;

  /// <summary>
  /// A control binding.
  /// </summary>
  public string Binding { get; set; }

  /// <summary>
  /// A control default value, if any.
  /// </summary>
  public string DefaultValue { get; set; }

  /// <summary>
  /// Handles an event.
  /// </summary>
  /// <param name="context">A <see cref="IContext"/> instance.</param>
  /// <param name="e">An <see cref="Event"/> instance.</param>
  /// <returns>
  /// <c>true</c> if application can run an event handler, and 
  /// <c>false</c> otherwise.
  /// </returns>
  public virtual bool Run(IContext context, Event e)
  {
    return true;
  }

  /// <summary>
  /// Executes an after run logic, if any.
  /// </summary>
  /// <param name="context">a context instance.</param>
  /// <param name="e">An <see cref="Event"/> instance.</param>
  /// <param name="run"><see cref="Run(IContext)"/> outcome.</param>
  public virtual void AfterRun(IContext context, Event e, bool run)
  {
  }

  /// <summary>
  /// Invokes a method of <see cref="UIObject"/> instance.
  /// </summary>
  /// <param name="name">A method name.</param>
  /// <param name="args">Method's arguments.</param>
  /// <returns>Result type.</returns>
  public override object Invoke(string name, params object[] args)
  {
    if (string.Compare(name, "Redraw", true) == 0)
    {
      return null;
    }

    if (string.Compare(name, "SetBitmapBackground", true) == 0)
    {
      if (args.Length != 1)
      {
        throw new ArgumentException("One argument is expected.", nameof(args));
      }

      BitmapBackground = args[0]?.ToString();

      return null;
    }

    return base.Invoke(name, args);
  }

  /// <summary>
  /// Gets length value, from a value.
  /// </summary>
  /// <param name="value">A value to convert to length value.</param>
  /// <returns>
  /// A length value, or <c>null</c> if no conversion is possible.
  /// </returns>
  protected static int? GetLengthValue(string value)
  {
    if (IsEmpty(value))
    {
      return null;
    }

    value = Trim(value);

    if (value.EndsWith("px"))
    {
      value = value[..(value.Length - "px".Length)];
    }

    return double.TryParse(value, out var result) ? (int?)result : null;
  }

  /// <summary>
  /// Focused indicator.
  /// </summary>
  protected bool focused;

  /// <summary>
  /// A control value.
  /// </summary>
  protected string value;

  /// <summary>
  /// A control caption.
  /// </summary>
  protected string caption;

  /// <summary>
  /// A default value indicator.
  /// </summary>
  protected bool isDefaultValue;

  /// <summary>
  /// A default caption indicator.
  /// </summary>
  protected bool isDefaultCaption;

  /// <summary>
  /// A background RGB value. 
  /// </summary>
  protected int? backgroundColor;

  /// <summary>
  /// A default background color indicator.
  /// </summary>
  protected bool isDefaultBackgroundColor;

  /// <summary>
  /// A bitmap background.
  /// </summary>
  protected string bitmapBackground;

  /// <summary>
  /// Disabled indicator.
  /// </summary>
  protected bool disabled;

  /// <summary>
  /// A size of the font.
  /// </summary>
  protected int? fontSize;

  /// <summary>
  /// A font style. 
  /// </summary>
  protected string fontStyle;

  /// <summary>
  /// A font type, such as "Helvetica".
  /// </summary>
  protected string fontType;

  /// <summary>
  /// A foreground RGB value. 
  /// </summary>
  protected int? foregroundColor;

  /// <summary>
  /// A default foreground color indicator.
  /// </summary>
  protected bool isDefaultForegroundColor;

  /// <summary>
  /// A height of the control.
  /// </summary>
  protected string height;

  /// <summary>
  /// A leftmost location of the control.
  /// </summary>
  protected string left;

  /// <summary>
  /// A read only state.
  /// </summary>
  protected bool readOnly;

  /// <summary>
  /// A visible state.
  /// </summary>
  protected bool hidden;

  /// <summary>
  /// A width of the control.
  /// </summary>
  protected string width;

  /// <summary>
  /// A topmost location of the control.
  /// </summary>
  protected string top;
}
