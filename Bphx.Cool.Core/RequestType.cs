using System.Xml.Serialization;

namespace Bphx.Cool;

/// <summary>
/// Defines type of a request.
/// </summary>
public enum RequestType
{
  /// <summary>
  /// Execute request.
  /// </summary>
  [XmlEnum("execute")]
  Execute,

  /// <summary>
  /// Event request.
  /// </summary>
  [XmlEnum("event")]
  Event
}
