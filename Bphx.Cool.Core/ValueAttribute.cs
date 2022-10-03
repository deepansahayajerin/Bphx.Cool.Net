using System;
using System.ComponentModel;
using System.Collections;
using System.Xml;
using System.Linq;

using static Bphx.Cool.Functions;

namespace Bphx.Cool
{
  /// <summary>
  /// An attribute defining a permitted value.
  /// </summary>
  [AttributeUsage(
    AttributeTargets.Property,
    Inherited = true,
    AllowMultiple = true)]
  public class ValueAttribute: System.Attribute
  {
    /// <summary>
    /// Creates a <see cref="ValueAttribute"/> instance.
    /// </summary>
    /// <param name="value">a permitted value.</param>
    public ValueAttribute(object value)
    {
      LowValue = value;
      HighValue = value;
    }

    /// <summary>
    /// Creates a <see cref="ValueAttribute"/> instance.
    /// </summary>
    /// <param name="lowValue">A low permitted value.</param>
    /// <param name="highValue">A high permitted value.</param>
    public ValueAttribute(object lowValue, object highValue)
    {
      LowValue = lowValue;
      HighValue = highValue;
    }

    /// <summary>
    /// Type id is unique per instance.
    /// </summary>
    public override object TypeId => this;

    /// <summary>
    /// A value.
    /// </summary>
    public object Value => LowValue == HighValue ? LowValue : null;

    /// <summary>
    /// A low value.
    /// </summary>
    public object LowValue { get; }

    /// <summary>
    /// A high value.
    /// </summary>
    public object HighValue { get; }

    /// <summary>
    /// Gets list of permitted values.
    /// </summary>
    /// <param name="property">A property descriptor.</param>
    /// <returns>A collection of values.</returns>
    public static ValueAttribute[] GetValues(PropertyDescriptor property) =>
      property.Attributes.OfType<ValueAttribute>().ToArray();

    /// <summary>
    /// Tests whether the value is valid for the property.
    /// </summary>
    /// <param name="property">A property descriptor.</param>
    /// <param name="value">A value to test.</param>
    /// <returns>true if value is valid, and false otherwise.</returns>
    public static bool IsValid(PropertyDescriptor property, object value)
    {
      try
      {
        var comparer = Comparer.Default;
        var type = property.PropertyType;
        var hasValues = false;

        value = Convert(value, property.PropertyType);

        var comparable = value as IComparable;

        foreach(var attribute in property.Attributes)
        {
          if (attribute is ValueAttribute valueAttribute)
          {
            hasValues = true;

            if (valueAttribute.LowValue == null)
            {
              if (value is string stringValue ? 
                IsEmpty(stringValue) : IsNullOrZero(comparable))
              {
                return true;
              }
            }
            else if (valueAttribute.LowValue == valueAttribute.HighValue)
            {
              if (comparer.Compare(
                Convert(valueAttribute.LowValue, type), 
                value) == 0)
              {
                return true;
              }
            }
            else
            {
              if ((comparer.Compare(
                  Convert(valueAttribute.LowValue, type),
                  value) >= 0) &&
                (comparer.Compare(
                  Convert(valueAttribute.HighValue, type),
                  value) <= 0))
              {
                return true;
              }
            }
          }
        }

        return !hasValues;
      }
      catch
      {
        return false;
      }
    }

    /// <summary>Converts a value to a specified type.</summary>
    /// <param name="value">A value to convert.</param>
    /// <param name="type">A type to convert to.</param>
    /// <returns>Converted value.</returns>
    public static object Convert(object value, Type type)
    {
      if ((value == null) || (type == null))
      {
        return value;
      }

      type = Nullable.GetUnderlyingType(type) ?? type;

      if (value is string stringValue)
      {
        stringValue = stringValue.TrimEnd();

        if (type == typeof(DateTime))
        {
          return XmlConvert.ToDateTime(
            stringValue,
            XmlDateTimeSerializationMode.Unspecified);
        }
        else if (type == typeof(TimeSpan))
        {
          return XmlConvert.ToTimeSpan(stringValue);
        }
      }

      return System.Convert.ChangeType(value, type);
    }
  }
}