namespace Bphx.Cool;

using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Principal;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using System.Text.Json.Serialization;

using Bphx.Cool.Events;
using Bphx.Cool.UI;
using Bphx.Cool.Impl;

using static Bphx.Cool.Functions;

/// <summary>
/// A class to process a server request.
/// </summary>
public class Client
{
  /// <summary>
  /// <para>
  /// Starts the application from the specified procedure step name or 
  /// transaction code with optional clear screen input parameters.
  /// </para>
  /// <para>
  /// <b>Note:</b> when procedure step name is defined then it will be executed,
  /// otherwise the transaction code from command line arguments will be taken.
  /// When neither procedure nor command line arguments are specified the 
  /// exception will be thrown.
  /// </para>
  /// </summary>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/> instance.
  /// </param>
  /// <param name="principal">A <see cref="IPrincipal"/> instance.</param>
  /// <param name="request">
  /// A <see cref="StartRequest"/> instance that contains either
  /// a procedure step name to execute or a transaction code with optional
  /// clear screen input parameters and displayFirst flag.
  /// </param>
  /// <returns>
  /// A <see cref="ResponseType.Navigate"/> <see cref="ApplicationResponse"/>
  /// instance.
  /// </returns>
  public static ApplicationResponse Start(
    IServiceProvider serviceProvider,
    IPrincipal principal,
    StartRequest request) =>
    Start(
      serviceProvider, 
      serviceProvider.GetService<IEnvironment>(), 
      serviceProvider.GetService<ISessions>(), 
      principal,
      request);

  /// <summary>
  /// <para>
  /// Starts the application from the specified procedure step name or 
  /// transaction code with optional clear screen input parameters.
  /// </para>
  /// <para>
  /// <b>Note:</b> when procedure step name is defined then it will be executed,
  /// otherwise the transaction code from command line arguments will be taken.
  /// When neither procedure nor command line arguments are specified the 
  /// exception will be thrown.
  /// </para>
  /// </summary>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/> instance.
  /// </param>
  /// <param name="environment">
  /// An <see cref="IEnvironment"/> instance.
  /// </param>
  /// <param name="sessions">A <see cref="ISessions"/> instance.</param>
  /// <param name="principal">A <see cref="IPrincipal"/> instance.</param>
  /// <param name="request">
  /// A <see cref="StartRequest"/> instance that contains either
  /// a procedure step name to execute or a transaction code with optional
  /// clear screen input parameters and displayFirst flag.
  /// </param>
  /// <returns>
  /// A <see cref="ResponseType.Navigate"/> <see cref="ApplicationResponse"/>
  /// instance.
  /// </returns>
  public static ApplicationResponse Start(
    IServiceProvider serviceProvider,
    IEnvironment environment,
    ISessions sessions,
    IPrincipal principal,
    StartRequest request) =>
    new Client(serviceProvider, environment, sessions, principal).
      Start(request);

  /// <summary>
  /// Gets the current state.
  /// </summary>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/> instance.
  /// </param>
  /// <param name="principal">A <see cref="IPrincipal"/> instance.</param>
  /// <param name="index">A state index.</param>
  /// <returns>
  /// A <see cref="ResponseType.Navigate"/> <see cref="ApplicationResponse"/>
  /// instance.
  /// </returns>
  public static ApplicationResponse Current(
    IServiceProvider serviceProvider,
    IPrincipal principal,
    int index) =>
    Current(
      serviceProvider,
      serviceProvider.GetService<IEnvironment>(),
      serviceProvider.GetService<ISessions>(),
      principal,
      index);

  /// <summary>
  /// Gets the current state.
  /// </summary>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/> instance.
  /// </param>
  /// <param name="environment">
  /// An <see cref="IEnvironment"/> instance.
  /// </param>
  /// <param name="sessions">A <see cref="ISessions"/> instance.</param>
  /// <param name="principal">A <see cref="IPrincipal"/> instance.</param>
  /// <param name="index">A state index.</param>
  /// <returns>
  /// A <see cref="ResponseType.Navigate"/> <see cref="ApplicationResponse"/>
  /// instance.
  /// </returns>
  public static ApplicationResponse Current(
    IServiceProvider serviceProvider,
    IEnvironment environment,
    ISessions sessions,
    IPrincipal principal,
    int index) =>
    new Client(serviceProvider, environment, sessions, principal).
      Current(index);

  /// <summary>
  /// Finishes the application and route to the end page.
  /// </summary>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/> instance.
  /// </param>
  /// <param name="environment">
  /// An <see cref="IEnvironment"/> instance.
  /// </param>
  /// <param name="sessions">A <see cref="ISessions"/> instance.</param>
  /// <param name="principal">A <see cref="IPrincipal"/> instance.</param>
  /// <param name="index">A state index.</param>
  /// <returns>
  /// A <see cref="ResponseType.End"/> <see cref="ApplicationResponse"/>
  /// instance.
  /// </returns>
  public static ApplicationResponse Logout(
    IServiceProvider serviceProvider,
    IPrincipal principal,
    int index) =>
    Logout(
      serviceProvider,
      serviceProvider.GetService<IEnvironment>(),
      serviceProvider.GetService<ISessions>(),
      principal,
      index);

