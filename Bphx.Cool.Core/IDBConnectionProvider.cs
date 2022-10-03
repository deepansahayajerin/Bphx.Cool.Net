using System.Data;
using System.Diagnostics;

namespace Bphx.Cool;

/// <summary>
/// An interface to expose <see cref="IDbConnection"/> and <see cref="IDbTransaction"/>.
/// </summary>
public interface IDbConnectionProvider
{
  /// <summary>
  /// Gets a database connection instance.
  /// </summary>
  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  IDbConnection Connection { get; }

  /// <summary>
  /// Gets the current database transaction.
  /// </summary>
  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  IDbTransaction Transaction { get; set; }
}
