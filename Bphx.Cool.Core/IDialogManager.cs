using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Principal;

using Bphx.Cool.Events;
using Bphx.Cool.UI;
using Bphx.Cool.Xml;

using static Bphx.Cool.Functions;

namespace Bphx.Cool;

/// <summary>
/// Defines an interface for the dialog manager.
/// </summary>
public interface IDialogManager
{
  /// <summary>
  /// A <see cref="IServiceProvider"/> instance.
  /// </summary>
  IServiceProvider ServiceProvider { get; }

  /// <summary>
  /// An <see cref="IEnvironment"/> instance.
  /// </summary>
  IEnvironment Environment { get; }

  /// <summary>
  /// A <see cref="IResources"/> instance.
  /// </summary>
  /// <seealso cref="IEnvironment.Resources"/>
  IResources Resources { get; }

  /// <summary>
  /// An <see cref="ISessionManager"/> instance.
  /// </summary>
  ISessionManager SessionManager { get; }

  /// <summary>
  /// An application user.
  /// </summary>
  IPrincipal Principal { get; }

  /// <summary>
  /// Gets launch commands.
  /// </summary>
  List<LaunchCommand> LaunchCommands { get; }

  /// <summary>
  /// Creates a new <see cref="IProcedure"/> instance.
  /// </summary>
  /// <param name="actionInfo">An <see cref="ActionInfo"/> instance.</param>
  /// <returns>A <see cref="IProcedure"/> instance.</returns>
  IProcedure CreateProcedure(ActionInfo actionInfo);

  /// <summary>
  /// Runs the batch.
  /// </summary>
  /// <param name="actionInfo">An <see cref="ActionInfo"/> instance.</param>
  /// <param name="arguments">An arguments.</param>
  /// <param name="savepoint">
  /// Optional <see cref="ISavepoint"/> instance.
  /// </param>
  void Run(ActionInfo actionInfo, string[] arguments, ISavepoint savepoint);

  /// <summary>
  /// Executes a specified procedure step.
  /// </summary>
  /// <param name="procedure">A <see cref="IProcedure"/> instance.</param>
  void Execute(IProcedure procedure);

  /// <summary>
  /// Performs flow navigation after procedure steps' logic execution or
  /// auto flow, according with current state of procedure.
  /// </summary>
  /// <param name="procedure">A <see cref="IProcedure"/> instance.</param>
  /// <param name="exitState">a current ExitState instance.</param>
  /// <returns>A current <see cref="IProcedure"/> instance.</returns>
  IProcedure Navigate(IProcedure procedure, ExitState exitState);

  /// <summary>
  /// Performs events handling in a loop.
  /// </summary>
  /// <param name="procedure">A <see cref="IProcedure"/> instance.</param>
  /// <param name="execute">
  /// true when procedure execute is required, and false otherwise.
  /// </param>
  /// <returns>A current <see cref="IProcedure"/> instance.</returns>
  IProcedure HandleEvents(IProcedure procedure, bool execute);
}

/// <summary>
/// Dialog manager extensions.
/// </summary>
public static class DialogManagerExtensions
{
  /// <summary>
  /// Creates a new <see cref="IProcedure"/> instance.
  /// </summary>
  /// <param name="dialog">A <see cref="IDialogManager"/> instance.</param>
  /// <param name="name">A name of the procedure's step.</param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <returns>A <see cref="IProcedure"/> instance.</returns>
  public static IProcedure CreateProcedure(
    this IDialogManager dialog,
    string name,
    int resourceID) =>
    dialog.CreateProcedure(
      dialog.Resources.GetActionInfo(
        name ?? throw new ArgumentNullException(nameof(name)),
        resourceID) ??
        throw new InvalidOperationException(
          $"Cannot find \"{name}\" procedure definition."));

