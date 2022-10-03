using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Security.Principal;

using Bphx.Cool.UI;
using Bphx.Cool.Data;
using System.Diagnostics;

namespace Bphx.Cool.Impl;

/// <summary>
/// An <see cref="IEnvironment"/> implementation.
/// </summary>
public class Environment: IEnvironment
{
  /// <summary>
  /// Creates an <see cref="Environment"/> instance.
  /// </summary>
  public Environment()
  {
    SessionFactory = (serviceProvicer, options, original) => 
      original?.Copy(this) ?? new SessionManager(options);

    DialogFactory = (serviceProvider, session, principal) =>
      new DialogManager
      {
        ServiceProvider = serviceProvider,
        Environment = this,
        SessionManager = session,
        Principal = principal
      };
  }

  /// <summary>
  /// Global service provider.
  /// </summary>
  public IServiceProvider ServiceProvider { get; set; }

  /// <summary>
  /// A <see cref="IResources"/> instance.
  /// </summary>
  public IResources Resources { get; set; }

  /// <summary>
  /// An environment mode.
  /// </summary>
  public string Mode { get; set; }

  /// <summary>
  /// Gets a number attempts to retry transaction before give up and fail.
  /// </summary>
  public virtual int TransactionRetryLimit { get; set; } = 1;

  /// <summary>
  /// Gets a "Local System Id".
  /// </summary>
  public virtual string LocalSystemId { get; set; }

  /// <summary>
  /// Encoding used to convert bytes to and from strings. 
  /// </summary>
  public virtual Encoding Encoding { get; set; } = Encoding.Default;

  /// <summary>
  /// Optional string collator.
  /// </summary>
  public virtual IComparer<string> Collator { get; set; }

  /// <summary>
  /// Optional start handler.
  /// </summary>
  public virtual Func<IContext, bool> OnStart { get; set; }

  /// <summary>
  /// Optional launch command handler.
  /// </summary>
  public virtual Func<IContext, LaunchCommand, bool> OnLaunch { get; set; }

  /// <summary>
  /// Optional transaction error handler.
  /// </summary>
  public virtual Action<IContext, Exception> OnTransactionError { get; set; }

  /// <summary>
  /// Optional error log handler.
  /// </summary>
  public Func<Exception, bool> OnLogError { get; set; }

  /// <summary>
  /// Optional <see cref="Global"/> tracer.
  /// </summary>
  public Action<string, Global, object> GlobalTracer { get; set; }

  /// <summary>
  /// Gets or sets a transaction factory
  /// </summary>
  public virtual Func<IContext, ITransaction> TransactionFactory { get; set; }

  /// <summary>
  /// Gets optional <see cref="IContext"/> factory.
  /// </summary>
  public virtual Func<IProcedure, IContext> ContextFactory { get; }

  /// <summary>
  /// Gets or sets a error code resolver.
  /// </summary>
  public virtual IErrorCodeResolver ErrorCodeResolver { get; set; }

  /// <summary>
  /// Gets or sets a error code resolver.
  /// </summary>
  public virtual DataConverter DataConverter { get; set; }

  /// <summary>
  /// A service to load <see cref="UIWindow"/>s.
  /// </summary>
  public IUIWindowLoader WindowLoader { get; set; }

  /// <summary>
  /// A serializer service.
  /// </summary>
  public ISerializer Serializer { get; set; }

  /// <summary>
  /// A lock manager service.
  /// </summary>
  public ILockManager LockManager { get; set; }

  /// <summary>
  /// Optional function to resolve user id by principal.
  /// </summary>
  public Func<IDialogManager, IPrincipal, string> UserIdProvider { get; set; }

  /// <summary>
  /// A procedure authorizer.
  /// </summary>
  public Func<IDialogManager, IProcedure, IPrincipal, bool> Authorizer { get; set; }

  /// <summary>
  /// A clock instance.
  /// </summary>
  public Func<ISessionManager, DateTime> Clock { get; set; } = 
    session =>
    {
      var now = DateTime.Now;

      return now.AddTicks(-(now.Ticks % (TimeSpan.TicksPerMillisecond / 1000)));
    };

  /// <summary>
  /// An <see cref="IIO"/> factory.
  /// </summary>
  public Func<IContext, IIO> IOFactory { get; set; }

  /// <summary>
  /// Named object factory.
  /// </summary>
  public Func<string, IContext, UIObject> ObjectFactory { get; set; }

  /// <summary>
  /// An OLE methods handler.
  /// </summary>
  public Func<string, IContext, object[], object> Methods { get; set; }

