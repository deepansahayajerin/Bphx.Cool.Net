using System;

namespace Bphx.Cool.Impl;

/// <summary>
/// An implementation of <see cref="ISessions"/> interface.
/// </summary>
public class Sessions: ISessions
{
  /// <summary>
  /// Creates a <see cref="Sessions"/> instance.
  /// </summary>
  /// <param name="id">A session id.</param>
  public Sessions(string id = null)
  {
    Id = id ?? Guid.NewGuid().ToString();
  }

  /// <summary>
  /// Disposes sessions associated with this instance.
  /// </summary>
  public void Dispose()
  {
    var states = this.states;

    this.states = null;
    Functions.Dispose(states);
  }

  /// <summary>
  /// A unique id associated with the instance.
  /// </summary>
  public string Id { get; }

  /// <summary>
  /// A <see cref="ISessionManager"/> indexer.
  /// </summary>
  /// <param name="index">A slot index.</param>
  /// <returns>A <see cref="ISessionManager"/> in the slot.</returns>
  public ISessionManager this[int index]
  {
    get => (index >= 0) && (index < states.Length) ? states[index] : null;
    set
    {
      if((index >= 0) && (index < states.Length))
      {
        states[index] = value;
      }
    }
  }

  /// <summary>
  /// Creates a new slot.
  /// </summary>
  /// <returns>A new slot index.</returns>
  public int Create()
  {
    Array.Resize(ref states, states.Length + 1);

    return states.Length - 1;
  }

  /// <summary>
  /// Application sessions.
  /// </summary>
  private ISessionManager[] states = Array.Empty<ISessionManager>();
}
