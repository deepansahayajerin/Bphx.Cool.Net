using System;
using System.Xml.Serialization;

namespace Bphx.Cool.Log;

/// <summary>
/// A class to support xml serialization.
/// </summary>
[Serializable]
public struct Item
{
  /// <summary>
  /// A name.
  /// </summary>
  [XmlAttribute("name")]
  public string Name { get; set; }

  /// <summary>
  /// A value.
  /// </summary>
  [XmlElement("int", Type = typeof(int))]
  [XmlElement("short", Type = typeof(short))]
  [XmlElement("long", Type = typeof(long))]
  [XmlElement("double", Type = typeof(double))]
   [XmlElement("decimal", Type = typeof(decimal))]
  [XmlElement("string", Type = typeof(string))]
  [XmlElement("bytes", Type = typeof(byte[]))]
  [XmlElement("date", DataType = "date", Type = typeof(DateTime))]
  [XmlElement("time", Type = typeof(XmlTime))]
  [XmlElement("dateTime", DataType = "dateTime", Type = typeof(DateTime))]
  [XmlElement("bool", Type = typeof(bool))]
  public object Value { get; set; }
}

/// <summary>
/// A list of values.
/// </summary>
[XmlRoot("values")]
public class Values
{
    [XmlElement("int", Type = typeof(int))]
    [XmlElement("short", Type = typeof(short))]
    [XmlElement("long", Type = typeof(long))]
    [XmlElement("double", Type = typeof(double))]
    [XmlElement("decimal", Type = typeof(decimal))]
    [XmlElement("string", Type = typeof(string))]
    [XmlElement("bytes", Type = typeof(byte[]))]
    [XmlElement("date", DataType = "date", Type = typeof(DateTime))]
    [XmlElement("time", Type = typeof(XmlTime))]
    [XmlElement("dateTime", DataType = "dateTime", Type = typeof(DateTime))]
    [XmlElement("bool", Type = typeof(bool))]
    public object[] Items { get; set; }
}

/// <summary>
/// A list of values.
/// </summary>
[XmlRoot("map")]
public class Map
{
  [XmlElement("item")]
  public Item[] Items { get; set; }
}
