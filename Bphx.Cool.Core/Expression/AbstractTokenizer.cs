namespace Bphx.Cool.Expression;

/// <summary>
/// A basic abstract implementation of the ITokenizer interface.
/// </summary>
public abstract class AbstractTokenizer : ITokenizer
{
  /// <summary>
  /// Tests that the current token is of a specified type.
  /// </summary>
  /// <param name="tokenType">a token type to check.</param>
  /// <returns>true if check succeeds, and false otherwise.</returns>
  public bool Test(int tokenType) => this.TokenType == tokenType;

  /// <summary>
  /// Tests that the current token is of a specified value.
  /// </summary>
  /// <param name="token">a token value to check.</param>
  /// <returns>true if check succeeds, and false otherwise.</returns>
  public bool Test(string token) => 
    Value == null ? false :
    Value == token ? true :
    IsCaseInsensitive ? string.Compare(Value, token, true) == 0 : false;

  /// <summary>
  /// Verifies that the current token is of a specified type.
  /// If token is of different type ParseException is thrown.
  /// </summary>
  /// <param name="tokenType">a token type to verify.</param>
  /// <seealso cref="Bphx.Cool.Expresion.ParseException"/>
  public void Verify(int tokenType)
  {
    if (Test(tokenType))
    {
      return;
    }

    if (TokenType == 0)
    {
      throw Error(
        "Invalid token type 0 has occured, while token of type " +
        tokenType + " is expected.",
        Text.Length);
    }
    else
    {
      throw Error(
        "Invalid token \"" + Value +
        "\" has occured in the expression, while token of type " +
        tokenType + " is expected.",
        -1);
    }
  }

  /// <summary>
  /// Verifies that the current token is of a specified value.
  /// If token is of different type ParseException is thrown.
  /// </summary>
  /// <param name="tokenType">a token type to verify.</param>
  /// <seealso cref="Bphx.Cool.Expresion.ParseException"/>
  public void Verify(string token)
  {
    if (Test(token))
    {
      return;
    }

    throw Error(
      "Invalid token \"" + Value +
      "\" appeared, while token \"" +
      token + "\" is expected.",
      -1);
  }

  /// <summary>
  /// Consumes a token.
  /// </summary>
  /// <returns>true if there is a next token, and false otherwise.</returns>
  public bool Eat() => Next();

  /// <summary>
  /// Tests and consumes the current token is of a specified type.
  /// </summary>
  /// <param name="tokenType">a token type to check.</param>
  /// <returns>true if there is a next token, and false otherwise.</returns>
  /// <remarks>
  /// Throws ParseException whenever expected token is different
  /// from current token.
  /// </remarks>
  public bool Eat(int tokenType)
  {
    Verify(tokenType);

    return Next();
  }

  /// <summary>
  /// Tests and consumes the current token is of a specified value.
  /// </summary>
  /// <param name="token">a token value to check.</param>
  /// <returns>true if there is a next token, and false otherwise.</returns>
  /// <remarks>
  /// Throws ParseException whenever expected token is different
  /// from current token.
  /// </remarks>
  public bool Eat(string token)
  {
    Verify(token);

    return Next();
  }

  /**
   * Creates an ParseExpression instance.
   * @param message a error message.
   * @param position a token position.
   */
  public ParseException Error(string message, int position)
  {
    if (message == null)
    {
      message = "Internal error during parsing";
    }

    return new(message, position == -1 ? Begin : position);
  }

  #region ITokenizer Members
  /// <summary>
  /// Indicates whether a tokenizer is case insensitive.
  /// Returns true if tokenizer is case insensitive, and false otherwise.
  /// </summary>
  public abstract bool IsCaseInsensitive { get; }

  /// <summary>
  /// Starts tokenizing a text.
  /// </summary>
  /// <param name="text">a text to tokenize.</param>
  /// <returns>true if there is a token, and false otherwise.</returns>
  public abstract bool Reset(string text);

  /// <summary>
  /// Starts tokenizing a text from a specified position.
  /// </summary>
  /// <param name="text">a text to tokenize.</param>
  /// <param name="position">a position to start with.</param>
  /// <returns>true if there is a token, and false otherwise.</returns>
  public abstract bool Reset(string text, int position);

  /// <summary>
  /// Starts tokenizing the current text from a specified position.
  /// </summary>
  /// <param name="position">a position to start with.</param>
  /// <returns>true if there is a token, and false otherwise.</returns>
  public abstract bool Reset(int position);

  /// <summary>
  /// Moves to the next token.
  /// </summary>
  /// <returns>true if the is next element, and false otherwise.</returns>
  public abstract bool Next();

  /// <summary>
  /// Gets the tokenized text.
  /// </summary>
  public abstract string Text { get; }

  /// <summary>
  /// Gets the token begin position.
  /// </summary>
  public abstract int Begin { get; }

  /// <summary>
  /// Gets the token end position.
  /// </summary>
  public abstract int End { get; }

  /// <summary>
  /// Gets the token type.
  /// </summary>
  public abstract int TokenType { get; }

  /// <summary>
  /// Gets the token value.
  /// </summary>
  public abstract string Value { get; }
  #endregion
}
