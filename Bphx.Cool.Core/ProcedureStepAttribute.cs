using System;

namespace Bphx.Cool;

/// <summary>
/// <para>An attribute defining a procedure step.</para>
/// <para>
/// Classes annotated with <see cref="ProcedureStepAttribute "/> attribute are
/// considered a procedure steps. Such classes should be executed in 
/// a separate transaction.
/// </para>
/// <para>
/// There are several types of procedure steps that define 
/// navigation behavior. Procedure step type is defined by the 
/// <see cref="Type"/> property.
/// </para>
/// </summary>
[AttributeUsage(
  AttributeTargets.Class |
    AttributeTargets.Interface |
    AttributeTargets.Struct,
  AllowMultiple = false)]
public class ProcedureStepAttribute : System.Attribute
{
  /// <summary>
  /// Creates a <see cref="ProcedureStepAttribute"/> instance.
  /// </summary>
  public ProcedureStepAttribute() { }

  /// <summary>
  /// Creates a <see cref="ProcedureStepAttribute"/> instance.
  /// </summary>
  /// <param name="type">A procedure type.</param>
  public ProcedureStepAttribute(ProcedureType type)
  {
    Type = type;
  }

  /// <summary>
  /// A procedure step type.
  /// </summary>
  public ProcedureType Type { get; set; }

  /// <summary>
  /// Indicates whether the server procedure step should participate in
  /// an existing transaction formed by a caller procedure step.
  /// 
  /// Default is false - no.
  /// </summary>
  public bool ParticipateInTransaction { get; set; }

  /// <summary>
  /// A procedure step role.
  /// </summary>
  public string Role { get; set; }
}
