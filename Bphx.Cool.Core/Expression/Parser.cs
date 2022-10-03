using System;
using System.Text.RegularExpressions;

namespace Bphx.Cool.Expression;

/// <summary>
///  An expression parser.
/// </summary>
public class Parser: PatternTokenizer
{
  /// <summary>
  /// Creates an instance of the expression parser.
  /// </summary>
  public Parser(): base(tokens)
  {
  }

  /// <summary>
  /// Creates an instance of the expression parser.
  /// </summary>
  /// <param name="expression">an expression to parse.</param>
  public Parser(string expression): base(tokens) => Reset(expression);

  /// <summary>
  /// Parses an expression and verifies that end of token stream is reached.
  /// </summary>
  /// <returns>a parsed expression.</returns>
  public IExpression Parse()
  {
    var expression = ParseExpression();

    Eat(Eof);

    return expression;
  }

  /// <summary>
  /// Parses an expression.
  /// </summary>
  /// <param name="expression">an expression to parse.</param>
  /// <returns>a parsed expression.</returns>
  public IExpression Parse(string expression)
  {
    if (expression == null)
    {
      throw new ArgumentNullException(nameof(expression));
    }

    Reset(expression);

    return ParseExpression();
  }

  /// <summary>
  ///   <para>Parses an expression.</para>
  ///   <para>
  ///     expression:
  ///       not-expression;
  ///   </para>
  /// </summary>
  /// <returns>a parsed expression.</returns>
  public IExpression ParseExpression() => ParseOrExpression();

  /// <summary>
  ///   <para>Parses an OR expression.</para>
  ///   <para>
  ///     or-expression:
  ///       and-expression |
  ///       or-expression 'or' and-expression;
  ///   </para>
  /// </summary>
  /// <returns>a parsed expression.</returns>
  public IExpression ParseOrExpression()
  {
    var param1 = ParseAndExpression();

    while(Test(Or))
    {
      Eat();

      var param2 = ParseOrExpression();

      param1 = new LogicalExpression(Operator.Or, param1, param2);
    }

    return param1;
  }

  /// <summary>
  ///   <para>Parses an AND expression.</para>
  ///   <para>
  ///     and-expression:
  ///       parenthesis-expression |
  ///       and-expression 'and' parenthesis-expression;
  ///   </para>
  /// </summary>
  /// <returns>a parsed expression.</returns>
  public IExpression ParseAndExpression()
  {
    var param1 = ParseParenthesisExpression();

    while(Test(And))
    {
      Eat();

      var param2 = ParseAndExpression();

      param1 = new LogicalExpression(Operator.And, param1, param2);
    }

    return param1;
  }

  /// <summary>
  ///   <para>Parses a parenthesis-expression.</para>
  ///   <para>
  ///     parenthesis-expression:
  ///       relational-expression |
  ///       '(' relational-expression ')';
  ///   </para>
  /// </summary>
  /// <returns>a parsed expression.</returns>
  public IExpression ParseParenthesisExpression()
  {
    if (Test(OpenBrace))
    {
      Eat();

      var result = ParseExpression();

      Eat(CloseBrace);

      return result;
    }
    else
    {
      return ParseRelationalExpression();
    }
  }

  /// <summary>
  ///   <para>Parses a relational-expression.</para>
  ///   <para>
  ///     relational-expression:
  ///       attribute-index '=' value |
  ///       attribute-index '>' value |
  ///       attribute-index '&lt;' value |
  ///       attribute-index '>=' value |
  ///       attribute-index '&lt;=' value |
  ///       attribute-index '&lt;>' value;
  ///   </para>
  /// </summary>
  /// <returns>a parsed expression.</returns>
  public IExpression ParseRelationalExpression()
  {
    Verify(Number);

    var index = Convert.ToInt32(Value);

    Eat();

    Operator Operator;

    if (Test(IsEqual))
    {
      Eat();
      Operator = Operator.Equals;
    }
    else if (Test(GreaterThan))
    {
      Eat();
      Operator = Operator.GreaterThan;
    }
    else if (Test(LessThan))
    {
      Eat();
      Operator = Operator.LessThan;
    }
    else if (Test(GreaterThanOrEquals))
    {
      Eat();
      Operator = Operator.GreaterThanOrEqauls;
    }
    else if (Test(LessThanOrEquals))
    {
      Eat();
      Operator = Operator.LessThanOrEquals;
    }
    else
    {
      Eat(NotEquals);
      Operator = Operator.NotEquals;
    }

    string stringValue;

    switch(TokenType)
    {
      case Plus:
      {
        Eat();
        Verify(Number);
        stringValue = Value;

        break;
      }
      case Minus:
      {
        Eat();
        Verify(Number);
        stringValue = "-" + Value;

        break;
      }
      case Date1:
      {
        stringValue = Value.Replace('/', '-');
        Eat();

        break;
      }
      case Time2:
      {
        stringValue = Value.Replace('.', ':');
        Eat();

        break;
      }
      case And:
      case Or:
      case Not:
      case Null:
      case Identifier:
      case String1:
      case String2:
      case Number:
      case Date2:
      case Time1:
      {
        stringValue = Value;
        Eat();

        break;
      }
      default:
      {
        // Issue an error.
        stringValue = null;
        Verify(String1);

        break;
      }
    }

    return new ComparisionExpression(Operator, index, stringValue);
  }

