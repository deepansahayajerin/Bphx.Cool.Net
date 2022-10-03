using System;
using System.Xml.Serialization;

namespace Bphx.Cool.Xml;

/// <summary>
/// autoflow element from procedure.xsd
/// </summary>
[Serializable]
[XmlType(TypeName = "autoflow", Namespace = Rule.Namespace)]
public class Autoflow
{
  [XmlAttribute("command")]
  public string Command { get; set; }

  [XmlAttribute("exit-state", DataType = "NMTOKEN")]
  public string ExitState { get; set; }
}
