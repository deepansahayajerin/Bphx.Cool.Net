using System;
using System.Xml.Serialization;

namespace Bphx.Cool.Xml;

/// <summary>
/// procedure element from procedure.xsd
/// </summary>
[Serializable]
[XmlType(Namespace = Rule.Namespace, TypeName = "procedure")]
public class Procedure
{
  [XmlAttribute(AttributeName = "name")]
  public string Name { get; set; }

  [XmlAttribute(AttributeName = "transaction")]
  public string Transaction { get; set; }
}
