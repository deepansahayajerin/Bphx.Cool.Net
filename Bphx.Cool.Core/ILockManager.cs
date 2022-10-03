using System;

namespace Bphx.Cool;

/// <summary>
/// A lock manager.
/// </summary>
public interface ILockManager
{
  /// <summary>
  /// Acquires a lock for a named resource.
  /// </summary>
  /// <param name="name">A resource name.</param>
  /// <param name="write">A write intention.</param>
  /// <returns>
  /// A resource that upon dispose releases the lock.
  /// </returns>
  IDisposable Acquire(string name, bool write);
}
