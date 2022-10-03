using System.Xml.Serialization;

namespace Bphx.Cool;

/// <summary>
/// <para>Defines type of a procedure.</para>
/// <para>
/// A type of procedure impacts on navigation behavior and on 
/// resources associated with the procedure.
/// </para>
/// </summary>
/// <seealso cref="ProcedureStepAttribute"/>
public enum ProcedureType
{
  /// <summary>
  /// A procedure of unknown type.
  /// </summary>
  [XmlEnum("default")]
  Default,

  /// <summary>
  /// <para>A batch procedure.</para>
  /// <list type="bullet">
  /// <listheader>Batch procedure:</listheader>
  /// <item>
  ///   <description>models original batch application;</description>
  /// </item>
  /// <item>
  ///   <description>
  ///   should be used to execute offline batches that usually run from a
  ///   command line, though it can be exposed as a SOAP or a REST service;
  ///   </description>
  /// </item>
  /// <item>
  ///   <description>
  ///   may use <see cref="Bphx.Cool.Xml.Action.Transfer"/> navigation action
  ///   to other <see cref="Batch"/> procedure;
  ///   </description>
  /// </item>
  /// <item>
  ///   <description>
  ///   runs as execute first (there is no display first mode);  
  ///   </description>
  /// </item>
  /// <item>
  ///   <description>may receive unformatted parameters.</description>
  /// </item>
  /// </list>
  /// </summary>
  [XmlEnum("batch")]
  Batch,

  /// <summary>
  /// <para>An online procedure.</para>
  /// <list type="bullet">
  /// <listheader>Online procedure:</listheader>
  /// <item>
  ///   <description>models original online application;</description>
  /// </item>
  /// <item>
  ///   <description>
  ///   should be used to execute web requests through the REST service;
  ///   </description>
  /// </item>
  /// <item>
  ///   <description>
  ///   supports all navigation actions, as well as navigation 
  ///   <see cref="Global.NextTran"/> to other <see cref="Online"/> or 
  ///   <see cref="Server"/> procedures;
  ///   </description>
  /// </item>
  /// <item>
  ///   <description>supports display and execute first mode;</description>
  /// </item>
  /// <item>
  ///   <description>may receive unformatted parameters;</description>
  /// </item>
  /// <item>
  ///   <description>must have a screen associated.</description>
  /// </item>
  /// </list>
  /// </summary>
  /// </summary>
  [XmlEnum("online")]
  Online,

  /// <summary>
  /// <para>A window procedure.</para>
  /// <list type="bullet">
  /// <listheader>Window procedure:</listheader>
  /// <item>
  ///   <description>models original GUI application;</description>
  /// </item>
  /// <item>
  ///   <description>
  ///   should be used to execute web requests through the REST service;
  ///   </description>
  /// </item>
  /// <item>
  ///   <description>
  ///   supports all navigation actions, as well as navigation 
  ///   <see cref="Global.NextTran"/> to other <see cref="Window"/> or 
  ///   <see cref="Server"/> procedures;
  ///   </description>
  /// </item>
  /// <item>
  ///   <description>supports display and execute first mode;</description>
  /// </item>
  /// <item>
  ///   <description>may receive unformatted parameters;</description>
  /// </item>
  /// <item>
  ///   <description>must have one or more windows associated.</description>
  /// </item>
  /// </list>
  /// </summary>
  /// </summary>
  [XmlEnum("window")]
  Window,

  /// <summary>
  /// <para>A server procedure.</para>
  /// <list type="bullet">
  /// <listheader>Server procedure:</listheader>
  /// <item>
  ///   <description>models original server procedure;</description>
  /// </item>
  /// <item>
  ///   <description>models original screenless online procedure;</description>
  /// </item>
  /// <item>
  ///   <description>models original windowless client procedure;</description>
  /// </item>
  /// <item>
  ///   <description>may be exposed as a SOAP or a REST service;</description>
  /// </item>
  /// <item>
  ///   <description>
  ///   may use <see cref="Bphx.Cool.Xml.Action.Link"/> navigation action to
  ///   other <see cref="Server"/> procedure or to <see cref="Window"/> or 
  ///   <see cref="Online"/> procedure (depending on the caller);
  ///   </description>
  /// </item>
  /// <item>
  ///   <description>
  ///   runs as execute first (there is no display first mode).
  ///   </description>
  /// </item>
  /// </list>
  /// </summary>
  [XmlEnum("server")]
  Server
}
