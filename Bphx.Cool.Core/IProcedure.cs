using System;
using System.Linq;
using System.Collections.Generic;

using Bphx.Cool.UI;
using Bphx.Cool.Xml;

using static Bphx.Cool.Functions;

namespace Bphx.Cool;

/// <summary>Defines a procedure step.</summary>
public interface IProcedure: IAttributes
{
  /// <summary>
  /// A procedure ID.
  /// </summary>
  int Id { get; }

  /// <summary>
  /// A procedure step name.
  /// </summary>
  string Name { get; }

  /// <summary>
  /// A procedure type.
  /// </summary>
  ProcedureType Type { get; }

  /// <summary>
  /// A resource ID hint.
  /// </summary>
  int ResourceID { get; }

  /// <summary>
  /// A procedure role.
  /// </summary>
  string Role { get; }

  /// <summary>
  /// Optional scope probedure belongs to.
  /// </summary>
  string Scope { get; }

  /// <summary>
  /// A transaction code, if any. 
  /// </summary>
  string Transaction { get; }

  /// <summary>
  /// Gets and sets primary window name, if any.
  /// </summary>
  string PrimaryWindow { get; set; }

  /// <summary>
  /// Gets and sets an execution state of the procedure.
  /// </summary>
  ExecutionState ExecutionState { get; set; }

  /// <summary>
  /// A global instance.
  /// </summary>
  Global Global { get; }

  /// <summary>
  /// Gets and sets an import parameters for a step.
  /// </summary>
  object Import { get; set; }

  /// <summary>
  /// Gets and sets an export parameters for a step.
  /// </summary>
  object Export { get; set; }

  /// <summary>
  /// A caller procedure step in the link chain, if any. 
  /// </summary>
  IProcedure Caller { get; set; }

  /// <summary>
  /// A number of procedures currently having this procedure as a caller.
  /// </summary>
  int CalledCount { get; set; }

  /// <summary>
  /// Original exit state.
  /// </summary>
  string OriginalExitState { get; set; }

  /// <summary>
  /// Original command.
  /// </summary>
  string OriginalCommand { get; set; }

  /// <summary>
  /// Gets Timers service.
  /// </summary>
  ITimers Timers { get; }

  /// <summary>
  /// Gets a list of openned windows/dialogs.
  /// </summary>
  IList<UIWindow> Windows { get; }

  /// <summary>
  /// Gets and sets the active window/dialog view.
  /// </summary>
  UIWindow ActiveWindow { get; set; }

  /// <summary>
  /// Dirty indicator.
  /// </summary>
  bool Dirty { get; set; }

  /// <summary>
  /// Gets <see cref="ActionInfo"/> instance.
  /// </summary>
  /// <param name="resources">
  /// A <see cref="IResources"/> to use to resolve request.
  /// </param>
  /// <returns>An <see cref="ActionInfo"/>, if available.</returns>
  ActionInfo GetActionInfo(IResources resources);

  /// <summary>
  /// Sets an <see cref="ActionInfo"/> instance.
  /// </summary>
  /// <param name="value">An <see cref="ActionInfo"/> instance.</param>
  void SetActionInfo(ActionInfo value);
}

/// <summary>
/// Extension support for <see cref="IProcedure"/> interface.
/// </summary>
public static class ProcedureExtensions
{
  /// <summary>
  /// Indicates whether the procedure can be executed. 
  /// </summary>
  /// <param name="procedure">An <see cref="IProcedure"/> instance.</param>
  /// <returns>
  /// true when the procedure step can be executed, and false otherwise.
  /// </returns>
  public static bool CanExecute(this IProcedure procedure)
  {
    if (procedure != null)
    {
      switch(procedure.ExecutionState)
      {
        case ExecutionState.Initial:
        case ExecutionState.BeforeRun:
        case ExecutionState.RecoverableError:
        {
          return true;
        }
      }
    }

    return false;
  }

