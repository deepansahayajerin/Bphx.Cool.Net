using System;
using System.ComponentModel;

namespace Bphx.Cool;

/// <summary>
/// An attribute defining an implicit value.
/// </summary>
[AttributeUsage(
  AttributeTargets.Property,
  Inherited = true,
  AllowMultiple = true)]
public class ImplicitValueAttribute : System.Attribute
{
  /// <summary>
  /// Creates a <see cref="ImplicitValueAttribute"/> instance.
  /// </summary>
  /// <param name="value">an implicit value.</param>
  public ImplicitValueAttribute(object value)
  {
    Value = value;
  }

  /// <summary>
  /// An implicit value.
  /// </summary>
  public object Value { get; private set; }

  /// <summary>
  /// Gets an implicit value for a property, if any.
  /// </summary>
  /// <param name="property">A property descriptor.</param>
  /// <returns>An implicit value for a property or null.</returns>
  public static object GetValue(PropertyDescriptor property)
  {
    try
    {
      foreach(var attribute in property.Attributes)
      {
        if(attribute is ImplicitValueAttribute implicitValueAttribute)
        {
          return ValueAttribute.Convert(
            implicitValueAttribute.Value,
            property.PropertyType);
        }
      }

      return null;
    }
    catch
    {
      return false;
    }
  }
}
