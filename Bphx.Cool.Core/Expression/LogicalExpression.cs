using System;
using System.Diagnostics;

namespace Bphx.Cool.Expression;

/// <summary>
///  A logical expression.
/// </summary>
public class LogicalExpression : IExpression
{
  /// <summary>
  /// Expression operator.
  /// </summary>
  public readonly Operator Operator;

  /// <summary>
  /// First parameter.
  /// </summary>
  public readonly IExpression Param1;

  /// <summary>
  /// Second parameter.
  /// </summary>
  public readonly IExpression Param2;

  /// <summary>
  /// Creates a LogicalExpression instance.
  /// </summary>
  /// <param name="anOperator">an expression operator.</param>
  /// <param name="param1">an expression's first parameter.</param>
  /// <param name="param2">an expression's second parameter.</param>
  public LogicalExpression(
    Operator anOperator,
    IExpression param1,
    IExpression param2)
  {
    Debug.Assert(
      (anOperator == Operator.Not) ||
      (anOperator == Operator.And) ||
      (anOperator == Operator.Or));

    if((param2 == null) != (anOperator == Operator.Not))
    {
      throw new ArgumentException("Invalid param.", nameof(param2));
    }

    this.Operator = anOperator;
    this.Param1 = param1 ?? throw new ArgumentNullException(nameof(param1));
    this.Param2 = param2;
  }

  /// <summary>Evaluates a logical expression.</summary>
  /// <typeparam name="T">
  /// a type of bean defining expression context.
  /// </typeparam>
  /// <param name="resolver">a property resolver.</param>
  /// <param name="value">a context instance.</param>
  /// <returns>an expression value.</returns>
  public object Evaluate<T>(Func<T, int, object> resolver, T value)
  {
    bool result = (bool)Param1.Evaluate<T>(resolver, value);

    switch(Operator)
    {
      case Operator.Not:
      {
        return !result;
      }
      case Operator.And:
      {
        return result && (bool)Param2.Evaluate<T>(resolver, value);
      }
      case Operator.Or:
      {
        return result || (bool)Param2.Evaluate<T>(resolver, value);
      }
      default:
      {
        throw new InvalidOperationException();
      }
    }
  }
}
