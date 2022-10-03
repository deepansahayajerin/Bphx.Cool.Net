using System;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Bphx.Cool.Client
{
  [ComVisible(false)]
  [Serializable]
  [XmlType(Namespace = "http://www.bphx.com/coolgen/exit-states/xml")]
  public enum MessageTypeEnum
  {
    [XmlEnum("none")]
    None,
    [XmlEnum("info")]
    Info,
    [XmlEnum("warning")]
    Warning,
    [XmlEnum("error")]
    Error,
  }

  [ComVisible(false)]
  [Serializable]
  [XmlType(Namespace = "http://www.bphx.com/coolgen/exit-states/xml")]
  public enum TerminationActionEnum
  {
    [XmlEnum("normal")]
    Normal,
    [XmlEnum("rollback")]
    Rollback,
    [XmlEnum("abort")]
    Abort,
  }

  [ComVisible(false)]
  [Serializable]
  [XmlType(Namespace = "http://www.bphx.com/cool/xml")]
  public class Global
  {
    [XmlElement("clientPassword")]
    public string ClientPassword { get; set; }

    [XmlElement("clientUserId")]
    public string ClientUserId { get; set; }

    [XmlElement("command")]
    public string Command { get; set; }

    [XmlElement("currentDialect")]
    public string CurrentDialect { get; set; }

    [XmlElement("errmsg")]
    public string ErrMsg { get; set; }

    [XmlElement("exitStateId")]
    public int ExitStateId { get; set; }

    [XmlElement("exitstate")]
    public string Exitstate { get; set; }

    [XmlElement("localSystemId")]
    public string LocalSystemId { get; set; }

    [XmlElement("messageType")]
    [DefaultValue(MessageTypeEnum.None)]
    public MessageTypeEnum MessageType { get; set; }

    [XmlElement("nexttran")]
    public string Nexttran { get; set; }

    [XmlElement("nextlocation")]
    public string NextLocation { get; set; }

    [XmlElement("terminationAction")]
    [DefaultValue(TerminationActionEnum.Normal)]
    public TerminationActionEnum TerminationAction { get; set; }

    [XmlElement("trancode")]
    public string Trancode { get; set; }
  }
}