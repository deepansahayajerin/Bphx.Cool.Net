using System;

namespace Bphx.Cool.UI;

/// <summary>
/// A base class for all input controls like Field, ListBox etc.
/// </summary>
[Serializable]
public class UIInputControl : UIControl
{
  /// <summary>
  /// Indicates whether the control supports binding.
  /// </summary>
  public override bool SupportsBinding => true;

  /// <summary>
  /// Prompt for the input control. 
  /// </summary>
  public string Prompt
  {
    get => Caption;
    set => Caption = value;
  }

  /// <summary>
  /// Background RGB value for the prompt. 
  /// </summary>
  [State("promptBackgroundColor")]
  public int? PromptBackgroundColor
  {
    get => promptBackgroundColor;
    set
    {
      if(promptBackgroundColor != value)
      {
        promptBackgroundColor = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// Size of the prompt's font. 
  /// </summary>
  [State("promptFontSize")]
  public int? PromptFontSize
  {
    get => promptFontSize;
    set
    {
      if(promptFontSize != value)
      {
        promptFontSize = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// Prompt's font style. 
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
  [State("promptFontStyle")]
  public string PromptFontStyle
  {
    get => promptFontType;
    set
    {
      if(promptFontStyle != value)
      {
        promptFontStyle = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// Prompt's font type, such as "Helvetica".
  /// </summary>
  [State("promptFontType")]
  public string PromptFontType
  {
    get => promptFontType;
    set
    {
      if(promptFontType != value)
      {
        promptFontType = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// Foreground color as RGB. 
  /// </summary>
  [State("promptForegroundColor")]
  public int? PromptForegroundColor
  {
    get => promptForegroundColor;
    set
    {
      if(promptForegroundColor != value)
      {
        promptForegroundColor = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// Height of the prompt. 
  /// </summary>
  [State("promptHeight")]
  public string PromptHeightValue
  {
    get => promptHeight;
    set
    {
      if(promptHeight != value)
      {
        promptHeight = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// Height of the prompt. 
  /// </summary>
  public int? PromptHeight
  {
    get =>
      int.TryParse(promptHeight, out int result) ? result as int? : null;
    set => PromptHeightValue = value?.ToString();
  }

  /// <summary>
  /// Leftmost location of the prompt.
  /// </summary>
  [State("promptLeft")]
  public string PromptLeftValue
  {
    get => promptLeft;
    set
    {
      if(promptLeft != value)
      {
        promptLeft = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// Leftmost location of the prompt.
  /// </summary>
  public int? PromptLeft
  {
    get => int.TryParse(promptLeft, out int result) ? result as int? : null;
    set => PromptLeftValue = value?.ToString();
  }

  /// <summary>
  /// Width of the prompt.
  /// </summary>
  [State("promptWidth")]
  public string PromptWidthValue
  {
    get => promptWidth;
    set
    {
      if(promptWidth != value)
      {
        promptWidth = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// Width of the prompt.
  /// </summary>
  public int? PromptWidth
  {
    get => int.TryParse(promptWidth, out int result) ? result as int? : null;
    set => PromptWidthValue = value?.ToString();
  }

  /// <summary>
  /// Topmost location of the prompt. 
  /// </summary>
  [State("promptTop")]
  public string PromptTopValue
  {
    get => promptTop;
    set
    {
      if(promptTop != value)
      {
        promptTop = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// Topmost location of the prompt. 
  /// </summary>
  public int? PrompTop
  {
    get => int.TryParse(promptTop, out int result) ? result as int? : null;
    set => PromptTopValue = value?.ToString();
  }

  /// <summary>
  /// Background RGB value for the prompt. 
  /// </summary>
  protected int? promptBackgroundColor;

  /// <summary>
  /// Size of the prompt's font. 
  /// </summary>
  protected int? promptFontSize;

  /// <summary>
  /// Prompt's font style. 
  /// </summary>
  protected string promptFontStyle;

  /// <summary>
  /// Prompt's font type, such as "Helvetica".
  /// </summary>
  protected string promptFontType;

  /// <summary>
  /// Foreground color as RGB. 
  /// </summary>
  protected int? promptForegroundColor;

  /// <summary>
  /// Height of the prompt. 
  /// </summary>
  protected string promptHeight;

  /// <summary>
  /// Leftmost location of the prompt.
  /// </summary>
  protected string promptLeft;

  /// <summary>
  /// Width of the prompt.
  /// </summary>
  protected string promptWidth;

  /// <summary>
  /// Topmost location of the prompt. 
  /// </summary>
  protected string promptTop;
}
