using System;

namespace Bphx.Cool.Impl;

/// <summary>
/// Defines a facility to query a service.
/// </summary>
static class ServiceProvider
{
  /// <summary>
  /// Queries a specified service.
  /// </summary>
  /// <typeparam name="T">a service type.</typeparam>
  /// <returns>
  /// An instance of a specified service, 
  /// or null if a service is not available.
  /// </returns>
  public static T GetService<T>(this IServiceProvider serviceProvider) =>
    (T)serviceProvider.GetService(typeof(T));
}
