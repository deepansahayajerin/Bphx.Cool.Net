using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Bphx.Cool;

/// <summary>
/// A heap structure.
/// </summary>
/// <remarks>Note that names are compared by identity.</remarks>
[Serializable]
public class Heap: View, ICloneable
{
  /// <summary>
  /// Default constructor.
  /// </summary>
  public Heap() { }

  /// <summary>
  /// Copy constructor.
  /// </summary>
  /// <param name="that">another <see cref="Heap"/> instance.</param>
  public Heap(Heap that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>
  /// Creates a copy of this object.
  /// </summary>
  /// <returns>A copy of this instance.</returns>
  public new Heap Clone() => new(this);

  /// <summary>
  /// Creates a copy of this instance.
  /// </summary>
  /// <returns>A <see cref="Heap"/> instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>
  /// Assigns value from another instance.
  /// </summary>
  /// <param name="that">Another Heap instance.</param>
  public void Assign(Heap that)
  {
    base.Assign(that);
    name = that.name;
    value = that.value;
    state = that.state == null ? null : new(that.state);
  }

  /// <summary>
  /// Tests whether this heap is equivalent to the other heap.
  /// </summary>
  /// <param name="that">A heap to compare with.</param>
  /// <returns>
  /// <c>true</c> if this heap is equivalent to the other heap, and
  /// <c>false</c> otherwise.
  /// </returns>
  public bool IsEquivalent(Heap that)
  {
    if (this == that)
    {
      return true;
    }
    else if (that == null)
    {
      return false;
    }
    else if (name != null)
    {
      return (name == that.name) &&
        (((value == null) && (that.value == null)) ||
          value.Equals(that.value));
    }
    else if (that.name != null)
    {
      return false;
    }
    else if (state == null)
    {
      return that.state == null;
    }
    else if ((that.state == null) || (state.Count != that.state.Count))
    {
      return false;
    }
    else
    {
      foreach(var entry in state)
      {
        if (!that.state.TryGetValue(entry.Key, out var thatValue) ||
          !Equals(entry.Value, thatValue))
        {
          return false;
        }
      }

      return true;
    }
  }

  /// <summary>
  /// Gets a property.
  /// </summary>
  /// <typeparam name="T">A property type.</typeparam>
  /// <param name="name">A property name.</param>
  /// <returns>A property value.</returns>
  protected T Get<T>(object name) =>
    (T)(this.name == name ? value : state?.Get(name)) ?? default;

  /// <summary>
  /// Sets a property.
  /// </summary>
  /// <typeparam name="T">A property type.</typeparam>
  /// <param name="name">A property name.</param>
  /// <param name="value">A property value.</param>
  protected void Set<T>(object name, T value)
  {
    if (this.name == name)
    {
      this.value = value;
    }
    else if (state != null)
    {
      if (value == null)
      {
        state.Remove(name);
      }
      else
      {
        state[name] = value;
      }
    }
    else if (value != null)
    {
      if (this.name == null)
      {
        this.name = name;
        this.value = value;
      }
      else
      {
        state = new() { { name, value } };

        if (this.value != null)
        {
          state[this.name] = this.value;
          this.value = null;
        }

        this.name = null;
      }
    }
    // No more cases.
  }

  /// <summary>
  /// Prepares the data after deserialization.
  /// </summary>
  /// <param name="context">A <see cref="StreamingContext"/> instance.</param>
  [OnDeserialized]
  private void OnDeserialized(StreamingContext context)
  { 
    if (name is string key)
    {
      name = string.Intern(key);
    }
  }

  /// <summary>
  /// Last set property name.
  /// </summary>
  private object name;

  /// <summary>
  /// Last set property value.
  /// </summary>
  private object value;

  /// <summary>
  /// A heap state.
  /// </summary>
  private State state;

  /// <summary>
  /// A heap state.
  /// </summary>
  [Serializable]
  private sealed class State: Dictionary<object, object>, ISerializable
  {
    /// <summary>
    /// Creates a heap state.
    /// </summary>
    public State(): base(ReferenceEqualityComparer.Instance)
    {
    }

    /// <summary>
    /// Creates a copy of the state.
    /// </summary>
    /// <param name="other">A state to copy.</param>
    public State(State other): base(other, ReferenceEqualityComparer.Instance)
    {
    }

    /// <summary>
    /// Constructor used for the deserialization.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    public State(SerializationInfo info, StreamingContext context) :
      base(
        ((KeyValuePair<object, object>[])
          info.GetValue("Content", typeof(KeyValuePair<object, object>[]))).
          Select(p => p.Key is string n ? new(string.Intern(n), p.Value) : p),
        ReferenceEqualityComparer.Instance)
    {
    }

    /// <summary>
    /// Serializes state.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    public override void GetObjectData(
      SerializationInfo info, 
      StreamingContext context) =>
      info.AddValue("Content", this.ToArray());
  }
}
