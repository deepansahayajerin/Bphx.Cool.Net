using System.Xml.Serialization;

namespace Bphx.Cool.Log;

/// <summary>
/// Defines a serialized error message.
/// </summary>
public class Error
{
  /// <summary>
  /// An error message.
  /// </summary>
  [XmlElement("exceptionMessage")]
  public string ExceptionMessage { get; set; }

  /// <summary>
  /// An error type.
  /// </summary>
  [XmlElement("exceptionType")]
  public string ExceptionType { get; set; }

  /// <summary>
  /// An error stack trace.
  /// </summary>
  [XmlElement("stackTrace")]
  public string StackTrace { get; set; }

  /// <summary>
  /// Additional binary data.
  /// </summary>
  [XmlElement("data")]
  public byte[] Data { get; set; }
}