  /// <summary>
  /// Optional procedure scope resolver.
  /// </summary>
  public Func<ActionInfo, IEnvironment, string> ProcedureScopeResolver
  {
    get;
    set;
  }

  /// <summary>
  /// A session factory.
  /// </summary>
  public Func<
    IServiceProvider, 
    Dictionary<string, object>, 
    ISessionManager, 
    ISessionManager> SessionFactory { get; set; }

  /// <summary>
  /// A <see cref="IDialogManager"/> factory.
  /// </summary>
  public Func<
    IServiceProvider, 
    ISessionManager, 
    IPrincipal, 
    IDialogManager> DialogFactory { get; set; }

  /// <summary>
  /// Creates a <see cref="ISessionManager"/> instance.
  /// </summary>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/> instance.
  /// </param>
  /// <param name="options">
  /// Options to pass into a session.
  /// </param>
  /// <param name="original">
  /// Optional <see cref="ISessionManager"/> to copy.
  /// </param>
  /// <returns>New <see cref="ISessionManager"/> instance.</returns>
  public ISessionManager CreateSession(
    IServiceProvider serviceProvider,
    Dictionary<string, object> options = null,
    ISessionManager original = null) => 
    SessionFactory(serviceProvider, options, original);

  /// <summary>
  /// Creates a <see cref="IDialogManager"/> instance.
  /// </summary>
  /// <param name="serviceProvider">
  /// A <see cref="IServiceProvider"/> instance.
  /// </param>
  /// <param name="session">A <see cref="ISessionManager"/> instance.</param>
  /// <param name="principal">
  /// Optional <see cref="IPrincipal"/> instance.
  /// </param>
  /// <returns>New <see cref="IDialogManager"/> instance.</returns>
  public IDialogManager CreateDialog(
    IServiceProvider serviceProvider,
    ISessionManager session,
    IPrincipal principal) => 
    DialogFactory(serviceProvider, session, principal);

  /// <summary>
  /// Creates <see cref="ITransaction"/> instance.
  /// </summary>
  /// <param name="context">A <see cref="IContext"/> instance.</param>
  /// <returns>New <see cref="ITransaction"/> instance.</returns>
  public ITransaction CreateTransaction(IContext context) => 
    TransactionFactory(context);

  /// <summary>
  /// Creates <see cref="IContext"/> instance.
  /// </summary>
  /// <param name="procedure">A <see cref="IProcedure"/> instance.</param>
  /// <returns>New <see cref="IContext"/> instance.</returns>
  public IContext CreateContext(IProcedure procedure) =>
    ContextFactory?.Invoke(procedure) ?? new Context(procedure);

  /// <summary>
  /// Creates new <see cref="IIO"/>, if available, to work 
  /// in the specified <see cref="IContext"/>.
  /// </summary>
  /// <param name="context">A <see cref="IContext"/> instance.</param>
  /// <returns>New <see cref="IIO"/> instance or null.</returns>
  public IIO CreateIO(IContext context) => IOFactory?.Invoke(context);

  /// <summary>
  /// Creates <see cref="UIObject"/> by object type name.
  /// </summary>
  /// <param name="type">An object type name.</param>
  /// <param name="context">A <see cref="IContext"/> instance.</param>
  /// <returns>A <see cref="UIObject"/> instance, if available.</returns>
  public UIObject CreateObject(string type, IContext context) =>
    ObjectFactory?.Invoke(type, context);

  /// <summary>
  /// Invokes an OLE method.
  /// </summary>
  /// <param name="name">A method name.</param>
  /// <param name="context">A <see cref="IContext"/> instance.</param>
  /// <param name="parameters">Optional method parameters.</param>
  /// <returns>The method's return value.</returns>
  public object Invoke(
    string name, 
    IContext context, 
    params object[] parameters)
  {
    if (Methods == null)
    {
      Trace.Write(
        $@"Call {name} is not supported with parameters: [{
          string.Join(", ", parameters)}].",
        "DEBUG");

      throw new NotSupportedException($"Call method {name} is not supported.");
    }

    return Methods(name, context, parameters);
  }

  /// <summary>
  /// Retrieves a new  unique identifier for asynchronous result.
  /// </summary>
  public int NewAsyncResultID()
  {
    while(true)
    {
      var value = Interlocked.Increment(ref nextAsyncResultID);

      if(value != 0)
      {
        return value;
      }
    }
  }

  /// <summary>
  /// A next asynchronous result ID value;
  /// </summary>
  private int nextAsyncResultID;
}
