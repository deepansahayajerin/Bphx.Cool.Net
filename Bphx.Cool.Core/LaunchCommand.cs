using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Bphx.Cool;

/// <summary>
/// A launch command
/// </summary>
public class LaunchCommand: IAttributes
{
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

  /// <summary>
  /// Attributes map.
  /// </summary>
  protected Dictionary<string, object> attributes;
}