  /// <summary>
  /// A pattern to match tokens.
  /// </summary>
  public static readonly Regex tokens =
    new(
      "(\\s+)|" +                           // 1. Space
      "(<>)|" +                             // 2. NotEquals
      "(<=)|" +                             // 3. LessThanOrEquals
      "(<)|" +                              // 4. LessThan
      "(>=)|" +                             // 5. GreaterThanOrEquals
      "(>)|" +                              // 6. GreaterThan
      "(=)|" +                              // 7. Equals
      "(\\()|" +                            // 8. OpenBrace
      "(\\))|" +                            // 9. CloseBrace
      "(\\*)|" +                            // 10. Asterisk
      "(/)|" +                              // 11. Slash
      "(\\+)|" +                            // 12. Plus
      "(-)|" +                              // 13. Minus
      "(and)|" +                            // 14. And
      "(or)|" +                             // 15. Or
      "(not)|" +                            // 16. Not
      "(null)|" +                           // 17. Null
      "(?:\"((?:\\\\\"|.)*?)?\")|" +        // 18. String
      "(?:'((?:\\\\'|.)*?)?')|" +           // 19. String
      "((?:\\p{L}|_)(?:\\p{L}|_|\\d)*)|" +  // 20. Identifier
      "(\\d{4}/\\d{1,2}/\\d{1,2})|" +       // 21. Date1
      "(\\d{4}-\\d{1,2}-\\d{1,2})|" +       // 22. Date2
      "(\\d{1,2}:\\d{1,2}:\\d{1,2})|" +     // 23. Time1
      "(\\d{1,2}\\.\\d{1,2}\\.\\d{1,2})|" + // 24. Time2
      "(\\d*\\.?\\d+(?:[eE][-+]?\\d+)?)|" + // 25. Number
      "(.)",                                // 26. Unknown
      RegexOptions.Compiled | RegexOptions.Multiline |
      RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

  // Tokens

  /**
   * Unknown token.
   */
  public const int Unknown = -1;

  /**
   * Token marking end of input.
   */
  public const int Eof = 0;

  /**
   * Token to mark space.
   */
  public const int Space = 1;

  /**
   * Token to mark "<>" operator.
   */
  public const int NotEquals = 2;

  /**
   * Token to mark "<=" operator.
   */
  public const int LessThanOrEquals = 3;

  /**
   * Token to mark "<" operator.
   */
  public const int LessThan = 4;

  /**
   * Token to mark ">=" operator.
   */
  public const int GreaterThanOrEquals = 5;

  /**
   * Token to mark ">" operator.
   */
  public const int GreaterThan = 6;

  /**
   * Token to mark "=" operator.
   */
  public const int IsEqual = 7;

  /**
   * Token to mark "(" open parenthesis.
   */
  public const int OpenBrace = 8;

  /**
   * Token to mark "(" close parenthesis.
   */
  public const int CloseBrace = 9;

  /**
   * Token to mark "*".
   */
  public const int Asterisk = 10;

  /**
   * Token to mark "/".
   */
  public const int Slash = 11;

  /**
   * Token to mark "+".
   */
  public const int Plus = 12;

  /**
   * Token to mark "-".
   */
  public const int Minus = 13;

  /**
   * Token to mark "and" operator.
   */
  public const int And = 14;

  /**
   * Token to mark "or" operator.
   */
  public const int Or = 15;

  /**
   * Token to mark "not" operator.
   */
  public const int Not = 16;

  /**
   * Token to mark "null" literal.
   */
  public const int Null = 17;

  /**
   * Token to mark string literal.
   */
  public const int String1 = 18;

  /**
   * Token to mark string literal.
   */
  public const int String2 = 19;

  /**
   * Token to mark identifier.
   */
  public const int Identifier = 20;

  /**
   * Token to mark date literal in the format yyyy/mm/dd.
   */
  public const int Date1 = 21;

  /**
   * Token to mark date literal in the format yyyy-mm-dd.
   */
  public const int Date2 = 22;

  /**
   * Token to mark time literal in the format hh:mm:ss.
   */
  public const int Time1 = 23;

  /**
   * Token to mark time literal in the format hh.mm.ss.
   */
  public const int Time2 = 24;

  /**
   * Token to mark number or timestamp literals.
   */
  public const int Number = 25;
}
