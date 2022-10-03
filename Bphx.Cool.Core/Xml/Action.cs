using System;
using System.Xml.Serialization;

namespace Bphx.Cool.Xml;

/// <summary>
/// action element from procedure.xsd
/// </summary>
[Serializable]
[XmlType(TypeName = "action", Namespace = Rule.Namespace)]
public enum Action
{
  [XmlEnum("transfer")]
  Transfer,

  [XmlEnum("link")]
  Link
}