  /// <summary>
  /// Indicates whether procedure is complete.
  /// </summary>
  /// <param name="procedure">An <see cref="IProcedure"/> instance.</param>
  /// <returns>
  /// <c>true</c> if <see cref="IProcedure"/>  can continue, and <c>false</c>
  /// if it is in the final state.
  /// </returns>
  public static bool IsComplete(this IProcedure procedure)
  {
    if (procedure == null)
    {
      return true;
    }

    switch(procedure.ExecutionState)
    {
      case ExecutionState.Terminated:
      case ExecutionState.Closed:
      case ExecutionState.CriticalError:
      {
        return true;
      }
    }

    return false;
  }

  /// <summary>
  /// Indicates whether the <see cref="IProcedure"/> is pending for
  /// message box or a state machine to complete.
  /// </summary>
  /// <param name="procedure">An <see cref="IProcedure"/> instance.</param>
  /// <param name="state">A <see cref="ISessionManager"/> inatance.</param>
  /// <returns>
  /// true if <see cref="IProcedure"/> is pending, and false otherwise.
  /// </returns>
  public static bool IsPending(this IProcedure procedure, ISessionManager state)
  {
    return (procedure?.ExecutionState == ExecutionState.Running) ||
      (state?.CurrentMessageBox?.HasResult == false);
  }

  /// <summary>
  /// Returns true if the specified exit state causes return flow. 
  /// </summary>
  /// <param name="procedure">An <see cref="IProcedure"/> to check.</param>
  /// <param name="resources">An <see cref="IResources"/> instance.</param>
  /// <param name="exitState">An <see cref="ExitState"/> instance.</param>
  /// <param name="navigationCase">Optional navigation case, if any.</param>
  /// <returns>
  /// <see langword="false"/> when then exit state is not return flow;
  /// <see langword="true"/> when the exit state causes return flow; 
  /// <see langword="null"/> for implicit return flow for server procedure.
  /// </returns>
  public static bool? IsReturnFlow(
    this IProcedure procedure, 
    IResources resources, 
    ExitState exitState,
    On navigationCase)
  {
    var caller = procedure.Caller;

    if (caller == null)
    {
      if ((procedure.Type == ProcedureType.Server) ||
        ((procedure.Type == ProcedureType.Window) &&
          (procedure.ActiveWindow == null) &&
          (procedure.PrimaryWindow == null)))
      {
        return navigationCase == null;
      }
    }
    else
    {
      var exitStateName = exitState == null ? "" : exitState.Name;
      var flow =
        GetNavigationCase(caller, resources, procedure.OriginalExitState);

      if ((flow != null) &&
        Global.IsMatchingExitState(
          flow.ReturnWhen,
          exitStateName,
          resources,
          caller.ResourceID))
      {
        return true;
      }

      // the exit state doesn't define an implicit return flow
      // and if it doesn't lead also to any navigation 
      // then this is explicit return flow
      if (GetNavigationCase(procedure, resources, exitStateName) == null)
      {
        switch(procedure.Type)
        {
          case ProcedureType.Server:
          {
            return null;
          }
          case ProcedureType.Window:
          {
            if ((procedure.ActiveWindow == null) &&
              (procedure.PrimaryWindow == null) &&
              (procedure.ExecutionState == ExecutionState.AfterRun))
            {
              return null;
            }

            break;
          }
        }
      }
    }

    if ((procedure.ExecutionState == ExecutionState.Closing) &&
      (procedure.CalledCount == 0))
    {
      return null;
    }

    return false;
  }

  /// <summary>
  /// Gets a <see cref="NavigationCase"/> for a <see cref="NavigationRule"/>
  /// by an exit state value.
  /// </summary>
  /// <param name="procedure">An <see cref="IProcedure"/> to check.</param>
  /// <param name="resources">An <see cref="IResources"/> instance.</param>
  /// <param name="exitState">exitState an exit state value.</param>
  /// <returns>a <see cref="NavigationCase"/> instance, or null.</returns>
  public static On GetNavigationCase(
    this IProcedure procedure,
    IResources resources,
    string exitState)
  {
    var cases = procedure.GetActionInfo(resources)?.Rule.On;

    if (cases != null)
    {
      foreach(var navigationCase in cases)
      {
        if (Global.IsMatchingExitState(
          navigationCase.ExitState,
          exitState,
          resources,
          procedure.ResourceID))
        {
          return navigationCase;
        }
      }
    }

    return null;
  }