  /// <summary>
  /// Finishes the application and route to the end page.
  /// </summary>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/> instance.
  /// </param>
  /// <param name="environment">
  /// An <see cref="IEnvironment"/> instance.
  /// </param>
  /// <param name="sessions">A <see cref="ISessions"/> instance.</param>
  /// <param name="principal">A <see cref="IPrincipal"/> instance.</param>
  /// <param name="index">A state index.</param>
  /// <returns>
  /// A <see cref="ResponseType.End"/> <see cref="ApplicationResponse"/>
  /// instance.
  /// </returns>
  public static ApplicationResponse Logout(
    IServiceProvider serviceProvider,
    IEnvironment environment,
    ISessions sessions,
    IPrincipal principal,
    int index) =>
    new Client(serviceProvider, environment, sessions, principal).
      Logout(index);

  /// <summary>
  /// Forks the state of the application.
  /// </summary>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/> instance.
  /// </param>
  /// <param name="principal">A <see cref="IPrincipal"/> instance.</param>
  /// <param name="index">A state index.</param>
  /// <returns>
  /// A <see cref="ResponseType.Navigate"/> <see cref="ApplicationResponse"/>
  /// instance.
  /// </returns>
  public static ApplicationResponse Fork(
    IServiceProvider serviceProvider,
    IPrincipal principal,
    int index) =>
    Fork(
      serviceProvider,
      serviceProvider.GetService<IEnvironment>(),
      serviceProvider.GetService<ISessions>(),
      principal,
      index);

  /// <summary>
  /// Forks the state of the application.
  /// </summary>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/> instance.
  /// </param>
  /// <param name="environment">
  /// An <see cref="IEnvironment"/> instance.
  /// </param>
  /// <param name="sessions">A <see cref="ISessions"/> instance.</param>
  /// <param name="principal">A <see cref="IPrincipal"/> instance.</param>
  /// <param name="index">A state index.</param>
  /// <returns>
  /// A <see cref="ResponseType.Navigate"/> <see cref="ApplicationResponse"/>
  /// instance.
  /// </returns>
  public static ApplicationResponse Fork(
    IServiceProvider serviceProvider,
    IEnvironment environment,
    ISessions sessions,
    IPrincipal principal,
    int index) =>
    new Client(serviceProvider, environment, sessions, principal).
      Fork(index);

  /// <summary>
  /// Changes the current dialect.
  /// </summary>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/> instance.
  /// </param>
  /// <param name="principal">A <see cref="IPrincipal"/> instance.</param>
  /// <param name="request">
  /// A <see cref="ChangeDialectRequest"/> instance.
  /// </param>
  /// <returns>
  /// A <see cref="ResponseType.Navigate"/> <see cref="ApplicationResponse"/>
  /// instance.
  /// </returns>
  public static ApplicationResponse ChangeDialect(
    IServiceProvider serviceProvider,
    IPrincipal principal,
    ChangeDialectRequest request) =>
    ChangeDialect(
      serviceProvider,
      serviceProvider.GetService<IEnvironment>(),
      serviceProvider.GetService<ISessions>(),
      principal,
      request);

  /// <summary>
  /// Changes the current dialect.
  /// </summary>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/> instance.
  /// </param>
  /// <param name="environment">
  /// An <see cref="IEnvironment"/> instance.
  /// </param>
  /// <param name="sessions">A <see cref="ISessions"/> instance.</param>
  /// <param name="principal">A <see cref="IPrincipal"/> instance.</param>
  /// <param name="request">
  /// A <see cref="ChangeDialectRequest"/> instance.
  /// </param>
  /// <returns>
  /// A <see cref="ResponseType.Navigate"/> <see cref="ApplicationResponse"/>
  /// instance.
  /// </returns>
  public static ApplicationResponse ChangeDialect(
    IServiceProvider serviceProvider,
    IEnvironment environment,
    ISessions sessions,
    IPrincipal principal,
    ChangeDialectRequest request) =>
    new Client(serviceProvider, environment, sessions, principal).
      ChangeDialect(request);

  /// <summary>
  /// Gets the current state of the application.
  /// </summary>
  /// <typeparam name="R">A type of response.</typeparam>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/> instance.
  /// </param>
  /// <param name="principal">A <see cref="IPrincipal"/> instance.</param>
  /// <param name="index">A state index.</param>
  /// <param name="id">A procedure id.</param>
  /// <returns>A response instance.</returns>
  public static R Get<R>(
    IServiceProvider serviceProvider,
    IPrincipal principal,
    int index,
    int id)
    where R : Response, new() =>
    Get<R>(
      serviceProvider,
      serviceProvider.GetService<IEnvironment>(),
      serviceProvider.GetService<ISessions>(),
      principal,
      index,
      id);

  /// <summary>
  /// Gets the current state of the application.
  /// </summary>
  /// <typeparam name="R">A type of response.</typeparam>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/> instance.
  /// </param>
  /// <param name="environment">
  /// An <see cref="IEnvironment"/> instance.
  /// </param>
  /// <param name="sessions">A <see cref="ISessions"/> instance.</param>
  /// <param name="principal">A <see cref="IPrincipal"/> instance.</param>
  /// <param name="index">A state index.</param>
  /// <param name="id">A procedure id.</param>
  /// <returns>A response instance.</returns>
  public static R Get<R>(
    IServiceProvider serviceProvider,
    IEnvironment environment,
    ISessions sessions,
    IPrincipal principal,
    int index,
    int id)
    where R: Response, new()
  {
    var response = new R();
    var client = 
      new Client(serviceProvider, environment, sessions, principal);

    client.Get(index, id, response);

    return response;
  }

