using System;
using System.ComponentModel;

namespace Bphx.Cool;

/// <summary>
/// An object property.
/// </summary>
/// <typeparam name="T">A property type.</typeparam>
public class Property<T>
{
  /// <summary>
  /// Creates a <see cref="Property{I, T}"/> instance.
  /// </summary>
  /// <param name="instance">An instance.</param>
  /// <param name="name">A property name.</param>
  public Property(IAttributes instance, string name)
  {
    Instance = instance;
    Name = name;
  }

  /// <summary>
  /// Creates a <see cref="Property{I, T}"/> instance.
  /// </summary>
  /// <param name="instance">An instance.</param>
  /// <param name="name">A property name.</param>
  /// <param name="arguments">A property arguments.</param>
  public Property(
    IAttributes instance,
    string name,
    params object[] arguments)
  {
    Instance = instance;
    Name = name;
    Arguments = arguments;
  }

  /// <summary>
  /// A reference instance.
  /// </summary>
  public IAttributes Instance { get; }

  /// <summary>
  /// A property name.
  /// </summary>
  public string Name { get; }


  /// <summary>
  /// A reference instance.
  /// </summary>
  public object[] Arguments { get; }

  /// <summary>
  /// A property value.
  /// </summary>
  public T Value
  {
    get
    {
      if (Instance == null)
      {
        return default;
      }

      if (Arguments?.Length > 0)
      {
        return Instance is IInvocable invocable ?
          invocable.Invoke<T>(Name, Arguments) :
          default;
      }

      var property = TypeDescriptor.GetProperties(Instance)[Name];
      var value = property == null ?
        Instance.Attributes.Get(Name) : property.GetValue(Instance);

      if (value == null)
      {
        return default;
      }
      else if ((value.GetType() == typeof(bool)) &&
        (typeof(T) == typeof(string)))
      {
        return (T)(object)((bool)value ? "True" : "False");
      }
      else
      {
        try
        {
          return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
          System.Diagnostics.Trace.Write(
            $@"Cannot get property {Name} of type {
              typeof(T).Name} of instance of type {
              Instance.GetType().Name}.",
            "DEBUG");

          return default;
        }
      }
    }
    set
    {
      if (Instance == null)
      {
        return;
      }

      try
      {
        if (Arguments?.Length > 0)
        {
          if (Instance is IInvocable invocable)
          {
            var parameters = new object[Arguments.Length + 1];

            parameters[0] = value;
            Array.Copy(Arguments, 0, parameters, 1, Arguments.Length);

            invocable.Invoke<T>("Set" + Name, parameters);
          }

          return;
        }

        var property = TypeDescriptor.GetProperties(Instance)[Name];

        if (property != null)
        {
          property.SetValue(Instance, Convert.ChangeType(value, typeof(T)));
        }
        else if (Instance is ISetAttribute setter)
        {
          setter.SetAttribute(Name, Convert.ChangeType(value, typeof(T)));
        }
        else if (value == null)
        {
          Instance.Attributes.Remove(Name);
        }
        else
        {
          Instance.Attributes[Name] = Convert.ChangeType(value, typeof(T));
        }
      }
      catch
      {
        System.Diagnostics.Trace.Write(
          $@"Cannot set property {Name} of instance of type {
            Instance.GetType().Name} to value {value}.",
          "DEBUG");
      }
    }
  }
}

/// <summary>
/// Extension methods to support properties.
/// </summary>
public static class PropertyExtensions
{
  /// <summary>
  /// Gets <see cref="Property"/> instance.
  /// </summary>
  /// <typeparam name="T">A property type.</typeparam>
  /// <param name="instance">A property instance.</param>
  /// <param name="name">A property name.</param>
  /// <returns>A <see cref="Property"/> instance.</returns>
  public static Property<T> Get<T>(this IAttributes instance, string name)
  {
    return new(instance, name);
  }

  /// <summary>
  /// Gets <see cref="Property"/> instance.
  /// </summary>
  /// <typeparam name="T">A property type.</typeparam>
  /// <param name="instance">A property instance.</param>
  /// <param name="name">A property name.</param>
  /// <param name="arguments">A property arguments.</param>
  /// <returns>A <see cref="Property"/> instance.</returns>
  public static Property<T> Get<T>(
    this IAttributes instance,
    string name,
    params object[] arguments)
  {
    return new(instance, name, arguments);
  }

  /// <summary>
  /// Gets a string <see cref="Property"/> instance.
  /// </summary>
  /// <param name="instance">A property instance.</param>
  /// <param name="name">A property name.</param>
  /// <returns>A string <see cref="Property"/> instance.</returns>
  public static Property<string> Get(this IAttributes instance, string name)
  {
    return new(instance, name);
  }

  /// <summary>
  /// Gets a string <see cref="Property"/> instance.
  /// </summary>
  /// <param name="instance">A property instance.</param>
  /// <param name="name">A property name.</param>
  /// <param name="arguments">A property arguments.</param>
  /// <returns>A string <see cref="Property"/> instance.</returns>
  public static Property<string> Get(
    this IAttributes instance,
    string name,
    params object[] arguments)
  {
    return new(instance, name, arguments);
  }
}
