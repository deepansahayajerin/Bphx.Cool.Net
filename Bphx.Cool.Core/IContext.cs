using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Runtime.CompilerServices;

using Bphx.Cool.Events;
using Bphx.Cool.Xml;
using Bphx.Cool.Data;
using Bphx.Cool.UI;

using static Bphx.Cool.Functions;

namespace Bphx.Cool;

/// <summary>
/// <para>Defines an application execution context.</para>
/// <para>
/// The <see cref="IContext"/> defines a way for an action block
/// to communicate with other components and framework.
/// </para>
public interface IContext: IServiceProvider, IAttributes, IFactoryProvider
{
  /// <summary>
  /// A <see cref="IDialogManager"/> instance.
  /// </summary>
  IDialogManager Dialog { get; set; }

  /// <summary>
  /// A <see cref="IProcedure"/> instance.
  /// </summary>
  IProcedure Procedure { get; }

  /// <summary>
  /// A <see cref="Global"/> instance.
  /// </summary>
  Global Global { get; }

  /// <summary>
  /// A <see cref="IProfiler"/> instance, if available.
  /// </summary>
  IProfiler Profiler { get; set; }

  /// <summary>
  /// Encoding used to convert bytes to and from strings. 
  /// </summary>
  Encoding Encoding { get; }

  /// <summary>
  /// Optional string collator.
  /// </summary>
  IComparer<string> Collator { get; }

  /// <summary>
  /// Current <see cref="ITransaction"/> instance.
  /// </summary>
  ITransaction Transaction { get; set; }

  /// <summary>
  /// Gets and sets the current event object.
  /// </summary>
  Event CurrentEvent { get; set; }

  /// <summary>
  /// Current state machine instance.
  /// </summary>
  IEnumerator CurrentStateMachine { get; set; }

  /// <summary>
  /// <para>Provides a view instance of a specified type.</para>
  /// <para><see cref="GetData(Type)"/> is used to support local views
  /// that survive <see cref="Action"/> call.
  /// </para>
  /// <para>Lifecycle of such views is bound to:</para>
  /// <list type="bullet">
  ///   <item>
  ///     <description>
  ///       procedure in case of window procedure;
  ///     </description>
  ///   </item>
  ///   <item>
  ///     <description>
  ///       transaction for all other types of procedures.
  ///     </description>
  ///   </item>
  /// </list>
  /// </summary>
  /// <param name="type">A data type.</param>
  /// <returns>an instance of the requested type.</returns>
  object GetData(Type type);
}

/// <summary>
/// Context extensions.
/// </summary>
public static class ContextExtensions
{
  /// <summary>
  /// Provides an existing instance of the specified type or creates a new one.
  /// </summary>
  /// <param name="context">A <see cref="IContext"/> instance.</param>
  /// <returns>an instance of the requested type.</returns>
  public static T GetData<T>(this IContext context)
    where T: class, new() =>
    context.Profiler.Value("data", null, (T)context.GetData(typeof(T)));

  /// <summary>
  /// Gets a <see cref="DataConverter"/> instance.
  /// </summary>
  /// <param name="context">A <see cref="IContext"/> instance.</param>
  /// <returns>A <see cref="DataConverter"/> instance.</returns>
  public static DataConverter GetDataConverter(this IContext context)
  {
    var db = context.Dialog.Environment.DataConverter;
    var profiler = context.Profiler;

    return profiler == null ? db : new ProfilingDataConverter(profiler, db);
  }

