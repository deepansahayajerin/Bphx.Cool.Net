using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bphx.Cool;

/// <summary>
/// An async API.An async API.
/// </summary>
public class Async
{
  /// <summary>
  /// An key owner context for a context that participates 
  /// in another transaction.
  /// </summary>
  public static readonly string OwnerContext = "bphx:owner-context";

  /// <summary>
  /// Creates <see cref="Async"/> instance.
  /// </summary>
  /// <param name="context"></param>
  public Async(IContext context) =>
    Context = context ?? throw new ArgumentNullException(nameof(context));

  /// <summary>
  /// A <see cref="IContext"/> instance.
  /// </summary>
  public IContext Context { get; }

  /// <summary>
  /// Calls action in synchronously in separate transaction.
  /// </summary>
  /// <typeparam name="I">An import type.</typeparam>
  /// <typeparam name="E">An export type.</typeparam>
  /// <param name="procedureName">A procedure step name to run.</param>
  /// <param name="action">An action instance.</param>
  /// <param name="import">An import instance.</param>
  /// <param name="export">An export instance.</param>
  /// <returns>A result instance.</returns>
  public E SyncCall<I, E>(
    string procedureName,
    Action<IContext, I, E> action,
    I import,
    E export)
  {
    E task(IContext asyncContext)
    {
      try
      {
        asyncContext.Call(action, import, export);
      }
      finally
      {
        // See https://docops.ca.com/ca-gen/8-6/en/developing/designing/using-toolset/perform-file-transfers-error/olecontrol-textalign-property/procedure-step-use
        // The current exit state and command values are preserved across 
        // procedure step usage. For example, procedure step A uses procedure 
        // step B. When procedure step B sets the exit state to "Successful" 
        // and the command to "ADD," these values are preserved when control 
        // returns to procedure step A.
        var global = Context.Global;
        var asyncGlobal = asyncContext.Global;

        global.Command = asyncGlobal.Command;
        global.Errmsg = asyncGlobal.Errmsg;
        global.Exitstate = asyncGlobal.Exitstate;
        global.ExitStateId = asyncGlobal.ExitStateId;
        global.MessageType = asyncGlobal.MessageType;
        global.TerminationAction = asyncGlobal.TerminationAction;
      }

      return export;
    };

    var participateInTransaction =
      (Context.Procedure.Type == ProcedureType.Server) &&
      (Context.Procedure.GetActionInfo(Context.Dialog.Resources)?.
        ParticipateInTransaction == true);

    if (participateInTransaction ||
      !Context.Transaction.HasEnlistedResources)
    {
      return CreateCall(procedureName, task, participateInTransaction)();
    }

    var asyncRequest = new AsyncRequest();

    Call(procedureName, ResponseScope.Procedure, asyncRequest, task);

    return Get<E>(asyncRequest);
  }

  /// <summary>
  /// Calls action asynchronously.
  /// </summary>
  /// <typeparam name="T">A result type.</typeparam>
  /// <param name="procedureName">an owner procedure step name.</param>
  /// <param name="scope">a scope of an asynchronous response.</param>
  /// <param name="asyncRequest">an AsyncRequest instance.</param>
  /// <param name="call">A call instance.</param>
  public void Call<T>(
    string procedureName,
    ResponseScope scope,
    AsyncRequest asyncRequest,
    Func<IContext, T> call)
  {
    if (asyncRequest == null)
    {
      throw new ArgumentNullException(nameof(asyncRequest));
    }

    var contextDialog = Context.Dialog;
    var environment = contextDialog.Environment;
    var contextState = contextDialog.SessionManager;
    var asyncID = environment.NewAsyncResultID();

    // reset ASYNC_RESULT workset and set newly generated request ID
    asyncRequest.Id = asyncID;
    asyncRequest.ErrorMessage = null;
    asyncRequest.Label = null;
    asyncRequest.ReasonCode = null;

    var task = CreateCall(procedureName, call, false);

    // Submit an asynchronous task
    switch(scope)
    {
      case ResponseScope.NoResponse:
      {
        CancellableTask.Run<T>(task);

        break;
      }
      case ResponseScope.Procedure:
      {
        AddTask(
          Context.Procedure.Attributes,
          asyncID,
          CancellableTask.Run<T>(task));

        break;
      }
      case ResponseScope.Global:
      {
        AddTask(
          contextState.Attributes,
          asyncID,
          CancellableTask.Run<T>(task));

        break;
      }
    }
  }

