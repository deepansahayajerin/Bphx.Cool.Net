namespace Bphx.Cool.Expression;

/// <summary>
/// An expression tokenizer.
/// </summary>
public interface ITokenizer
{
  /// <summary>
  /// Indicates whether a tokenizer is case insensitive.
  /// Returns true if tokenizer is case insensitive, and false otherwise.
  /// </summary>
  bool IsCaseInsensitive { get; }

  /// <summary>
  /// Starts tokenizing a text.
  /// </summary>
  /// <param name="text">a text to tokenize.</param>
  /// <returns>true if there is a token, and false otherwise.</returns>
  bool Reset(string text);

  /// <summary>
  /// Starts tokenizing a text from a specified position.
  /// </summary>
  /// <param name="text">a text to tokenize.</param>
  /// <param name="position">a position to start with.</param>
  /// <returns>true if there is a token, and false otherwise.</returns>
  bool Reset(string text, int position);

  /// <summary>
  /// Starts tokenizing the current text from a specified position.
  /// </summary>
  /// <param name="position">a position to start with.</param>
  /// <returns>true if there is a token, and false otherwise.</returns>
  bool Reset(int position);

  /// <summary>
  /// Moves to the next token.
  /// </summary>
  /// <returns>true if the is next element, and false otherwise.</returns>
  bool Next();

  /// <summary>
  /// Gets the tokenized text.
  /// </summary>
  string Text { get; }

  /// <summary>
  /// Gets the token begin position.
  /// </summary>
  int Begin { get; }

  /// <summary>
  /// Gets the token end position.
  /// </summary>
  int End { get; }

  /// <summary>
  /// Gets the token type.
  /// </summary>
  int TokenType { get; }

  /// <summary>
  /// Gets the token value.
  /// </summary>
  string Value { get; }
}
