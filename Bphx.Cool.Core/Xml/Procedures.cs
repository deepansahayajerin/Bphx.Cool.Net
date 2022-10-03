using System;
using System.Xml.Serialization;

namespace Bphx.Cool.Xml;

/// <summary>
/// procedure element from procedure.xsd
/// </summary>
[Serializable]
[XmlType(Namespace = Rule.Namespace, TypeName = "procedures")]
[
  XmlRoot(
    ElementName = "procedures",
    Namespace = Rule.Namespace,
    IsNullable = false)
]
public class Procedures
{
  [XmlElementAttribute("procedure")]
  public Procedure[] Procedure { get; set; }
}
