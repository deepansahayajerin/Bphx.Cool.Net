using System;

namespace Bphx.Cool;

/// <summary>
/// A factory provider service.
/// </summary>
public interface IFactoryProvider
{
  /// <summary>
  /// Gets a factory for a type.
  /// </summary>
  /// <param name="type">A type to get factory for.</param>
  /// <returns>
  /// A delegate to create an instance of the type.
  /// Delegate is of the form <see cref="Func{T, TResult}"/>, 
  /// <see cref="Func{T1, T2, TResult}"/>, and so on.
  /// </returns>
  Delegate GetFactory(Type type);
}

/// <summary>
/// An API to supporty <see cref="IFactoryProvider"/>.
/// </summary>
public static class FactoryProvider
{
  /// <summary>
  /// Gets a factory for a type.
  /// </summary>
  /// <typeparam name="T">A type to get factory for,</typeparam>
  /// <typeparam name="Arg1">
  /// A type of the factory first parameter.
  /// </typeparam>
  /// <typeparam name="Arg2">
  /// A type of the factory second parameter.
  /// </typeparam>
  /// <param name="provider">
  /// A <see cref="IFactoryProvider"/> instance.
  /// </param>
  /// <returns>A factory instance.</returns>
  public static Func<IContext, Arg1, Arg2, T> GetFactory<Arg1, Arg2, T>(
    this IFactoryProvider provider)
  {
    return (Func<IContext, Arg1, Arg2, T>)provider.GetFactory(typeof(T));
  }
}
