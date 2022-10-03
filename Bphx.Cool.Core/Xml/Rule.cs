using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bphx.Cool.Xml;

/// <summary>
/// <code>rule</code> element from procedure.xsd
/// </summary>
[Serializable]
[XmlType(TypeName = "rule", Namespace = Rule.Namespace)]
[XmlRoot("procedure", Namespace = Rule.Namespace, IsNullable = false)]
public class Rule
{
  /// <summary>
  /// Xml namespace.
  /// </summary>
  public const string Namespace = "http://www.bphx.com/coolgen/procedure/2008-11-04";

  [XmlElement("on")]
  public On[] On { get; set; }

  [XmlElement("map")]
  public Map[] Map { get; set; }

  [XmlElement("autoflow")]
  public Autoflow[] Autoflow { get; set; }

  [XmlAttribute("name")]
  public string Name { get; set; }

  [XmlAttribute("program-name")]
  public string ProgramName { get; set; }

  [XmlAttribute("class")]
  public string Type { get; set; }

  [XmlAttribute("transaction", DataType = "NMTOKEN")]
  public string Transaction { get; set; }

  [XmlAttribute("display-first")]
  [DefaultValueAttribute(true)]
  public bool DisplayFirst { get; set; } = true;

  [XmlAttribute("unformatted-input", DataType = "NMTOKENS")]
  public string UnformattedInput { get; set; }

  [XmlAttribute("unformatted-input-delimiter")]
  public string UnformattedInputDelimiter { get; set; }

  [XmlAttribute("description")]
  public string Description { get; set; }

  [XmlAttribute("primary-window")]
  public string PrimaryWindow { get; set; }

  /// <summary>
  /// Gets string representation of this rule.
  /// </summary>
  /// <returns>the rule name for debug purposes.</returns>
  public override string ToString()
  {
    return Name;
  }
}
