using System.Xml.Serialization;

namespace Bphx.Cool;

/// <summary>
/// Defines type of a response.
/// </summary>
public enum ResponseType 
{
  /// <summary>
  /// Default response. No navigation has occurred.
  /// </summary>
  [XmlEnum("default")]
  Default,

  /// <summary>
  /// Navigation has occured.
  /// </summary>
  [XmlEnum("navigate")]
  Navigate,

  /// <summary>
  /// Message box should be shown.
  /// </summary>
  [XmlEnum("messageBox")]
  MessageBox,

  /// <summary>
  /// End of application.
  /// </summary>
  [XmlEnum("end")]
  End
}
