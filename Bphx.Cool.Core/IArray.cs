using System.Collections;

namespace Bphx.Cool;

/// <summary>
/// A list implementation with fixed capacity.
/// </summary>
public interface IArray : IList
{
  /// <summary>
  /// Gets and sets the list capacity.
  /// </summary>
  int Capacity { get; }

  /// <summary>
  /// Gets and sets the list size.
  /// </summary>
  new int Count { get; set; }

  /// <summary>
  /// Current index.
  /// </summary>
  int Index { get; set; }

  /// <summary>
  /// Initial index.
  /// </summary>
  int InitialIndex { get; }

  /// <summary>
  /// Creates a new element for this array.
  /// </summary>
  /// <returns>An object that could be stored into this array.</returns>
  object NewElement();
}
