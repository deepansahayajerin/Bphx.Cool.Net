using System;
using System.Xml.Serialization;

namespace Bphx.Cool.Xml;

/// <summary>
/// <code>map</code> element from procedure.xsd
/// </summary>
[Serializable]
[XmlType(TypeName = "map", Namespace = Rule.Namespace)]
public class Map
{
  [XmlAttribute("from")]
  public string From { get; set; }

  [XmlAttribute("to")]
  public string To { get; set; }
}
