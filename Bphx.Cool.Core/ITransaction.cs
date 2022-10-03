using System;

namespace Bphx.Cool;

/// <summary>
/// Defines a transaction manager interface.
/// </summary>
public interface ITransaction : IDisposable, IServiceProvider, IAttributes
{
  /// <summary>
  /// Indicates whether the <see cref="ITransaction"/> has resources enlisted 
  /// in a transaction.
  /// </summary>
  bool HasEnlistedResources { get; set; }

  /// <summary>
  /// Commits database modifications made within the transaction scope.
  /// </summary>
  void Commit();

  /// <summary>
  /// Rolls back database modifications made within the transaction scope.
  /// </summary>
  void Rollback();
}