  /// <summary>
  /// Executes a client event.
  /// </summary>
  /// <typeparam name="R">A type of response.</typeparam>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/> instance.
  /// </param>
  /// <param name="environment">
  /// An <see cref="IEnvironment"/> instance.
  /// </param>
  /// <param name="principal">A <see cref="IPrincipal"/> instance.</param>
  /// <param name="request">A <see cref="Request"/> instance.</param>
  /// <returns>A response instance.</returns>
  public static R Event<R>(
    IServiceProvider serviceProvider,
    IPrincipal principal,
    Request request)
    where R : Response, new() =>
    Event<R>(
      serviceProvider,
      serviceProvider.GetService<IEnvironment>(),
      serviceProvider.GetService<ISessions>(),
      principal,
      request);

  /// <summary>
  /// Executes a client event.
  /// </summary>
  /// <typeparam name="R">A type of response.</typeparam>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/> instance.
  /// </param>
  /// <param name="environment">
  /// An <see cref="IEnvironment"/> instance.
  /// </param>
  /// <param name="sessions">A <see cref="ISessions"/> instance.</param>
  /// <param name="principal">A <see cref="IPrincipal"/> instance.</param>
  /// <param name="request">A <see cref="Request"/> instance.</param>
  /// <returns>A response instance.</returns>
  public static R Event<R>(
    IServiceProvider serviceProvider,
    IEnvironment environment,
    ISessions sessions,
    IPrincipal principal,
    Request request)
    where R : Response, new()
  {
    var response = new R();
    var client = 
      new Client(serviceProvider, environment, sessions, principal);

    client.Execute(request, response);

    return response;
  }

  /// <summary>
  /// Processes server request.
  /// </summary>
  /// <typeparam name="R">A type of response.</typeparam>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/> instance.
  /// </param>
  /// <param name="principal">A <see cref="IPrincipal"/> instance.</param>
  /// <param name="name">A procedure name.</param>
  /// <param name="request">A request instance.</param>
  /// <param name="response">A response instance.</param>
  public static R Execute<R>(
    IServiceProvider serviceProvider,
    IPrincipal principal,
    string name,
    Data request)
    where R : Data, new() =>
    Execute<R>(
      serviceProvider,
      serviceProvider.GetService<IEnvironment>(),
      principal,
      name,
      request);

  /// <summary>
  /// Processes server request.
  /// </summary>
  /// <typeparam name="R">A type of response.</typeparam>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/> instance.
  /// </param>
  /// <param name="environment">
  /// An <see cref="IEnvironment"/> instance.
  /// </param>
  /// <param name="principal">A <see cref="IPrincipal"/> instance.</param>
  /// <param name="name">A procedure name.</param>
  /// <param name="request">A request instance.</param>
  /// <param name="response">A response instance.</param>
  public static R Execute<R>(
    IServiceProvider serviceProvider,
    IEnvironment environment,
    IPrincipal principal,
    string name,
    Data request)
    where R: Data, new()
  {
    if (environment == null)
    {
      throw new ArgumentNullException(nameof(environment));
    }

    if (name == null)
    {
      throw new ArgumentNullException(nameof(name));
    }

    var response = new R();
    using var state = environment.CreateSession(serviceProvider);
    using var profiler = state.Profiler;
    using var action = profiler.Scope("request", "execute");
    using var input = action.Scope("input", null);
    using var output = action.Scope("output", null);

    try
    { 
      input.Value(request);

      var dialog = environment.CreateDialog(serviceProvider, state, principal);
      var procedure = dialog.CreateProcedure(name, 0);
      var global = procedure.Global;
      var requestGlobal = request.Global;

      if (requestGlobal != null)
      {
        if (IsEmpty(requestGlobal.Exitstate))
        {
          var exitstate = environment.Resources.
            GetExitState(requestGlobal.Exitstate, procedure.ResourceID);

          if (exitstate != null)
          {
            requestGlobal.Exitstate = exitstate.Name;
          }
        }

        requestGlobal.TranCode = procedure.Transaction;
        requestGlobal.UserId = procedure.Global.UserId;
        global.Assign(requestGlobal);
      }

      procedure.ExecutionState = ExecutionState.Initial;
      procedure.Import = request.Value;
      state.Procedures.Add(procedure);
      dialog.Execute(procedure);
      response.Global = global;
      output.Value(response);
    }
    catch(Exception e)
    {
      action.Value("error", null, e);

      throw;
    }

    return response;
  }

  /// <summary>
  /// A start request type.
  /// </summary>
  [Serializable]
  public class StartRequest
  {
    /// <summary>
    /// A state index.
    /// </summary>
    [XmlElement("index")]
    public int Index { get; set; }

    /// <summary>
    /// A procedure step to start.
    /// </summary>
    [XmlElement("procedure")]
    public string Procedure { get; set; }

    /// <summary>
    /// A command line arguments. The first argument is a transaction code
    /// to execute.The rest, if any, are clear screen input parameters 
    /// separated by space.
    /// </summary>
    [XmlElement("commandLine")]
    public string CommandLine { get; set; }

    /// <summary>
    /// A display-first flag value, if any.
    /// </summary>
    [XmlElement("displayFirst")]
    public bool? DisplayFirst { get; set; }

