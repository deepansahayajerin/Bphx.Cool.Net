using System;

namespace Bphx.Cool;

/// <summary>
/// A profiler for dynamic program analysis,
/// tracking usage of particular generated classes,
/// record and execute tests, measure execution metrics etc.
/// </summary>
public interface IProfiler : IDisposable
{
  /// <summary>
  /// <para>
  /// If <c><see cref="Playback"/> == true</c> then this method 
  /// returns next recorder <see cref="IProfiler"/> instance that 
  /// should have the same type and code. If no matching scope is 
  /// found then <c>null</c> is returned.
  /// </para>
  /// <para>
  /// If <c><see cref="Playback"/> == false</c> then this method creates 
  /// a scope within this <see cref="IProfiler"/> instance.
  /// </para>
  /// </summary>
  /// <param name="type">A scope type.</param>
  /// <param name="name">A scope name.</param>
  /// <returns>A <see cref="IProfiler"/> instance for the scope.</returns>
  IProfiler GetScope(string type, string name);

  /// <summary>
  /// <para>Logs a value.</para>
  /// <para>
  /// <b>Note:</b> this method may be called once, and only before
  /// <see cref="GetScope(string, string)"/>.
  /// </para>
  /// </summary>
  /// <param name="value">A value</param>
  /// <returns>A value.</returns>
  object ScopeValue(object value);

  /// <summary>
  /// A playback indicator.
  /// <c>true</c> for playback mode when some data are retrieved or 
  /// validated from <see cref="IProfiler"/>, and <c>false</c> for tracing.
  /// </summary>
  bool Playback => false;
}

/// <summary>
/// Extension methods for <see cref="IProfiler"/> type.
/// </summary>
public static class ProfilerExtensions
{
  /// <summary>
  /// <para>
  /// If<c></c><see cref="IProfiler.Playback"/> == true</c> then  this 
  /// method returns next recorded <see cref="IProfiler"/> instance that
  /// should have the same <c>type</c> and <c>name</c>. If no matching scope
  /// is found then <c>null</c> is returned.
  /// </para> 
  /// <para>
  /// If <c><see cref="IProfiler.Playback"/> == false</c> then this method
  /// creates a scope within this <see cref="IProfiler"/> instance.
  /// </para>
  /// </summary>
  /// <param name="profiler">A <see cref="IProfiler"/> instance.</param>
  /// <param name="type">A scope type.</param>
  /// <param name="name">A scope name.</param>
  /// <returns>A <see cref="IProfiler"/> instance for the scope.</returns>
  public static IProfiler Scope(
    this IProfiler profiler,
    string type,
    string name) => 
    profiler?.GetScope(type, name);

  /// <summary>
  /// Creates a scope to wrap a method call.
  /// </summary>
  /// <param name="profiler">A <see cref="IProfiler"/> instance.</param>
  /// <param name="type">A scope type.</param>
  /// <param name="instance">An instance, type or string value.</param>
  /// <param name="method">A method name.</param>
  /// <returns>A <see cref="IProfiler"/> instance for the scope.</returns>
  public static IProfiler Scope(
    this IProfiler profiler,
    string type,
    object instance,
    string method) => 
    profiler?.GetScope(
      type,
      (instance is string stringValue ? stringValue :
        instance is Type typeValue ? typeValue.FullName :
        instance?.GetType().FullName) +
          "." + method);

  /// <summary>
  /// Returns <c>true</c> for playback mode, and  <c>false</c> otherwise.
  /// </summary>
  /// <param name="profiler">A <see cref="IProfiler"/> instance.</param>
  /// <returns>A playback mode.</returns>
  public static bool Playback(this IProfiler profiler) => 
    profiler?.Playback == true;

  /// <summary>
  /// Logs a value.
  /// </summary>
  /// <typeparam name="T">A value type.</typeparam>
  /// <param name="profiler">A <see cref="IProfiler"/> instance.</param>
  /// <param name="value">A value.</param>
  /// <returns>A value.</returns>
  public static T Value<T>(this IProfiler profiler, T value) => 
    profiler == null ? value : (T)profiler.ScopeValue(value);

  /// <summary>
  /// Creates a <see cref="IProfiler"/> scope, assign a value to it, 
  /// closes it, and then returns a value.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="profiler">A <see cref="IProfiler"/> instance.</param>
  /// <param name="type">A scope type.</param>
  /// <param name="name">A scope name.</param>
  /// <param name="value">A value.</param>
  /// <returns>A value.</returns>
  public static T Value<T>(
    this IProfiler profiler,
    string type,
    string name,
    T value)
  {
    if (profiler == null)
    {
      return value;
    }

    using var scope = profiler.GetScope(type, name);

    return scope.Value(value);
  }
}
