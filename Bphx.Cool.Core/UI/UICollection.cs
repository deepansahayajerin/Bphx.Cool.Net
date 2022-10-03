using System;
using System.Collections;
using System.Collections.Generic;

namespace Bphx.Cool.UI;

/// <summary>
/// A IUICollection interface.
/// </summary>
public interface IUICollection
{
  /// <summary>
  /// A list of items.
  /// </summary>
  IList List { get; }
}

/// <summary>
/// A collection of <see cref="UIObject"/>s.
/// </summary>
[Serializable]
public class UICollection<T> : UIObject, IUICollection
  where T : UIObject
{
  /// <summary>
  /// Gets item by the index.
  /// </summary>
  /// <param name="index">Item index.</param>
  /// <returns>Item value.</returns>
  public T Item(int index)
  {
    if((index <= 0) || (index > List.Count))
    {
      return null;
    }

    return List[index - 1];
  }

  /// <summary>
  /// A collection size.
  /// </summary>
  public int Count => List.Count;

  /// <summary>
  /// A list of items.
  /// </summary>
  public List<T> List { get; } = new();

  /// <summary>
  /// A list of items.
  /// </summary>
  IList IUICollection.List => List;

  /// <summary>
  /// Invokes a method of <see cref="UIObject"/> instance.
  /// </summary>
  /// <param name="name">A method name.</param>
  /// <param name="args">Method's arguments.</param>
  /// <returns>Result type.</returns>
  public override object Invoke(string name, params object[] args)
  {
    if(name == null)
    {
      throw new ArgumentException("Invalid name", nameof(name));
    }

    switch(name.ToLower())
    {
      case "item":
      {
        if(args.Length != 1)
        {
          throw new ArgumentException("Invalid arguments", nameof(args));
        }

        return Item(Convert.ToInt32(args[0]));
      }
      case "count":
      {
        if(args.Length != 0)
        {
          throw new ArgumentException("Invalid arguments", nameof(args));
        }

        return Count;
      }
      default:
      {
        return base.Invoke(name, args);
      }
    }
  }

  /// <summary>
  /// Sets the owner window.
  /// </summary>
  /// <param name="window">An owner window.</param>
  public override void SetOwner(UIWindow window)
  {
    base.SetOwner(window);

    foreach(var item in List)
    {
      item.SetOwner(window);
    }
  }
}
