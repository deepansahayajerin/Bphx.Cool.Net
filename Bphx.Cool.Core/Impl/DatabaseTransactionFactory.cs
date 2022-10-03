using System;
using System.Data;
using System.Data.Common;

namespace Bphx.Cool.Impl;

/// <summary>
/// An ADO.NET transaction factory.
/// </summary>
public class DatabaseTransactionFactory
{
  /// <summary>
  /// Creates an <see cref="DatabaseTransactionFactory"/> instance.
  /// </summary>
  /// <param name="factory">A database provider factory.</param>
  /// <param name="connectionString">A database connection string.</param>
  /// <param name="isolationLevel">A transaction isolation level</param>
  public DatabaseTransactionFactory(
    DbProviderFactory factory,
    string connectionString,
    IsolationLevel isolationLevel = IsolationLevel.Unspecified)
  {
    this.factory = factory ??
      throw new ArgumentNullException(nameof(factory));
    this.connectionString = connectionString;
    this.isolationLevel = isolationLevel;
  }

  /// <summary>
  /// Creates an <see cref="DatabaseTransactionFactory"/> instance.
  /// </summary>
  /// <param name="connection">A <see cref="IDbConnection"/> instance.</param>
  /// <param name="transaction">Optional <see cref="IDbTransaction"/> instance.</param>
  public DatabaseTransactionFactory(
    IDbConnection connection,
    IDbTransaction transaction = null)
  {
    this.connection = connection ??
      throw new ArgumentNullException(nameof(connection));
    this.transaction = transaction;
  }

  #region ITransactionFactory Members
  /// <summary>
  /// Creates a new transaction.
  /// </summary>
  /// <returns>an ITransaction instance.</returns>
  public virtual ITransaction CreateTransaction() =>
    factory != null ?
      new DatabaseTransaction(factory, connectionString, isolationLevel) :
      new DatabaseTransaction(connection, transaction);
  #endregion

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
  /// A database transaction.
  /// </summary>
  private readonly IDbTransaction transaction;
}