  /// <summary>
  /// Gets result of the asynchronous call.
  /// </summary>
  /// <typeparam name="T">A result type.</typeparam>
  /// <param name="asyncRequest">an AsyncRequest instance.</param>
  /// <returns>A result instance.</returns>
  protected T Get<T>(AsyncRequest asyncRequest)
  {
    if (asyncRequest != null)
    {
      var asyncID = asyncRequest.Id;

      if (asyncID != 0)
      {
        var task = Get(asyncID, true);

        if (task != null)
        {
          try
          {
            return task.GetResult<T>();
          }
          catch(Exception e)
          {
            if (e.InnerException != null)
            {
              e = e.InnerException;
            }

            asyncRequest.ErrorMessage = e.Message;

            throw new ServerException(e);
          }
        }
      }

      asyncRequest.ErrorMessage = "Invalid asynchronous request ID.";
    }

    throw new InvalidRequestIdException();
  }

  /// <summary>
  /// Checks the status of asynchronous call.
  /// </summary>
  /// <param name="asyncRequest">an AsyncRequest instance.</param>
  /// <returns>
  /// true if result is ready, and false if request is being processed.
  /// </returns>
  protected bool Check(AsyncRequest asyncRequest)
  {
    if (asyncRequest != null)
    {
      var asyncID = asyncRequest.Id;

      if (asyncID != 0)
      {
        var task = Get(asyncID, false);

        if (task != null)
        {
          return task.IsCompleted;
        }
      }

      asyncRequest.ErrorMessage = "Invalid asynchronous request ID.";
    }

    throw new InvalidRequestIdException();
  }

  /// <summary>
  /// Cancels asynchronous call.
  /// </summary>
  /// <param name="asyncRequest">an AsyncRequest instance.</param>
  protected void Cancel(AsyncRequest asyncRequest)
  {
    if (asyncRequest != null)
    {
      var asyncID = asyncRequest.Id;

      if (asyncID != 0)
      {
        var task = Get(asyncID, true);

        if (task != null)
        {
          task.Cancel();

          return;
        }
      }

      asyncRequest.ErrorMessage = "Invalid asynchronous request ID.";
    }

    throw new InvalidRequestIdException();
  }

  /// <summary>
  /// Gets a requested asynchronous task.
  /// </summary>
  /// <param name="asyncID">an unique asynchronous task identifier.</param>
  /// <param name="delete">
  /// determines whether the task should be deleted from the storage or not.
  /// </param>
  /// <returns>a CancellableTask instance.</returns>
  private CancellableTask Get(int asyncID, bool delete) =>
    Get(Context.Procedure.Attributes, asyncID, delete) ??
      Get(Context.Dialog.SessionManager.Attributes, asyncID, delete) ??
      Get(Context.Attributes, asyncID, false);

  /// <summary>
  /// Gets a requested asynchronous task.
  /// </summary>
  /// <param name="attributes">
  /// a storage for collection of asynchronous tasks.
  /// </param>
  /// <param name="asyncID">an unique asynchronous task identifier.</param>
  /// <param name="delete">
  /// determines whether the task should be deleted from the storage or not.
  /// </param>
  /// <returns>a CancellableTask instance.</returns>
  private CancellableTask Get(
    IDictionary<string, object> attributes,
    int asyncID,
    bool delete)
  {
    if (attributes.Get("bphx:tasks") is Requests requests)
    {
      if (requests.tasks.TryGetValue(asyncID, out var task))
      {
        if (delete)
        {
          requests.tasks.Remove(asyncID);

          if (task != null)
          {
            var contextAttributes = Context.Attributes;

            requests = contextAttributes.Get("bphx:tasks") as Requests;

            if (requests == null)
            {
              requests = new();
              contextAttributes.Add("bphx:tasks", requests);
            }

            requests.tasks[asyncID] = task;
          }
        }

        return task;
      }
    }

    return null;
  }

  /// <summary>
  /// Stores an asynchronous task in collection of working tasks.
  /// </summary>
  /// <param name="attributes">a storage for collection of asynchronous tasks.</param>
  /// <param name="asyncID">an unique asynchronous task identifier.</param>
  /// <param name="task">an asynchronous task to store.</param>
  private static void AddTask(
    IDictionary<string, object> attributes,
    int asyncID,
    CancellableTask task)
  {
    if (attributes.Get("bphx:tasks") is not Requests requests)
    {
      requests = new();
      attributes.Add("bphx:tasks", requests);
    }

    requests.tasks.Add(asyncID, task);
  }

