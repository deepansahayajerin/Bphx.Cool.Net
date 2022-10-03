using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Principal;

using Bphx.Cool.Events;
using Bphx.Cool.UI;
using Bphx.Cool.Xml;

using static Bphx.Cool.Functions;

namespace Bphx.Cool.Impl;
  
/// <summary>
/// A dialog manager implementation.
/// </summary>
public class DialogManager: IDialogManager
{
  /// <summary>
  /// A <see cref="IServiceProvider"/> instance.
  /// </summary>
  public IServiceProvider ServiceProvider { get; set; }

  /// <summary>
  /// An environment.
  /// </summary>
  public IEnvironment Environment { get; set; }

  /// <summary>
  /// Environment resources.
  /// </summary>
  public IResources Resources => Environment?.Resources;

  /// <summary>
  /// An application state.
  /// </summary>
  public ISessionManager SessionManager { get; set; }

  /// <summary>
  /// Gets a logged in application's user.
  /// </summary>
  public IPrincipal Principal { get; set; }

  /// <summary>
  /// Gets launch commands.
  /// </summary>
  public List<LaunchCommand> LaunchCommands =>
    launchCommands ??= new List<LaunchCommand>();

  /// <summary>
  /// Creates a new <see cref="IProcedure"/> instance.
  /// </summary>
  /// <param name="actionInfo">An <see cref="ActionInfo"/> instance.</param>
  /// <returns>A <see cref="IProcedure"/> instance.</returns>
  public IProcedure CreateProcedure(ActionInfo actionInfo)
  {
    if (actionInfo == null)
    {
      throw new ArgumentNullException(nameof(actionInfo));
    }

    var rule = actionInfo.Rule;
    var displayFirst = false;

    var procedure = new Procedure
    {
      Scope = Environment.ProcedureScopeResolver?.
        Invoke(actionInfo, Environment)
    };

    if (rule != null)
    {
      if (actionInfo.ProcedureType == ProcedureType.Window)
      {
        displayFirst = rule.DisplayFirst;
        procedure.PrimaryWindow = rule.PrimaryWindow;
      }
      else if (actionInfo.ProcedureType == ProcedureType.Online)
      {
        displayFirst = rule.DisplayFirst;
      }
      // No more cases.

      procedure.Transaction = rule.Transaction;
      procedure.Name = rule.Name;
    }
    else
    {
      procedure.Name = actionInfo.ActionType.Name;
    }

    procedure.Import = actionInfo.CreateImport();
    procedure.Export = actionInfo.CreateExport();

    procedure.Id = SessionManager.NewProcedureId();
    procedure.Type = actionInfo.ProcedureType;
    procedure.ResourceID = actionInfo.ResourceID;
    procedure.Role = actionInfo.ProcedureRole;
    procedure.ExecutionState = displayFirst ?
      ExecutionState.WaitForUserInputDisplayFirst :
      ExecutionState.Initial;

    var userIdProvider = Environment.UserIdProvider;
    var global = procedure.Global;

    global.UserId = userIdProvider != null ? userIdProvider(this, Principal) :
      Principal?.Identity?.Name;
    global.LocalSystemId = Environment.LocalSystemId;
    global.TransactionRetryLimit = Environment.TransactionRetryLimit;
    global.TranCode = procedure.Transaction;

    if (Environment.Authorizer?.Invoke(this, procedure, Principal) == false)
    {
      throw new SecurityException(
        $"Access is denied to procedure \"{procedure.Name}\".");
    }

    return procedure;
  }

