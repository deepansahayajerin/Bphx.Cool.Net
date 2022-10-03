using System;
using System.Xml.Serialization;

namespace Bphx.Cool.Xml;

/// <summary>
/// Defines a message type.
/// </summary>
[Serializable]
[
  XmlType(
    TypeName = "message-type",
    Namespace = "http://www.bphx.com/coolgen/exit-states/xml")
]
public enum MessageType
{
  /// <summary>
  /// Message type has not been defined.
  /// </summary>
  [XmlEnum("none")]
  None,

  /// <summary>
  /// Message gives information only; no action required.
  /// </summary>
  [XmlEnum("info")]
  Info,

  /// <summary>
  /// Message advises user action may be required as a result 
  /// of processing.
  /// </summary>
  [XmlEnum("warning")]
  Warning,

  /// <summary>
  ///  Message advises that an error occurred in an entry and 
  ///  action is required to correct.
  /// </summary>
  [XmlEnum("error")]
  Error
}
