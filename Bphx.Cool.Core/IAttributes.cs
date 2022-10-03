using System;
using System.Collections.Generic;
using System.Diagnostics;

using static Bphx.Cool.Functions;

namespace Bphx.Cool;

/// <summary>
/// Provides a map of attributes.
/// </summary>
public interface IAttributes
{
  /// <summary>
  /// Gets attributes map.
  /// </summary>
  Dictionary<string, object> Attributes { get; }
}

/// <summary>
/// An interface to set attribute value.
/// </summary>
public interface ISetAttribute
{
  /// <summary>
  /// Sets attribute value.
  /// </summary>
  /// <param name="name">An attribute name.</param>
  /// <param name="value">An attribute value.</param>
  void SetAttribute(string name, object value);
}

/// <summary>
/// Interface to dynamically invoke methods.
/// </summary>
public interface IInvocable
{
  /// <summary>
  /// Invokes a method.
  /// </summary>
  /// <param name="name">A method name.</param>
  /// <param name="args">Method's arguments.</param>
  /// <returns>Result value.</returns>
  object Invoke(string name, params object[] args);
}

/// <summary>
/// Extensions to dynamically invoke methods.
/// </summary>
public static class InvocableExtensions
{
  /// <summary>
  /// Invokes a method of <see cref="IInvocable"/> instance.
  /// </summary>
  /// <param name="instance">an instance to call method for.</param>
  /// <param name="name">A method name.</param>
  /// <param name="parameters">Method's parameters.</param>
  /// <returns>A result instance or null.</returns>
  public static IInvocable Invoke(
    this IInvocable instance,
    string name,
    params object[] parameters) => 
    instance.Invoke<IInvocable>(name, parameters);

  /// <summary>
  /// Invokes a method of <see cref="IInvocable"/> instance.
  /// </summary>
  /// <typeparam name="R">A return type.</typeparam>
  /// <param name="instance">an instance to call method for.</param>
  /// <param name="name">A method name.</param>
  /// <param name="parameters">Method's parameters.</param>
  /// <returns>Result type.</returns>
  public static R Invoke<R>(
    this IInvocable instance,
    string name,
    params object[] parameters)
  {
    if (instance == null)
    {
      Trace.Write($"Call {name} over null object.", "DEBUG");

      return default;
    }

    try
    {
      return (R)Convert.ChangeType(instance.Invoke(name, parameters), typeof(R));
    }
    catch(NotSupportedException)
    {
      var objectName = (instance as INamed)?.Name;

      Trace.Write(
        $@"Call {name} on {instance.GetType().Name}{
          (IsEmpty(objectName) ? "" : "(" + objectName + ")")
          } is not supported with parameters: [{
          string.Join(", ", parameters)}].",
        "DEBUG");

      return default;
    }
  }
}
