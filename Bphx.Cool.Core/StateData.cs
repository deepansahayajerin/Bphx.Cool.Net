using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Caching;
using System.Xml.Serialization;

namespace Bphx.Cool;

/// <summary>
/// A state data used for xml and json serialization..
/// </summary>
[XmlRoot("map")]
public class StateData
{
  /// <summary>
  /// Optional name.
  /// </summary>
  [XmlElement("name")]
  public string Name { get; set; }

  /// <summary>
  /// Optional value.
  /// </summary>
  [XmlElement("dateTime", Type = typeof(DateTime))]
  [XmlElement("date", DataType = "date", Type = typeof(DateTime))]
  [XmlElement("time", Type = typeof(XmlTime))]
  [XmlElement("int", Type = typeof(int))]
  [XmlElement("long", Type = typeof(long))]
  [XmlElement("decimal", Type = typeof(decimal))]
  [XmlElement("double", Type = typeof(double))]
  [XmlElement("boolean", Type = typeof(bool))]
  [XmlElement("string", Type = typeof(string))]
  public object Value { get; set; }

  /// <summary>
  /// Optional list or map value.
  /// </summary>
  [XmlElement("map")]
  public StateData[] Map { get; set; }

  /// <summary>
  /// Creates a <see cref="StateData"/> instance.
  /// </summary>
  /// <param name="instance">
  /// An instance to create <see cref="StateData"/> for.
  /// </param>
  /// <returns>A <see cref="StateData"/> instance.</returns>
  public static StateData GetState(object instance)
  {
    if ((instance == null) ||
      (instance is string) ||
      instance.GetType().IsValueType)
    {
      return null;
    }

    if (instance is StateData instanceItem)
    {
      return instanceItem;
    }

    var values = new List<StateData>();

    if (instance is IEnumerable enumerable)
    {
      if (enumerable is IDictionary dictionary)
      {
        foreach(DictionaryEntry entry in dictionary)
        {
          var key = entry.Key;
          var value = entry.Value;

          var item = (value is int) ||
              (value is long) ||
              (value is decimal) ||
              (value is double) ||
              (value is string) ||
              (value is DateTime) ||
              (value is bool) ?
              new StateData { Value = value } :
            value is TimeSpan time ?
              new StateData { Value = (XmlTime)time } :
            value is Enum ? new StateData { Value = value.ToString() } :
            GetState(value);

          if (item != null)
          {
            item.Name = key.ToString();
            values.Add(item);
          }
        }
      }
      else
      {
        foreach(var value in enumerable)
        {
          values.Add(
            (value is int) ||
              (value is long) ||
              (value is decimal) ||
              (value is double) ||
              (value is string) ||
              (value is DateTime) ||
              (value is bool) ?
              new StateData { Value = value } :
            value is TimeSpan time ?
              new StateData { Value = (XmlTime)time } :
            value is Enum ? new StateData { Value = value.ToString() } :
            GetState(value) ?? new());
        }
      }
    }
    else
    {
      foreach(var property in GetProperties(instance).Values)
      {
        var attribute =
          property.Attributes[typeof(StateAttribute)] as StateAttribute;

        if (attribute?.Read != true)
        {
          continue;
        }

        var value = property.GetValue(instance);
        var checkSerialize = true;

        StateData Convert()
        {
          checkSerialize = false;

          return GetState(value);
        }

        var item = (value is int) ||
            (value is long) ||
            (value is decimal) ||
            (value is double) ||
            (value is string) ||
            (value is DateTime) ||
            (value is bool) ?
            new StateData { Value = value } :
          value is TimeSpan time ?
            new StateData { Value = (XmlTime)time } :
          value is Enum ? new StateData { Value = value.ToString() } :
          Convert();

        if ((item != null) &&
          (!checkSerialize || property.ShouldSerializeValue(instance)))
        {
          item.Name = property.Name;
          values.Add(item);
        }
      }
    }

    return values.Count == 0 ? null : new() { Map = values.ToArray() };
  }

