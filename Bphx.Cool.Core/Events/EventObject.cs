using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Bphx.Cool.Events;

/// <summary>
/// Represents the state of an event, such as the state of the keyboard keys,
/// the location of the mouse, and the state of the mouse buttons etc. 
/// </summary>
[Serializable]
public class EventObject: IAttributes
{
  /// <summary>
  /// Event type.
  /// </summary>
  public string Type { get; set; }

  /// <summary>
  /// Window or dialog box name.
  /// </summary>
  public string Window { get; set; }

  /// <summary>
  /// Component name.
  /// </summary>
  public string Component { get; set; }

  /// <summary>
  /// A command.
  /// </summary>
  public string Command { get; set; }

  /// <summary>
  ///   <para>Gets and sets state of <b>Alt</b> key.</para>
  ///   <para>Possible values are "up" or "down".</para>
  /// </summary>
  public string Alt { get => alt; set => alt = NormalizeValue(value); }
    
  /// <summary>
  ///   <para>Gets and sets state of <b>Ctrl</b> key.</para>
  ///   <para>Possible values are "up" or "down".</para>
  /// </summary>
  public string Ctrl { get => ctrl; set => ctrl = NormalizeValue(value); }

  /// <summary>
  ///   <para>Gets and sets state of <b>Shift</b> key.</para>
  ///   <para>Possible values are "up" or "down".</para>
  /// </summary>
  public string Shift { get => shift; set => shift = NormalizeValue(value); }
    
  /// <summary>
  /// Gets and sets a single printable character pressed on the keyboard.
  /// </summary>
  public string Character { get; set; }
    
  /// <summary>
  /// Gets and sets the current text in edit control.
  /// </summary>
  public string CurrentText { get; set; }
    
  /// <summary>
  /// Gets and sets a X coordinate as an integer value.
  /// </summary>
  public int X { get; set; }
    
  /// <summary>
  /// Gets and sets a Y coordinate as an integer value.
  /// </summary>
  public int Y { get; set; }

  /// <summary>
  ///   <para>Gets and sets a return value action.</para>
  ///   <para>
  ///     Note: possible values are "accept", "reject" and "modify".
  ///   </para>
  /// </summary>
  public string Keypress
  {
    get => keypress;
    set
    {
      if (string.IsNullOrEmpty(value))
      {
        keypress = "accept";
      }
      else
      {
        value = value.ToLower();

        switch (value.ToLower())
        {
          case "accept":
          case "reject":
          case "modify":
          {
            keypress = value;
            break;
          }
          default:
          {
            keypress = "accept";

            break;
          }
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

  /// <summary>
  /// Gets an event attribute.
  /// </summary>
  /// <param name="name">An attribute name.</param>
  /// <returns>An attribute value.</returns>
  public object GetAttribute(string name)
  {
    return attributes?.Get(name);
  }
  #endregion

  /// <summary>
  /// Generic data.
  /// </summary>
  [XmlElement("data"), JsonPropertyName("data")]
  public StateData[] Data { get; set; }

  /// <summary>
  /// Normalizes string value.
  /// </summary>
  /// <param name="value">A value to normalize.</param>
  /// <returns>either "up" or "down" values.</returns>
  private static string NormalizeValue(string value)
  {
    if (value != null)
    {
      if (("1" == value) || 
        (string.Compare("true", value, true) == 0) ||
        (string.Compare("on", value, true) == 0) ||
        (string.Compare("down", value, true) == 0))
      {
        return "down";
      }
    }
      
    return "up";
  }

  /// <summary>
  /// State of <b>Alt</b> key. Possible values are "up" or "down".
  /// </summary>
  private string alt = "up";

  /// <summary>
  /// State of <b>Ctrl</b> key. Possible values are "up" or "down".
  /// </summary>
  private string ctrl = "up";

  /// <summary>
  /// State of <b>Shift</b> key. Possible values are "up" or "down".
  /// </summary>
  private string shift = "up";

  /// <summary>
  /// Return value action. 
  /// Possible values are "accept", "reject" and "modify".
  /// </summary>
  private string keypress = "accept";

  /// <summary>
  /// Attributes map.
  /// </summary>
  private Dictionary<string, object> attributes;
}