  /// <summary>
  /// Runs action within transaction.
  /// </summary>
  /// <param name="context">A <see cref="IContext"/> instance.</param>
  /// <param name="action">An action to run.</param>
  /// <returns>A return value.</returns>
  public static void Run(this IContext context, Action<IContext> action)
  {
    if (context.Transaction != null)
    {
      throw new InvalidOperationException("Cannot run nested transaction.");
    }

    var dialog = context.Dialog ?? 
      throw new InvalidOperationException("No dialog is set");
    var environment = dialog.Environment;
    var procedure = context.Procedure;
    var global = procedure.Global;
    object import = null;
    object export = null;
    Global globalClone = null;

    // Don't clone import of the window procedure.
    if ((procedure.Type != ProcedureType.Window) &&
      (global.TransactionRetryLimit > 1))
    {
      import = DataUtils.Clone(procedure.Import);
      export = DataUtils.Clone(procedure.Export);
      globalClone = global.Clone();
    }

    while(true)
    {
      using var transaction = environment.CreateTransaction(context);

      try
      {
        var success = false;

        try
        {
          context.Run(action, transaction);

          switch(context.Global.TerminationAction)
          {
            case TerminationAction.Normal:
            {
              transaction.Commit();
              success = true;

              break;
            }
            case TerminationAction.Abort:
            {
              var message = IsEmpty(global.Exitstate) ? global.Errmsg :
                global.Exitstate + ": " + global.Errmsg;

              throw new AbortTransactionException(message);
            }
          }
        }
        finally
        {
          if (!success)
          {
            transaction.Rollback();
          }
        }

        // Exit from retry transaction loop.
        break;
      }
      catch(RetryTransactionException e)
      {
        var retryCount = global.TransactionRetryCount + 1;

        if ((globalClone != null) &&
          (retryCount <= global.TransactionRetryLimit))
        {
          // Restore state of procedure step that was before run
          // in case of any not critical error, in order to be
          // able restart the same procedure step again.
          globalClone.TransactionRetryCount = retryCount;
          global.Assign(globalClone);
          procedure.Import = DataUtils.Clone(import);
          procedure.Export = DataUtils.Clone(export);
        }
        else
        {
          throw new AbortTransactionException(
            "Transaction retry limit is exceeded.",
            e);
        }
      }
    }
  }

  /// <summary>
  /// Runs action within transaction.
  /// </summary>
  /// <param name="context">A <see cref="IContext"/> instance.</param>
  /// <param name="action">An action to run.</param>
  /// <param name="transaction">A <see cref="ITransaction"/> instance.</param>
  /// <returns>A return value.</returns>
  public static void Run(
    this IContext context,
    Action<IContext> action,
    ITransaction transaction)
  {
    if (context.Transaction != null)
    {
      throw new InvalidOperationException("Cannot run nested transaction.");
    }

    var dialog = context.Dialog ??
      throw new InvalidOperationException("No dialog is set");
    var state = dialog.SessionManager;
    var environment = dialog.Environment;
    var procedure = context.Procedure;

    //if (state.Application.Context != null)
    //{
    //  throw new InvalidOperationException("There is current context.");
    //}

    context.Transaction = transaction ??
      throw new ArgumentNullException(nameof(transaction));
    state.Application.Context = context;

    try
    {
      try
      {
        action(context);
      }
      finally
      {
        procedure.Global.InitErrMsg(environment.Resources, procedure.ResourceID);
      }
    }
    catch(Exception e)
    {
      environment.OnTransactionError?.Invoke(context, e);

      throw;
    }
    finally
    {
      context.Transaction = null;
      state.Application.Context = null;
    }
  }

  /// <summary>
  /// Calls action function.
  /// </summary>
  /// <typeparam name="I">An import type.</typeparam>
  /// <typeparam name="E">An export type.</typeparam>
  /// <typeparam name="T">An action type.</typeparam>
  /// <param name="context">A context instance.</param>
  /// <param name="factory">An instance factory.</param>
  /// <param name="action">An action function.</param>
  /// <param name="import">An import.</param>
  /// <param name="export">An export.</param>
  [MethodImpl(
    MethodImplOptions.AggressiveInlining |
    MethodImplOptions.AggressiveOptimization)]
  public static void Call<I, E, T>(
    this IContext context,
    Func<IContext, I, E, T> factory,
    Action<T> action,
    I import,
    E export)
  {
    if (context.Profiler == null)
    {
      action(factory(context, import, export));
    }
    else
    {
      TraceCall(
        context, 
        (IContext c, I i, E e) => action(factory(c, i, e)), 
        import, 
        export);
    }
  }

  /// <summary>
  /// Calls action function.
  /// </summary>
  /// <typeparam name="I">An import type.</typeparam>
  /// <typeparam name="E">An export type.</typeparam>
  /// <param name="context">A context instance.</param>
  /// <param name="action">An action function.</param>
  /// <param name="import">An import.</param>
  /// <param name="export">An export.</param>
  [MethodImpl(
    MethodImplOptions.AggressiveInlining |
    MethodImplOptions.AggressiveOptimization)]
  public static void Call<I, E>(
    this IContext context,
    Action<IContext, I, E> action,
    I import,
    E export)
  {
    if (context.Profiler == null)
    {
      action(context, import, export);
    }
    else
    {
      TraceCall(context, action, import, export);
    }
  }

