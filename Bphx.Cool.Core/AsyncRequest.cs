using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bphx.Cool;

/// <summary>
/// An async request structure.
/// </summary>
[Serializable]
[XmlRoot(Namespace = Global.Namespace)]
public class AsyncRequest: ICloneable
{
  /// <summary>
  /// Creates an AsyncRequest instance.
  /// </summary>
  public AsyncRequest() { }

  /// <summary>
  /// Creates an AsyncRequest instance.
  /// </summary>
  /// <param name="that">A value to copy from.</param>
  public AsyncRequest(AsyncRequest that)
  {
    Assign(that);
  }

  /// <summary>
  /// Clones this instance.
  /// </summary>
  /// <returns>A new AsyncRequest instance.</returns>
  public AsyncRequest Clone() => new(this);

  /// <summary>
  /// Clones this instance.
  /// </summary>
  /// <returns>A new AsyncRequest instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>
  /// Assigns AsyncRequest from other instance.
  /// </summary>
  /// <param name="that">An AsyncRequest to assign from.</param>
  /// <returns>This instance.</returns>
  public AsyncRequest Assign(AsyncRequest that)
  {
    Id = that.Id;
    ReasonCode = that.ReasonCode;
    Label = that.Label;
    ErrorMessage = that.ErrorMessage;

    return this;
  }

  /// <summary>
  /// Gets or sets the value of the ID attribute.
  /// </summary>
  [XmlElement("id")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 8)]
  public int Id { get; set; }

  /// <summary>
  /// Length of the REASON_CODE attribute.
  /// </summary>
  public const int ReasonCode_MaxLength = 8;

  /// <summary>
  /// Gets or sets the value of the REASON_CODE attribute.
  /// </summary>
  [XmlElement("reasonCode")]
  [Member(Index = 2, Type = MemberType.Char, Length = ReasonCode_MaxLength)]
  public string ReasonCode { get; set; }

  /// <summary>
  /// Length of the LABEL attribute.
  /// </summary>
  public const int Label_MaxLength = 128;

  /// <summary>
  /// Gets or sets the value of the LABEL attribute.
  /// </summary>
  [XmlElement("label")]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Label_MaxLength)]
  public string Label { get; set; }

  /// <summary>
  /// Length of the ERROR_MESSAGE attribute.
  /// </summary>
  public const int ErrorMessage_MaxLength = 2048;

  /// <summary>
  /// Gets or sets the value of the ERROR_MESSAGE attribute.
  /// </summary>
  [XmlElement("errorMessage")]
  [Member(
    Index = 4,
    Type = MemberType.Varchar,
    Length = ErrorMessage_MaxLength)]
  public string ErrorMessage { get; set; }
}
