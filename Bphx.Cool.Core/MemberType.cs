namespace Bphx.Cool;

using System;

/// <summary>
/// Defines type of original Cool:GEN program.
/// </summary>
[Serializable]
public enum MemberType
{
  /// <summary>
  /// Object type.
  /// </summary>
  Object = 0,

  /// <summary>
  /// A fixed length string type.
  /// </summary>
  Char,

  /// <summary>
  /// A variable length string type.
  /// </summary>
  Varchar,

  /// <summary>
  /// A fixed length binary type.
  /// </summary>
  Binary,

  /// <summary>
  /// A variable length binary type.
  /// </summary>
  Varbinary,

  /// <summary>
  /// A date type.
  /// </summary>
  Date,

  /// <summary>
  /// A time type.
  /// </summary>
  Time,

  /// <summary>
  /// A timestamp type.
  /// </summary>
  Timestamp,

  /// <summary>
  /// A number type.
  /// </summary>
  Number,

  /// <summary>
  /// A binary number.
  /// </summary>
  BinaryNumber,

  /// <summary>
  /// A packed decimal number.
  /// </summary>
  PackedDecimal
}