    /// <summary>
    /// A restart flag, if any. If no value is specified then it 
    /// will restart.
    /// </summary>
    [XmlElement("restart")]
    public bool? Restart { get; set; }

    /// <summary>
    /// A <see cref="Global"/> instance.
    /// </summary>
    [XmlElement("global")]
    public Global Global { get; set; }
  }

  /**
    * A change dialect request type.
    */
  [Serializable]
  public class ChangeDialectRequest
  {
    /// <summary>
    /// A state index.
    /// </summary>
    [XmlElement("index")]
    public int Index { get; set; }

    /// <summary>
    /// A <see cref="Global"/> instance.
    /// </summary>
    [XmlElement("global")]
    public Global Global { get; set; }
  }

  /// <summary>
  /// A procedure digest.
  /// </summary>
  [Serializable]
  public class ProcedureDigest
  {
    /// <summary>
    /// A procedure ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// A procedure name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// A procedure scope.
    /// </summary>
    public string Scope { get; set; }

    /// <summary>
    /// A procedure type.
    /// </summary>
    public ProcedureType Type { get; set; }

    /// <summary>
    /// Gets a list of opened windows/dialogs states.
    /// </summary>
    [XmlElement("windows")]
    public StateData[] Windows { get; set; }

    /// <summary>
    /// A list of commands.
    /// </summary>
    [XmlElement("commands")]
    public CommandView[] Commands { get; set; }

    /// <summary>
    /// Changed indicator.
    /// </summary>
    [DefaultValue(false)]
    public bool Changed { get; set; }

    /// <summary>
    /// A locked indicator.
    /// </summary>
    [DefaultValue(false)]
    public bool Locked { get; set; }
  }

  /// <summary>
  /// A response type.
  /// </summary>
  [Serializable]
  public class ApplicationResponse
  {
    /// <summary>
    /// Environment mode.
    /// </summary>
    [XmlElement("mode")]
    public string Mode { get; set; }

    /// <summary>
    /// A state index.
    /// </summary>
    [XmlElement("index")]
    public int Index { get; set; }

    /// <summary>
    /// A procedure id.
    /// </summary>
    [XmlElement("id")]
    public int Id { get; set; }

    /// <summary>
    /// A response timestamp.
    /// </summary>
    [XmlElement("timestamp")]
    public DateTime? Timestamp { get; set; }

    /// <summary>
    /// Procedure digest.
    /// </summary>
    [XmlElement("procedures")]
    public ProcedureDigest[] Procedures { get; set; }

    /// <summary>
    /// A response type.
    /// </summary>
    [XmlElement("responseType")]
    public ResponseType ResponseType { get; set; }

    /// <summary>
    /// A list of launch commands.
    /// </summary>
    [XmlElement("launches")]
    public LaunchCommand[] Launches { get; set; }
  }

  /// <summary>
  /// An action request type.
  /// </summary>
  [Serializable]
  public class Request
  {
    /// <summary>
    /// A state index.
    /// </summary>
    [XmlElement("index")]
    public int Index { get; set; }

    /// <summary>
    /// A procedure id.
    /// </summary>
    [XmlElement("id")]
    public int Id { get; set; }

    /// <summary>
    /// An import instance.
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public object InValue { get; set; }

    /// <summary>
    /// A <see cref="Global"/> instance.
    /// </summary>
    [XmlElement("global")]
    public Global Global { get; set; }

    /// <summary>
    /// An event objects.
    /// </summary>
    [XmlElement("events")]
    public EventObject[] Events { get; set; }

    /// <summary>
    /// Focused control.
    /// </summary>
    [XmlElement("focused")]
    public string Focused { get; set; }

    /// <summary>
    /// A list of states of controls, used for xml serialization.
    /// </summary>
    [XmlElement("controls")]
    public StateData[] Controls { get; set; }
  }

  /// <summary>
  /// An action response type.
  /// </summary>
  public class Response : ApplicationResponse
  {
    /// <summary>
    /// An import instance.
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public object InValue { get; set; }

    /// <summary>
    /// An import instance.
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public object OutValue { get; set; }

    /// <summary>
    /// A <see cref="Global"/> instance.
    /// </summary>
    [XmlElement("global")]
    public Global Global { get; set; }

    /// <summary>
    /// Current message box, if any.
    /// </summary>
    [XmlElement("messageBox")]
    public MessageBox MessageBox { get; set; }
  }

  /// <summary>
  /// A server request or response type.
  /// </summary>
  [Serializable]
  public class Data
  {
    /// <summary>
    /// A Global instance.
    /// </summary>
    [XmlElement("global")]
    public Global Global { get; set; }

    /// <summary>
    /// An data instance.
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public object Value { get; set; }
  }

  /// <summary>
  /// A server request type.
  /// </summary>
  public class ServerRequest<I> : Data
  {
    /// <summary>
    /// Import instance.
    /// </summary>
    [XmlElement("in")]
    public I In { get => (I)Value; set => Value = value; }
  }

  /// <summary>
  /// A server response type.
  /// </summary>
  public class ServerResponse<E> : Data
  {
    /// <summary>
    /// Export instance.
    /// </summary>
    [XmlElement("out")]
    public E Out { get => (E)Value; set => Value = value; }
  }

  /// <summary>
  /// A client request type.
  /// </summary>
  public class Request<I> : Request
  {
    /// <summary>
    /// Import instance.
    /// </summary>
    [XmlElement("in")]
    public I In { get => (I)InValue; set => InValue = value; }
  }

