namespace Bphx.Cool;

/// <summary>
/// A savepoint state.
/// </summary>
public interface ISavepoint
{
  /// <summary>
  /// Gets a procedure from a savepoint, or <c>null</c> 
  /// if no procedure is available in a savepoint.
  /// </summary>
  /// <returns>A <see cref="IProcedure"/> instance or <c>null</c>.</returns>
  IProcedure Get();

  /// <summary>
  /// Sets a procedure into the savepoint. 
  /// </summary>
  /// <param name="procedure">
  /// A <see cref="IProcedure"/> to set, or <c>null</c> to
  /// reset savepoint state.
  /// </param>
  void Set(IProcedure procedure);
}