  /// <summary>
  /// Runs the batch.
  /// </summary>
  /// <param name="actionInfo">An <see cref="ActionInfo"/> instance.</param>
  /// <param name="arguments">An arguments.</param>
  /// <param name="savepoint">
  /// Optional <see cref="ISavepoint"/> instance.
  /// </param>
  public void Run(
    ActionInfo actionInfo,
    string[] arguments, 
    ISavepoint savepoint)
  {
    if (actionInfo == null)
    {
      throw new ArgumentNullException(nameof(actionInfo));
    }

    var environment = Environment;
    var state = SessionManager;
    var procedure = savepoint?.Get() ?? CreateProcedure(actionInfo);
    var global = procedure.Global;

    state.Arguments = arguments;
    global.TranCode = procedure.Transaction;
    state.Procedures.Add(procedure);

    if ((arguments != null) && (arguments.Length > 0))
    {
      // set unformated parameters
      var rule = actionInfo.Rule;

      if (!string.IsNullOrEmpty(rule?.UnformattedInput))
      {
        // set unformated parameters
        var unformattedInput = ParseCommandLine(rule.UnformattedInput);

        for (var i = 0; i < unformattedInput.Length; ++i)
        {
          // TODO: implement C escaping
          var value = i < arguments.Length ?
            arguments[i].Trim().Replace("\\\"", "\"") : null;
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
    }

    // executes a batch procedure
    try
    {
      HandleEvents(procedure, true);
    }
    catch (Exception e)
    {
      environment.LogError(e);

      if ((savepoint != null) && (state.Procedures.Count == 1))
      {
        procedure = state.Procedures[0];

        if (procedure.ExecutionState == ExecutionState.RecoverableError)
        {
          savepoint.Set(procedure);
        }
      }

      throw;
    }

    savepoint?.Set(null);
  }

  /// <summary>
  /// Executes a specified procedure step.
  /// </summary>
  /// <param name="procedure">A <see cref="IProcedure"/> instance.</param>
  public void Execute(IProcedure procedure)
  {
    if (!procedure.CanExecute())
    {
      return;
    }

    var environment = Environment;
    var resources = Resources;
    var state = SessionManager;
    
    var actioninfo = procedure.GetActionInfo(resources) ??
      throw new InvalidOperationException(
        $"Cannot find \"{procedure.Name}\" procedure definitions.");

    var context = environment.CreateContext(procedure);
    var global = procedure.Global;

    context.Dialog = this;
    // initialize some properties of the GLOBAL instance
    // NOTE: ExitState value is not set here intentionally!
    global.Errmsg = "";
    global.MessageType = MessageType.None;
    global.TerminationAction = TerminationAction.Normal;
    global.TransactionRetryCount = 0;

    if ((procedure.ExecutionState == ExecutionState.Initial) &&
      (environment.OnStart?.Invoke(context) == false))
    {
      procedure.ExecutionState = ExecutionState.Terminated;

      return;
    }

    // resets the export before execution
    procedure.Export = actioninfo.CreateExport();
    Invoke("Execute", actioninfo, procedure, context);
  }

  /// <summary>
  /// Performs flow navigation after procedure steps' logic execution or
  /// auto flow, according with current state of procedure.
  /// </summary>
  /// <param name="procedure">A <see cref="IProcedure"/> instance.</param>
  /// <param name="exitState">a current ExitState instance.</param>``
  /// <returns>A current <see cref="IProcedure"/> instance.</returns>
  public IProcedure Navigate(IProcedure procedure, ExitState exitState)
  {
    var state = SessionManager;

    if ((procedure == null) || procedure.IsPending(state))
    {
      return procedure;
    }

    var resources = Resources;
    var profiler = state.Profiler;
    var global = procedure.Global;

    var pstep = procedure;
    var pstepGlobal = global;
    var type = pstep.Type;
    var commandType = CommandType.None;
    var definedCommand = "";
    var setExitState = false;
    var displayFirst = true;
    var navigationCase = exitState == null ? null :
      procedure.GetNavigationCase(resources, exitState.Name);
    var isReturn = 
      procedure.IsReturnFlow(resources, exitState, navigationCase);

    // The returns on exit state takes precedence over the
    // flows on exit state.
    if (isReturn != false)
    {
      this.CloseProcedure(procedure, true);

      if (!procedure.IsComplete())
      {
        // There are open windows with Close events are queued.
        // But meantime we just return.
        procedure.ExecutionState = ExecutionState.WaitForUserInput;

        return procedure;
      }

      pstep = procedure.Caller;

      if (pstep == null)
      {
        return null;
      }

      pstepGlobal = pstep.Global;
      type = pstep.Type;

      if (isReturn != null)
      {
        navigationCase =
          pstep.GetNavigationCase(resources, procedure.OriginalExitState);
        displayFirst = navigationCase.DisplayFirstOnReturn;
      }

      if (profiler != null)
      {
        Log(
          profiler,
          "Return navigation",
          procedure,
          pstep.ToString(),  // navigation target
          null,             // import
          null);            // export
      }

      // From "Block Mode Design Guide", "Designing the Procedure Dialog" 5-17,
      // page 91:
      //
      // Any data in the destination's export data view can be passed to 
      // the source procedure step's import data view, as long as the views 
      // are compatible. Remember that during a return from a link, the source 
      // procedure step's import view is populated from the export view of 
      // its previous execution. However, any data returned from the
      // destination to the source procedure step overlays the corresponding 
      // elements of that step's import view.

      // From "Client Server Design Guide", "Flows" 5-13, page 107:
      //
      // A link flow allows a user to acquire additional information from 
      // another procedure when it is needed by the initial procedure. After 
      // the information is acquired, the initial procedure executes again 
      // from the beginning with all the information from its initial
      // execution intact. Any new data or commands returned from the 
      // destination procedure is also utilized.

      // Perform view mapping on explicit return flow.
      // Note: implicit return flow is when server procedure returns by
      // not matched return exit state.
      if (navigationCase != null)
      {
        if (DataUtils.Copy(
          procedure.Export,
          pstep.Import,
          navigationCase.ReturnMap,
          CopyDirection.ExportToImport))
        {
          pstep.Dirty = true;
        }

        if (profiler != null)
        {
          Log(
            profiler,
            procedure.Name + ".Export to " +
              pstep.Name + ".Import mapping",
            pstep,              // procedure
            null,               // navigation case
            pstep.Import,
            procedure.Export);
        }

        commandType = navigationCase.ReturnCommandType;
        definedCommand = navigationCase.ReturnCommand;
        setExitState = navigationCase.ReturnCurrentExitState;
      }
      else
      {
        commandType = CommandType.Current;
        setExitState = true;
      }

      pstep.ExecutionState =
        displayFirst ?
          ExecutionState.WaitForUserInputDisplayFirst :
          ExecutionState.BeforeRun;

      var executionState = pstep.ExecutionState;

      if ((type != ProcedureType.Window) &&
        ((executionState == ExecutionState.WaitForUserInput) ||
        (executionState == ExecutionState.WaitForUserInputDisplayFirst)))
      {
        var actionInfo = pstep.GetActionInfo(resources) ??
          throw new InvalidOperationException("ActionInfo not found.");

        pstep.Export = actionInfo.CreateExport();
      }
    }
    else // check whether this is a transfer or link
    {
      if (procedure.ExecutionState == ExecutionState.Closing)
      {
        return procedure;
      }

      if (profiler != null)
      {
        Log(
          profiler,
          navigationCase == null ? "No navigation" :
          navigationCase.Action == Xml.Action.Link ?
            "Link navigation" : "Transfer navigation",
          procedure,
          navigationCase?.To,
          null,               // import
          null);              // export
      }

      if (navigationCase == null)
      {
        if ((procedure.Caller == null) &&
          (type != ProcedureType.Online) &&
          (type != ProcedureType.Window))
        {
          setExitState = true;
        }

        // was the last window of this procedure step closed?
        if (procedure.ExecutionState == ExecutionState.Closed)
        {
          pstep = procedure.Caller;

          if (pstep == null)
          {
            // a main window is closed, the application is stopped
            pstep = procedure;
            pstep.ExecutionState = ExecutionState.Terminated;
          }
          else
          {
            pstepGlobal = pstep.Global;
            pstep.ExecutionState = ExecutionState.WaitForUserInput;
          }

          type = pstep.Type;
        }
        else
        {
          // display the current procedure step's screen
          procedure.ExecutionState = ExecutionState.WaitForUserInput;

          // Don't change the current command when there is no navigation
          commandType = CommandType.Current;
        }

        goto NavigationFlow;
      }

      // this is a transfer or link, so get the next procedure step
      var name = navigationCase.To;

      pstep = this.CreateProcedure(name, procedure.ResourceID);
      pstepGlobal = pstep.Global;
      pstepGlobal.TerminalId = global.TerminalId;
      pstepGlobal.CurrentDialect = global.CurrentDialect;

      if (navigationCase.Action == Xml.Action.Link)
      {
        // link
        pstep.Caller = procedure;
        pstep.OriginalExitState = global.Exitstate;
        pstep.OriginalCommand = global.Command;

        var actionInfo = procedure.GetActionInfo(resources) ??
          throw new InvalidOperationException("ActionInfo not found.");

        procedure.Import = actionInfo.CreateImport();

        DataUtils.Copy(
          procedure.Export,
          procedure.Import,
          actionInfo.Rule.Map,
          CopyDirection.ExportToImport);

        if (profiler != null)
        {
          Log(
            profiler,
            procedure.Name + ".Export to " +
              procedure.Name + ".Import mapping",
            procedure,              // procedure
            null,               // navigation case
            procedure.Import,
            procedure.Export);
        }
      }

      displayFirst = navigationCase.DisplayFirst;

      pstep.ExecutionState = displayFirst ?
        ExecutionState.WaitForUserInputDisplayFirst :
        ExecutionState.Initial;

      if (DataUtils.Copy(
        procedure.Export,
        pstep.Import,
        navigationCase.Map,
        CopyDirection.ExportToImport))
      {
        pstep.Dirty = true;
      }

      if (profiler != null)
      {
        Log(
          profiler,
          procedure.Name + ".Export to " +
          pstep.Name + ".Import mapping",
          procedure,
          null,               // navigation case
          pstep.Import,
          procedure.Export);
      }

      commandType = navigationCase.SendCommandType;
      definedCommand = navigationCase.SendCommand;
      setExitState = navigationCase.SendCurrentExitState;
    }

  NavigationFlow:
    pstepGlobal.TransactionRetryCount = 0;
    pstepGlobal.NextTran = null;
    pstepGlobal.DatabaseErrorMessage = null;
    pstepGlobal.DatabaseSqlcode = 0;
    pstepGlobal.DatabaseSqlstate = null;
    pstepGlobal.Pfkey = null;

    switch(commandType)
    {
      case CommandType.Current:
      {
        pstepGlobal.Command = global.Command;

        break;
      }
      case CommandType.Defined:
      {
        pstepGlobal.Command = definedCommand;

        break;
      }
      case CommandType.None:
      {
        pstepGlobal.Command = null;

        break;
      }
      case CommandType.Previous:
      {
        pstepGlobal.Command = procedure.OriginalCommand;

        break;
      }
    }

    if (global != pstepGlobal)
    {
      global.TransactionRetryCount = 0;
      global.NextTran = null;
      global.DatabaseErrorMessage = null;
      global.DatabaseSqlcode = 0;
      global.DatabaseSqlstate = null;
      global.Pfkey = null;

      if (setExitState)
      {
        pstepGlobal.Exitstate = global.Exitstate;
        pstepGlobal.ExitStateId = global.ExitStateId;
        pstepGlobal.MessageType = global.MessageType;
        pstepGlobal.TerminationAction = global.TerminationAction;
        pstepGlobal.Errmsg = global.Errmsg;
      }
      else
      {
        pstepGlobal.SetExitState(null);
      }

      global.SetExitState(null);
    }
    else
    {
      if (!setExitState)
      {
        if (type == ProcedureType.Window)
        {
          pstepGlobal.SetExitState(null);
        }
        else
        {
          pstepGlobal.Exitstate = null;
          pstepGlobal.ExitStateId = 0;
          pstepGlobal.TerminationAction = TerminationAction.Normal;
        }
      }
    }

    if (profiler != null)
    {
      Log(
        profiler,
        "After navigation",
        pstep,
        null,     // navigation case
        null,     // import
        null);    // export
    }

    if (procedure != pstep)
    {
      if (pstep.Caller == procedure)
      {
        // link
        state.Procedures.Add(pstep);
        ++procedure.CalledCount;
      }
      else if (procedure.Caller == pstep)
      {
        // return
        // Close is already called.
      }
      else
      {
        // transfer
        this.CloseProcedure(procedure.GetRoot(), true);
        state.Procedures.Add(pstep);
      }
    }

    return pstep;
  }

  /// <summary>
  /// Performs events handling in a loop.
  /// </summary>
  /// <param name="procedure">A <see cref="IProcedure"/> instance.</param>
  /// <param name="execute">
  /// true when procedure execute is required, and false otherwise.
  /// </param>
  /// <returns>A current <see cref="IProcedure"/> instance.</returns>
  public IProcedure HandleEvents(IProcedure procedure, bool execute)
  {
    var environment = Environment;
    var resources = Resources;
    var state = SessionManager;
    var profiler = state.Profiler;
    var events = state.Events;
    var anEvent = default(Event);

    if (procedure.IsPending(state))
    {
      anEvent = events.
        FirstOrDefault(
          item =>
            (item.Procedure == procedure) &&
            !item.Canceled &&
            (item is CommandEvent));

      if (anEvent != null)
      {
        execute = false;
      }
    }

    // See "How Window Manager Controls a Flow" chapter in 
    // "Client Server Design Guide" book.
    while(true)
    {
      var navigate = false;
      var checkMessage = false;
      ExitState exitState = null;
      Global global;

      // If the procedure is Execute First, the main procedure logic is
      // executed. At the end of the execution of the logic, the window 
      // manager checks to see if an exit state is set to cause a flow.
      // 
      // If the exit state does not cause a flow, the primary window or 
      // dialog box associated with this procedure is opened.
      // 
      // If the procedure contains an open window event, the event is
      // executed, and the primary window or dialog box is opened.
      // 
      // If the procedure does not contain an open window event, the
      // primary window or dialog box is opened.
      if (execute)
      {
        execute = false;

        // Execute a business logic on "execute first".
        if (procedure.CanExecute())
        {
          checkMessage = true;
          global = procedure.Global;

          var command = global.Command;
          var nextTran = global.NextTran;

          if (IsEmpty(nextTran) || !Equal(command, "ENTER"))
          {
            Execute(procedure);

            // Break when there is a state machine waiting for user's input.
            if (procedure.IsPending(state))
            {
              anEvent = CheckPendingEvent(procedure);

              if (anEvent != null)
              {
                continue;
              }

              break;
            }

            nextTran = global.NextTran;
          }

          // When a procedure step completes, control is transferred
          // based on the value for NEXTTRAN. After the value of NEXTTRAN
          // is evaluated, the value of the exit state is considered.
          if (!IsEmpty(nextTran))
          {
            this.CloseProcedure(procedure.GetRoot(), true);
              
            procedure = this.CreateProcedureFromTrancode(
              nextTran, 
              procedure.ResourceID);

            procedure.ExecutionState = ExecutionState.Initial;
            procedure.Global.TerminalId = global.TerminalId;
            procedure.Global.CurrentDialect = global.CurrentDialect;
            state.Procedures.Add(procedure);
            execute = true;
            anEvent = null;

            if (profiler != null)
            {
              Log(
                profiler,
                "Go to next transaction",
                procedure,
                null,  // navigation case
                null,  // import
                null); // export
            }

            continue;
          }

          // Checks whether the exit state, if any, causes flow.
          exitState = 
            resources.GetCheckedExitState(global.Exitstate, procedure.ResourceID);

          if (exitState == null)
          {
            // for online and GUI mode - redisplay the current page,
            // for batch mode - terminate execution
            procedure.ExecutionState = ExecutionState.WaitForUserInput;
          }
          else
          {
            navigate = true;
          }
        }
      }
      else
      {
        var currentEvent = anEvent;

        anEvent = null;

        if ((currentEvent == null) && (events.Count > 0))
        {
          var i = events.Count - 1;

          currentEvent = events[i];
          events.RemoveAt(i);
        }

        if (currentEvent != null)
        {
          procedure = currentEvent.Procedure;

          if (currentEvent.Canceled || procedure.IsComplete())
          {
            continue;
          }

          global = procedure.Global;

          var checkExitState = true;

          if (currentEvent is CommandEvent commandEvent)
          {
            var command = commandEvent.Command;
            var context = state.Application.Context;
            var messageBox = state.CurrentMessageBox;
            var hasMessageBox = messageBox?.HasResult == false;

            commandEvent.Canceled = true;
            checkExitState = false;

            if ((context == null) && !hasMessageBox)
            {
              // change procedure step execution state
              var originalExecutionState = procedure.ExecutionState;

              procedure.ExecutionState = ExecutionState.BeforeRun;
              global.Command = command;

              if (procedure.CanExecute())
              {
                checkMessage = true;
                exitState = procedure.GetAutoFlowExitState(resources, command);

                if (exitState != null)
                {
                  procedure.ExecutionState = originalExecutionState;
                  global.SetExitState(exitState);
                  global.InitErrMsg(resources, procedure.ResourceID);

                  var actionInfo = procedure.GetActionInfo(resources) ??
                    throw new InvalidOperationException(
                      "ActionInfo not found.");

                  // Syncronizes self export and import.
                  DataUtils.Copy(
                    procedure.Export,
                    procedure.Import,
                    actionInfo.Rule.Map,
                    CopyDirection.ImportToExport);

                  if (profiler != null)
                  {
                    Log(
                      profiler,
                      "Autoflow",
                      procedure,
                      null,  // navigation case
                      procedure.Import,
                      procedure.Export);
                  }

                  navigate = true;
                }
              }
            }
            else if (context?.CurrentStateMachine == null)
            {
              if (messageBox?.Type == "SystemMessage")
              {
                // Reset messageType to avoid reporting message second time.
                global.MessageType = MessageType.None;
              }
              else
              {
                checkMessage = true;
                checkExitState = true;
              }

              state.CurrentMessageBox = null;
            }
            else
            {
              if (messageBox != null)
              {
                messageBox.Result = command;
              }

              ExecuteStateMachine(context, commandEvent);

              if (messageBox == state.CurrentMessageBox)
              {
                state.CurrentMessageBox = null;
              }

              if (context.CurrentStateMachine != null)
              {
                // Display a next message box (or whatever it is), if any.
                state.Application.Context = context;
              }
              else
              {
                checkMessage = true;
                checkExitState = true;
              }

              // Break when there is a state machine waiting for user's input.
              if (procedure.IsPending(state))
              {
                anEvent = CheckPendingEvent(procedure);

                if(anEvent != null)
                {
                  continue;
                }

                break;
              }
            }
          }
          else
          {
            // Handle the current event.
            if (!HandleEvent(currentEvent) && !procedure.IsComplete())
            {
              CheckActiveWindow(procedure);

              continue;
            }

            // Break when there is a state machine waiting for user's input.
            if (procedure.IsPending(state))
            {
              anEvent = CheckPendingEvent(procedure);

              if(anEvent != null)
              {
                continue;
              }

              break;
            }

            var nextTran = global.NextTran;

            if (!IsEmpty(nextTran))
            {
              this.CloseProcedure(procedure.GetRoot(), true);
                
              procedure = this.CreateProcedureFromTrancode(
                nextTran, 
                procedure.ResourceID);

              procedure.ExecutionState = ExecutionState.Initial;
              procedure.Global.TerminalId = global.TerminalId;
              procedure.Global.CurrentDialect = global.CurrentDialect;
              state.Procedures.Add(procedure);
              execute = true;

              continue;
            }

            checkMessage = true;
          }

          if (checkExitState)
          {
            // Perform flow navigation, if any.
            exitState = resources.GetCheckedExitState(
              global.Exitstate, 
              procedure.ResourceID);

            navigate = true;
          }
        }
        else if (procedure != null)
        {
          var executionState2 = procedure.ExecutionState;

          if ((executionState2 == ExecutionState.WaitForUserInput) ||
            (executionState2 == 
              ExecutionState.WaitForUserInputDisplayFirst) ||
            (executionState2 == ExecutionState.Running) ||
            ((executionState2 == ExecutionState.Closing) &&
              (procedure.CalledCount > 0)))
          {
            CheckActiveWindow(procedure);

            // Process events if there are some more.
            if (events.Count > 0)
            {
              continue;
            }

            break;
          }

          navigate = true;
        }
        // No more cases
      }

      var executionState = procedure?.ExecutionState ?? ExecutionState.Terminated;

      if ((executionState == ExecutionState.Terminated) && (events.Count == 0))
      {
        procedure = state.GetProcedureThatCanAcceptRequest();

        if (procedure == null)
        {
          break;
        }

        executionState = procedure.ExecutionState;
        navigate = false;
        execute = false;
        checkMessage = false;
      }

      global = procedure?.Global;

      var window = procedure.ActiveWindow ??
        procedure?.GetWindow(this, null, false);
      var message = checkMessage ? global?.Errmsg : null;
      var messageType = IsEmpty(message) ?
        MessageType.None : global.MessageType;
      var displayMessage = window?.DisplayExitStateMessage == true;
      var pendingExecute = false;
      var next = navigate ? Navigate(procedure, exitState) : procedure;

      if (procedure != next)
      {
        state.CancelEvents(procedure);
        procedure = next;

        if (procedure != null)
        {
          global = procedure.Global;
          window = procedure.ActiveWindow ??
            procedure.GetWindow(this, null, false);

          if (checkMessage)
          {
            var flowMessage = global.Errmsg;
            var flowMessageType = IsEmpty(flowMessage) ?
              MessageType.None : global.MessageType;

            if ((flowMessageType == MessageType.Error) ||
              ((flowMessageType == MessageType.Warning) &&
                (messageType != MessageType.Error)) ||
              (flowMessageType == MessageType.Info) &&
                (messageType != MessageType.Error) &&
                (messageType != MessageType.Warning))
            {
              message = flowMessage;
              messageType = flowMessageType;
              displayMessage = window?.DisplayExitStateMessage == true;
            }
          }
        }
      }
      else
      {
        if (execute && (executionState == ExecutionState.WaitForUserInput))
        {
          pendingExecute = true;
        }
      }

      if (procedure.IsComplete())
      {
        procedure = state.GetProcedureThatCanAcceptRequest();

        if (procedure == null)
        {
          // there are no more procedures to execute
          break;
        }

        global = procedure.Global;
        window = procedure.ActiveWindow ??
          procedure.GetWindow(this, null, false);
      }

      if (checkMessage && (window != null))
      {
        message = IsEmpty(message) ? null : TrimEnd(message);
        window.Errmsg = message;
        window.MessageType = messageType;

        if ((message != null) &&
          ((messageType == MessageType.Error) ||
            ((messageType != MessageType.None) && displayMessage)))
        {
          // Note that this is to implement the bug in original Cool:GEN
          // runtime. When there was an informational message, and 
          // a new window is not yet active, and the window of 
          // the caller procedure has DisplayExitStateMessage set to true
          // then no message is displayed at all.
          if ((messageType == MessageType.Info) &&
            (procedure.ActiveWindow == null) &&
            (procedure.Caller?.ActiveWindow?.
              DisplayExitStateMessage == false))
          {
            // Do not show the message!
          }
          else
          {
            state.CurrentMessageBox = new()
            {
              Procedure = procedure,
              Text = message,
              Style = messageType == MessageType.Error ? "Critical" :
                messageType == MessageType.Warning ? "Exclamation" :
                "Information",
              Type = "SystemMessage"
            };

            state.CancelEvents(procedure);

            break;
          }
        }
      }

      execute = procedure.CanExecute();

      if (!execute)
      {
        // Display
        PrepareToDisplay(procedure);
      }

      if (pendingExecute)
      {
        execute = true;
        procedure.ExecutionState = ExecutionState.BeforeRun;
      }
    }

    return procedure;
  }

  /// <summary>
  /// Executes pending state machine.
  /// </summary>
  /// <param name="context">
  /// A current <see cref="IContext"/> instance.
  /// </param>
  /// <param name="commandEvent">A command event.</param>
  protected void ExecuteStateMachine(
    IContext context,
    CommandEvent commandEvent)
  {
    var procedure = context.Procedure;

    // Process a waiting state machine.
    try
    {
      context.Dialog = this;
      context.CurrentEvent = commandEvent;

      context.Run(runContext =>
      {
        var stateMachine = runContext.CurrentStateMachine;
        var dispose = true;

        try
        {
          dispose = !stateMachine.MoveNext();
        }
        catch(TerminateException)
        {
          procedure.ExecutionState = ExecutionState.Terminated;
        }
        catch(AbortTransactionException)
        {
          procedure.ExecutionState = ExecutionState.CriticalError;

          throw;
        }
        finally
        {
          if (dispose)
          {
            runContext.CurrentStateMachine = null;
            (stateMachine as IDisposable)?.Dispose();
          }
        }
      });
    }
    finally
    {
      context.Dialog = null;
    }
  }

  /// <summary>
  /// Performs an event handling.
  /// </summary>
  /// <param name="global">A <see cref="Global"/> instance.</param>
  /// <param name="procedure">an IProcedure instance.</param>
  /// <param name="eventObject">
  /// an object that contains information for event handler.
  /// </param>
  /// <returns>
  /// true when the event is handled and navigation should be checked,
  /// and false otherwise.
  /// </returns>
  protected bool HandleEvent(Event eventObject)
  {
    var procedure = eventObject.Procedure;

    if (procedure.IsComplete())
    {
      return false;
    }

    var context = Environment.CreateContext(procedure);

    context.Dialog = this;

    if (!eventObject.Run(context))
    {
      eventObject.AfterRun(context, false);

      return false;
    }

    var resources = Resources;
    var actionInfo = procedure.GetActionInfo(resources) ??
      throw new InvalidOperationException("ActionInfo not found.");
    var eventID = eventObject.Key;
    bool hasHandler = false;

    if ((eventObject is ProcedureEvent) && IsEmpty(eventObject.Window))
    {
      var primaryWindow = procedure.PrimaryWindow;

      if (primaryWindow != null)
      {
        eventID = Event.GetKey(eventObject.Type, primaryWindow, null);
        hasHandler = actionInfo.Entries.ContainsKey(eventID);
      }

      if (!hasHandler)
      {
        foreach(var item in actionInfo.Entries)
        {
          if (eventObject.Type == item.Value.anEvent?.Type)
          {
            hasHandler = true;
            eventID = item.Key;

            break;
          }
        }
      }
    }
    else
    {
      hasHandler = actionInfo.Entries.ContainsKey(eventID);
    }
      
    if (!hasHandler)
    {
      eventObject.AfterRun(context, false);

      return false;
    }

    var anEvent = actionInfo.Entries[eventID].anEvent;
    var global = procedure.Global;
    var command = anEvent?.Command;

    if (IsEmpty(command))
    {
      command = eventObject.EventObject.Command;

      if (!IsEmpty(command))
      {
        global.Command = command;
      }
    }
    else
    {
      global.Command = command;
    }

    context.CurrentEvent = eventObject;

    var profiler = SessionManager.Profiler;

    if (profiler != null)
    {
      Log(
        profiler,
        "Handle event " + eventID,
        procedure,
        null,    // navigation case
        null,
        null);
    }

    string[] dataBinding = null;
    object value = null;
    var copyImportToExport = anEvent?.CopyImportToExport ?? true ||
      IsEmpty(eventObject.Control?.Binding);

    if (!copyImportToExport)
    {
      dataBinding = DataUtils.Split(eventObject.Control.Binding);
      value = DataUtils.Get(procedure.Export, dataBinding);
    }

    // Event actions never clear export views.  
    // (When the main body of the PrAD executes, the export views are 
    // cleared.) Thus, an event action may begin execution with 
    // import/export values that resulted from the execution of another 
    // event action. 
    DataUtils.Copy(
      procedure.Export,
      procedure.Import,
      actionInfo.Rule.Map,
      CopyDirection.ImportToExport);

    if (!copyImportToExport)
    {
      DataUtils.Set(procedure.Export, dataBinding, value);
    }

    if (profiler != null)
    {
      Log(
        profiler,
        "Copy import to export",
        procedure,
        null,  // navigation case
        procedure.Import,
        procedure.Export);
    }

    Invoke(eventID, actionInfo, procedure, context);

    return true;
  }

  /// <summary>
  /// Prepares import and export views of the current procedure
  /// step to display.
  /// </summary>
  /// <param name="procedure">A <see cref="IProcedure"/> to check.</param>
  protected void PrepareToDisplay(IProcedure procedure)
  {
    if (procedure == null)
    {
      return;
    }

    var type = procedure.Type;

    switch (type)
    {
      case ProcedureType.Online:
      case ProcedureType.Window:
      {
        // Note: each procedure step must have a navigation rule.
        var actionInfo = procedure.GetActionInfo(Resources) ??
          throw new InvalidOperationException("ActionInfo not found.");
        var mapping = actionInfo.Rule.Map;

        switch (procedure.ExecutionState)
        {
          case ExecutionState.WaitForUserInput:
          {
            // Syncronizes self export and import beans
            procedure.Import = actionInfo.CreateImport();

            DataUtils.Copy(
              procedure.Export,
              procedure.Import,
              mapping,
              CopyDirection.ExportToImport);

            break;
          }
          case ExecutionState.WaitForUserInputDisplayFirst:
          {
            // In GUI both import and export are used in display,
            // thus both should be prepared.
            if (type == ProcedureType.Window)
            {
              // Syncronizes self export and import beans.
              DataUtils.Copy(
                procedure.Export,
                procedure.Import,
                mapping,
                CopyDirection.ImportToExport);
            }

            break;
          }
          default:
          {
            break;
          }
        }

        CheckActiveWindow(procedure);

        if (type == ProcedureType.Window)
        {
          procedure.UpdateDefaultValues(
            procedure.ActiveWindow ??
            procedure.GetWindow(this, null, false));
        }

        switch (procedure.ExecutionState)
        {
          case ExecutionState.WaitForUserInput:
          case ExecutionState.WaitForUserInputDisplayFirst:
          {
            var profiler = SessionManager.Profiler;

            if (profiler != null)
            {
              Log(
                profiler,
                "Prepare to display",
                procedure,
                null,  // navigation case
                procedure.Import,
                procedure.Export);
            }

            break;
          }
        }

        procedure.Dirty = true;

        break;
      }
    }
  }

  /// <summary>
  /// Ensures that there is active window, if procedure step is not closed.
  /// </summary>
  /// <param name="procedure">
  /// A <see cref="IProcedure"/> to check active window for.
  /// </param>
  protected void CheckActiveWindow(IProcedure procedure)
  {
    if ((procedure.Type == ProcedureType.Window) && 
      !procedure.IsComplete() &&
      (procedure.ExecutionState != ExecutionState.Closing))
    {
      var state = SessionManager;
      var window = procedure.ActiveWindow;

      if (window == null)
      {
        var events = state.Events;

        // Activate window after all other events are processed.
        foreach(var item in events)
        {
          if ((item.Procedure == procedure) && !item.Canceled)
          {
            return;
          }
        }

        // Queue events to open new window.
        var windows = procedure.Windows;

        for (var i = windows.Count; i-- > 0;)
        {
          window = windows[i];

          if (window.WindowState == WindowState.Opened)
          {
            break;
          }

          window = null;
        }

        var canAcceptRequest = state.CanAcceptRequest(procedure);

        var eventObject = window == null ?
          (procedure.PrimaryWindow == null) ||
            ((procedure.CalledCount > 0) && !canAcceptRequest) ? null :
            new OpenEvent(procedure, procedure.PrimaryWindow) :
          !canAcceptRequest ? null as Event :
            new ActivatedEvent(procedure, window.Name);

        if (eventObject != null)
        {
          state.Profiler.Value(
            "trace",
            "queueEvent in checkActiveWindow", 
            eventObject.EventObject);

          eventObject.Prepare(this, events, 0);
        }
      }
    }
  }

  /// <summary>
  /// Logs a navigation stage.
  /// </summary>
  /// <param name="profiler">A <see cref="IProfiler"/> instance.</param>
  /// <param name="description">A stage description.</param>
  /// <param name="procedure">A <see cref="IProcedure"/> instance.</param>
  /// <param name="target">A navigation target.</param>
  /// <param name="import">Optional import data.</param>
  /// <param name="export">Optional export data.</param>
  private static void Log(
    IProfiler profiler,
    string description, 
    IProcedure procedure,
    string target,
    object import,
    object export)
  {
    using var navigate = profiler.Scope("navigate", procedure.ToString());

    navigate.Value(description);
    navigate.Value("state", null, procedure.ExecutionState.ToString());

    var window = procedure.Type == ProcedureType.Window ?
      procedure.ActiveWindow : null;

    if (window != null)
    {
      navigate.Value("window", null, window.Name);
    }

    if (target != null)
    {
      navigate.Value("to", null, target);
    }

    var global = procedure.Global;

    if (global != null)
    {
      navigate.Value("global", null, global);
    }

    if (import != null)
    {
      navigate.Value("in", null, import);
    }

    if (export != null)
    {
      navigate.Value("out", null, export);
    }
  }

  /// <summary>
  /// Invokes a procedure step's method (either main entry point or 
  /// an event handler method) within a transaction scope.
  /// </summary>
  /// <param name="methodId">An identifier of method to execute.</param>
  /// <param name="actionInfo">
  /// An <see cref="ActionInfo"/> instance that corresponds to 
  /// the current procedure step.
  /// </param>
  /// <param name="procedure">The current procedure step.</param>
  /// <param name="context">An execution context.</param>
  protected static void Invoke(
    string methodId,
    ActionInfo actionInfo,
    IProcedure procedure,
    IContext context)
  {
    DataUtils.ForEachView(
      procedure.Import,
      null,
      array => array.Index = array.InitialIndex);

    DataUtils.ForEachView(
      procedure.Export,
      null,
      array => array.Index = array.InitialIndex);

    context.Run(runContext =>
    {
      procedure.ExecutionState = ExecutionState.Running;

      var currentEvent = runContext.CurrentEvent;

      try
      {
        var action = actionInfo.Entries[methodId];

        context.Attributes["bphx:methodId"] = action.name;

        var result = runContext.Call(
          action.action,
          procedure.Import,
          procedure.Export) as IEnumerable;

        if ((result != null) || 
          (runContext.Dialog.SessionManager.
            CurrentMessageBox?.HasResult == false))
        {
          IEnumerable Execution()
          {
            if (result != null)
            {
              foreach(var item in result)
              {
                yield return item;
              }
            }

            while(runContext.Dialog.SessionManager.
              CurrentMessageBox?.HasResult == false)
            {
              yield return true;
            }

            currentEvent?.AfterRun(runContext, true);
            procedure.ExecutionState = ExecutionState.AfterRun;
          }

          var stateMachine = Execution().GetEnumerator();

          try
          {
            if (stateMachine.MoveNext())
            {
              runContext.CurrentStateMachine = stateMachine;
              runContext.Dialog = null;
            }
          }
          catch
          {
            (stateMachine as IDisposable)?.Dispose();
          }
        }
        else
        {
          if (currentEvent != null)
          {
            currentEvent.AfterRun(runContext, true);
          }

          procedure.ExecutionState = ExecutionState.AfterRun;
        }
      }
      catch(TerminateException)
      {
        procedure.ExecutionState = ExecutionState.Terminated;
      }
      catch(AbortTransactionException)
      {
        procedure.ExecutionState = ExecutionState.CriticalError;

        throw;
      }
    });

    if (context.CurrentStateMachine != null)
    {
      context.Dialog.SessionManager.Application.Context = context;
    }
  }

  /// <summary>
  /// Checks event in pending state.
  /// </summary>
  /// <param name="procedure">A procedure to test.</param>
  /// <returns>
  /// An <see cref="Event"/> instance or null, if there is no event.
  /// </returns>
  private Event CheckPendingEvent(IProcedure procedure)
  {
    var messageBox = SessionManager.CurrentMessageBox;
    
    // Message box with empty text and OK button.
    if ((messageBox != null) &&
      IsEmpty(messageBox.Type) &&
      (IsEmpty(messageBox.Buttons) || 
        (string.Compare(messageBox.Buttons, "OK", true) == 0)) &&
      IsEmpty(messageBox.Text))
    {
      // Auto trigger OK.
      return Event.Create(
        procedure,
        new EventObject
        {
          Type = CommandEvent.EventType,
          Command = "OK"
        });
    }
    
    return null;
  }

  /// <summary>
  /// Launch commands.
  /// </summary>
  private List<LaunchCommand> launchCommands;
}