  /// <summary>
  /// Populates an instance with values from <see cref="StateData"/>.
  /// </summary>
  /// <param name="instance">
  /// An instance to populate with data from <see cref="StateData"/>.
  /// </param>
  /// <param name="value">A <see cref="StateData"/> instance.</param>
  public static void SetState(object instance, StateData value)
  {
    if ((value == null) ||
      (value.Map == null) ||
      (instance == null) ||
      (instance is string) ||
      instance.GetType().IsValueType)
    {
      return;
    }

    var values = value.Map;

    if (instance is StateData instanceState)
    {
      instanceState.Map = values;
    }
    else if (instance is IEnumerable enumerable)
    {
      if (enumerable is IDictionary dictionary)
      {
        foreach(var item in values)
        {
          if ((item is StateData state) && (state.Name != null))
          {
            SetState(dictionary[state.Name], state);
          }
        }
      }
      else
      {
        var index = 0;

        foreach(var item in enumerable)
        {
          if (index >= values.Length)
          {
            break;
          }

          var state = values[index++];

          if (state is StateData data)
          {
            SetState(item, data);
          }
        }
      }
    }
    else
    {
      var properties = GetProperties(instance);

      foreach(var item in values)
      {
        if (item.Name == null)
        {
          continue;
        }

        var property = properties[item.Name];

        if (property?.Attributes[typeof(StateAttribute)]
          is not StateAttribute attribute)
        {
          continue;
        }

        if (property.IsReadOnly || !attribute.Write)
        {
          SetState(property.GetValue(instance), item);
        }
        else
        {
          var type = property.PropertyType;

          if (((type == typeof(int)) || (type == typeof(int?))) && 
            (item.Value is int))
          {
            property.SetValue(instance, item.Value);

            continue;
          }
          else if (((type == typeof(long)) || (type == typeof(long?))) && 
            (item.Value is long))
          {
            property.SetValue(instance, item.Value);

            continue;
          }
          else if (((type == typeof(decimal)) || (type == typeof(decimal?))) && 
            (item.Value is decimal))
          {
            property.SetValue(instance, item.Value);

            continue;
          }
          else if (((type == typeof(double)) || (type == typeof(double?))) && 
            (item.Value is double))
          {
            property.SetValue(instance, item.Value);

            continue;
          }
          else if ((type == typeof(string)) && (item.Value is string))
          {
            property.SetValue(instance, item.Value);

            continue;
          }
          else if (((type == typeof(bool)) || (type == typeof(bool?))) && 
            (item.Value is bool))
          {
            property.SetValue(instance, item.Value);

            continue;
          }
          else if (((type == typeof(DateTime)) ||
            (type == typeof(DateTime?))) &&
            (item.Value is DateTime))
          {
            property.SetValue(instance, item.Value);

            continue;
          }
          else if (((type == typeof(TimeSpan)) ||
            (type == typeof(TimeSpan?))) &&
            (item.Value is TimeSpan))
          {
            property.SetValue(instance, item.Value);

            continue;
          }
          else if (((type == typeof(TimeSpan)) ||
            (type == typeof(TimeSpan?))) &&
            (item.Value is XmlTime xmlTime))
          {
            property.SetValue(instance, (TimeSpan)xmlTime);

            continue;
          }
          else if ((type == typeof(StateData)) && (item.Map?.Length == 1))
          {
            property.SetValue(instance, item.Map[0]);

            continue;
          }
          else if (type.IsEnum && (item.Value is string stringValue))
          {
            property.SetValue(instance, Enum.Parse(type, stringValue));

            continue;
          }
          else if (type.IsGenericType)
          {
            var genericType = type.GetGenericTypeDefinition();
            var arguments = type.GetGenericArguments();

            if (genericType == typeof(IList<>) && (item.Map != null))
            {
              var itemType = arguments[0];
              var list = Activator.CreateInstance(
                typeof(IList<>).MakeGenericType(itemType)) as IList;

              for(int i = 0; i < item.Map.Length; ++i)
              {
                list.Add(Activator.CreateInstance(itemType));
              }

              SetState(list, item);
              property.SetValue(instance, list);
            }
            else if ((genericType == typeof(IDictionary<,>)) &&
              (arguments[0] == typeof(string)) &&
              (item.Map != null))
            {
              var itemType = arguments[1];
              var map = Activator.CreateInstance(
                typeof(IDictionary<,>).
                  MakeGenericType(typeof(string), itemType))
                as IDictionary;

              foreach(var valueItem in item.Map)
              {
                if (valueItem is StateData valueState)
                {
                  map[valueState.Name] = Activator.CreateInstance(itemType);
                }
              }

              SetState(map, item);
              property.SetValue(instance, map);
            }
            else
            {
              property.SetValue(instance, item);
            }
          }
          else
          {
            SetState(property.GetValue(instance), item);
          }
        }
      }
    }
  }

  /// <summary>
  /// Gets state properties for the instance.
  /// </summary>
  /// <param name="instance">
  /// An instance to get state properties for.
  /// </param>
  /// <returns>State properties.</returns>
  private static IDictionary<string, PropertyDescriptor> GetProperties(
    object instance)
  {
    if (instance == null)
    {
      throw new ArgumentNullException(nameof(instance));
    }

    var type = (instance as Type) ?? instance.GetType();
    var cache = MemoryCache.Default;
    var key = typeof(StateData).FullName + ":" + type.AssemblyQualifiedName;

    if (cache[key] is not Dictionary<string, PropertyDescriptor> properties)
    {
      properties = new(StringComparer.InvariantCultureIgnoreCase);

      var typeProperties = TypeDescriptor.GetProperties(type);

      for(var i = 0; i < typeProperties.Count; ++i)
      {
        PropertyDescriptor property = typeProperties[i];

        if (property.Attributes[typeof(StateAttribute)] is 
          StateAttribute attribute)
        {
          properties[attribute.Name ?? property.Name] = property;
        }
      }

      cache.Add(key, properties, cachePolicy);
    }

    return properties;
  }

  /// <summary>
  /// A cache policy.
  /// </summary>
  private static readonly CacheItemPolicy cachePolicy =
    new() { SlidingExpiration = new TimeSpan(0, 10, 0) };
}