  /// <summary>
  /// Retrievs an exit state for autoflow command, if any.
  /// </summary>
  /// <param name="procedure">An <see cref="IProcedure"/> to check.</param>
  /// <param name="resources">An <see cref="IResources"/> instance.</param>
  /// <param name="command">a command to test.</param>
  /// <returns>
  /// an ExitState instance or null when the specified command
  /// is not autoflow one.
  /// </returns>
  /// <seealso cref="IProcedure"/>
  /// <seealso cref="ExitState"/>
  public static ExitState GetAutoFlowExitState(
    this IProcedure procedure,
    IResources resources,
    string command)
  {
    if (string.IsNullOrWhiteSpace(command))
    {
      return null;
    }

    var actionInfo = procedure.GetActionInfo(resources);

    var exitState = actionInfo?.Rule?.Autoflow?.
      Where(item => Equal(item.Command, command)).
      Select(item => item.ExitState).
      FirstOrDefault();

    if (exitState == null)
    {
      var caller = procedure.Caller;

      // check return autoflow first
      if (caller != null)
      {
        var navigationCase = caller.GetNavigationCase(
          resources, 
          procedure.OriginalExitState);

        exitState = navigationCase?.ReturnAutoflow?.
          Where(item => Equal(item.Command, command)).
          Select(item => item.ExitState).
          FirstOrDefault();
      }
    }

    return exitState == null ? null : 
      resources.GetCheckedExitState(exitState, procedure.ResourceID);
  }

  /// <summary>
  /// Gets set of effective auto flow commands. 
  /// </summary>
  /// <param name="procedure">An <see cref="IProcedure"/> to check.</param>
  /// <param name="resources">An <see cref="IResources"/> instance.</param>
  /// <returns>A set of auto flow commands.</returns>
  public static ISet<string> GetAutoFlowCommands(
    this IProcedure procedure,
    IResources resources)
  {
    var caller = procedure.Caller;
    var actionInfo = procedure.GetActionInfo(resources);
    var commands = actionInfo?.Rule?.Autoflow?.Select(item => item.Command) ??
      Array.Empty<string>();

    // check return autoflow first
    if (caller != null)
    {
      var navigationCase =
        caller.GetNavigationCase(resources, procedure.OriginalExitState);

      if ((navigationCase != null) &&
        (navigationCase.ReturnAutoflow != null))
      {
        commands = commands.Concat(
          navigationCase.ReturnAutoflow.Select(item => item.Command));
      }
    }

    return commands.ToHashSet();
  }
    
  /// <summary>
  /// Tests whether the caller procedure is in the caller 
  /// chain of the called procedure.
  /// </summary>
  /// <param name="caller">A caller procedure.</param>
  /// <param name="called">A called procedure.</param>
  /// <returns>
  /// true if the called procedure is called by caller procedure; and
  /// false otherwise.
  /// </returns>
  public static bool IsCaller(this IProcedure caller, IProcedure called)
  {
    while(called != null)
    {
      called = called.Caller;

      if (called == caller)
      {
        return true;
      }
    }
    
    return false;
  }

