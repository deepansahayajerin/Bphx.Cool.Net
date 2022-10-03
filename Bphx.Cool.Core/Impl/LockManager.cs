using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Bphx.Cool.Impl;

/// <summary>
/// In-process implementation of lock manager.
/// </summary>
public class LockManager: ILockManager
{
  /// <summary>
  /// Acquires a lock for a named resource.
  /// </summary>
  /// <param name="name">A resource name.</param>
  /// <param name="write">A write intention.</param>
  /// <returns>
  /// Resource that upon dispose releases the lock.
  /// </returns>
  public IDisposable Acquire(string name, bool write)
  {
    var rwlock = new ReaderWriterLock();

    var handle = locks.AddOrUpdate(
      name,
      n => (rwlock, 1),
      (n, h) => (h.rwlock, h.refcount + 1));

    rwlock = handle.rwlock;

    var resource = new Resource
    {
      Release = () =>
      {
        if(write)
        {
          rwlock.ReleaseWriterLock();
        }
        else
        {
          rwlock.ReleaseReaderLock();
        }

        handle = locks.
          AddOrUpdate(name, handle, (n, h) => (h.rwlock, h.refcount - 1));

        if(handle.refcount == 0)
        {
          locks.TryRemove(new(name, handle));
        }
      }
    };

    if (write)
    {
      rwlock.AcquireWriterLock(int.MaxValue);
    }
    else
    {
      rwlock.AcquireReaderLock(int.MaxValue);
    }

    return resource;
  }

  /// <summary>
  /// A running locks.
  /// </summary>
  private readonly
    ConcurrentDictionary<string, (ReaderWriterLock rwlock, int refcount)>
      locks = new();
}
