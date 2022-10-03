using System;
using System.Collections.Generic;

using Bphx.Cool.Events;
using Bphx.Cool.UI;

namespace Bphx.Cool.Impl;

/// <summary>
/// An application state.
/// </summary>
[Serializable]
public class SessionManager: ISessionManager
{
  /// <summary>
  /// Creates a <see cref="SessionManager"/> instance.
  /// </summary>
  /// <param name="options">Session options.</param>
  public SessionManager(Dictionary<string, object> options = null)
  {
    Application = new UIApplication { SessionManager = this };
    Options = options ?? new();
  }

  /// <summary>
  /// Disposes the instance.
  /// </summary>
  public void Dispose()
  {
    try
    {
      Procedures.Clear();
      Functions.Dispose(Attributes);
    }
    finally
    {
      Profiler?.Dispose();
    }
  }

  /// <summary>
  /// Session options.
  /// </summary>
  public Dictionary<string, object> Options { get; }

  /// <summary>
  /// Session arguments.
  /// </summary>
  public string[] Arguments { get; set; }

  /// <summary>
  /// An <see cref="UIApplication"/> instance.
  /// </summary>
  public UIApplication Application { get; }

  /// <summary>
  /// A list of <see cref="IProcedure"/>s.
  /// </summary>
  public List<IProcedure> Procedures { get; } = new();

  /// <summary>
  /// A queue of <see cref="Event"/>s.
  /// </summary>
  public List<Event> Events { get; } = new();

  /// <summary>
  /// The current MessageBox instance, if available.
  /// </summary>
  public MessageBox CurrentMessageBox { get; set; }

  /// <summary>
  /// Session attributes.
  /// </summary>
  public Dictionary<string, object> Attributes { get; } = new();

  /// <summary>
  /// Optional <see cref="IProfiler"/> instance.
  /// </summary>
  public IProfiler Profiler { get; set; }

  /// <summary>
  /// Returns new procedure id.
  /// </summary>
  /// <returns>A new procedure id value.</returns>
  public int NewProcedureId() => ++lastProcedureId;

  /// <summary>
  /// Last procedure id.
  /// </summary>
  private int lastProcedureId;
}
