using System;
using System.Diagnostics;

namespace Bphx.Cool.Expression;

/// <summary>
///  A comparision expression.
/// </summary>
public sealed class ComparisionExpression : IExpression
{
  /// <summary>
  /// Expression operator.
  /// </summary>
  public readonly Operator Operator;

  /// <summary>
  /// Property index.
  /// </summary>
  public readonly int PropertyIndex;

  /// <summary>
  /// A string value to compare against.
  /// </summary>
  public readonly string StringValue;

  /// <summary>
  /// Creates a ComparisionExpression instance.
  /// </summary>
  /// <param name="anOperator">an expression operator.</param>
  /// <param name="propertyIndex">a property index.</param>
  /// <param name="stringValue">a property value.</param>
  public ComparisionExpression(
    Operator anOperator,
    int propertyIndex,
    string stringValue)
  {
    Debug.Assert(
      (anOperator == Operator.Equals) ||
      (anOperator == Operator.GreaterThan) ||
      (anOperator == Operator.LessThan) ||
      (anOperator == Operator.GreaterThanOrEqauls) ||
      (anOperator == Operator.LessThanOrEquals) ||
      (anOperator == Operator.NotEquals));

    Operator = anOperator;
    PropertyIndex = propertyIndex;
    StringValue = stringValue ??
      throw new ArgumentNullException(nameof(stringValue));
  }

  /// <summary>Evaluates an expression.</summary>
  /// <typeparam name="T">
  /// a type of bean defining expression context.
  /// </typeparam>
  /// <param name="resolver">a property resolver.</param>
  /// <param name="value">a context instance.</param>
  /// <returns>an expression value.</returns>
  public object Evaluate<T>(Func<T, int, object> resolver, T value)
  {
    if (resolver(value, PropertyIndex) is not IComparable propertyValue)
    {
      return false;
    }

    if (!hasTypedValue)
    {
      if (propertyValue is string)
      {
        typedValue = StringValue;
      }
      else if (string.IsNullOrEmpty(StringValue))
      {
        typedValue = null;
      }
      else if (propertyValue is int)
      {
        typedValue = Convert.ToInt32(StringValue);
      }
      else if (propertyValue is double)
      {
        typedValue = Convert.ToDouble(StringValue);
      }
      else if (propertyValue is long)
      {
        typedValue = Convert.ToInt64(StringValue);
      }
      else if (propertyValue is short)
      {
        typedValue = Convert.ToInt16(StringValue);
      }
      else if (propertyValue is byte)
      {
        typedValue = Convert.ToByte(StringValue);
      }
      else if (propertyValue is float)
      {
        typedValue = float.Parse(StringValue);
      }
      else if (propertyValue is decimal)
      {
        typedValue = Convert.ToDecimal(StringValue);
      }
      else if (propertyValue is DateTime)
      {
        typedValue = Convert.ToDateTime(StringValue);
      }
      else if (propertyValue is TimeSpan)
      {
        typedValue = TimeSpan.Parse(StringValue);
      }
      else
      {
        throw new ArgumentException("Unsupported property type.");
      }

      hasTypedValue = true;
    }

    int c = Object.ReferenceEquals(typedValue, StringValue) ?
      Functions.Compare((string)propertyValue, StringValue) :
      Functions.Compare(propertyValue, typedValue);

    switch(Operator)
    {
      case Operator.Equals:
      {
        return c == 0;
      }
      case Operator.GreaterThan:
      {
        return c > 0;
      }
      case Operator.LessThan:
      {
        return c < 0;
      }
      case Operator.GreaterThanOrEqauls:
      {
        return c >= 0;
      }
      case Operator.LessThanOrEquals:
      {
        return c <= 0;
      }
      case Operator.NotEquals:
      {
        return c != 0;
      }
      default:
      {
        throw new InvalidOperationException();
      }
    }
  }

  /// <summary>
  /// Indicates whether the typed value is available.
  /// </summary>
  private bool hasTypedValue;

  /// <summary>
  /// A typed value that corresponds to a value.
  /// </summary>
  private IComparable typedValue;
}
