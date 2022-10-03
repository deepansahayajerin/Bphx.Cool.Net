using System;

namespace Bphx.Cool;

/// <summary>
/// A timers service.
/// </summary>
public interface ITimers
{
  /// <summary>
  /// Starts a timer and returns a current timer handle.
  /// </summary>
  /// <param name="timestamp">A timestamp value.</param>
  /// <returns>A timer handle. Returned value is different from 0.</returns>
  int Start(DateTime timestamp);

  /// <summary>
  /// Stop a timer identified by a timer handle.
  /// </summary>
  /// <param name="handle">
  /// A timer handle. Zero means last started timer.
  /// </param>
  /// <param name="timestamp">A timestamp value.</param>
  /// <returns>Elapsed time in milliseconds.</returns>
  int Stop(int handle, DateTime timestamp);

  /// <summary>
  /// Returns a timer identified by a timer handler.
  /// </summary>
  /// <param name="handle">
  /// A timer handle. Zero means last started timer.
  /// </param>
  /// <param name="timestamp">A timestamp value.</param>
  /// <returns>Elapsed time in milliseconds.</returns>
  int ElapsedTime(int handle, DateTime timestamp);
}
