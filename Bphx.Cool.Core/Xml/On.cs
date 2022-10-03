using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bphx.Cool.Xml;

/// <summary>
/// <code>on</code> element from procedure.xsd
/// </summary>
[Serializable]
[XmlType(TypeName = "on", Namespace = Rule.Namespace)]
public class On
{
  /// <summary>
  /// Default constructor.
  /// </summary>
  public On()
  {
    this.SendCommandType = CommandType.Defined;
    this.ReturnCommandType = CommandType.Defined;
  }

  [XmlElement("map")]
  public Map[] Map { get; set; }

  [XmlElement("return-map")]
  public Map[] ReturnMap { get; set; }

  [XmlElement("return-autoflow")]
  public Autoflow[] ReturnAutoflow { get; set; }

  [XmlAttribute("exit-state")]
  public string ExitState { get; set; }

  [XmlAttribute("to")]
  public string To { get; set; }

  [XmlAttribute("action")]
  public Action Action { get; set; }

  [XmlAttribute("send-command-type")]
  [DefaultValue(CommandType.Defined)]
  public CommandType SendCommandType { get; set; }

  [XmlAttribute("send-command")]
  public string SendCommand { get; set; }

  [XmlAttribute("send-current-exit-state")]
  [DefaultValue(false)]
  public bool SendCurrentExitState { get; set; }

  [XmlAttribute("return-when")]
  public string ReturnWhen { get; set; }

  [XmlAttribute("return-command-type")]
  [DefaultValue(CommandType.Defined)]
  public CommandType ReturnCommandType { get; set; }

  [XmlAttribute("return-command")]
  public string ReturnCommand { get; set; }

  [XmlAttribute("return-current-exit-state")]
  [DefaultValue(false)]
  public bool ReturnCurrentExitState { get; set; }

  [XmlAttribute("display-first-on-return")]
  [DefaultValue(false)]
  public bool DisplayFirstOnReturn { get; set; }

  [XmlAttribute("display-first")]
  [DefaultValue(false)]
  public bool DisplayFirst { get; set; }

  [XmlAttribute("description")]
  public string Description { get; set; }
}