  /// <summary>
  /// Calls action function.
  /// </summary>
  /// <typeparam name="I">An import type.</typeparam>
  /// <typeparam name="E">An export type.</typeparam>
  /// <typeparam name="T">An action type.</typeparam>
  /// <param name="context">A context instance.</param>
  /// <param name="factory">An instance factory.</param>
  /// <param name="action">An action function.</param>
  /// <param name="import">An import.</param>
  /// <param name="export">An export.</param>
  /// <returns>A continuation.</returns>
  [MethodImpl(
    MethodImplOptions.AggressiveInlining |
    MethodImplOptions.AggressiveOptimization)]
  public static IEnumerable<bool> Call<I, E, T>(
    this IContext context,
    Func<IContext, I, E, T> factory,
    Func<T, IEnumerable<bool>> action,
    I import,
    E export) =>
    context.Profiler == null ?
      action(factory(context, import, export)) :
      TraceCall(
        context,
        (IContext c, I i, E e) => action(factory(c, i, e)),
        import, 
        export) as IEnumerable<bool>;

  /// <summary>
  /// Calls action function.
  /// </summary>
  /// <typeparam name="I">An import type.</typeparam>
  /// <typeparam name="E">An export type.</typeparam>
  /// <param name="context">A context instance.</param>
  /// <param name="action">An action function.</param>
  /// <param name="import">An import.</param>
  /// <param name="export">An export.</param>
  /// <returns>A continuation.</returns>
  [MethodImpl(
    MethodImplOptions.AggressiveInlining |
    MethodImplOptions.AggressiveOptimization)]
  public static IEnumerable<bool> Call<I, E>(
    this IContext context,
    Func<IContext, I, E, IEnumerable<bool>> action,
    I import,
    E export) =>
    context.Profiler == null ?
      action(context, import, export) :
      TraceCall(context, action, import, export) as IEnumerable<bool>;

  /// <summary>
  /// Calls action function.
  /// </summary>
  /// <param name="context">A context instance.</param>
  /// <param name="action">An action function.</param>
  /// <param name="import">An import.</param>
  /// <param name="export">An export.</param>
  /// <returns>Invoke result.</returns>
  public static object Call(
    this IContext context,
    Delegate action,
    object import,
    object export) =>
    context.Profiler == null ?
      ActionInfo.WrapExecute(action)(context, import, export) :
      TraceCall(context, action, import, export);

  /// <summary>
  /// Calls action function with tracing.
  /// </summary>
  /// <param name="context">A context instance.</param>
  /// <param name="action">An action function.</param>
  /// <param name="import">An import.</param>
  /// <param name="export">An export.</param>
  private static object TraceCall(
    IContext context,
    Delegate action,
    object import,
    object export)
  {
    var importType = import?.GetType();
    var actionInfo = context.Procedure.GetActionInfo(context.Dialog.Resources);

    var caller = importType == null ? null :
      (actionInfo?.ImportType == importType) &&
        context.Attributes.TryGetValue("bphx:methodId", out var name) ?
        $"{actionInfo.ActionType.FullName}.{name}" :
        $"{importType.DeclaringType?.FullName}.Execute";

    var profiler = context.Profiler;
    using var call = profiler.Scope("call", caller);
    using var globalInput = call.Scope("input", "global");
    using var inInput = call.Scope("input", "in");
    using var outInput = call.Scope("input", "out");
    using var globalOutput = call.Scope("output", "global");
    using var inOutput = call.Scope("output", "in");
    using var outOutput = call.Scope("output", "out");
    var global = context.Global;

    context.Profiler = call;
    globalInput.Value(global);
    inInput.Value(import);
    outInput.Value(export);

    IEnumerable<bool> stateMachine = null;

    try
    {
      if (call.Playback())
      {
        var error = call.Value("error", null, null as Exception);

        if (error != null)
        {
          throw error;
        }
      }
      else
      {
        try
        {
          stateMachine = ActionInfo.WrapExecute(action)(context, import, export) as 
            IEnumerable<bool>;
        }
        catch(Exception e)
        {
          call.Value("error", null, e);

          throw;
        }
      }
    }
    finally
    {
      if (stateMachine == null)
      {
        globalOutput.Value(global);
        inOutput.Value(import);
        outOutput.Value(export);
        context.Profiler = profiler;
      }
    }

    if (stateMachine != null)
    {
      IEnumerable<bool> enumerator()
      {
        using var enumerator = stateMachine.GetEnumerator();

        while(true)
        {
          try
          {
            if (enumerator?.MoveNext() != true)
            {
              break;
            }
          }
          catch(Exception e)
          {
            call.Value("error", null, e);

            throw;
          }

          yield return enumerator.Current;
        }
      }

      return enumerator();
    }

