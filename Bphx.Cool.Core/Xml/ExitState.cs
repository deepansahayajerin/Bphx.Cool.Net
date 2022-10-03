using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bphx.Cool.Xml;

/// <summary>
/// An exit-state element from exit-states.xsd
/// </summary>
[Serializable]
[
  XmlType(
    TypeName = "exit-state",
    Namespace = "http://www.bphx.com/coolgen/exit-states/xml")
]
public class ExitState
{
  [DefaultValue(0)]
  [XmlAttribute(AttributeName = "id")]
  public int Id { get; set; }

  [XmlAttribute(AttributeName = "name")]
  public string Name { get; set; }

  [XmlAttribute(AttributeName = "action")]
  [DefaultValue(TerminationAction.Normal)]
  public TerminationAction Action { get; set; }

  [XmlAttribute(AttributeName = "type")]
  [DefaultValue(MessageType.None)]
  public MessageType Type { get; set; }

  //[XmlAttribute(AttributeName = "display")]
  //[DefaultValue(true)]
  //public bool Display { get; set; } = true;

  //[XmlAttribute(AttributeName = "end")]
  //[DefaultValue(true)]
  //public bool End { get; set; } = true;
}
