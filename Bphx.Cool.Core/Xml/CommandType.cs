using System;
using System.Xml.Serialization;

namespace Bphx.Cool.Xml;

/// <summary>
/// Defines a command type.
/// </summary>
[Serializable]
[XmlType(TypeName = "command-type", Namespace = Rule.Namespace)]
public enum CommandType
{
  /// <summary>
  /// Command type has not been defined.
  /// </summary>
  [XmlEnum("none")]
  None,

  /// <summary>
  /// Defined by user command type.
  /// </summary>
  [XmlEnum("defined")]
  Defined,

  /// <summary>
  /// Current command type.
  /// </summary>
  [XmlEnum("current")]
  Current,

  /// <summary>
  /// Previous command type.
  /// </summary>
  [XmlEnum("previous")]
  Previous
}
