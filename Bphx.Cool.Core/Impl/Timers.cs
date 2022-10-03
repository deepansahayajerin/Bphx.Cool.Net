using System;
using System.Collections.Generic;

namespace Bphx.Cool.Impl;

/// <summary>
/// Implementation of a timers service.
/// </summary>
[Serializable]
public class Timers : ITimers
{
  /// <summary>
  /// Starts a timer and returns a current timer handle.
  /// </summary>
  /// <param name="timestamp">A timestamp value.</param>
  /// <returns>A timer handle. Returned value is different from 0.</returns>
  public int Start(DateTime timestamp)
  {
    var handle = ++id;

    if(handle == 0)
    {
      handle = ++id;
    }

    Stop(handle, timestamp);

    last = runningTimers[handle] = new()
    {
      ID = handle,
      Prev = last,
      StartTime = timestamp
    };

    return handle;
  }

  /// <summary>
  /// Stop a timer identified by a timer handle.
  /// </summary>
  /// <param name="handle">
  /// A timer handle. Zero means last started timer.
  /// </param>
  /// <param name="timestamp">A timestamp value.</param>
  /// <returns>Elapsed time in milliseconds.</returns>
  public int Stop(int handle, DateTime timestamp)
  {
    if(handle == 0)
    {
      if(last == null)
      {
        return 0;
      }

      handle = last.ID;
    }

    var timer = runningTimers.Get(handle);

    if(timer == null)
    {
      return 0;
    }

    runningTimers.Remove(handle);

    if(timer == last)
    {
      last = timer.Prev;
    }

    if(timer.Prev != null)
    {
      timer.Prev.Next = timer.Next;
    }

    if(timer.Next != null)
    {
      timer.Next.Prev = timer.Prev;
    }

    long d = (timestamp - timer.StartTime).Ticks /
      TimeSpan.TicksPerMillisecond;

    return d > int.MaxValue ? int.MaxValue :
      d < int.MinValue ? int.MinValue : (int)d;
  }

  /// <summary>
  /// Returns a timer identified by a timer handler.
  /// </summary>
  /// <param name="handle">
  /// A timer handle. Zero means last started timer.
  /// </param>
  /// <param name="timestamp">A timestamp value.</param>
  /// <returns>Elapsed time in milliseconds.</returns>
  public int ElapsedTime(int handle, DateTime timestamp)
  {
    var timer = handle == 0 ? last : runningTimers.Get(handle);

    if(timer == null)
    {
      return 0;
    }

    long d = (timestamp - timer.StartTime).Ticks /
      TimeSpan.TicksPerMillisecond;

    return d > int.MaxValue ? int.MaxValue :
      d < int.MinValue ? int.MinValue : (int)d;
  }

  /// <summary>
  /// Id of the last timer.
  /// </summary>
  private int id;

  /// <summary>
  /// Last timer.
  /// </summary>
  private Timer last;

  /// <summary>
  /// A set of timers.
  /// </summary>
  private readonly Dictionary<int, Timer> runningTimers = new();

  /// <summary>
  /// A timer entry.
  /// </summary>
  [Serializable]
  private class Timer
  {
    /// <summary>
    /// A timer ID.
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// A start time.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Next timer.
    /// </summary>
    public Timer Next { get; set; }

    /// <summary>
    /// Previous timer.
    /// </summary>
    public Timer Prev { get; set; }
  }
}
