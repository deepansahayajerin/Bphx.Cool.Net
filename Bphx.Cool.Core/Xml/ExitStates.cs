using System;
using System.Xml.Serialization;

namespace Bphx.Cool.Xml;

/// <summary>
/// exit-states element from exit-states.xsd
/// </summary>
[Serializable]
[
  XmlType(
    TypeName = "exit-states",
    Namespace = "http://www.bphx.com/coolgen/exit-states/xml")
]
[
  XmlRoot(
    "exit-states",
    Namespace = "http://www.bphx.com/coolgen/exit-states/xml",
    IsNullable = false)
]
public class ExitStates
{
  [XmlElement("exit-state")]
  public ExitState[] ExitState { get; set; }
}
