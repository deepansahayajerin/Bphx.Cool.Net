using System;
using System.Xml.Serialization;

namespace Bphx.Cool.Xml;

/// <summary>
/// Defines a termination action.
/// </summary>
[Serializable]
[
  XmlType(
    TypeName = "termination-action",
    Namespace = "http://www.bphx.com/coolgen/exit-states/xml")
]
public enum TerminationAction
{
  /// <summary>
  /// The exit state message will be displayed or a flow will take place.
  /// </summary>
  [XmlEnum("normal")]
  Normal,

  /// <summary>
  /// All database updates (the results of CREATE, UPDATE, and 
  /// DELETE actions) performed by the procedure since the previous 
  /// checkpoint will be backed out.
  /// </summary>
  [XmlEnum("rollback")]
  Rollback,

  /// <summary>
  /// The procedure will abnormally terminate and will roll back all 
  /// database updates.
  /// </summary>
  [XmlEnum("abort")]
  Abort
}
