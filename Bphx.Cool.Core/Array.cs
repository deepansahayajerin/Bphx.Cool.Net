using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Linq;

namespace Bphx.Cool;

/// <summary>
/// <para>A list implementation with fixed capacity.</para> 
/// <para>
/// Elements of collection are created on demand, thus it's impossible to 
/// store null value.
/// </para>
/// <para>
/// The collection is expanded whenever one accesses index, which is 
/// greater than current size. Access to the index greater that capacity
/// is not allowed.
/// </para>
/// <para>
/// Note that when size of the Array is reduced and then enlarged back, 
/// collection elements are restored.  
/// </para>
/// </summary>
/// <typeparam name="T">An array element.</typeparam>
[Serializable]
public class Array<T>: IList<T>, IArray
  where T: class, new()
{
  /// <summary>
  /// Creates an empty array.
  /// </summary>
  /// <param name="capacity">A capacity of the list.</param>
  public Array(int capacity)
  {
    this.capacity = capacity >= 0 ? capacity :
      throw new ArgumentException("Invalid capacity", nameof(capacity));
    InitialIndex = index = -1;
  }

  /// <summary>
  /// Creates a list with a specified capacity.
  /// </summary>
  /// <param name="capacity">A capacity of the list.</param>
  /// <param name="initialIndex">
  /// Initial value of the index.
  /// </param>
  public Array(int capacity, int initialIndex)
  {
    this.capacity = capacity >= 0 ? capacity :
      throw new ArgumentException("Invalid capacity", nameof(capacity));
    InitialIndex = index = initialIndex;
  }

  /// <summary>
  /// Gets and set the list capacity.
  /// </summary>
  [XmlIgnore, JsonIgnore]
  public int Capacity => capacity;

  /// <summary>
  /// Initial index.
  /// </summary>
  [XmlIgnore, JsonIgnore]
  public int InitialIndex { get; private set; }

  /// <summary>
  /// Current index.
  /// </summary>
  [XmlIgnore, JsonIgnore]
  public int Index
  {
    get => index;
    set
    {
      index = value;
      Updated = false;
    }
  }

  /// <summary>
  /// Update indicator for a current element.
  /// </summary>
  [XmlIgnore, JsonIgnore]
  public bool Updated { get; private set; }

  /// <summary>
  /// Gets current element.
  /// </summary>
  [XmlIgnore, JsonIgnore]
  public T Item => 
    (index >= 0) && (index < count) ? (items[index] ??= new()) : GetAt(index);

  /// <summary>
  /// Gets current element for update.
  /// </summary>
  [XmlIgnore, JsonIgnore]
  public T Update
  {
    get
    {
      Updated = true;

      return GetAt(index);
    }
  }

  /// <summary>
  /// Indicates whether the array is empty (true).
  /// </summary>
  [XmlIgnore, JsonIgnore]
  public bool IsEmpty => count == 0;

  /// <summary>
  /// Indicates whether the array is full (true),
  /// namely whether the size is equal to capacity.
  /// </summary>
  [XmlIgnore, JsonIgnore]
  public bool IsFull => count >= capacity;

  /// <summary>
  /// Returns the number of elements in this list. 
  /// </summary>
  [XmlIgnore, JsonIgnore]
  public int Count
  {
    get => count;
    set => EnsureLength(
      count = value < 0 ? 0 : value > capacity ? capacity : value);
  }

  /// <summary>
  /// <para>Gets and sets the element at the specified index.</para>
  /// <para>
  /// <b>Note</b>: this property increases the array size 
  /// when the specified index is bigger than the current size, 
  /// returned by <code>Count</code> property.
  /// </para>
  /// </summary>
  /// <param name="index">
  /// The zero-based index of the element to get or set.
  /// </param>
  /// <returns>
  /// An element at the specified position in this list.
  /// </returns>
  [IndexerName("Element")]
  public T this[int index]
  {
    get
    {
      CheckIndex(index, true);

      try
      {
        return items[index] ??= new();
      }
      catch(Exception e)
      {
        if ((index != -1) && (index != capacity))
        {
          throw;
        }

        Trace.TraceError(
          "Attempt to access to invalid index " + index +
          " in Array<" + typeof(T).FullName +
          ">. Fallback to index 0.\n" + e,
          "DEBUG");

        return new();
      }
    }
    set
    {
      CheckIndex(index, true);
        
      items[index] = value;
    }
  }

  /// <summary>
  /// Advances index if current value is populated.
  /// </summary>
  public void Next()
  {
    if (Updated)
    {
      Updated = false;

      if ((++index >= Count) && (index <= capacity))
      {
        Count = index;
      }
    }
  }

  /// <summary>
  /// Appends an element to the end of this list.
  /// </summary>
  /// <param name="item">The element to append.</param>
  public void Add(T item)
  {
    if (count == items.Length)
    {
      EnsureLength(count + 1);
    }

    items[count] = item;
    Updated = false;
    ++count;
  }

  /// <summary>
  /// Assigns items to an array.
  /// </summary>
  /// <param name="items">Items to assign.</param>
  public void Assign(IEnumerable<T> items)
  {
    var array = items.ToArray();

    if (array.Length > capacity)
    {
      throw new ArgumentException(
        $"Number of items {array.Length} is greater than capacity {capacity}",
        nameof(items));
    }

    index = InitialIndex;
    count = array.Length;
    this.items = array;
  }

  /// <summary>
  /// Returns an element at the specified position in this list without
  /// changing of the array size.
  /// </summary>
  /// <param name="index">an index of the element to return.</param>
  /// <returns>the element at the specified position in this list.</returns>
  public T GetAt(int index)
  {
    var count = this.count;
    var value = this[index];

    this.count = count;

    if ((index > count) && !Updated)
    {
      Trace.TraceError(
        "Access at index " + index +
        " past size " + count +
        " in Array<" + typeof(T).FullName +
        ">.",
        "DEBUG");
    }

    return value;
  }

  /// <summary>
  /// Checks the index and adjusts it if it's greater than size.
  /// </summary>
  /// <returns>
  /// Returns true if operation succeeds, and false if index is less 
  /// than 0 or greater than capacity.
  /// </returns>
  public bool CheckIndex()
  {
    if (index < 0)
    {
      index = 0;

      return false;
    }

    if (index >= capacity)
    {
      index = capacity - 1;

      return false;
    }

    return true;
  }

  /// <summary>
  /// Checks the size and adjusts it if index is greater than size.
  /// </summary>
  /// <returns>
  /// Returns true if operation succeeds, and false if index is less 
  /// than 0 or greater than capacity.
  /// </returns>
  public bool CheckSize()
  {
    if ((index < 0) || (index >= capacity))
    {
      return false;
    }

    if (index >= count)
    {
      Count = index + 1;
    }

    return true;
  }

  /// <summary>
  /// Sets list's size to 0.
  /// </summary>
  public void Clear() => count = 0;

  /// <summary>
  /// Checks whether this list contains the specified item.
  /// </summary>
  /// <param name="item">an item to look for.</param>
  /// <returns>
  /// true when this list contains the specified item, false - otherwise.
  /// </returns>
  public bool Contains(T item)
  {
    var comparer = EqualityComparer<T>.Default;
      
    for(var i = 0; i < count; i++)
    {
      if (comparer.Equals(items[i], item))
      {
        return true;
      }
    }
      
    return false;
  }

  /// <summary>
  /// Copies the entire list to a compatible one-dimensional array, 
  /// starting at the specified index of the target array.
  /// </summary>
  /// <param name="array">the one-dimensional <code>Array</code> that 
  /// is the destination of the elements copied from this list. 
  /// The <code>Array</code> must have zero-based indexing.
  /// </param>
  /// <param name="arrayIndex">
  /// The zero-based index in array at which copying begins.
  /// </param>
  public void CopyTo(T[] array, int arrayIndex) =>
    Array.Copy(items, 0, array, arrayIndex, count);

  /// <summary>
  /// Returns an enumerator that iterates through the list.
  /// </summary>
  /// <returns>an enumerator that iterates through the list.</returns>
  public IEnumerator<T> GetEnumerator() =>
    Enumerable.Range(0, Count).Select(i => this[i]).GetEnumerator();

  /// <summary>
  /// Searches for the specified object and returns the zero-based index of 
  /// the first occurrence within the entire list.
  /// </summary>
  /// <param name="item">The object to locate in the list.</param>
  /// <returns>
  /// The zero-based index of the first occurrence of item within the entire 
  /// list, if found; otherwise, –1.
  /// </returns>
  public int IndexOf(T item) => Array.IndexOf(items, item, 0, count);

  /// <summary>
  /// Inserts an element into the list at the specified index.
  /// </summary>
  /// <param name="index">
  /// The zero-based index at which item should be inserted.
  /// </param>
  /// <param name="item">The object to insert.</param>
  public void Insert(int index, T item)
  {
    CheckIndex(index < count ? count : index, true);

    if (index < count)
    {
      Array.Copy(items, index, items, index + 1, count - index);
    }

    items[index] = item;
    Updated = false;
    ++count;
  }

  /// <summary>
  /// Removes the specified element from the list.
  /// </summary>
  /// <param name="item">the element to be removed.</param>
  /// <returns>
  /// true if an element was removed as a result of this call, 
  /// otherwise false.
  /// </returns>
  public bool Remove(T item)
  {
    var index = IndexOf(item);
      
    if (index >= 0)
    {
      RemoveAt(index);

      return true;
    }
      
    return false;
  }

  /// <summary>
  /// Removes the element at the specified position in this list. 
  /// Shifts any subsequent elements to the left 
  /// (subtracts one from their indices).
  /// </summary>
  /// <param name="index">the index of the element to be removed</param>
  public void RemoveAt(int index)
  {
    if ((index >= 0) && !CheckIndex(index, false))
    {
      int offset = count - index - 1;

      if (offset > 0)
      {
        var last = (T)DataUtils.Clone(items[count - 1]);

        Array.Copy(items, index + 1, items, index, offset);
        items[count - 1] = last;
      }

      --count;
      Updated = false;
    }
  }

  /// <summary>
  /// Creates a new element for this array.
  /// </summary>
  /// <returns>An object that could be stored into this array.</returns>
  public object NewElement() => new T();

  /// <summary>
  /// Sorts the elements in the entire Array(Of T) 
  /// using the default comparer.
  /// </summary>
  public void Sort() => Array.Sort(items);

  /// <summary>
  /// Sorts the elements in the entire Array(Of T) 
  /// using the specified comparer.
  /// </summary>
  /// <param name="comparer">
  /// The IComparer(Of T) implementation to use when comparing elements, 
  /// or null to use the default comparer Comparer(Of T).
  /// </param>
  public void Sort(IComparer<T> comparer) =>
    Array.Sort(items, comparer);

  /// <summary>
  /// Sorts the elements in the entire Array(Of T) 
  /// using the specified Comparison.
  /// </summary>
  /// <param name="comparison">
  /// The Comparison to use when comparing elements.
  /// </param>
  public void Sort(Comparison<T> comparison) =>
    Array.Sort(items, comparison);

  /// <summary>
  /// Copies data between this and other <see cref="Array"/> instances.
  /// </summary>
  /// <typeparam name="S">A target element type.</typeparam>
  /// <param name="target">A target <see cref="Array"/>.</param>
  /// <param name="copt">A copy function.</param>
  public void CopyTo<S>(
    Array<S> target,
    Action<T, S> copy)
    where S: class, new()
  {
    var length = items.Length;
    var targetCount = count;

    if (count > target.capacity)
    {
      length = target.capacity;
      targetCount = length;

      Trace.TraceWarning(
        "Target capacity " + target.capacity +
          " is less than source size " + count +
          " during copying.",
        "DEBUG");
    }

    var targetItems = new S[length];

    for(var i = 0; i < targetItems.Length; ++i)
    {
      var item = items[i];
      var targetItem = i < target.items.Length ? target.items[i] : null;

      if ((item != null) || (targetItem != null))
      {
        if (targetItem == null)
        {
          targetItem = new();
        }

        copy(item ?? new(), targetItem);
        targetItems[i] = targetItem;
      }
    }

    target.items = targetItems;
    target.count = targetCount;
  }

  /// <summary>
  /// Gets a value indicating whether the list is read-only.
  /// This method always returns false.
  /// </summary>
  bool ICollection<T>.IsReadOnly => false;

  /// <summary>
  /// Gets a value indicating whether access to the list is synchronized 
  /// (thread safe).
  /// This method always returns false.
  /// </summary>
  bool ICollection.IsSynchronized => false;

  /// <summary>
  /// Gets an object that can be used to synchronize access to the list.
  /// </summary>
  object ICollection.SyncRoot => this;

  /// <summary>
  /// Gets a value indicating whether the list has a fixed size.
  /// This method always returns false.
  /// </summary>
  bool IList.IsFixedSize => false;

  /// <summary>
  /// Gets a value indicating whether the list is read-only.
  /// This method always returns false.
  /// </summary>
  bool IList.IsReadOnly => false;

  /// <summary>
  /// Gets and sets the element at the specified index.
  /// <b>Note</b>: this property increases the array size 
  /// when the specified index is bigger than the current size, 
  /// returned by <code>Count</code> property.
  /// </summary>
  /// <param name="index">
  /// The zero-based index of the element to get or set.
  /// </param>
  /// <returns>
  /// An element at the specified position in this list.
  /// </returns>
  object IList.this[int index]
  {
    get => this[index];
    set => this[index] = (T)value;
  }

  /// <summary>
  /// Copies the entire list to a compatible one-dimensional array, 
  /// starting at the specified index of the target array.
  /// </summary>
  /// <param name="array">the one-dimensional <code>Array</code> that 
  /// is the destination of the elements copied from this list. 
  /// The <code>Array</code> must have zero-based indexing.
  /// </param>
  /// <param name="arrayIndex">
  /// The zero-based index in array at which copying begins.
  /// </param>
  void ICollection.CopyTo(Array array, int arrayIndex) =>
    Array.Copy(items, 0, array, arrayIndex, count);

  /// <summary>
  /// Returns an enumerator that iterates through the list.
  /// </summary>
  /// <returns>an enumerator that iterates through the list.</returns>
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

  /// <summary>
  /// Append an element to the end of this list.
  /// </summary>
  /// <param name="item">the element to append.</param>
  /// <returns>
  /// The position into which the new element was inserted.
  /// </returns>
  int IList.Add(object item)
  {
    Add((T)item);

    return Count - 1;
  }

  /// <summary>
  /// Checks whether this list contains the specified item.
  /// </summary>
  /// <param name="item">an item to look for.</param>
  /// <returns>
  /// true when this list contains the specified item, false - otherwise.
  /// </returns>
  bool IList.Contains(object item) => Contains((T)item);

  /// <summary>
  /// Searches for the specified object and returns the zero-based index of 
  /// the first occurrence within the entire list.
  /// </summary>
  /// <param name="item">The object to locate in the list.</param>
  /// <returns>
  /// The zero-based index of the first occurrence of item within the entire 
  /// list, if found; otherwise, –1.
  /// </returns>
  int IList.IndexOf(object item) => IndexOf((T)item);

  /// <summary>
  /// Inserts an element into the list at the specified index.
  /// </summary>
  /// <param name="index">
  /// The zero-based index at which item should be inserted.
  /// </param>
  /// <param name="item">The object to insert.</param>
  void IList.Insert(int index, object item) => Insert(index, (T)item);

  /// <summary>
  /// Removes the first occurrence of a specific object from the list.
  /// </summary>
  /// <param name="item">The object to remove from the list.</param>
  void IList.Remove(object item) => Remove((T)item);

  /// <summary>
  /// Increases the base storage length of this collection instance.
  /// </summary>
  /// <param name="min">the desired minimum capacity.</param>
  /// <remarks>
  ///   Throws an ArgumentException in case when the requested new capacity 
  ///   is bigger than an allowed capacity for this array.
  /// </remarks>
  protected void EnsureLength(int min)
  {
    var length = items.Length;

    if (min > length)
    {
      if (min > capacity)
      {
        throw new ArgumentException(
          $"Length {min} is greater than capacity {capacity}");
      }

      var newLength = (length * 3) / 2 + 1;

      if (newLength < min)
      {
        newLength = min;
      }

      if (newLength > capacity)
      {
        newLength = capacity;
      }

      if (newLength != length)
      {
        var newItems = new T[newLength];

        Array.Copy(items, 0, newItems, 0, length);

        items = newItems;
      }
    }
  }

  /// <summary>
  /// Verifies that an index is in range of capacity.
  /// </summary>
  /// <param name="index">an index to check.</param>
  /// <param name="resize">
  /// true to resize collection, if required, and false to 
  /// not resize collection.
  /// </param>
  /// <returns>true if index is out of size, and false otherwise.</returns>
  protected bool CheckIndex(int index, bool resize)
  {
    var outOfSize = index >= count;

    if (resize && outOfSize)
    {
      Count = index + 1;
    }

    return outOfSize;
  }

  /// <summary>
  /// An array storage.
  /// </summary>
  private T[] items = Array.Empty<T>();

  /// <summary>
  /// Number of items in the array.
  /// </summary>
  private int count;

  /// <summary>
  /// Array's capacity.
  /// </summary>
  private readonly int capacity;

  /// <summary>
  /// Array's current index.
  /// </summary>
  private int index;
}
