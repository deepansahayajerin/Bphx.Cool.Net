using System;
using System.Collections.Generic;

using Bphx.Cool.UI;

using static Bphx.Cool.Functions;

namespace Bphx.Cool.Impl;

/// <summary>
/// A Procedure implementation.
/// </summary>
[Serializable]
public class Procedure: IProcedure
{
  /// <summary>
  /// A procedure ID.
  /// </summary>
  public int Id { get; set; }

  /// <summary>
  /// A procedure type.
  /// </summary>
  public ProcedureType Type { get; set; }

  /// <summary>
  /// A resource ID hint.
  /// </summary>
  public int ResourceID { get; set; }

  /// <summary>
  /// A procedure role.
  /// </summary>
  public string Role { get; set; }

  /// <summary>
  /// Optional scope probedure belongs to.
  /// </summary>
  public string Scope { get; set; }

  /// <summary>
  /// A procedure step name.
  /// </summary>
  public string Name { get; set; }

  /// <summary>
  /// A current transaction code, if any.
  /// </summary>
  public string Transaction { get; set; }

  /// <summary>
  /// Gets and sets primary window name, if any.
  /// </summary>
  public string PrimaryWindow { get; set; }

  /// <summary>
  /// Gets and sets an execution state.
  /// </summary>
  public ExecutionState ExecutionState
  {
    get => executionState;
    set
    {
      if (this.IsComplete() ||
        ((executionState == ExecutionState.Closing) &&
          (value != ExecutionState.Closed)) ||
        ((executionState == ExecutionState.Initial) &&
          (value == ExecutionState.Running)))
      {
        return;
      }

      switch(Type)
      {
        case ProcedureType.Batch:
        {
          switch(value)
          {
            case ExecutionState.WaitForUserInputDisplayFirst:
            case ExecutionState.WaitForUserInput:
            {
              value = ExecutionState.Terminated;

              break;
            }
          }

          break;
        }
        case ProcedureType.Server:
        {
          switch(value)
          {
            case ExecutionState.WaitForUserInputDisplayFirst:
            {
              value = ExecutionState.BeforeRun;

              break;
            }
            case ExecutionState.WaitForUserInput:
            {
              value = ExecutionState.AfterRun;

              break;
            }
          }

          break;
        }
        case ProcedureType.Window:
        {
          if ((ActiveWindow == null) && (PrimaryWindow == null))
          {
            goto case ProcedureType.Server;
          }

          break;
        }
      }

      switch(value)
      {
        case ExecutionState.Terminated:
        case ExecutionState.Closed:
        case ExecutionState.CriticalError:
        case ExecutionState.RecoverableError:
        {
          Dispose(attributes);

          break;
        }
      }

      executionState = value;
    }
  }

  /// <summary>
  /// A global instance.
  /// </summary>
  public Global Global { get; } = new();

  /// <summary>
  /// Gets and sets an import object.
  /// </summary>
  public object Import { get; set; }

  /// <summary>
  /// Gets and sets an import object.
  /// </summary>
  public object Export { get; set; }

  /// <summary>
  /// Gets an attribute map.
  /// </summary>
  public Dictionary<string, object> Attributes => attributes ??= new();

  /// <summary>
  /// Gets and sets a caller procedure step in the link chain, if any.
  /// </summary>
  public IProcedure Caller { get; set; }

  /// <summary>
  /// A number of procedures currently having this procedure as a caller.
  /// </summary>
  public int CalledCount { get; set; }

  /// <summary>
  /// Original exit state.
  /// </summary>
  public string OriginalExitState { get; set; }

  /// <summary>
  /// Original command.
  /// </summary>
  public string OriginalCommand { get; set; }

  /// <summary>
  /// Gets Timers service.
  /// </summary>
  public ITimers Timers => timers ??= new();

  /// <summary>
  /// A list of openned windows/dialogs.
  /// </summary>
  public IList<UIWindow> Windows => windows ??= new();

  /// <summary>
  /// Gets and sets an active window/dialog view.
  /// </summary>
  public UIWindow ActiveWindow
  {
    get => activeWindow;
    set
    {
      if (activeWindow == value)
      {
        return;
      }

      if (value != null)
      {
        var windows = Windows;
        var i = windows.IndexOf(value);

        if (i == -1)
        {
          throw new ArgumentException(
            "Invalid window: \"" + value.Name + "\"");
        }

        CircularMove(windows, 0, windows.Count, windows.Count - 1 - i);
      }

      activeWindow = value;
    }
  }

  /// <summary>
  /// Dirty indicator.
  /// </summary>
  public bool Dirty
  {
    get => dirty;
    set => dirty = value;
  }

  /// <summary>
  /// Gets <see cref="ActionInfo"/> instance.
  /// </summary>
  /// <param name="resources">
  /// A <see cref="IResources"/> to use to resolve request.
  /// </param>
  /// <returns>An <see cref="ActionInfo"/>, if available.</returns>
  public ActionInfo GetActionInfo(IResources resources) =>
    actionInfo ??= resources.GetActionInfo(Name, ResourceID);

  /// <summary>
  /// Sets an <see cref="ActionInfo"/> instance.
  /// </summary>
  /// <param name="value">An <see cref="ActionInfo"/> instance.</param>
  public void SetActionInfo(ActionInfo value) => actionInfo = value;

  /// <summary>
  /// Returns procedure name.
  /// </summary>
  /// <returns>a string representation of this Procedure instance.</returns>
  public override string ToString() => $"{Name}: {Type} ({Id})";

  /// <summary>
  /// An execution state.
  /// </summary>
  private ExecutionState executionState = ExecutionState.Initial;

  /// <summary>
  /// A procedure attributes map.
  /// </summary>
  private Dictionary<string, object> attributes;

  /// <summary>
  /// Timers instance.
  /// </summary>
  private Timers timers;

  /// <summary>
  /// Active window.
  /// </summary>
  private UIWindow activeWindow;

  /// <summary>
  /// Windows list.
  /// </summary>
  private List<UIWindow> windows;

  /// <summary>
  /// Dirty indicator.
  /// </summary>
  [NonSerialized]
  private bool dirty = true;

  /// <summary>
  /// Cached <see cref="ActionInfo"/> instance.
  /// </summary>
  [NonSerialized]
  private ActionInfo actionInfo;
}