    return null;
  }

  /// <summary>
  /// Gets handle for <see cref="UIObject"> instance.
  /// </summary>
  /// <param name="context">A context instance.</param>
  /// <param name="value">A <see cref="UIObject"> instance.</param>
  /// <returns>A handle value.</returns>
  public static int GetHandle(this IContext context, UIObject value)
  {
    if (value == null)
    {
      return 0;
    }

    var handle = value.Handle;

    if (handle != 0)
    {
      return handle;
    }

    handle = context.Dialog.SessionManager.Application.NewHandle();
    value.Handle = handle;

    var window = (value as UIWindow) ?? value.Owner;

    if (window != null)
    {
      window.Handles[handle] = value;
    }

    var attributes = context.Procedure.Attributes;
    var handles = attributes.Get("bphx:ui-handles") as Handles;

    if (handles == null)
    {
      handles = new();
      attributes["bphx:ui-handles"] = handles;
    }

    handles.map[handle] = value;

    return handle;
  }

  /// <summary>
  /// Gets <see cref="UIObject"> instance for a handle.
  /// </summary>
  /// <param name="context">A context instance.</param>
  /// <param name="handle">A handle value.</param>
  /// <returns>A <see cref="UIObject"> instance</returns>
  public static UIObject GetUIObject(this IContext context, int handle)
  {
    var handles = (Handles)context.Procedure.Attributes.Get("bphx:ui-handles");

    if (handles != null)
    {
      var instance = handles.map.Get(handle);

      if (instance != null)
      {
        return instance;
      }
    }

    var dialog = context.Dialog;

    if (dialog != null)
    {
      foreach(var procedure in dialog.SessionManager.Procedures)
      {
        var window = procedure.ActiveWindow;

        if (window != null)
        {
          var instance = window.Handles.Get(handle);

          if (instance != null)
          {
            return instance;
          }
        }

        foreach(var item in procedure.Windows)
        {
          var instance = item.Handles.Get(handle);

          if (instance != null)
          {
            return instance;
          }
        }

        handles = (Handles)procedure.Attributes.Get("bphx:ui-handles");

        if (handles != null)
        {
          var instance = handles.map.Get(handle);

          if(instance != null)
          {
            return instance;
          }
        }
      }
    }

    return null;
  }

  /// <summary>
  /// Queues an event for a further execution. 
  /// </summary>
  /// <param name="context">A context instance.</param>
  /// <param name="anEvent">An event to queue.</param>
  public static void QueueEvent(this IContext context, Event anEvent)
  {
    var dialog = context.Dialog;
    var events = dialog.SessionManager.Events;
    var index = 0;

    if (!anEvent.Client)
    {
      index = events.Count;

      for(var i = 0; i < events.Count; ++i)
      {
        if (!events[i].Client)
        {
          index = i;

          break;
        }
      }
    }

    context.Profiler.Value("trace", "queueEvent", anEvent.EventObject);

    anEvent.Prepare(dialog, events, index);
  }

  /// <summary>
  /// Triggers a start of a given application. 
  /// The calling application remains active.
  /// </summary>
  /// <param name="args">An application arguments.</param>
  /// <returns>A <see cref="LaunchCommand"/> instance, if any.</returns>
  public static LaunchCommand Launch(this IContext context, string args)
  {
    var dialog = context.Dialog;
    var launch = new LaunchCommand();

    launch.Get("program").Value = args;

    var handler = dialog.Environment.OnLaunch;

    if (handler != null)
    {
      if (!handler(context, launch))
      {
        return null;
      }
    }
    else
    {
      var arguments = ParseCommandLine(args);

      if ((arguments.Length > 1) &&
        (dialog.Resources.GetProcedureByTransactionCode(
          arguments[1],
          context.Procedure.ResourceID) != null))
      {
        launch.Get("transaction").Value = arguments[1];
      }
    }

    dialog.LaunchCommands.Add(launch);

    context.Profiler.Value("trace", "launch", launch);

    return launch;
  }
}

/// <summary>
/// Class encapsulating map of UI objects.
/// </summary>
class Handles: IDisposable
{
  /// <summary>
  /// Map of objects..
  /// </summary>
  public readonly Dictionary<int, UIObject> map = new();

  /// <summary>
  /// Cancels remaining tasks.
  /// </summary>
  public void Dispose()
  {
    Functions.Dispose(map);
  }
}