  /// <summary>
  /// Creates a functions used in 
  /// <see cref="Call{T}(string, ResponseScope, AsyncRequest, Func{IContext, T})"/>.
  /// </summary>
  /// <typeparam name="T">A result type.</typeparam>
  /// <param name="procedureName">an owner procedure step name.</param>
  /// <param name="scope">a scope of an asynchronous response.</param>
  /// <param name="asyncRequest">an AsyncRequest instance.</param>
  /// <param name="call">A call instance.</param>
  /// <param name="participateInTransaction">
  /// Indicates whether to participate in the existing transaction.
  /// </param>
  /// <returns>A function instance.</returns>
  private Func<T> CreateCall<T>(
    string procedureName,
    Func<IContext, T> call,
    bool participateInTransaction)
  {
    if (call == null)
    {
      throw new ArgumentNullException(nameof(call));
    }

    var profiler = Context.Profiler.Scope("async", null);
    var contextDialog = Context.Dialog;
    var options = contextDialog.SessionManager.Options;
    var environment = contextDialog.Environment;
    var serviceProvider = environment.ServiceProvider;
    var contextState = contextDialog.SessionManager;
    var principal = contextDialog.Principal;
    var asyncGlobal = Context.Global.Clone();
    var resourceID = Context.Procedure.ResourceID;

    T task()
    {
      var result = default(T);

      using var state = environment.CreateSession(serviceProvider, options);

      state.Profiler = profiler;

      var dialog = environment.CreateDialog(serviceProvider, state, principal);
      var procedure = dialog.CreateProcedure(procedureName, resourceID);

      procedure.ExecutionState = ExecutionState.Initial;
      asyncGlobal.TranCode = procedure.Transaction;
      procedure.Global.Assign(asyncGlobal);
      state.Procedures.Add(procedure);

      var asyncContext = environment.CreateContext(procedure);

      asyncContext.Dialog = dialog;

      void action(IContext runContext) => result = call(runContext);

      if (participateInTransaction)
      {
        asyncContext.Attributes[OwnerContext] = Context;
        asyncContext.Run(action, Context.Transaction);
      }
      else
      {
        asyncContext.Run(action);
      }

      var messageBox = state.CurrentMessageBox;

      if (messageBox != null)
      {
        throw new InvalidOperationException(
          "Cannot call message box in async call. " + messageBox.Text);
      }

      return result;
    }

    return task;
  }

  /// <summary>
  /// Asynchronous task wrapper that allows to cancel that task.
  /// </summary>
  class CancellableTask
  {
    /// <summary>
    /// Creates and runs a Task instance.
    /// </summary>
    public static CancellableTask Run<T>(Func<T> call)
    {
      var wrapper = new CancellableTask();

      wrapper.task = Task.Run(call, wrapper.tokenSource.Token);

      return wrapper;
    }

    /// <summary>
    /// Cancels the background Task instance.
    /// </summary>
    public void Cancel() => tokenSource.Cancel();

    /// <summary>
    /// Returns the asynchronous task result.
    /// </summary>
    /// <returns></returns>
    public T GetResult<T>()
    {
      var task = this.task as Task<T>;

      if (tokenSource.IsCancellationRequested || (task == null))
      {
        return default;
      }

      task.Wait();

      return task.Result;
    }

    /// <summary>
    /// Checks whether the corresponding task is completed.
    /// </summary>
    public bool IsCompleted =>
      task.IsCompleted && !tokenSource.IsCancellationRequested;

    /// <summary>
    /// Checks whether the corresponding task is canceled.
    /// </summary>
    public bool IsCanceled => tokenSource.IsCancellationRequested;

    /// <summary>
    /// Default constructor.
    /// </summary>
    private CancellableTask() { }

    private Task task;
    private readonly CancellationTokenSource tokenSource = new();
  }

  /// <summary>
  /// Class encapsulating collection queued and running async tasks.
  /// </summary>
  class Requests: IDisposable
  {
    /// <summary>
    /// Collections of tasks.
    /// </summary>
    public readonly Dictionary<int, CancellableTask> tasks = new();

    /// <summary>
    /// Cancels remaining tasks.
    /// </summary>
    public void Dispose()
    {
      foreach(var task in tasks.Values)
      {
        task.Cancel();
      }
    }
  }
}
