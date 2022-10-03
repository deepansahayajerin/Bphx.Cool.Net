using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text.Json.Serialization;

namespace Bphx.Cool;

/// <summary>
/// An attribute.
/// </summary>
public struct Attribute: INamed
{
  /// <summary>
  /// Gets a dictionary of attribute values as a enumerable of attributes.
  /// </summary>
  /// <param name="map">A dictionary instance.</param>
  /// <returns>A enumerable of attributes.</returns>
  public static Attribute[] ToArray(IDictionary<string, object> map)
  {
    return map == null ? Array.Empty<Attribute>() :
      map.
        Select(
          entry => new Attribute { Name = entry.Key, Value = entry.Value }).
        ToArray();
  }

  /// <summary>
  /// Gets a dictionary of attribute value from a attribute enumerable.
  /// </summary>
  /// <param name="items">An attribute enumerable.</param>
  /// <returns>A dictionary of attribute values.</returns>
  public static Dictionary<string, object> ToDictionary(Attribute[] items)
  {
    if(items == null)
    {
      return null;
    }

    var map = items.ToDictionary(item => item.Name, item => item.Value);

    return map.Count == 0 ? null : map;
  }

  /// <summary>
  /// An attribute name.
  /// </summary>
  public string Name { get; set; }

  /// <summary>
  /// An attribute value.
  /// </summary>
  [XmlIgnore, JsonIgnore]
  public object Value { get; set; }

  /// <summary>
  /// An attribute value for xml serialization.
  /// </summary>
  [XmlElement("value"), JsonPropertyName("value")]
  public string Value_Xml
  {
    get => Value?.ToString();
    set => Value = value;
  }
}
