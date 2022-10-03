using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;

using Bphx.Cool.Data;
using Bphx.Cool.UI;

namespace Bphx.Cool;

/// <summary>
/// <para>An interface providing access to an application environment.</para>
/// <para>
/// <b>Note:</b> the implementation of Environment must be safe for
/// multithreading. This means that all methods of this interface of
/// the same instance can be called from multiple threads at the same time.
/// </para>
public interface IEnvironment
{
  /// <summary>
  /// Global service provider.
  /// </summary>
  IServiceProvider ServiceProvider { get; }

  /// <summary>
  /// An environment mode.
  /// </summary>
  string Mode { get; }

  /// <summary>
  /// A <see cref="IResources"/> instance.
  /// </summary>
  IResources Resources { get; }

  /// <summary>
  /// Gets a "Local System Id".
  /// </summary>
  string LocalSystemId { get; }

  /// <summary>
  /// Encoding used to convert bytes to and from strings. 
  /// </summary>
  Encoding Encoding { get; }

  /// <summary>
  /// Optional string collator.
  /// </summary>
  IComparer<string> Collator { get; }

  /// <summary>
  /// Gets or sets a error code resolver.
  /// </summary>
  IErrorCodeResolver ErrorCodeResolver { get; }

  /// <summary>
  /// Gets or sets a error code resolver.
  /// </summary>
  DataConverter DataConverter { get; }

  /// <summary>
  /// A service to load <see cref="UIWindow"/>s.
  /// </summary>
  IUIWindowLoader WindowLoader { get; }

  /// <summary>
  /// A serializer service.
  /// </summary>
  ISerializer Serializer { get; }

  /// <summary>
  /// A lock manager service.
  /// </summary>
  ILockManager LockManager { get; }

  /// <summary>
  /// Optional function to resolve user id by principal.
  /// </summary>
  Func<IDialogManager, IPrincipal, string> UserIdProvider { get; }

  /// <summary>
  /// A procedure authorizer.
  /// </summary>
  Func<IDialogManager, IProcedure, IPrincipal, bool> Authorizer { get; }

  /// <summary>
  /// A clock instance.
  /// </summary>
  Func<ISessionManager, DateTime> Clock { get; }

  /// <summary>
  /// Optional procedure scope resolver.
  /// </summary>
  Func<ActionInfo, IEnvironment, string> ProcedureScopeResolver { get; }

  /// <summary>
  /// Optional start handler.
  /// </summary>
  Func<IContext, bool> OnStart { get; }

  /// <summary>
  /// Optional launch command handler.
  /// </summary>
  Func<IContext, LaunchCommand, bool> OnLaunch { get; }

  /// <summary>
  /// Optional transaction error handler.
  /// </summary>
  Action<IContext, Exception> OnTransactionError { get; }

  /// <summary>
  /// Optional error log handler.
  /// </summary>
  Func<Exception, bool> OnLogError { get; }

  /// <summary>
  /// Optional <see cref="Global"/> tracer.
  /// </summary>
  Action<string, Global, object> GlobalTracer { get; }

  /// <summary>
  /// Gets and sets a number attempts to retry transaction 
  /// before give up and fail.
  /// </summary>
  int TransactionRetryLimit { get; }

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
  ISessionManager CreateSession(
    IServiceProvider serviceProvider,
    Dictionary<string, object> options = null,
    ISessionManager original = null);

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
  IDialogManager CreateDialog(
    IServiceProvider serviceProvider,
    ISessionManager session,
    IPrincipal principal);

  /// <summary>
  /// Creates <see cref="ITransaction"/> instance.
  /// </summary>
  /// <param name="context">A <see cref="IContext"/> instance.</param>
  /// <returns>New <see cref="ITransaction"/> instance.</returns>
  ITransaction CreateTransaction(IContext context);

  /// <summary>
  /// Creates <see cref="IContext"/> instance.
  /// </summary>
  /// <param name="procedure">A <see cref="IProcedure"/> instance.</param>
  /// <returns>New <see cref="IContext"/> instance.</returns>
  IContext CreateContext(IProcedure procedure);

  /// <summary>
  /// Creates new <see cref="IIO"/>, if available, to work 
  /// in the specified <see cref="IContext"/>.
  /// </summary>
  /// <param name="context">A <see cref="IContext"/> instance.</param>
  /// <returns>New <see cref="IIO"/> instance or null.</returns>
  IIO CreateIO(IContext context);

  /// <summary>
  /// Creates <see cref="UIObject"/> by object type name.
  /// </summary>
  /// <param name="type">An object type name.</param>
  /// <param name="context">A <see cref="IContext"/> instance.</param>
  /// <returns>A <see cref="UIObject"/> instance, if available.</returns>
  UIObject CreateObject(string type, IContext context);

  /// <summary>
  /// Invokes an OLE method.
  /// </summary>
  /// <param name="name">A method name.</param>
  /// <param name="context">A <see cref="IContext"/> instance.</param>
  /// <param name="parameters">Optional method parameters.</param>
  /// <returns>The method's return value.</returns>
  object Invoke(string name, IContext context, params object[] parameters);

  /// <summary>
  /// Retrieves a new  unique identifier for asynchronous result.
  /// Return an asynchronous result ID as an integer number.
  /// </summary>
  /// <returns></returns>
  int NewAsyncResultID();
}

/// <summary>
/// Environment extensions.
/// </summary>
public static class EnvironmentExtensions
{
  /// <summary>
  /// Logs an error.
  /// </summary>
  /// <param name="e">An exception to log.</param>
  public static void LogError(Exception e) =>
    Trace.TraceError(e.ToString(), "DEBUG");

  /// <summary>
  /// <para>Logs an error.</para>
  /// <para>
  /// If environment is <code>null</code>, or 
  /// <see cref="IEnvironment.OnLogError"/> returns <code>true</code>, 
  /// then default <see cref="LogError(Exception)"/> is called.
  /// </para>
  /// </summary>
  /// <param name="environment">
  /// An <see cref="IEnvironment"/> instance.
  /// </param>
  /// <param name="e">An exception to log.</param>
  public static void LogError(this IEnvironment environment, Exception e)
  {
    if (environment?.OnLogError?.Invoke(e) == true)
    {
      LogError(e);
    }
  }
}
