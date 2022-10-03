using System;
using System.Collections.Generic;

namespace Bphx.Cool.Expression;

/// <summary>
/// A class to support sorting on expression string.
/// </summary>
/// <typeparam name="T">
/// A type of class whose properties are resolved.
/// </typeparam>
public class SortExpression<T>: IComparer<T>
{
  /// <summary>
  /// Creates an SortExpression instance.
  /// </summary>
  /// <param name="expression">
  ///   <para>a sort expression.</para>
  ///   <para>Expression syntax is:</para>
  ///   <para>
  ///     expression:
  ///       expression-part+;
  ///     expression-part:
  ///       index order;
  ///   </para>
  ///   <para>
  ///     index: INTEGER;
  ///     order: a | A | d | D;
  /// </para>
  /// <para>Comma character is considered as a space.</para>
  /// </param>
  /// <param name="getter">A property getter.</param>
  /// <param name="collator">Optional string collator.</param>
  public SortExpression(
    string expression,
    Func<T, int, object> getter,
    IComparer<string> collator = null)
  {
    if (Functions.Equal(expression, ""))
    {
      throw new ArgumentException(
        "Invalid sort expression",
        nameof(expression));
    }

    var tokens = expression.Split(
      new[] { ' ', '\t', '\n', '\r', '\f', ',' },
      StringSplitOptions.RemoveEmptyEntries);

    var sorts = new List<Sort>();

    for(int i = 0, c = tokens.Length - 1; i < c; i += 2)
    {
      var index = Convert.ToInt32(tokens[i]);
      var ascending = true;
      var order = tokens[i + 1];

      if (order.Length != 1)
      {
        throw new ArgumentException(
          "Invalid sort expression",
          nameof(expression));
      }

      switch(order[0])
      {
        case 'a':
        case 'A':
        {
          break;
        }
        case 'd':
        case 'D':
        {
          ascending = false;

          break;
        }
        default:
        {
          throw new ArgumentException(
            "Invalid expression",
            nameof(expression));
        }
      }

      sorts.Add(new() { index = index, ascending = ascending });
    }

    this.collator = collator;
    this.getter = getter ?? 
      throw new ArgumentNullException(nameof(getter));
    this.sorts = sorts.ToArray();
  }

  #region IComparer<T> Members
  /// <summary>
  /// Compares its two arguments for order.  
  /// Returns a negative integer,zero, or a positive integer as the first
  /// argument is less than, equal to, or greater than the second.
  /// </summary>
  /// <param name="first">a first object to be compared.</param>
  /// <param name="second">a second object to be compared.</param>
  /// <returns>
  /// A negative integer, zero, or a positive integer as the first 
  /// argument is less than, equal to, or greater than the second.
  /// </returns>
  public int Compare(T first, T second)
  {
    foreach(var sort in sorts)
    {
      var firstValue = getter(first, sort.index);
      var secondValue = getter(second, sort.index);

      var c = (firstValue is string firstString) && 
        (secondValue is string secondString) ?
        Functions.Compare(firstString, secondString, collator) :
        (firstValue is byte[] firstBytes) &&
          (secondValue is byte[] secondBytes) ?
          Functions.Compare(firstBytes, secondBytes) :
          Functions.Compare(
            firstValue as IComparable, 
            secondValue as IComparable);

      if (c != 0)
      {
        return sort.ascending ? c : -c;
      }
    }

    return 0;
  }
  #endregion

  /// <summary>
  /// A sort element.
  /// </summary>
  private struct Sort
  {
    /// <summary>
    /// Sort property index.
    /// </summary>
    public int index;

    /// <summary>
    /// Flag defining sort order: true - ascending, false descending.
    /// </summary>
    public bool ascending;
  }

  /// <summary>
  /// A sort list.
  /// </summary>
  private readonly Sort[] sorts;

  /// <summary>
  /// A string collator.
  /// </summary>
  private readonly IComparer<string> collator;

  /// <summary>
  /// A property getter.
  /// </summary>
  private readonly Func<T, int, object> getter;
}
