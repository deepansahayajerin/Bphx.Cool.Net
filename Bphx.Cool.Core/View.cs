using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Bphx.Cool;

/// <summary>
/// <para>A base class for data structures.</para>
/// 
/// </summary>
[Serializable]
public class View: ICloneable
{
  /// <summary>
  /// Default constructor.
  /// </summary>
  public View() { }

  /// <summary>
  /// Copy constructor.
  /// </summary>
  /// <param name="that">another <see cref="View"/> instance.</param>
  public View(View that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>
  /// Initializes view.
  /// </summary>
  /// <param name="that">another <see cref="View"/> instance.</param>
  [MethodImpl(
    MethodImplOptions.AggressiveInlining |
    MethodImplOptions.AggressiveOptimization)]
  protected void Init(View that) => AssignScreenFields(that);

  /// <summary>
  /// Creates a copy of this object.
  /// </summary>
  /// <returns>A copy of this instance.</returns>
  public View Clone() => new(this);

  /// <summary>
  /// Creates a copy of this instance.
  /// </summary>
  /// <returns>a ScreenField instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>
  /// Assigns value from another instance of Heap.
  /// </summary>
  /// <param name="that">another <see cref="View"/> instance.</param>
  public void Assign(View that) => Populated = that.Populated;

  /// <summary>
  /// Assigns screen field from other instance.
  /// </summary>
  /// <param name="that">another <see cref="View"/> instance.</param>
  public void AssignScreenFields(View that) =>
    screenFields = that.screenFields?.
      ToDictionary(item => item.Key, item => item.Value.Clone());

  /// <summary>
  /// Gets a Screen feld
  /// </summary>
  /// <param name="name">A field name.</param>
  /// <param name="initialize">
  /// true to initialize a <see cref="ScreenField"/>, and false 
  /// to return a cached value, if any.
  /// </param>
  /// <returns>A <see cref="ScreenField"/> instance.</returns>
  public ScreenField GetField(string name, bool initialize = true)
  {
    var fields = ScreenFields;
    var field = initialize ? null : fields.Get(name);

    if (field == null)
    {
      field = new() { Name = name };
      fields[name] = field;
    }

    return field;
  }

  /// <summary>
  /// A dictionary of <see cref="ScreenField"/>s.
  /// </summary>
  [Computed]
  [XmlIgnore, JsonIgnore]
  public Dictionary<string, ScreenField> ScreenFields => 
    screenFields ??= new();

  /// <summary>
  /// Xml representation of <see cref="ScreenField"/>s.
  /// </summary>
  [Computed]
  [XmlElement("screenField"), JsonPropertyName("screenField")]
  public ScreenField[] ScreenFields_Xml
  {
    get
    {
      if (screenFields == null)
      {
        return null;
      }

      var items = Functions.
        ToEnumerable(screenFields).
        Where(item => !item.IsDefault()).
        ToArray();

      return items.Length == 0 ? null : items;
    }
    set => screenFields = Functions.ToDictionary(value);
  }

  /// <summary>
  /// A populated indicator.
  /// </summary>
  [XmlIgnore, JsonIgnore]
  public bool Populated { get; set; }

  /// <summary>
  /// Screen fields storage.
  /// </summary>
  private Dictionary<string, ScreenField> screenFields;
}
