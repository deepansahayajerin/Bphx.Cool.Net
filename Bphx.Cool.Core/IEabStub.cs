using System;

namespace Bphx.Cool;

[Flags]
public enum EabOptions
{
  /// <summary>
  /// Indicates that no access fields are serialized. 
  /// </summary>
  NoAS = 4,

  /// <summary>
  /// Corresponds to High Performance View Passing option. 
  /// </summary>
  Hpvp = 2,

  /// <summary>
  /// Addition CICS parameters.
  /// </summary>
  NoIefParams = 1,

  /// <summary>
  /// Indicates that no High Performance View Passing is in effect. 
  /// </summary>
  NoHpvp = 0,

  /// <summary>
  /// Indicates that IEFPARAMS are passed.
  /// </summary>
  IefParams = 0
}

/// <summary>
/// An interface to set communication between EAB and COBOL derived code.
/// </summary>
public interface IEabStub
{
  /// <summary>
  /// Executes an external program. 
  /// </summary>
  /// <param name="name">A program name.</param>
  /// <param name="context">A <see cref="IContext"/> instance.</param>
  /// <param name="import">An import view.</param>
  /// <param name="export">An export view.</param>
  /// <param name="options">An execution options.</param>
  void Execute(
    string name,
    IContext context,
    object import,
    object export,
    EabOptions options);
}
