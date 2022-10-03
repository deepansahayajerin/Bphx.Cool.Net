using System;
using System.Collections.Generic;
using System.Linq;

using Bphx.Cool.Xml;

using static Bphx.Cool.Functions;

namespace Bphx.Cool;

/// <summary>
/// Composite resources.
/// </summary>
public class CompositeResources : IResources
{
  /// <summary>
  /// Creates <see cref="CompositeResources"/> from <see cref="IResources"/> 
  /// in current <see cref="AppDomain"/>.
  /// </summary>
  public CompositeResources() :
    this(
      AppDomain.CurrentDomain.
        GetAssemblies().
        SelectMany(
          item =>
            item.GetCustomAttributes(typeof(ResourcesAttribute), false)).
        OfType<ResourcesAttribute>().
        Select(item => (IResources)Activator.CreateInstance(item.Type)))
  {
  }

  /// <summary>
  /// Creates <see cref="CompositeResources"/> from a enumeration 
  /// of <see cref="IResources"/>.
  /// </summary>
  /// <param name="resources">
  /// A enumeration of <see cref="IResources"/>.
  /// </param>
  public CompositeResources(IEnumerable<IResources> resources)
  {
    this.resources = resources.ToArray();

    foreach(var item in this.resources.OfType<Resources>())
    {
      item.Init();
    }
  }

  /// <summary>
  /// Gets an <see cref="NavigationRule"/> instance for a procedure name.
  /// </summary>
  /// <param name="name">A procedure name.</param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <returns>An <see cref="NavigationRule"/> instance</returns>
  public ActionInfo GetActionInfo(string name, int resourceID)
  {
    return name == null ? null :
      Find((r, i) => r?.GetActionInfo(name, i), resourceID, name);
  }

  /// <summary>
  /// Gets a procedure name by a transaction code. 
  /// </summary>
  /// <param name="transactionCode">A transaction code.</param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <returns>
  /// A procedure name, or null if there is no procedure for a specified 
  /// transaction name.
  /// </returns>
  public string GetProcedureByTransactionCode(
    string transactionCode,
    int resourceID)
  {
    return transactionCode == null ? null :
      Find(
        (r, i) => r.GetProcedureByTransactionCode(transactionCode, i),
        resourceID);
  }

  /// <summary>
  /// Gets <see cref="ExitState"/> instance by name.
  /// </summary>
  /// <param name="exitState">An exit state name.</param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <returns>
  /// An <see cref="ExitState"/> instance, or null if no exitstate 
  /// is available for the name.
  /// </returns>
  public ExitState GetExitState(string exitState, int resourceID)
  {
    return IsEmpty(exitState) ? null :
      Find((r, i) => r.GetExitState(exitState, i), resourceID, exitState);
  }

  /// <summary>
  /// Gets <see cref="ExitState"/> instance by ID.
  /// </summary>
  /// <param name="exitState">An exit state ID.</param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <returns>
  /// An <see cref="ExitState"/> instance, or null if no exitstate 
  /// is available for the ID.
  /// </returns>
  public ExitState GetExitState(int exitStateID, int resourceID)
  {
    return exitStateID == 0 ? null :
      Find((r, i) => r.GetExitState(exitStateID, i), resourceID);
  }

  /// <summary>
  /// Gets a message for an exit state.
  /// </summary>
  /// <param name="name">An exit state name.</param>
  /// <param name="dialect">Optional, a dialect name.</param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <returns>An exit state message.</returns>
  public string GetExitStateMessage(
    string name,
    string dialect,
    int resourceID)
  {
    return IsEmpty(name) ? null :
      Find(
        (r, i) =>
          r.GetExitState(name, i) == null ? null :
            r.GetExitStateMessage(name, dialect, i) ?? "",
      resourceID,
      name);
  }

  /// <summary>
  /// Gets index of cached resource.
  /// </summary>
  /// <param name="name">A resource name.</param>
  /// <returns>A resource ID, if available.</returns>
  public int GetResources(string name)
  {
    lock(sync)
    {
      if (map.TryGetValue(name, out var item))
      {
        if (item.Prev != null)
        {
          item.Prev.Next = item.Next;
        }

        if (item.Next != null)
        {
          item.Next.Prev = item.Prev;
        }

        item.Prev = last;
        item.Next = null;
        item.Access = DateTime.Now;
        last = item;

        return item.Value;
      }
    }

    return 0;
  }

  /// <summary>
  /// Caches resource index.
  /// </summary>
  /// <param name="name">A resource name</param>
  /// <param name="resourceID">A resource index.</param>
  public void SetResources(string name, int resourceID)
  {
    var now = DateTime.Now;
    var eldest = now.Subtract(TimeSpan.FromSeconds(CacheExpiration));

    lock(sync)
    {
      while((last != null) && (last.Access < eldest))
      {
        map.Remove(last.Key);

        if (last.Next != null)
        {
          last.Next.Prev = null;
        }

        last = last.Next;
      }

      var item = new Item
      {
        Key = name,
        Value = resourceID,
        Access = now,
        Next = last
      };

      if (last != null)
      {
        last.Prev = item;
      }

      last = item;
      map.Add(name, item);
    }
  }

  /// <summary>
  /// Finds value filtering resources.
  /// </summary>
  /// <typeparam name="T">A type of result.</typeparam>
  /// <param name="accessor">Value accessor.</param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <param name="name">Name of resource.</param>
  /// <returns>Accessed value or <c>null</c>, if not available.</returns>
  private T Find<T>(
    Func<IResources, int, T> accessor,
    int resourceID,
    string name = null)
    where T : class
  {
    var result = Get(resourceID, accessor);

    if (result != null)
    {
      return result;
    }

    if (name != null)
    {
      result = Get(GetResources(name), accessor);

      if (result != null)
      {
        return result;
      }
    }

    for(var i = 0; i < resources.Length; ++i)
    {
      result = Get(i + 1, accessor);

      if (result != null)
      {
        if (name != null)
        {
          SetResources(name, i + 1);
        }

        return result;
      }
    }

    return null;
  }

  /// <summary>
  /// Gets a resource value.
  /// </summary>
  /// <typeparam name="T">A type of result.</typeparam>
  /// <param name="accessor">Value accessor.</param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <returns>A resource value, if any.</returns>
  private T Get<T>(int resourceID, Func<IResources, int, T> accessor)
    where T : class
  {
    return (resourceID <= 0) || (resourceID > resources.Length) ? null :
      accessor(resources[resourceID - 1], resourceID);
  }

  /// <summary>
  /// An array of resources.
  /// </summary>
  private readonly IResources[] resources;

  /// <summary>
  /// A resource cache.
  /// </summary>
  private readonly Dictionary<string, Item> map = new();

  /// <summary>
  /// Last cached resource.
  /// </summary>
  private Item last;

  /// <summary>
  /// A sync object.
  /// </summary>
  private readonly object sync = new();

  /// <summary>
  /// Cache item.
  /// </summary>
  private class Item
  {
    public string Key;
    public int Value;
    public DateTime Access;
    public Item Prev;
    public Item Next;
  }

  /// <summary>
  /// Expiration interval in ms (10 minutes).
  /// </summary>
  private const long CacheExpiration = 600000;
}