  /// <summary>
  /// A client response type.
  /// </summary>
  public class Response<I, E> : Response
  {
    /// <summary>
    /// Import instance.
    /// </summary>
    [XmlElement("in")]
    public I In { get => (I)InValue; set => InValue = value; }

    /// <summary>
    /// Export instance.
    /// </summary>
    [XmlElement("out")]
    public E Out { get => (E)OutValue; set => OutValue = value; }
  }

  /// <summary>
  /// Creates a <see cref="Client"/> instance.
  /// </summary>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/> instance.
  /// </param>
  /// <param name="environment">
  /// An <see cref="IEnvironment"/> instance.
  /// </param>
  /// <param name="sessions">A <see cref="ISessions"/> instance.</param>
  /// <param name="principal">A <see cref="IPrincipal"/> instance.</param>
  /// <param name="procedure">A procedure name.</param>
  private Client(
    IServiceProvider serviceProvider,
    IEnvironment environment,
    ISessions sessions,
    IPrincipal principal)
  {
    this.serviceProvider = serviceProvider;
    this.environment = environment ??
      throw new ArgumentNullException(nameof(environment));
    this.sessions = sessions ??
      throw new ArgumentNullException(nameof(sessions));
    this.principal = principal;
  }

  /// <summary>
  /// Acquires sessions lock.
  /// </summary>
  /// <param name="write">A lock intent.</param>
  /// <returns>A lock resource.</returns>
  private IDisposable Lock(bool write)
  {
    return environment.LockManager.Acquire(sessions.Id, write);
  }

  /// <summary>
  /// <para>
  /// Starts the application from the specified procedure step name or 
  /// transaction code with optional clear screen input parameters.
  /// </para>
  /// <para>
  /// <b>Note:</b> when procedure step name is defined then it will be executed,
  /// otherwise the transaction code from command line arguments will be taken.
  /// When neither procedure nor command line arguments are specified the 
  /// exception will be thrown.
  /// </para>
  /// </summary>
  /// <param name="request">
  /// A <see cref="StartRequest"/> instance that contains either
  /// a procedure step name to execute or a transaction code with optional
  /// clear screen input parameters and displayFirst flag.
  /// </param>
  /// <returns>
  /// A <see cref="ResponseType.Navigate"/> <see cref="ApplicationResponse"/>
  /// instance.
  /// </returns>
  private ApplicationResponse Start(StartRequest request)
  {
    if (request == null)
    {
      request = new();
    }

    index = request.Index;

    var success = true;

    using var sync = Lock(true);
    var state = GetSession(false, false, request.Restart != false);

    dialog = environment.CreateDialog(serviceProvider, state, principal);

    var prev = state.GetProcedureThatCanAcceptRequest();
    int resourceID = prev?.ResourceID ?? 0;
    var value = request.Procedure;

    if (IsEmpty(value))
    {
      value = request.CommandLine;

      if (!IsEmpty(value))
      {
        procedure = dialog.CreateProcedureFromTrancode(value, resourceID);

        if (request.DisplayFirst != null)
        {
          procedure.ExecutionState = request.DisplayFirst == true ?
            ExecutionState.WaitForUserInputDisplayFirst :
            ExecutionState.Initial;
        }
      }
    }

    if (procedure == null)
    {
      procedure = dialog.CreateProcedure(value, resourceID);
    }

    var profiler = state.Profiler;
    using var action = profiler.Scope("request", procedure.Name, "start");
    using var input = action.Scope("input", null);
    using var output = action.Scope("output", null);

    state.Profiler = action;

    try
    {
      input.Value(request);
      state.Procedures.Add(procedure);

      var global = procedure.Global;
      var requestGlobal = request.Global;

      if (requestGlobal != null)
      {
        if (!IsEmpty(requestGlobal.Command))
        {
          global.Command = requestGlobal.Command;
        }

        if (!IsEmpty(requestGlobal.Exitstate))
        {
          global.SetExitState(
            requestGlobal.Exitstate,
            dialog.Resources,
            procedure.ResourceID);
        }

        if (!IsEmpty(requestGlobal.CurrentDialect))
        {
          global.CurrentDialect = requestGlobal.CurrentDialect;
        }
      }

      dialog.HandleEvents(procedure, true);
      InitProcedure(0);
      success = true;

      return output.Value(Populate(new ApplicationResponse()));
    }
    catch(Exception e)
    {
      action.Value("error", null, e);

      throw;
    }
    finally
    {
      state.Profiler = profiler;

      if (success)
      {
        SaveSession(index, state);
      }
    }
  }

  /// <summary>
  /// Gets the current state.
  /// </summary>
  /// <param name="index">A state index.</param>
  /// <returns>
  /// A <see cref="ResponseType.Navigate"/> <see cref="ApplicationResponse"/>
  /// instance.
  /// </returns>
  private ApplicationResponse Current(int index)
  {
    this.index = index;

    using var sync = Lock(false);
    var state = GetSession(true, false);
    var profiler = state.Profiler;
    using var action = profiler.Scope("request", "current");

    return action.Value("output", null, Populate(new ApplicationResponse()));
  }

