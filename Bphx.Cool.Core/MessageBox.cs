using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text.Json.Serialization;

namespace Bphx.Cool;

/// <summary>
/// Defines a message box.
/// </summary>
[Serializable]
public class MessageBox : IAttributes
{
  /// <summary>
  /// Associated procedure.
  /// </summary>
  [XmlIgnore, JsonIgnore]
  public IProcedure Procedure { get; set; }

  /// <summary>
  /// A message box type.
  /// </summary>
  public string Type { get; set; }

  /// <summary>
  /// Gets and sets a message displayed in the dialog box. 
  /// </summary>
  public string Text { get; set; }

  /// <summary>
  /// Gets and sets a caption or title in the dialog box.
  /// </summary>
  public string Title { get; set; }

  /// <summary>
  /// <para>Gets and sets a caption for the pushbutton(s).</para>
  /// <para>The valid values are:
  ///   <list type="bullet">
  ///     <item>
  ///       <description>OK</description>
  ///     </item>
  ///     <item>
  ///       <description>OKCancel</description>
  ///     </item>
  ///     <item>
  ///       <description>AbortRetryIgnore</description>
  ///     </item>
  ///     <item>
  ///       <description>YesNo</description>
  ///     </item>
  ///     <item>
  ///       <description>YesNoCancel</description>
  ///     </item>
  ///     <item>
  ///       <description>RetryCancel</description>
  ///     </item>
  ///   </list>
  /// </para>
  /// </summary>
  /// <remarks>
  /// Pressing the <b>Esc</b> key activates the <b>Cancel</b> button when 
  /// the <b>Cancel</b> button is present. <b>OK</b> is the default button.
  /// </remarks>
  public string Buttons
  {
    get => buttons;
    set
    {
      if (string.IsNullOrEmpty(value))
      {
        buttons = "OK";
      }
      else if (string.Compare(value, "OK", true) == 0)
      {
        buttons = "OK";
      }
      else if (string.Compare(value, "OKCancel", true) == 0)
      {
        buttons = "OKCancel";
      }
      else if (string.Compare(value, "AbortRetryIgnore", true) == 0)
      {
        buttons = "AbortRetryIgnore";
      }
      else if (string.Compare(value, "YesNo", true) == 0)
      {
        buttons = "YesNo";
      }
      else if (string.Compare(value, "YesNoCancel", true) == 0)
      {
        buttons = "YesNoCancel";
      }
      else if (string.Compare(value, "RetryCancel", true) == 0)
      {
        buttons = "RetryCancel";
      }
      else
      {
        throw new ArgumentException(
          "The valid values are OK, OKCancel, AbortRetryIgnore, YesNo, " +
          "YesNoCancel, and RetryCancel.");
      }

      VisibleButtons.Clear();

      if (buttons.Contains("OK"))
      {
        VisibleButtons.Add("OK");
      }

      if (buttons.Contains("Yes"))
      {
        VisibleButtons.Add("Yes");
      }

      if (buttons.Contains("No"))
      {
        VisibleButtons.Add("No");
      }

      if (buttons.Contains("Cancel"))
      {
        VisibleButtons.Add("Cancel");
      }

      if (buttons.Contains("Retry"))
      {
        VisibleButtons.Add("Retry");
      }

      if (buttons.Contains("Ignore"))
      {
        VisibleButtons.Add("Ignore");
      }

      if (buttons.Contains("Abort"))
      {
        VisibleButtons.Add("Abort");
      }
    }
  }

  /// <summary>
  /// Gets a map of visible on message box buttons.
  /// </summary>
  [XmlIgnore, JsonIgnore]
  public HashSet<string> VisibleButtons { get; } = new();

  /// <summary>
  /// Gets and sets a default pushbutton.
  /// </summary>
  [DefaultValue(0)]
  public int DefaultButton { get; set; }

  /// <summary>
  /// <para>Gets and sets a display icon type.</para>
  /// <para>The valid values are:
  ///   <list type="bullet">
  ///     <item>
  ///       <description>None</description>
  ///     </item>
  ///     <item>
  ///       <description>Critical</description>
  ///     </item>
  ///     <item>
  ///       <description>Question</description>
  ///     </item>
  ///     <item>
  ///       <description>Exclamation</description>
  ///     </item>
  ///     <item>
  ///       <description>Information</description>
  ///     </item>
  ///   </list>
  /// </para>
  /// </summary>
  /// <remarks>
  /// Note: <b>None</b> is the default value.
  /// </remarks>
  public string Style
  {
    get => style;
    set
    {
      style = "None";

      if (string.IsNullOrEmpty(value) ||
        (string.Compare(value, style, true) == 0))
      {
        return;
      }

      style = "Critical";

      if (string.Compare(value, style, true) == 0)
      {
        return;
      }

      style = "Question";

      if (string.Compare(value, style, true) == 0)
      {
        return;
      }

      style = "Exclamation";

      if (string.Compare(value, style, true) == 0)
      {
        return;
      }

      style = "Information";

      if (string.Compare(value, style, true) == 0)
      {
        return;
      }

      throw new ArgumentException(
        "The valid values are None, Critical, Question, Exclamation, " +
        "and Information");
    }
  }

  /// <summary>
  /// Indicates whether the message box has the result.
  /// </summary>
  public bool HasResult => result != null;

  /// <summary>
  /// <para>Gets and sets a caption from the selected pushbutton.</para>
  /// <para>The valid values are:
  ///   <list type="bullet">
  ///     <item>
  ///       <description>OK</description>
  ///     </item>
  ///     <item>
  ///       <description>Cancel</description>
  ///     </item>
  ///     <item>
  ///       <description>Abort</description>
  ///     </item>
  ///     <item>
  ///       <description>Ignore</description>
  ///     </item>
  ///     <item>
  ///       <description>Yes</description>
  ///     </item>
  ///     <item>
  ///       <description>No</description>
  ///     </item>
  ///     <item>
  ///       <description>Retry</description>
  ///     </item>
  ///   </list>
  /// </para>
  /// </summary>
  public string Result
  {
    get => string.IsNullOrEmpty(result) &&
      ((buttons == null) || buttons.Contains("OK")) ?
      "OK" : result;
    set
    {
      if (value == null)
      {
        result = null;
      }
      else
      {
        if (VisibleButtons.Contains(value))
        {
          result = value;
        }
      }
    }
  }

  #region IAttributes Members
  /// <summary>
  /// Gets attributes map.
  /// </summary>
  [XmlIgnore, JsonIgnore]
  public Dictionary<string, object> Attributes => attributes ??= new();

  [XmlElement("attribute"), JsonPropertyName("attribute")]
  public Attribute[] Attributes_Xml
  {
    get => Attribute.ToArray(attributes);
    set => attributes = Attribute.ToDictionary(value);
  }
  #endregion

  // A caption for the pushbutton(s). 
  private string buttons = "OK";

  // A display icon type.
  private string style;

  // A caption from the selected pushbutton.
  private string result;

  /// <summary>
  /// Attributes map.
  /// </summary>
  protected Dictionary<string, object> attributes;
}
