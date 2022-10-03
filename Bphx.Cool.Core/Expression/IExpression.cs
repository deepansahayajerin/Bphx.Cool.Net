using System;

namespace Bphx.Cool.Expression;

/// <summary>
/// Defines an expression.
/// </summary>
public interface IExpression
{
  /// <summary>Evaluates an expression.</summary>
  /// <typeparam name="T">
  /// A type of bean defining expression context.
  /// </typeparam>
  /// <param name="resolver">a property resolver.</param>
  /// <param name="value">a context instance.</param>
  /// <returns>an expression value.</returns>
  object Evaluate<T>(Func<T, int, object> resolver, T value);
}