  /// <summary>
  /// Finishes the application and route to the end page.
  /// </summary>
  /// <param name="index">A state index.</param>
  /// <returns>
  /// A <see cref="ResponseType.Navigate"/> <see cref="ApplicationResponse"/>
  /// instance.
  /// </returns>
  private ApplicationResponse Logout(int index)
  {
    this.index = index;

    using var sync = Lock(true);
    var state = GetSession(true, false);
    var profiler = state.Profiler;
    using var action = profiler.Scope("request", "logout");

    state?.Dispose();
    SaveSession(index, null);

    return action.Value("output", null, Populate(new ApplicationResponse()));
  }

  /// <summary>
  /// Forks the state of the application.
  /// </summary>
  /// <param name="index">A state index.</param>
  /// <returns>
  /// A <see cref="ResponseType.Navigate"/> <see cref="ApplicationResponse"/>
  /// instance.
  /// </returns>
  private ApplicationResponse Fork(int index)
  {
    this.index = index;

    using var sync = Lock(true);
    var state = GetSession(true, true);
    var profiler = state.Profiler;
    using var action = profiler.Scope("request", "fork");
        
    dialog = environment.CreateDialog(serviceProvider, state, principal);
    SaveSession(index, state);

    return action.Value("output", null, Populate(new ApplicationResponse()));
  }

  /// <summary>
  /// Changes the current dialect.
  /// </summary>
  /// <param name="request">
  /// A <see cref="ChangeDialectRequest"/> instance.
  /// </param>
  /// <returns>
  /// A <see cref="ResponseType.Navigate"/> <see cref="ApplicationResponse"/>
  /// instance.
  /// </returns>
  private ApplicationResponse ChangeDialect(ChangeDialectRequest request)
  {
    index = request.Index;

    using var sync = Lock(true);
    var state = GetSession(true, false);
    var profiler = state.Profiler;
    using var action = profiler.Scope("request", "changeDialect");
    var requestGlobal = request.Global;

    if (!IsEmpty(requestGlobal?.CurrentDialect))
    {
      var dialect = requestGlobal.CurrentDialect;

      GetDialog();

      if (procedure != null)
      {
        procedure.Global.CurrentDialect = dialect;
      }
    }

    SaveSession(index, state);

    return action.Value("output", null, Populate(new ApplicationResponse()));
  }

  /// <summary>
  /// Gets the current state of the application.
  /// </summary>
  /// <param name="index">A state index.</param>
  /// <param name="id">A procedure id.</param>
  /// <param name="response">A <see cref="Response"/> instance.</param>
  private void Get(int index, int id, Response response)
  {
    this.index = index;
    this.id = id;

    using var sync = Lock(true);
    var state = GetSession(true, false);

    GetDialog();

    var profiler = state.Profiler;
    using var action = profiler.Scope("request", procedure.Name, "get");

    action.Value("output", null, Populate(response));
  }

  /// <summary>
  /// Executes a client command or event.
  /// </summary>
  /// <param name="request">A <see cref="Request"/> instance.</param>
  /// <param name="response">A <see cref="Response"/> instance.</param>
  private void Execute(Request request, Response response)
  {
    index = request.Index;
    id = request.Id;

    var resources = environment.Resources;
    using var sync = Lock(true);
    var state = GetSession(true, false);
    var dialog = GetDialog();
    var profiler = state.Profiler;

    using var action = profiler.Scope("request", procedure.Name, "action");
    using var input = action.Scope("input", null);
    using var output = action.Scope("output", null);
    var success = false;

    state.Profiler = action;

    try
    {
      input.Value(request);

      var pending = procedure.IsPending(state);

      if (procedure.IsComplete() ||
        (procedure.Id != id) ||
        !(pending ||
          state.CanAcceptRequest(procedure) ||
          // This is to allow activate event on locked window 
          // to reorder procedures. 
          ((request.Events?.Length == 1) &&
          (string.Compare(
            ActivatedEvent.EventType, request.Events[0].Type,
            true) == 0))))
      {
        InitProcedure(0);

        goto AfterProcess;
      }

      var queue = state.Events;

      if (request.Events != null)
      {
        foreach(var eventObject in request.Events)
        {
          var anEvent = Events.Event.Create(procedure, eventObject);

          anEvent.Client = true;
          anEvent.Prepare(dialog, queue, 0);
        }
      }

      var activeWindow = null as UIWindow;

      procedures = new(state.Procedures);

      if (!pending)
      {
        var global = procedure.Global;
        var requestGlobal = request.Global;

        if (requestGlobal != null)
        {
          global.ScrollAmt = requestGlobal.ScrollAmt;
          global.NextTran = requestGlobal.NextTran;
          global.ClientPassword = requestGlobal.ClientPassword;
          global.ClientUserId = requestGlobal.ClientUserId;

          if (!IsEmpty(requestGlobal.Command))
          {
            global.Command = requestGlobal.Command;
          }

          if (!IsEmpty(requestGlobal.Exitstate))
          {
            global.SetExitState(
              requestGlobal.Exitstate,
              resources,
              procedure.ResourceID);
          }
          else if (requestGlobal.ExitStateId != 0)
          {
            global.SetExitState(
              requestGlobal.ExitStateId,
              resources,
              procedure.ResourceID);
          }
          // No more cases.

          if (!IsEmpty(requestGlobal.CurrentDialect))
          {
            global.CurrentDialect = requestGlobal.CurrentDialect;
          }
        }

        var import = request.InValue;

        if (import != null)
        {
          procedure.Import = import;
        }

        activeWindow = procedure.ActiveWindow;

        if (activeWindow != null)
        {
          if (request.Focused != null)
          {
            activeWindow.Focused = request.Focused;
          }

          procedure.GetScreenFields().Focused = null;

          var controls = request.Controls;

          if (controls != null)
          {
            StateData.SetState(
              activeWindow.ControlsByName,
              new() { Map = controls });
          }
        }

        // Reset dirty state.
        foreach(var other in state.Procedures)
        {
          if (other.Type == ProcedureType.Window)
          {
            foreach(var window in other.Windows)
            {
              window.ResetDirty();
            }
          }

          other.Dirty = false;
        }
      }

      dialog.HandleEvents(procedure, false);

      if (activeWindow != null)
      {
        foreach(var windowControl in activeWindow.Controls)
        {
          windowControl.DisabledState = false;
        }
      }

      InitProcedure(0);

AfterProcess:
      output.Value(Populate(response));
      success = true;
    }
    catch(Exception e)
    {
      action.Value("error", null, e);

      throw;
    }
    finally
    {
      state.Profiler = profiler;

      if (success)
      {
        SaveSession(index, state);
      }
    }
  }

