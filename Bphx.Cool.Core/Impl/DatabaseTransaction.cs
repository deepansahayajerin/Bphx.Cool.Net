using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace Bphx.Cool.Impl;

/// <summary>
/// An ADO.NET transaction manager implementation.
/// </summary>
public class DatabaseTransaction : ITransaction, IDbConnectionProvider
{
  /// <summary>
  /// Creates an <see cref="DatabaseTransaction"/> instance.
  /// </summary>
  /// <param name="factory">A database provider factory.</param>
  /// <param name="connectionString">A database connection string.</param>
  /// <param name="isolationLevel">A transaction isolation level</param>
  public DatabaseTransaction(
    DbProviderFactory factory,
    string connectionString,
    IsolationLevel isolationLevel = IsolationLevel.Unspecified)
  {
    this.factory = factory;
    this.connectionString = connectionString;
    this.isolationLevel = isolationLevel;
    this.ownedConnection = true;
  }

  /// <summary>
  /// Creates an <see cref="DatabaseTransaction"/> instance.
  /// </summary>
  /// <param name="connection">
  /// A <see cref="IDbConnection"/> instance.
  /// </param>
  /// <param name="transaction">
  /// Optional <see cref="IDbTransaction"/> instance.
  /// </param>
  public DatabaseTransaction(
    IDbConnection connection,
    IDbTransaction transaction = null)
  {
    this.connection = connection ??
      throw new ArgumentNullException(nameof(connection));
    this.ownedConnection = false;
    this.Transaction = transaction;
    this.HasEnlistedResources = transaction != null;
  }

  /// <summary>
  /// Indicates whether the <see cref="ITransaction"/> has resources enlisted 
  /// in a transaction.
  /// </summary>
  public bool HasEnlistedResources { get; set; }

  /// <summary>
  /// Indicates whether there is a database connection.
  /// </summary>
  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public bool IsConnected => connection?.State == ConnectionState.Open;

  /// <summary>
  /// Gets and sets a database connection instance.
  /// </summary>    
  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public IDbConnection Connection
  {
    get
    {
      var connection = this.connection;

      if (connection == null)
      {
        if (!ownedConnection)
        {
          throw new ObjectDisposedException("Connection is closed.");
        }

        {
          using var transaction = this.transaction;

          this.transaction = null;
          HasEnlistedResources = false;
        }

        connection = factory.CreateConnection();

        if (!string.IsNullOrEmpty(connectionString))
        {
          connection.ConnectionString = connectionString;
        }

        connection.Open();

        this.connection = connection;
      }

      return connection;
    }
  }

  /// <summary>
  /// Gets the current database transaction, if any.
  /// </summary>
  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  public IDbTransaction Transaction 
  {
    get
    {
      if (transaction == null)
      {
        transaction = Connection.BeginTransaction(isolationLevel);
        HasEnlistedResources = true;
      }

      return transaction;
    }
    set => transaction = value;
  }

  /// <summary>
  /// Commits database modifications made within the transaction scope.
  /// </summary>
  public void Commit()
  {
    using var transaction = this.transaction;

    this.transaction = null;
    HasEnlistedResources = false;
    transaction?.Commit();
  }

  /// <summary>
  /// Rolls back database modifications made within the transaction scope.
  /// </summary>
  public void Rollback()
  {
    using var transaction = this.transaction;

    this.transaction = null;
    HasEnlistedResources = false;
    transaction?.Rollback();
  }

  /// <summary>
  /// Gets attributes map.
  /// </summary>
  public Dictionary<string, object> Attributes => attributes ??= new();

  #region IDisposable Members
  /// <summary>
  /// Implement IDisposable.
  /// </summary>
  /// <remarks>
  /// A derived class should not be able to override this method.
  /// </remarks>
  public void Dispose()
  {
    using var connection = this.connection;
    using var transaction = this.transaction;

    this.connection = null;
    this.transaction = null;
    HasEnlistedResources = false;
    Functions.Dispose(attributes);
  }

  /// <summary>
  /// The destructor will run only if the Dispose method does not get called.
  /// It gives your base class the opportunity to finalize.
  /// </summary>
  /// <remarks>
  /// Do not provide destructors in types derived from this class.
  /// </remarks>
  ~DatabaseTransaction() => Dispose();
  #endregion

  /// <summary>
  /// Queries a specified service.
  /// </summary>
  /// <param name="type">A service type.</param>
  /// <returns>
  /// an instance of a specified service, 
  /// or null if a service is not available.
  /// </returns>
  public virtual object GetService(Type type) =>
    type == typeof(ITransaction) ? this :
      type == typeof(IDbConnectionProvider) ? this :
      type == typeof(IAttributes) ? this : null;

  /// <summary>
  /// A database provider factory.
  /// </summary>
  protected DbProviderFactory factory;

  /// <summary>
  /// A database connection string.
  /// </summary>
  protected string connectionString;

  /// <summary>
  /// A transaction isolation level.
  /// </summary>
  protected IsolationLevel isolationLevel;

  /// <summary>
  /// A database connection.
  /// </summary>
  protected IDbConnection connection;

  /// <summary>
  /// Indicates whether the connection is owned by the object.
  /// </summary>
  protected bool ownedConnection;

  /// <summary>
  /// A database transaction.
  /// </summary>
  protected IDbTransaction transaction;

  /// <summary>
  /// Attributes map.
  /// </summary>
  private Dictionary<string, object> attributes;
}
