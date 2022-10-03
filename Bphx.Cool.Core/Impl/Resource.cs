using System;

namespace Bphx.Cool.Impl;
  
/// <summary>
/// A disposable resource.
/// </summary>
public class Resource: IDisposable
{
  /// <summary>
  /// A dispose action.
  /// </summary>
  public System.Action Release { get; init; }

  public void Dispose() => Release?.Invoke();
}