  /// <summary>
  /// Gets or creates a new <see cref="ISessionManager"/>.
  /// </summary>
  /// <param name="required">true to expect session exists.</param>
  /// <param name="copy">true to copy existing sessions.</param>
  /// <param name="reset">true to reset session.</param>
  /// <returns>A <see cref="ISessionManager"/> instance.</returns>
  private ISessionManager GetSession(
    bool required, 
    bool copy, 
    bool reset = false)
  {
    if (state == null)
    {
      if ((index <= 0) && (required || copy))
      {
        index = 1;
      }

      if (index > 0)
      {
        state = sessions[index - 1] ??
          throw new SecurityException("NO-APP: No application is running.");

        if (!copy && reset)
        {
          state.Dispose();
          state = environment.CreateSession(serviceProvider);
        }
      }
      else
      {
        copy = true;
      }

      if (copy)
      {
        state = environment.CreateSession(serviceProvider, null, state);
        index = sessions.Create() + 1;
      }
    }

    return state;
  }

  /// <summary>
  /// Saves a session state.
  /// </summary>
  /// <param name="index">A state index.</param>
  /// <param name="state">A <see cref="ISessionManager"/> instance.</param>
  private void SaveSession(int index, ISessionManager state)
  {
    try
    {
      sessions[index - 1] = state;
    }
    catch
    {
      throw new SecurityException("NO-APP: No application is running.");
    }
  }

  /// <summary>
  /// Gets a dialog instance.
  /// </summary>
  /// <returns>A <see cref="IDialogManager"/> instance.</returns>
  private IDialogManager GetDialog()
  {
    if (dialog == null)
    {
      dialog = environment.
        CreateDialog(serviceProvider, GetSession(true, false), principal);

      InitProcedure(id);
    }

    return dialog;
  }

  /// <summary>
  /// Initializes procedure.
  /// </summary>
  /// <param name="id">A procedure id.</param>
  private void InitProcedure(int id)
  {
    var state = GetSession(true, false);

    procedure = state.GetProcedureByID(id) ??
      state.GetProcedureThatCanAcceptRequest();
  }

  /// <summary>
  /// Gets an array of <see cref="UIWindow"/> for a 
  /// <see cref="ProcedureDigest"/>, or null if non window procedure.
  /// </summary>
  /// <param name="procedure">A <see cref="IProcedure"/> instance.</param>
  /// <returns>An array of <see cref="UIWindow"/>.</returns>
  private static UIWindow[] GetDigestWindows(IProcedure procedure)
  {
    if ((procedure == null) || (procedure.Type != ProcedureType.Window))
    {
      return null;
    }

    var windows = procedure.Windows.Select(GetDigestWindow).ToArray();

    return windows.Length == 0 ? null : windows;
  }

  /// <summary>
  /// Gets window or its digest depending on dirty property.   
  /// </summary>
  /// <param name="window">A <see cref="UIWindow"/> instance.</param>
  /// <returns>A <see cref="UIWindow"/> instance.</returns>
  private static UIWindow GetDigestWindow(UIWindow window)
  {
    return window.Dirty ? 
      window :
      new()
      {
        Name = window.Name,
        Caption = window.Caption,
        WindowState = window.WindowState,
        Minimized = window.Minimized,
        Maximized = window.Maximized,
        Modal = window.Modal,
        Digest = true,
        LeftValue = window.LeftValue,
        TopValue = window.TopValue,
        WidthValue = window.WidthValue,
        HeightValue = window.HeightValue,
        Focused = window.Focused,
        Errmsg = window.Errmsg,
        MessageType = window.MessageType,
        Resizable = window.Resizable,
        Position = window.Position
      };
  }

