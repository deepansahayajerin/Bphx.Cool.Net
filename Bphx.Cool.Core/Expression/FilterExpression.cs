using System;
using System.Diagnostics;

using static Bphx.Cool.Functions;

namespace Bphx.Cool.Expression;

/// <summary>
/// Defines a filter expression.
/// </summary>
/// <typeparam name="T">
/// A type of class whose properties are resolved.
/// </typeparam>
public class FilterExpression<T>
{
  /// <summary>
  /// Create a FilterExpression instance.
  /// </summary>
  /// <param name="expression">an expression for the filter.</param>
  /// <param name="getter">An attribute getter.</param>
  public FilterExpression(
    string expression,
    Func<T, int, object> getter)
  {
    if(!Equal(expression, ""))
    {
      try
      {
        var parser = new Parser(expression);

        this.expression = parser.Parse();
      }
      catch
      {
        // Treat invalid expression as empty.
        Trace.TraceError(
          $@"Invalid filter expression ""{expression}""",
          "DEBUG");
      }
    }

    this.getter = getter;
  }

  /// <summary>
  /// Tests whether a value fits for the filter expression.
  /// </summary>
  /// <param name="value">a value to test.</param>
  /// <returns>true if value fits, and false otherwise.</returns>
  public bool Test(T value) => 
    (expression == null) || true.Equals(expression.Evaluate(getter, value));

  /// <summary>
  /// An <see cref="IExpression"/> instance.
  /// </summary>
  public readonly IExpression expression;

  /// <summary>
  /// An attribute getter.
  /// </summary>
  private readonly Func<T, int, object> getter;
}