  /// <summary>
  /// Initializes the dialog manager with nextTran.
  /// </summary>
  /// <param name="dialog">A <see cref="IDialogManager"/> instance.</param>
  /// <param name="trancode">
  /// A transaction code with optional unformatted parameters.
  /// <para>
  /// If trancode starts with <c>"procedure:"</c> prefix then 
  /// a procedure name is expected.
  /// </para>
  /// </param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <returns>A current <see cref="IProcedure"/> instance.</returns>
  public static IProcedure CreateProcedureFromTrancode(
    this IDialogManager dialog,
    string trancode,
    int resourceID)
  {
    if (IsEmpty(trancode))
    {
      throw new ArgumentException(
        "Non empty trancode is expected.",
        nameof(trancode));
    }

    // Note: NEXTTRAN may contain an unformated parameters additionally
    // to a code transaction.
    var position = trancode.IndexOf(' ');
    var transactionCode = position == -1 ? trancode : trancode[..position];

    var resources = dialog.Resources;

    var name = transactionCode.StartsWith("procedure:") ?
      transactionCode["procedure:".Length..] :
      resources.GetProcedureByTransactionCode(transactionCode, resourceID) ??
      throw new InvalidOperationException(
        @$"Invalid transaction code: ""{transactionCode}"".");

    // Retrieves another procedure step according to NEXTTRAN.
    var procedure = dialog.CreateProcedure(name, resourceID);
    var global = procedure.Global;
    var type = procedure.Type;

    // Reset Global properties and set the current transaction code.
    global.Command = null;
    global.DatabaseErrorMessage = null;
    global.DatabaseSqlcode = 0;
    global.DatabaseSqlstate = null;
    global.SetExitState(null);
    global.Nextlocation = null;
    global.NextTran = null;
    global.Pfkey = null;
    global.TransactionRetryCount = 0;

    var actionInfo = procedure.GetActionInfo(resources);
    var rule = actionInfo?.Rule;
    var args = trancode[(position + 1)..];

    var parameters =
      (position == -1) || (position + 1 >= trancode.Length) ? null :
      (type == ProcedureType.Window) || (type == ProcedureType.Server) ?
        ParseCommandLine(args) :
        rule?.UnformattedInputDelimiter == null ?
          args.Split(',') :
          args.Split(rule.UnformattedInputDelimiter.ToCharArray());

    if (parameters != null)
    {
      if (!string.IsNullOrEmpty(rule?.UnformattedInput))
      {
        // set unformated parameters
        var unformattedInput = ParseCommandLine(rule.UnformattedInput);

        for(var i = 0; i < unformattedInput.Length; ++i)
        {
          // TODO: implement C escaping
          var value = i < parameters.Length ?
            parameters[i].Trim().Replace("\\\"", "\"") : null;
          var param = DataUtils.Split(unformattedInput[i]);

          if ((param != null) && (param.Length > 0))
          {
            DataUtils.Set(
              param.Length == 1 ? global : procedure.Import,
              param,
              value);
          }
        }
      }
      else if ((type == ProcedureType.Window) ||
        (type == ProcedureType.Server))
      {
        global.Command = parameters.Length == 1 ? parameters[0] : args;
      }
      else
      {
        // Inform the user about potential problem before run.
        global.MessageType = MessageType.Warning;
        global.Errmsg = "Unformatted input not allowed in this procedure.";

        switch(procedure.ExecutionState)
        {
          case ExecutionState.Initial:
          case ExecutionState.BeforeRun:
          {
            procedure.ExecutionState = ExecutionState.WaitForUserInput;

            break;
          }
        }
      }
    }

    return procedure;
  }

  /// <summary>
  /// Closes a <see cref="IProcedure"/>.
  /// </summary>
  /// <param name="dialog">An <see cref="IDialogManager"/> instance.</param>
  /// <param name="procedure">A procedure to close.</param>
  /// <param name="force">
  /// Indicates whether to try to force the close bypassing Close events, 
  /// if any.
  /// </param>
  public static void CloseProcedure(
    this IDialogManager dialog,
    IProcedure procedure,
    bool force)
  {
    if (procedure == null)
    {
      return;
    }

    var state = dialog.SessionManager;
    var procedures = state.Procedures;
    var close = procedure.CalledCount == 0;

    procedure.ExecutionState = ExecutionState.Closing;

    if (!close)
    {
      foreach(var other in
        procedures.Where(item => item.Caller == procedure).ToArray())
      {
        dialog.CloseProcedure(other, force);
      }

      close = procedure.CalledCount == 0;
    }

    if (procedure.Type == ProcedureType.Window)
    {
      var events = state.Events;

      if (!close || !force || (events.Count > 0))
      {
        foreach(var window in procedure.Windows)
        {
          if (window.WindowState != WindowState.Closed)
          {
            var windowEvent = new CloseEvent(procedure, window.Name)
            {
              Global = procedure.Global.Clone()
            };

            windowEvent.Prepare(dialog, events, 0);
            close = false;
          }
        }
      }
    }

    if (close)
    {
      var removed = procedures.Remove(procedure);
      var caller = procedure.Caller;

      if (removed && (caller != null))
      {
        --caller.CalledCount;

        if (state.CurrentMessageBox?.Procedure == procedure)
        {
          state.CurrentMessageBox = null;
        }

        int start = procedures.IndexOf(caller);

        if (start != -1)
        {
          CircularMove(
            procedures,
            start,
            procedures.Count,
            procedures.Count - 1);
        }

        if (caller.IsComplete())
        {
          dialog.CloseProcedure(caller, true);
        }
      }

      procedure.ExecutionState = ExecutionState.Closed;
    }

    procedure.Dirty = true;
  }

  /// <summary>
  /// Runs the batch.
  /// </summary>
  /// <param name="name">A procedure name.</param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <param name="arguments">An arguments.</param>
  /// <param name="savepoint">
  /// Optional <see cref="ISavepoint"/> instance.
  /// </param>
  public static void Run(
    this IDialogManager dialog,
    string name,
    int resourceID,
    string[] arguments,
    ISavepoint savepoint) =>
    dialog.Run(
      dialog.Resources.GetActionInfo(
        name ?? throw new ArgumentNullException(nameof(name)),
        resourceID) ??
        throw new InvalidOperationException(
          $"Cannot find \"{name}\" procedure definition."),
      arguments,
      savepoint);
}