  /// <summary>
  /// Builds an application response.
  /// </summary>
  /// <param name="response">A response to populate.</param>
  /// <returns>An <see cref="ApplicationResponse"/> instance.</returns>
  private ApplicationResponse Populate(ApplicationResponse response)
  {
    var state = GetSession(true, false);
    var dialog = GetDialog();

    response.Mode = environment.Mode;
    response.Index = index;

    ProcedureDigest procedureDigest;
    var procedureDigests = new List<ProcedureDigest>();

    foreach(var procedure in state.Procedures)
    {
      var type = procedure.Type;

      if (type == ProcedureType.Window)
      {
        var hasWindows = false;

        foreach(var window in procedure.Windows)
        {
          window.SetDirty();
          hasWindows = true;
        }

        if (!hasWindows)
        {
          continue;
        }
      }
      else if (type == ProcedureType.Online)
      {
        if (procedure.CalledCount != 0)
        {
          continue;
        }
      }
      else
      {
        continue;
      }

      procedure.Dirty = true;

      procedureDigest = new()
      {
        Id = procedure.Id,
        Type = procedure.Type,
        Name = procedure.Name,
        Scope = procedure.Scope,
        Changed = true
      };

      if (!state.CanAcceptRequest(procedure))
      {
        procedureDigest.Locked = true;
      }

      procedureDigests.Add(procedureDigest);
    }

    if (procedureDigests.Count == 0)
    {
      response.ResponseType = ResponseType.End;
    }
    else
    {
      response.ResponseType = ResponseType.Navigate;
      response.Procedures = procedureDigests.ToArray();
    }

    var launches = dialog.LaunchCommands;

    if (launches.Count > 0)
    {
      response.Launches = launches.ToArray();
    }

    return response;
  }

  /// <summary>
  /// Builds the response.
  /// </summary>
  /// <param name="response">A response instance.</param>
  /// <returns>A response instance.</returns>
  private Response Populate(Response response)
  {
    var resources = environment.Resources;
    var state = GetSession(true, false);
    var dialog = GetDialog();
    var navigate = (procedure == null) || (procedure.Id != id);
    var messageBox = state.CurrentMessageBox;
    var procedureDigests = new List<ProcedureDigest>();
    ProcedureDigest procedureDigest = null;

    foreach(var other in state.Procedures)
    {
      var type = other.Type;

      if (type == ProcedureType.Window)
      {
        if (other.Windows.Count == 0)
        {
          continue;
        }
      }
      else if (type == ProcedureType.Online)
      {
        if (other.CalledCount != 0)
        {
          continue;
        }
      }
      else
      {
        continue;
      }

      var otherDigest = new ProcedureDigest
      {
        Id = other.Id,
        Type = other.Type,
        Name = other.Name,
        Scope = other.Scope,
        Changed = other.Dirty ||
          (procedure == other) ||
            ((procedures != null) && !procedures.Contains(other)) ||
          ((messageBox != null) && (messageBox.Procedure == other)),
      };

      if (!state.CanAcceptRequest(other))
      {
        otherDigest.Locked = true;
      }

      procedureDigests.Add(otherDigest);

      if (procedure == other)
      {
        procedureDigest = otherDigest;
      }
      else
      {
        navigate |= (procedures != null) && otherDigest.Changed;
      }
    }

    if (procedureDigest != null)
    {
      if (!navigate)
      {
        procedureDigest.Windows = GetDigestWindows(procedure)?.
          Select(StateData.GetState).
          ToArray();

        var autoFlowCommands = procedure.GetAutoFlowCommands(resources);

        if (autoFlowCommands.Count > 0)
        {
          var commands = new CommandView[autoFlowCommands.Count];
          var i = 0;

          foreach(var command in autoFlowCommands)
          {
            var commandView = new CommandView
            {
              Name = command,
              Autoflow = true
            };

            commands[i++] = commandView;
          }

          procedureDigest.Commands = commands;
        }
      }
    }

    if (procedureDigests.Count == 0)
    {
      response.ResponseType = ResponseType.End;
    }
    else
    {
      response.Procedures = procedureDigests.ToArray();

      if (navigate)
      {
        response.ResponseType = ResponseType.Navigate;
      }
      else
      {
        response.Id = id;
        response.Timestamp = environment.Clock(state);

        if ((messageBox != null) && (messageBox.Procedure == procedure))
        {
          response.ResponseType = ResponseType.MessageBox;
          response.MessageBox = messageBox;
        }

        response.Global = procedure.Global;
        response.InValue = procedure.Import;
        response.OutValue = procedure.Export;
      }
    }

    response.Index = index;

    var launches = dialog.LaunchCommands;

    if (launches.Count > 0)
    {
      response.Launches = launches.ToArray();
    }

    return response;
  }

  /// <summary>
  /// An <see cref="IServiceProvider"/> instance.
  /// </summary>
  private readonly IServiceProvider serviceProvider;

  /// <summary>
  /// An <see cref="IEnvironment"/> instance.
  /// </summary>
  private readonly IEnvironment environment;

  /// <summary>
  /// A <see cref="ISessions"/> instance.
  /// </summary>
  private readonly ISessions sessions;

  /// <summary>
  /// A <see cref="IPrincipal"/> instance.
  /// </summary>
  private readonly IPrincipal principal;

  /// <summary>
  /// A state index.
  /// </summary>
  private int index;

  /// <summary>
  /// A procedure ID.
  /// </summary>
  private int id;

  /// <summary>
  /// A <see cref="ISessionManager"/> instance.
  /// </summary>
  private ISessionManager state;

  /// <summary>
  /// An <see cref="IDialogManager"/> instance.
  /// </summary>
  private IDialogManager dialog;

  /// <summary>
  /// An <see cref="IProcedure"/> instance.
  /// </summary>
  private IProcedure procedure;

  /// <summary>
  /// Set of procedures before run.
  /// </summary>
  private HashSet<IProcedure> procedures;
}
