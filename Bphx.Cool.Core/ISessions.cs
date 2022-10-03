namespace Bphx.Cool;

using System;

/// <summary>
/// Defines states of the <see cref="ISessionManager"/>
/// stored in multiple slots to support multiple 
/// application sessions in single session of web container.
/// </summary>
public interface ISessions: IDisposable
{
  /// <summary>
  /// A unique id associated with the instance.
  /// </summary>
  string Id { get; }

  /// <summary>
  /// A <see cref="ISessionManager"/> indexer.
  /// </summary>
  /// <param name="index">A slot index.</param>
  /// <returns>A <see cref="ISessionManager"/> in the slot.</returns>
  ISessionManager this[int index] { get; set; }

  /// <summary>
  /// Creates a new slot.
  /// </summary>
  /// <returns>A new slot index.</returns>
  int Create();
}
