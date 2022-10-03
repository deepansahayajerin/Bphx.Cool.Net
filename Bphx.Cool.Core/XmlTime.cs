using System;
using System.Xml;
using System.Xml.Serialization;

namespace Bphx.Cool;

/// <summary>
/// A structure to support xml time serialization.
/// </summary>
[Serializable]
public struct XmlTime
{
  /// <summary>
  /// Creates a <see cref="TimeSpan"/> value. 
  /// </summary>
  /// <param name="value">A <see cref="XmlTime"/> value.</param>
  public static implicit operator TimeSpan(XmlTime value)
  {
    return value.Value.TimeOfDay;
  }

  /// <summary>
  /// Creates a <see cref="TimeSpan"/> value. 
  /// </summary>
  /// <param name="value">A <see cref="XmlTime"/> value.</param>
  public static implicit operator TimeSpan?(XmlTime? value)
  {
    return value?.Value.TimeOfDay;
  }

  /// <summary>
  /// Creates a <see cref="XmlTime"/> value. 
  /// </summary>
  /// <param name="value">A <see cref="TimeSpan"/> value.</param>
  public static implicit operator XmlTime(TimeSpan value)
  {
    return new() { Value = DateTime.MinValue + value };
  }

  /// <summary>
  /// Creates a <see cref="XmlTime"/> value. 
  /// </summary>
  /// <param name="value">A <see cref="TimeSpan"/> value.</param>
  public static implicit operator XmlTime?(TimeSpan? value)
  {
    return value == null ? null : 
      new() { Value = DateTime.MinValue + value.Value };
  }

  /// <summary>
  /// A value being serialized.
  /// </summary>
  [XmlText(DataType = "time")]
  public DateTime Value { get; set; }
}
