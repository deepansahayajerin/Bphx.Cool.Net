using System;

namespace Bphx.Cool;

/// <summary>
/// An interface to set communication between Proxy and implementation.
/// </summary>
public interface IProxyStub
{
  /// <summary>
  /// Executes a proxy.
  /// </summary>
  /// <param name="context">A <see cref="IContext"/> instance.</param>
  /// <param name="actionType">A type of proxy.</param>
  /// <param name="import">An import instance.</param>
  /// <param name="export">An export instance..</param>
  void Execute(
    IContext context,
    Type actionType,
    object import,
    object export);
}

/// <summary>
/// Extension methods for <see cref="IProxyStub"/>.
/// </summary>
public static class ProxyStub
{
  /// <summary>
  /// Execute a proxy.
  /// </summary>
  /// <typeparam name="A">A proxy interface.</typeparam>
  /// <typeparam name="I">An import type.</typeparam>
  /// <typeparam name="E">An export type.</typeparam>
  /// <typeparam name="R">A response type.</typeparam>
  /// <param name="proxyStub">An <see cref="IProxyStub"/> instance.</param>
  /// <param name="context">A <see cref="IContext"/> instance.</param>
  /// <param name="import">An import instance.</param>
  /// <param name="export">An export instance.</param>
  public static void Execute<A, I, E>(
    this IProxyStub proxyStub,
    IContext context,
    I import,
    E export)
  {
    proxyStub.Execute(context, typeof(A), import, export);
  }
}
