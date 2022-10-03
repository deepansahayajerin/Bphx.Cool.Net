using System;
using System.Text.RegularExpressions;

namespace Bphx.Cool.Expression;

/// <summary>
/// Tokenizes an expression.
/// </summary>
public class PatternTokenizer : AbstractTokenizer
{
  /// <summary>
  /// Creates an PatternTokenizer instance.
  /// </summary>
  /// <param name="pattern">a pattern that defines tokens.</param>
  public PatternTokenizer(Regex pattern)
  {
    regexp = pattern ?? throw new ArgumentNullException(nameof(pattern));

    if(regexp.GetGroupNumbers().Length < 3)
    {
      throw new ArgumentException("Invalid pattern.", nameof(pattern));
    }
  }

  /// <summary>
  /// Indicates whether a tokenizer is case insensitive.
  /// Returns true if tokenizer is case insensitive, and false otherwise.
  /// </summary>    
  public override bool IsCaseInsensitive =>
    (regexp.Options & RegexOptions.IgnoreCase) == RegexOptions.IgnoreCase;

  /// <summary>
  /// Starts tokenizing a text.
  /// </summary>
  /// <param name="text">a text to tokenize.</param>
  /// <returns>true if there is a token, and false otherwise.</returns>
  public override bool Reset(string text)
  {
    this.text = text ?? throw new ArgumentNullException(nameof(text));

    return CheckNext();
  }

  /// <summary>
  /// Starts tokenizing a text from a specified position.
  /// </summary>
  /// <param name="text">a text to tokenize.</param>
  /// <param name="position">a position to start with.</param>
  /// <returns>true if there is a token, and false otherwise.</returns>
  public override bool Reset(string text, int position)
  {
    this.text = text ?? throw new ArgumentNullException(nameof(text));

    return CheckNext(position);
  }

  /// <summary>
  /// Starts tokenizing the current text from a specified position.
  /// </summary>
  /// <param name="position">a position to start with.</param>
  /// <returns>true if there is a token, and false otherwise.</returns>
  public override bool Reset(int position) => CheckNext(position);

  /// <summary>
  /// Moves to the next token.
  /// </summary>
  /// <returns>true if the is next element, and false otherwise.</returns>
  public override bool Next() => CheckNext();

  /// <summary>
  /// Gets the tokenized text.
  /// </summary>
  public override string Text => text;

  /// <summary>
  /// Gets the token's begin position.
  /// </summary>
  public override int Begin => begin;

  /// <summary>
  /// Gets the token's end position.
  /// </summary>
  public override int End => end;

  /// <summary>
  /// Gets the token type.
  /// </summary>
  public override int TokenType => tokenType;

  /// <summary>
  /// Gets the token value.
  /// </summary>
  public override string Value =>
    tokenType == 0 ? null : match.Groups[tokenType].Value;

  /// <summary>
  /// Tests if current token is nonsignificant and can be skipped 
  /// (spaces an so). By default the first group is considered as 
  /// nonsignificant. 
  /// </summary>
  /// <param name="match">a Match instance to test.</param>
  /// <returns>
  /// true if this token can be skipped, and false otherwise.
  /// </returns>
  public static bool SkipToken(Match match) => match.Groups[1].Success;

  /// <summary>
  /// Gets a token type for the matched value.
  /// </summary>
  /// <param name="match">a Match instance to test.</param>
  /// <returns>
  /// true if this token can be skipped, and false otherwise.
  /// </returns>
  public static int GetTokenType(Match match)
  {
    var groups = match.Groups;
    var last = groups.Count - 1;

    if(last >= 0)
    {
      for(var i = 1; i < last; i++)
      {
        if(groups[i].Success)
        {
          return i;
        }
      }

      if(groups[last].Success)
      {
        return -1; // Unknown token
      }
    }

    return 0; // End
  }

  /// <summary>
  /// Scans tokens and returns a value indicating whether 
  /// there is a next token. 
  /// </summary>
  /// <returns>true if there is a next token, and false otherwise.</returns>
  private bool CheckNext()
  {
    match = match == null ? regexp.Match(text) : match.NextMatch();

    if(match.Success)
    {
      return Scan();
    }

    tokenType = 0;

    return false;
  }

  /// <summary>
  /// Scans tokens from a specified position and returns a value indicating 
  /// whether there is a next token.
  /// </summary>
  /// <param name="position">a position to start scan with.</param>
  /// <returns>true if there is a next token, and false otherwise.</returns>
  private bool CheckNext(int position)
  {
    match = regexp.Match(text, position);

    if(match.Success)
    {
      return Scan();
    }

    tokenType = 0;

    return false;
  }

  /// <summary>
  /// Scans tokens and discards space tokens.
  /// </summary>
  /// <returns>true if there is a next token, and false otherwise.</returns>
  private bool Scan()
  {
    while(SkipToken(match))
    {
      match = match.NextMatch();

      if(match.Success)
      {
        continue;
      }

      tokenType = 0;

      return false;
    }

    tokenType = GetTokenType(match);

    if(tokenType == 0)
    {
      begin = -1;
      end = -1;

      return false;
    }

    begin = match.Index;
    end = begin + match.Length;

    return true;
  }

  /// <summary>
  /// A sequence to scan.
  /// </summary>
  private string text;

  /// <summary>
  /// A regular expression to detect tokens.
  /// </summary>
  private readonly Regex regexp;

  /// <summary>
  /// Defines the current matched token.
  /// </summary>
  private Match match;

  /// <summary>
  /// Current token type.
  /// </summary>
  private int tokenType;

  /// <summary>
  /// Current token start position.
  /// </summary>
  private int begin;

  /// <summary>
  /// A position after a current token.
  /// </summary>
  private int end;
}