  /// <summary>
  /// Gets a window view by name.
  /// </summary>
  /// <param name="procedure">A <see cref="IProcedure"/> instance.</param>
  /// <param name="dialog">A <see cref="IDialogManager"/> instance.</param>
  /// <param name="name">
  /// Optional, a window or dialog name.
  /// If value is not specified, or null, the active window is returned.
  /// </param>
  /// <param name="checkCallers">
  /// <code>true</code> to check caller before trying to call loader.
  /// </param>
  /// <returns>
  /// a WindowView instance if available, or null otherwise.
  /// </returns>
  public static UIWindow GetWindow(
    this IProcedure procedure,
    IDialogManager dialog,
    string name,
    bool checkCallers)
  {
    if (procedure == null)
    {
      return null;
    }

    var currentProcedure = procedure;

    if (name == null)
    {
      var activeWindow = procedure.ActiveWindow;

      if (activeWindow != null)
      {
        return activeWindow;
      }

      checkCallers = false;
      name = procedure.PrimaryWindow;

      if (name == null)
      {
        return null;
      }
    }

    while(procedure != null)
    {
      if (procedure.Type == ProcedureType.Window)
      {
        foreach(var window in procedure.Windows)
        {
          if (name == window.Name)
          {
            return window;
          }
        }
      }

      if (!checkCallers)
      {
        break;
      }

      procedure = procedure.Caller;
    }

    if (procedure == null)
    {
      if (checkCallers)
      {
        // Look through all procedures.
        foreach(var other in dialog.SessionManager.Procedures)
        {
          if ((other != currentProcedure) && 
            (other.Type == ProcedureType.Window))
          {
            foreach(var window in other.Windows)
            {
              if (name == window.Name)
              {
                return window;
              }
            }
          }
        }
      }

      procedure = currentProcedure;
    }

    if (procedure.IsComplete() ||
      (procedure.Type != ProcedureType.Window) || 
      (procedure.PrimaryWindow == null))
    {
      return null;
    }

    var loader = dialog.Environment.WindowLoader ??
      throw new ArgumentException("No WindowLoader is available.");
    var newWindow = loader.Load(procedure, name);

    newWindow.Application = dialog.SessionManager.Application;
    procedure.Windows.Add(newWindow);

    return newWindow;
  }

  /// <summary>
  /// Updates default values, if any.
  /// </summary>
  /// <param name="procedure">A <see cref="IProcedure"/> instance.</param>
  /// <param name="window">
  /// <see cref="UIWindow"/> to get default values from.
  /// If <code>null</code> is passed then 
  /// <see cref="IProcedure.ActiveWindow"/> is used.
  /// </param>
  public static void UpdateDefaultValues(this IProcedure procedure, UIWindow window)
  {
    if ((procedure == null) ||
      (procedure.Type != ProcedureType.Window) ||
      (window == null))
    {
      return;
    }

    var import = procedure.Import;

    if (import == null)
    {
      return;
    }

    var changed = false;

    foreach (var control in window.ControlsWithDefaultValues)
    {
      var binding = control.Binding;
      var defaultValue = control.DefaultValue;

      if (!IsEmpty(binding) && !IsEmpty(defaultValue))
      {
        try
        {
          var dataBinding = DataUtils.Split(binding);

          if (dataBinding.Length > 1)
          {
            var instance = import;

            for(int i = 0; i < dataBinding.Length - 1; i++)
            {
              instance = DataUtils.Get(instance, dataBinding[i]);
            }

            if (instance != null)
            {
              var name = dataBinding[^1];

              if (!IsValid(instance, name, DataUtils.Get(instance, name)))
              {
                changed |= DataUtils.Set(instance, name, defaultValue);
              }
            }
          }
        }
        catch
        {
          // Forgive on exception.
        }
      }
    }

    if (changed)
    {
      procedure.Dirty = true;
    }
  }

  /// <summary>
  /// Gets a root procedure.
  /// </summary>
  /// <param name="procedure">A procedure to get root for.</param>
  /// <returns>A root procedure.</returns>
  public static IProcedure GetRoot(this IProcedure procedure)
  {
    IProcedure root = null;

    while(procedure != null)
    {
      root = procedure;
      procedure = procedure.Caller;
    }

    return root;
  }

  /// <summary>
  /// Gets a <see cref="IScreenFields"/> instance.
  /// </summary>
  /// <param name="procedure">A procedure instance.</param>
  /// <returns>A <see cref="IScreenFields"/> instance.</returns>
  public static ScreenFields GetScreenFields(this IProcedure procedure)
  {
    if (procedure == null)
    {
      return null;
    }

    var key = "bphx:screen-fields";

    if (procedure.Attributes.TryGetValue(key, out var value))
    {
      return (ScreenFields)value;
    }

    var screenFields = new ScreenFields();

    procedure.Attributes[key] = screenFields;

    return screenFields;
  }
}

