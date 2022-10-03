using System;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Collections.Generic;
using Bphx.Cool.Expression;

namespace Bphx.Cool;

/// <summary>
/// Functions defining a set of static functions used in the program.
/// </summary>
public static class Functions
{
  /// <summary>
  /// Gets dictionary value by key.
  /// </summary>
  /// <typeparam name="K">A key type.</typeparam>
  /// <typeparam name="V">A value type.</typeparam>
  /// <param name="dictionary">A dictionary instance.</param>
  /// <param name="key">A key value.</param>
  /// <returns>
  /// A value for the key, or null if dictionary does not 
  /// contain the specified key.
  /// </returns>
  public static V Get<K, V>(this IDictionary<K, V> dictionary, K key)
    where V : class =>
    dictionary.TryGetValue(key, out var value) ? value : default;

  /// <summary>
  /// Tests whether the value is null or containing only spaces.
  /// </summary>
  /// <param name="value">A value to test.</param>
  /// <returns>
  /// true if value is null or containing only spaces, and false otherwise.
  /// </returns>
  public static bool IsEmpty(string value) => string.IsNullOrWhiteSpace(value);

  /// <summary>
  /// Compares two strings and returns true if they are equal, and false
  /// otherwise.
  /// </summary>
  /// <param name="first">A first string to compare.</param>
  /// <param name="second">A second string to compare.</param>
  /// <returns>true if strings are equal, and false otherwise.</returns>
  public static bool Equal(string first, string second)
  {
    if (first == null)
    {
      first = "";
    }

    if (second == null)
    {
      second = "";
    }

    // Fast path.
    int delta = first.Length - second.Length;

    if (delta == 0)
    {
      return first == second;
    }

    // Slow path.
    if (delta > 0)
    {
      if (!first.StartsWith(second))
      {
        return false;
      }

      for(int i = second.Length, c = first.Length; i < c; i++)
      {
        if (first[i] != ' ')
        {
          return false;
        }
      }
    }
    else
    {
      if (!second.StartsWith(first))
      {
        return false;
      }

      for(int i = first.Length, c = second.Length; i < c; i++)
      {
        if (second[i] != ' ')
        {
          return false;
        }
      }
    }

    return true;
  }

  /// <summary>
  /// Compares two strings and returns true if they are equal, and false
  /// otherwise.
  /// </summary>
  /// <param name="first">A first string to compare.</param>
  /// <param name="second">A second string to compare.</param>
  /// <param name="secondPosition">
  /// Initial position in the second string.
  /// </param>
  /// <param name="secondLength">Length in the second string.</param>
  /// <returns>true if strings are equal, and false otherwise.</returns>
  public static bool Equal(
    string first,
    string second,
    int secondPosition,
    int secondLength) =>
    Equal(
      first,
      1,
      first == null ? 0 : first.Length,
      second,
      secondPosition,
      secondLength);

  /// <summary>
  /// Compares two strings and returns true if they are equal, and false
  /// otherwise.
  /// </summary>
  /// <param name="first">A first string to compare.</param>
  /// <param name="firstPosition">
  /// Initial position in the first string.
  /// </param>
  /// <param name="firstLength">Length in the first string.</param>
  /// <param name="second">A second string to compare.</param>
  /// <returns>true if strings are equal, and false otherwise.</returns>
  public static bool Equal(
    string first,
    int firstPosition,
    int firstLength,
    string second) =>
    Equal(
      first,
      firstPosition,
      firstLength,
      second,
      1,
      second == null ? 0 : second.Length);

  /// <summary>
  /// Compares two strings and returns true if they are equal, and false
  /// otherwise.
  /// </summary>
  /// <param name="first">a first string to compare.</param>
  /// <param name="firstPosition">
  /// Initial position in the first string.
  /// </param>
  /// <param name="firstLength">Length in the first string.</param>
  /// <param name="second">a second string to compare.</param>
  /// <param name="secondPosition">
  /// Initial position in the second string.
  /// </param>
  /// <param name="secondLength">Length in the second string.</param>
  /// <returns>true if strings are equal, and false otherwise.</returns>
  public static bool Equal(
    string first,
    int firstPosition,
    int firstLength,
    string second,
    int secondPosition,
    int secondLength)
  {
    if (first == null)
    {
      first = "";
    }

    var firstStart = firstPosition - 1;

    if (firstStart < 0)
    {
      firstStart = 0;
    }

    if ((firstLength < 1) || (firstStart > first.Length))
    {
      first = "";
      firstStart = 0;
      firstLength = 0;
    }

    var firstEnd = firstStart + firstLength;

    if (firstEnd > first.Length)
    {
      firstEnd = first.Length;
      firstLength = firstEnd - firstStart;
    }

    if (second == null)
    {
      second = "";
    }

    var secondStart = secondPosition - 1;

    if (secondStart < 0)
    {
      secondStart = 0;
    }

    if ((secondLength < 1) || (secondStart > second.Length))
    {
      second = "";
      secondStart = 0;
      secondLength = 0;
    }

    var secondEnd = secondStart + secondLength;

    if (secondEnd > second.Length)
    {
      secondEnd = second.Length;
      secondLength = secondEnd - secondStart;
    }

    if (string.CompareOrdinal(
      first,
      firstStart,
      second,
      secondStart,
      firstLength < secondLength ? firstLength : secondLength) != 0)
    {
      return false;
    }

    if (firstLength == secondLength)
    {
      return true;
    }

    if (firstLength > secondLength)
    {
      for(var i = firstStart + secondLength; i < firstEnd; i++)
      {
        if (first[i] != ' ')
        {
          return false;
        }
      }
    }
    else
    {
      for(var i = secondStart + firstLength; i < secondEnd; i++)
      {
        if (second[i] != ' ')
        {
          return false;
        }
      }
    }

    return true;
  }

  /// <summary>
  /// Compares two values and returns true if they are equal, and false
  /// otherwise.
  /// </summary>
  /// <param name="first">A first value to compare.</param>
  /// <param name="second">A second value to compare.</param>
  /// <returns>true if strings are equal, and false otherwise.</returns>
  public static bool Equal<T>(T first, T second)
    where T: IEquatable<T> =>
    first == null ? second == null : second != null && first.Equals(second);

  /// <summary>
  /// Compares two values and returns true if they are equal, and false
  /// otherwise.
  /// </summary>
  /// <param name="first">A first value to compare.</param>
  /// <param name="second">A second value to compare.</param>
  /// <returns>true if strings are equal, and false otherwise.</returns>
  public static bool Equal<T>(T? first, T? second)
    where T: struct, IEquatable<T> =>
    first == null ? second == null : second != null && first.Equals(second);

  /// <summary>
  /// <para>
  /// Compares two strings and returns -1, 0, or 1 depending on 
  /// the first string is less than, equal, or greater than second 
  /// string.
  /// </para>
  /// <para>
  /// The method does not take into account trailing spaces 
  /// in the strings.
  /// </para>
  /// </summary>
  /// <param name="first">a first string to compare.</param>
  /// <param name="second">a second string to compare.</param>
  /// <returns>
  /// -1, 0, or 1 depending on the first string is less than,
  /// equal, or greater than second string.
  /// </returns>
  public static int Compare(string first, string second)
  {
    if (first == null)
    {
      first = "";
    }

    if (second == null)
    {
      second = "";
    }

    if (first.Length > second.Length)
    {
      int d = string.Compare(first, 0, second, 0, second.Length);

      if (d != 0)
      {
        return d;
      }

      int i = second.Length;

      do
      {
        char c = first[i];

        if (c < ' ')
        {
          return -1;
        }

        if (c > ' ')
        {
          return 1;
        }
      }
      while(++i < first.Length);
    }
    else
    {
      int d = string.Compare(first, 0, second, 0, first.Length);

      if (d != 0)
      {
        return d;
      }

      for(int i = first.Length; i < second.Length; ++i)
      {
        char c = second[i];

        if (c < ' ')
        {
          return 1;
        }

        if (c > ' ')
        {
          return -1;
        }
      }
    }

    return 0;
  }

  /// <summary>
  /// <para>
  /// Compares two strings and returns -1, 0, or 1 depending on 
  /// the first string is less than, equal, or greater than second 
  /// string.
  /// </para>
  /// <para>
  /// The method does not take into account trailing spaces 
  /// in the strings.
  /// </para>
  /// </summary>
  /// <param name="first">A first string to compare.</param>
  /// <param name="second">A second string to compare.</param>
  /// <param name="collator">Optional collator to use during comparision.</param>
  /// <returns>
  /// -1, 0, or 1 depending on the first string is less than,
  /// equal, or greater than second string.
  /// </returns>
  public static int Compare(
    string first,
    string second,
    IComparer<string> collator) =>
    collator == null ? Compare(first, second) :
      collator.Compare(first?.TrimEnd(Space) ?? "", second?.TrimEnd(Space) ?? "");

  /// <summary>
  /// Compares two strings and returns -1, 0, or 1 depending on the first string 
  /// is less than, equal, or greater than second string. Case sensitivity is supported
  /// and can be switched off.
  /// </summary>
  /// <param name="first">A first string to compare.</param>
  /// <param name="second">A second string to compare.</param>
  /// <param name="caseSensitivity">a string containing "Case_Sensitive" or 
  /// "Case_Insensitive" value.</param>
  /// <returns>
  /// -1, 0, or 1 depending on the first string is less than,
  /// equal, or greater than second string.
  /// </returns>
  public static int Compare(
    string first,
    string second,
    string caseSensitivity) =>
    !string.IsNullOrEmpty(caseSensitivity) &&
      (string.Compare(caseSensitivity, "Case_Insensitive", true) == 0) ?
      Compare(first?.ToUpper(), second?.ToUpper()) :
      Compare(first, second);

  /// <summary>
  /// Compares two byte arrays and returns true if they are equal, and false
  /// otherwise.
  /// </summary>
  /// <param name="first">a first byte array to compare.</param>
  /// <param name="second">a second byte array to compare.</param>
  /// <returns>true if byte arrays are equal, and false otherwise.</returns>
  public static bool Equal(byte[] first, byte[] second) =>
    Compare(first, second) == 0;

  /// <summary>
  /// Compares two byte arrays and returns -1, 0, or 1 depending on 
  /// the first value is less than, equal, or greater than the second one.
  /// </summary>
  /// <param name="first">a first value to compare.</param>
  /// <param name="second">a second value to compare.</param>
  /// <returns>
  /// -1, 0, or 1 depending on the first value is less than, equal, or 
  /// greater than the second one.
  /// </returns>
  public static int Compare(byte[] first, byte[] second)
  {
    var firstLength = first == null ? 0 : first.Length;
    var secondLength = first == null ? 0 : first.Length;
    var length = firstLength > secondLength ? firstLength : secondLength;

    for(var i = 0; i < length; ++i)
    {
      var firstValue = i < firstLength ? first[i] : 0;
      var secondValue = i < secondLength ? second[i] : 0;

      if (firstValue > secondValue)
      {
        return 1;
      }

      if (firstValue < secondValue)
      {
        return -1;
      }
    }

    return 0;
  }

  /// <summary>
  /// <para>
  /// Compares two non nullable objects and returns -1, 0, or 1 depending on 
  /// the first object is less than, equal, or greater than second one.
  /// </para>
  /// </summary>
  /// <param name="first">A first object to compare.</param>
  /// <param name="second">A second object to compare.</param>
  /// <returns>
  /// -1, 0, or 1 depending on the first object is less than,
  /// equal, or greater than second one.
  /// </returns>
  public static int Compare<T>(T first, T second)
    where T : IComparable<T> =>
    first == null ? 
      second == null ? 0 : -1 :
      second == null ? 1 : first.CompareTo(second);

  /// <summary>
  /// <para>
  /// Compares two nullable objects and returns -1, 0, or 1 depending on 
  /// the first object is less than, equal, or greater than second one.
  /// </para>
  /// </summary>
  /// <param name="first">A first object to compare.</param>
  /// <param name="second">A second object to compare.</param>
  /// <returns>
  /// -1, 0, or 1 depending on the first object is less than,
  /// equal, or greater than second one.
  /// </returns>
  public static int Compare<T>(T? first, T? second)
    where T: struct, IComparable<T> =>
    first.GetValueOrDefault().CompareTo(second.GetValueOrDefault());

  /// <summary>
  /// <para>
  /// Compares two nullable dates and returns -1, 0, or 1 depending on 
  /// the first object is less than, equal, or greater than second one.
  /// </para>
  /// </summary>
  /// <param name="first">A first date to compare.</param>
  /// <param name="second">A second date to compare.</param>
  /// <returns>
  /// -1, 0, or 1 depending on the first date is less than,
  /// equal, or greater than second one.
  /// </returns>
  public static int Compare<T>(DateTime? first, DateTime? second) =>
    first == null ?
      second == null ? 0 : -1 :
      second == null ? 1 : first.Value.CompareTo(second.Value);

  /// <summary>
  /// <para>
  /// Compares two IComparable objects and returns -1, 0, or 1 depending on 
  /// the first object is less than, equal, or greater than second one.
  /// </para>
  /// </summary>
  /// <param name="first">a first object to compare.</param>
  /// <param name="second">a second object to compare.</param>
  /// <returns>
  /// -1, 0, or 1 depending on the first object is less than,
  /// equal, or greater than second one.
  /// </returns>
  public static int Compare(IComparable first, IComparable second) =>
    first == null ? IsNullOrZero(second) ? 0 : -1 :
      second == null ? IsNullOrZero(first) ? 0 : 1 :
      first.CompareTo(second);

  /// <summary>
  /// Checks whether an input comparable value is null or zero.
  /// </summary>
  /// <param name="value">a value to check.</param>
  /// <returns>
  /// True, when the specified value is null or zero (DateTime.MinValue).
  /// </returns>
  public static bool IsNullOrZero(IComparable value) =>
    (value == null) ||
    ((value is int intValue) && (intValue == 0)) ||
    ((value is long longValue) && (longValue == 0L)) ||
    ((value is decimal decimalValue) && (decimalValue == 0m)) ||
    ((value is DateTime dateTime) && (dateTime == DateTime.MinValue)) ||
    ((value is double doubleValue) && (doubleValue == 0.0)) ||
    ((value is short shortValue) && (shortValue == 0));

  /// <summary>
  /// <para>Returns an object of <c>DateTime</c> from the given 
  /// string timestamp into a COOL:Gen timestamp value. The following 
  /// strings are valid for input:</para>
  /// <list type="bullet">
  ///   <item>
  ///     <description>YYYY-MM-DD-HH.MI.SS.NNNNNN</description>
  ///   </item>
  ///   <item>
  ///     <description>YYYY-MM-DD-HH.MI.SS</description>
  ///   </item>
  ///   <item>
  ///     <description>YYYY-MM-DD</description>
  ///   </item>
  /// </list>
  /// </summary>
  /// <param name="value">a COOL:Gen timestamp value.</param>
  /// <returns>a DateTime or null if value is null.</returns>
  /// <exception cref="ArgumentException">
  /// In case of invalid format.
  /// </exception>
  public static DateTime? Timestamp(string value)
  {
    value = Trim(value);

    int year;
    int month;
    int day;
    var hour = 0;
    var minute = 0;
    var second = 0;
    var millisecond = 0;
    var microsecond = 0;

    switch(value.Length)
    {
      case 10:
      {
        year = int.Parse(value[..4]);
        month = int.Parse(value[5..7]);
        day = int.Parse(value[8..]);

        break;
      }
      case 19:
      case 20:
      case 21:
      case 22:
      case 23:
      case 24:
      case 25:
      case 26:
      {
        year = int.Parse(value[..4]);
        month = int.Parse(value[5..7]);
        day = int.Parse(value[8..10]);
        hour = int.Parse(value[11..13]);
        minute = int.Parse(value[14..16]);
        second = int.Parse(value[17..19]);

        var fraction = value.Length > 20 ? value[20..] : "";

        if (!IsEmpty(fraction))
        {
          microsecond = int.Parse(value[20..]);

          switch(fraction.Length)
          {
            case 1:
            {
              microsecond *= 100000;

              break;
            }
            case 2:
            {
              microsecond *= 10000;

              break;
            }
            case 3:
            {
              microsecond *= 1000;

              break;
            }
            case 4:
            {
              microsecond *= 100;

              break;
            }
            case 5:
            {
              microsecond *= 10;

              break;
            }
          }

          millisecond = microsecond / 1000;
        }

        break;
      }
      default:
      {
        return null;
      }
    }

    var nextDay = hour == 24;

    if (nextDay)
    {
      hour = 0;
    }

    var result = microsecond == 0 ?
      new(year, month, day, hour, minute, second) :
      millisecond == 0 ?
      new(year, month, day, hour, minute, second, millisecond) :
      AddMicroseconds(
        new(year, month, day, hour, minute, second),
        microsecond);

    return nextDay ? AddDays(result, 1) : result;
  }

  /// <summary>
  /// Converts a date which is of numeric domain format to the format 
  /// specified by the  user (format depends on the language of the use). 
  /// </summary>
  /// <param name="value">
  /// A date value. Ex: 19970319 stands for March 19, 1997.
  /// </param>
  /// <param name="format">
  /// A date format that uses following syntax:
  /// 
  /// d - numeric representation of the day of the month, 
  ///   without leading zeros;
  /// dd  - numeric representation of the day of the month,
  ///   with leading zeros;
  /// ddd - three-letter abbreviation for the day of the week;
  /// dddd - full name of the day of week;
  ///
  /// M - numeric representation of the month, without leading zeros;
  /// MM - numeric representation of the month, with leading zeros;
  /// MMM - three-letter abbreviation for the name of the month;
  /// MMMM - full name of the month;
  /// 
  /// y - year represented by only the last two digits, 
  ///   without leading zeros if less than 10;
  /// yy - year represented by only the last two digits;
  /// yyyy - year represented by all four digits.
  /// 
  /// Note:
  ///   The string is case sensitive. It only identifies above characters for
  ///   formatting, all other characters are used as separators.
  /// </param>
  /// <returns>A formatted date value.</returns>
  public static string DateFormat(int value, string format) =>
    throw new NotImplementedException();

  /// <summary>
  /// Gets time up to seconds including.
  /// </summary>
  /// <param name="value">A date time value.</param>
  /// <returns>time value.</returns>
  public static TimeSpan Time(DateTime value) =>
    TimeSpan.FromSeconds((long)value.TimeOfDay.TotalSeconds);

  /// <summary>
  /// Gets time up to seconds including.
  /// </summary>
  /// <param name="value">A date time value.</param>
  /// <returns>time value.</returns>
  public static TimeSpan? Time(DateTime? value) =>
    value == null ? null : Time(value.Value);

  /// <summary>
  /// Adds years, months and days to the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <param name="years">A years value.</param>
  /// <param name="months">A months values.</param>
  /// <param name="days">A days value.</param>
  /// <returns>Adjusted date value.</returns>
  public static DateTime Add(
    DateTime value,
    int years,
    int months,
    int days) =>
    value.AddDays(days).AddMonths(months).AddYears(years);

  /// <summary>
  /// Adds years, months and days to the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <param name="years">A years value.</param>
  /// <param name="months">A months values.</param>
  /// <param name="days">A days value.</param>
  /// <returns>Adjusted date value.</returns>
  public static DateTime? Add(
    DateTime? value,
    int years,
    int months,
    int days) =>
    value?.AddDays(days).AddMonths(months).AddYears(years);

  /// <summary>
  /// Adds years to the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <param name="years">A years value.</param>
  /// <returns>Adjusted date value.</returns>
  public static DateTime AddYears(DateTime value, int years) =>
    value.AddYears(years);

  /// <summary>
  /// Adds years to the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <param name="years">A years value.</param>
  /// <returns>Adjusted date value.</returns>
  public static DateTime? AddYears(DateTime? value, int years) =>
    value?.AddYears(years);

  /// <summary>
  /// Adds months to the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <param name="months">A months value.</param>
  /// <returns>Adjusted date value.</returns>
  public static DateTime AddMonths(DateTime value, int months) =>
    value.AddMonths(months);

  /// <summary>
  /// Adds months to the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <param name="months">A months value.</param>
  /// <returns>Adjusted date value.</returns>
  public static DateTime? AddMonths(DateTime? value, int months) =>
    value?.AddMonths(months);

  /// <summary>
  /// Adds days to the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <param name="days">A days value.</param>
  /// <returns>Adjusted date value.</returns>
  public static DateTime AddDays(DateTime value, int days) =>
    value.AddDays(days);

  /// <summary>
  /// Adds days to the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <param name="days">A days value.</param>
  /// <returns>Adjusted date value.</returns>
  public static DateTime? AddDays(DateTime? value, int days) =>
    value?.AddDays(days);

  /// <summary>
  /// Adds hours to the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <param name="hours">A hours value.</param>
  /// <returns>Adjusted date value.</returns>
  public static DateTime AddHours(DateTime value, int hours) =>
    value.AddHours(hours);

  /// <summary>
  /// Adds hours to the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <param name="hours">A hours value.</param>
  /// <returns>Adjusted date value.</returns>
  public static DateTime? AddHours(DateTime? value, int hours) =>
    value?.AddHours(hours);

  /// <summary>
  /// Adds minutes to the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <param name="minutes">A minutes value.</param>
  /// <returns>Adjusted date value.</returns>
  public static DateTime AddMinutes(DateTime value, int minutes) =>
    value.AddMinutes(minutes);

  /// <summary>
  /// Adds minutes to the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <param name="minutes">A minutes value.</param>
  /// <returns>Adjusted date value.</returns>
  public static DateTime? AddMinutes(DateTime? value, int minutes) =>
    value?.AddMinutes(minutes);

  /// <summary>
  /// Adds seconds to the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <param name="seconds">A seconds value.</param>
  /// <returns>Adjusted date value.</returns>
  public static DateTime AddSeconds(DateTime value, int seconds) =>
    value.AddSeconds(seconds);

  /// <summary>
  /// Adds seconds to the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <param name="seconds">A seconds value.</param>
  /// <returns>Adjusted date value.</returns>
  public static DateTime? AddSeconds(DateTime? value, int seconds) =>
    value?.AddSeconds(seconds);

  /// <summary>
  /// Adds milliseconds to the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <param name="milliseconds">A milliseconds value.</param>
  /// <returns>Adjusted date value.</returns>
  public static DateTime AddMilliseconds(DateTime value, int milliseconds) =>
    value.AddMilliseconds(milliseconds);

  /// <summary>
  /// Adds milliseconds to the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <param name="milliseconds">A milliseconds value.</param>
  /// <returns>Adjusted date value.</returns>
  public static DateTime? AddMilliseconds(DateTime? value, int milliseconds) =>
    value?.AddMilliseconds(milliseconds);

  /// <summary>
  /// Adds microseconds to the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <param name="microseconds">A microseconds value.</param>
  /// <returns>Adjusted date value.</returns>
  public static DateTime AddMicroseconds(DateTime value, int microseconds) =>
    value.AddTicks(microseconds * TimeSpan.TicksPerMillisecond / 1000);


  /// <summary>
  /// Adds microseconds to the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <param name="microseconds">A microseconds value.</param>
  /// <returns>Adjusted date value.</returns>
  public static DateTime? AddMicroseconds(DateTime? value, int microseconds) =>
    value?.AddTicks(microseconds * TimeSpan.TicksPerMillisecond / 1000);

  /// <summary>
  /// Adds ticks to the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <param name="ticks">A ticks value.</param>
  /// <returns>Adjusted date value.</returns>
  public static DateTime AddTicks(DateTime value, long ticks) =>
    value.AddTicks(ticks);

  /// <summary>
  /// Adds ticks to the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <param name="ticks">A ticks value.</param>
  /// <returns>Adjusted date value.</returns>
  public static DateTime? AddTicks(DateTime? value, long ticks) =>
    value?.AddTicks(ticks);

  /// <summary>
  /// Gets year of the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <returns>A year value.</returns>
  public static int Year(DateTime value) => value.Year;

  /// <summary>
  /// Gets year of the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <returns>A year value.</returns>
  public static int Year(DateTime? value) => value?.Year ?? 0;

  /// <summary>
  /// Gets month of the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <returns>A month value.</returns>
  public static int Month(DateTime value) => value.Month;

  /// <summary>
  /// Gets month of the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <returns>A month value.</returns>
  public static int Month(DateTime? value) => value?.Month ?? 0;

  /// <summary>
  /// Gets day of month of the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <returns>A a day of month value.</returns>
  public static int Day(DateTime value) => value.Day;

  /// <summary>
  /// Gets day of month of the date.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <returns>A a day of month value.</returns>
  public static int Day(DateTime? value) => value?.Day ?? 0;

  /// <summary>
  /// Gets day of the time value.
  /// </summary>
  /// <param name="value">A time value.</param>
  /// <returns>A a day of the time value.</returns>
  public static int Day(TimeSpan value) => value.Days;

  /// <summary>
  /// Gets day of the time value.
  /// </summary>
  /// <param name="value">A time value.</param>
  /// <returns>A a day of the time value.</returns>
  public static int Day(TimeSpan? value) => value?.Days ?? 0;

  /// <summary>
  /// Gets hour of the date value.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <returns>A a hour of the date value.</returns>
  public static int Hour(DateTime value) => value.Hour;

  /// <summary>
  /// Gets hour of the date value.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <returns>A a hour of the date value.</returns>
  public static int Hour(DateTime? value) => value?.Hour ?? 0;

  /// <summary>
  /// Gets hour of the time value.
  /// </summary>
  /// <param name="value">A time value.</param>
  /// <returns>A a hour of the time value.</returns>
  public static int Hour(TimeSpan value) => value.Hours;

  /// <summary>
  /// Gets hour of the time value.
  /// </summary>
  /// <param name="value">A time value.</param>
  /// <returns>A a hour of the time value.</returns>
  public static int Hour(TimeSpan? value) => value?.Hours ?? 0;

  /// <summary>
  /// Gets minute of the date value.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <returns>A a minute of the date value.</returns>
  public static int Minute(DateTime value) => value.Minute;

  /// <summary>
  /// Gets minute of the date value.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <returns>A a minute of the date value.</returns>
  public static int Minute(DateTime? value) => value?.Minute ?? 0;

  /// <summary>
  /// Gets minute of the time value.
  /// </summary>
  /// <param name="value">A time value.</param>
  /// <returns>A a minute of the time value.</returns>
  public static int Minute(TimeSpan value) => value.Minutes;

  /// <summary>
  /// Gets minute of the time value.
  /// </summary>
  /// <param name="value">A time value.</param>
  /// <returns>A a minute of the time value.</returns>
  public static int Minute(TimeSpan? value) => value?.Minutes ?? 0;

  /// <summary>
  /// Gets second of the date value.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <returns>A a second of the date value.</returns>
  public static int Second(DateTime value) => value.Second;

  /// <summary>
  /// Gets second of the date value.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <returns>A a second of the date value.</returns>
  public static int Second(DateTime? value) => value?.Second ?? 0;

  /// <summary>
  /// Gets second of the time value.
  /// </summary>
  /// <param name="value">A time value.</param>
  /// <returns>A a second of the time value.</returns>
  public static int Second(TimeSpan value) => value.Seconds;

  /// <summary>
  /// Gets second of the time value.
  /// </summary>
  /// <param name="value">A time value.</param>
  /// <returns>A a second of the time value.</returns>
  public static int Second(TimeSpan? value) => value?.Seconds ?? 0;

  /// <summary>
  /// Gets millisecond of the date value.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <returns>A a millisecond of the date value.</returns>
  public static int Millisecond(DateTime value) => value.Millisecond;

  /// <summary>
  /// Gets millisecond of the date value.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <returns>A a millisecond of the date value.</returns>
  public static int Millisecond(DateTime? value) => value?.Millisecond ?? 0;

  /// <summary>
  /// Gets millisecond of the time value.
  /// </summary>
  /// <param name="value">A time value.</param>
  /// <returns>A a millisecond of the time value.</returns>
  public static int Millisecond(TimeSpan value) => value.Milliseconds;

  /// <summary>
  /// Gets millisecond of the time value.
  /// </summary>
  /// <param name="value">A time value.</param>
  /// <returns>A a millisecond of the time value.</returns>
  public static int Millisecond(TimeSpan? value) => value?.Milliseconds ?? 0;

  /// <summary>
  /// Gets microsecond of the date value.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <returns>A a microsecond of the date value.</returns>
  public static int Microsecond(DateTime value) =>
    (int)(value.TimeOfDay.TotalMilliseconds * 1000 % 1000000);

  /// <summary>
  /// Gets microsecond of the date value.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <returns>A a microsecond of the date value.</returns>
  public static int Microsecond(DateTime? value) =>
    (int)(value?.TimeOfDay.TotalMilliseconds * 1000 % 1000000 ?? 0);

  /// <summary>
  /// Gets microsecond of the time value.
  /// </summary>
  /// <param name="value">A time value.</param>
  /// <returns>A a microsecond of the time value.</returns>
  public static int Microsecond(TimeSpan value) =>
    (int)(value.TotalMilliseconds * 1000 % 1000000);

  /// <summary>
  /// Gets microsecond of the time value.
  /// </summary>
  /// <param name="value">A time value.</param>
  /// <returns>A a microsecond of the time value.</returns>
  public static int Microsecond(TimeSpan? value) =>
    (int)(value?.TotalMilliseconds * 1000 % 1000000 ?? 0);

  /// <summary>
  /// Gets ticks of the date value.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <returns>A a ticks of the date value.</returns>
  public static long Ticks(DateTime value) => value.Ticks;

  /// <summary>
  /// Gets ticks of the date value.
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <returns>A a ticks of the date value.</returns>
  public static long Ticks(DateTime? value) => value?.Ticks ?? 0;

  /// <summary>
  /// Gets ticks of the time value.
  /// </summary>
  /// <param name="value">A time value.</param>
  /// <returns>A a ticks of the time value.</returns>
  public static long Ticks(TimeSpan value) => value.Ticks;

  /// <summary>
  /// Gets ticks of the time value.
  /// </summary>
  /// <param name="value">A time value.</param>
  /// <returns>A a ticks of the time value.</returns>
  public static long Ticks(TimeSpan? value) => value?.Ticks ?? 0;

  /// <summary>
  /// Gets a day of the week for given date.
  /// </summary>
  /// <param name="value">a date value.</param>
  /// <returns>
  /// a string representation of day of week for the given date.
  /// </returns>
  public static string DayOfWeek(DateTime value) =>
    value.DayOfWeek.ToString().ToUpper();

  /// <summary>
  /// Gets a day of the week for given date.
  /// </summary>
  /// <param name="value">a date value.</param>
  /// <returns>
  /// a string representation of day of week for the given date.
  /// </returns>
  public static string DayOfWeek(DateTime? value) =>
    value == null ? null : DayOfWeek(value.Value);

  /// <summary>
  /// Gets date part of the date value
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <returns>A date part value.</returns>
  public static DateTime Date(DateTime value) => value.Date;

  /// <summary>
  /// Gets date part of the date value
  /// </summary>
  /// <param name="value">A date value.</param>
  /// <returns>A date part value.</returns>
  public static DateTime? Date(DateTime? value) => value?.Date;

  /// <summary>
  /// Extracts a numeric value from a date value. For eg: if the date value 
  /// is July 28, 1992, it returns 19920728.
  /// </summary>
  /// <param name="value">a date value.</param>
  /// <returns>a numeric value from the date value.</returns>
  public static int DateToInt(DateTime value) =>
    value.Year * 10000 + value.Month * 100 + value.Day;

  /// <summary>
  /// Extracts a numeric value from a date value. For eg: if the date value 
  /// is July 28, 1992, it returns 19920728.
  /// </summary>
  /// <param name="date">a date value.</param>
  /// <returns>a numeric value from the date value.</returns>
  public static int DateToInt(DateTime? value) =>
    value == null ? 0 : DateToInt(value.Value);

  /// <summary>
  /// <para>Translates a date into cyymmdd format. 
  /// The elements of a date are represented in the format as CYYMMDD. 
  /// The century is represented as single digit in the first position 
  /// of the format. The following table provides the numbers that stand 
  /// for the centuries in this format.</para>
  /// <para>Using this format, the date October 10, 1996 would be stored 
  /// as 0961010. The date January 15, 1999 would be stored as 0990115. 
  /// The date March 15, 2001 would be stored as 1010315.</para>
  /// </summary>
  /// <remarks>
  /// This format can only accommodate dates between January 1, 1600 and 
  /// December 31, 2599.
  /// </remarks> 
  /// <param name="date">a date value</param>
  /// <returns>a numeric value from the date value.</returns>
  public static int DateToCYYInt(DateTime date) =>
    (date.Year - 1900) * 10000 + date.Month * 100 + date.Day;

  /// <summary>
  /// <para>Translates a date into cyymmdd format. 
  /// The elements of a date are represented in the format as CYYMMDD. 
  /// The century is represented as single digit in the first position 
  /// of the format. The following table provides the numbers that stand 
  /// for the centuries in this format.</para>
  /// <para>Using this format, the date October 10, 1996 would be stored 
  /// as 0961010. The date January 15, 1999 would be stored as 0990115. 
  /// The date March 15, 2001 would be stored as 1010315.</para>
  /// </summary>
  /// <remarks>
  /// This format can only accommodate dates between January 1, 1600 and 
  /// December 31, 2599.
  /// </remarks> 
  /// <param name="value">a date value</param>
  /// <returns>a numeric value from the date value.</returns>
  public static int DateToCYYInt(DateTime? value) =>
    value == null ? 0 : DateToCYYInt(value.Value);

  /// <summary>
  ///   <para>Translates a date into a valid integer Julian date.</para>
  ///   <para>In the following example, if the input date is 
  ///   January 10, 1992, the DateToJulianNumber() function returns 
  ///   1992010.</para>
  /// </summary>
  /// <param name="value">a date value</param>
  /// <returns>a numeric value from the date value.</returns>
  public static int DateToJulianNumber(DateTime value) =>
    value.Year * 10000 + value.DayOfYear;

  /// <summary>
  ///   <para>Translates a date into a valid integer Julian date.</para>
  ///   <para>In the following example, if the input date is 
  ///   January 10, 1992, the DateToJulianNumber() function returns 
  ///   1992010.</para>
  /// </summary>
  /// <param name="value">a date value</param>
  /// <returns>a numeric value from the date value.</returns>
  public static int DateToJulianNumber(DateTime? value) =>
    value == null ? 0 : DateToJulianNumber(value.Value);

  /// <summary>
  ///   <para>Translates a valid integer Julian date into 
  ///   a DateTime instance.</para>
  ///   <para>In the following example, if the input number is 1992010,
  ///   the JulianNumberToDate() function returns a DateTime instance 
  ///   that contains January 10, 1992.</para>
  /// </summary>
  /// <param name="value">a valid integer Julian date to translate.</param>
  /// <returns>a corresponding DateTime instance.</returns>
  public static DateTime? JulianNumberToDate(int value) =>
    new DateTime(value / 1000 % 10000, 1, 1).AddDays(value % 1000 - 1);

  /// <summary>
  /// Gets a DateTime from numerical representation of a date.
  /// </summary>
  /// <param name="value">a value of date in format: yyyyMMdd.</param>
  /// <returns>a DateTime from numerical representation of a date.</returns>
  public static DateTime? IntToDate(int value) =>
    value == 0 ? null :
      new(value / 10000 % 10000, value / 100 % 100, value % 100);

  /// <summary>
  /// Gets a quantity of days from and including 01.01.01 
  /// until specified date.
  /// </summary>
  /// <param name="date">a DateTime value.</param>
  /// <returns>a quantity of days.</returns>
  public static int DaysFromAD(DateTime date) =>
    (date - DateTime.MinValue).Days + 1;

  /// <summary>
  /// Gets a quantity of days from and including 01.01.01 
  /// until specified date.
  /// </summary>
  /// <param name="date">a DateTime value.</param>
  /// <returns>a quantity of days.</returns>
  public static int DaysFromAD(DateTime? date) =>
    DaysFromAD(date.GetValueOrDefault());

  /// <summary>
  /// Gets a date value for a given number of days passed from, and 
  /// including, 01.01.01.
  /// </summary>
  /// <param name="value">a number of days.</param>
  /// <returns>a DateTime value</returns>
  public static DateTime DaysFromADToDate(int value) =>
    DateTime.MinValue.AddDays(value - 1);

  /// <summary>
  /// Gets a numeric value from a time value. 
  /// For eg: if time value is 1:10:25p.m., the function returns 131025.
  /// </summary>
  /// <param name="time">a time value.</param>
  /// <returns>a numerical value for the time.</returns>
  public static int TimeToInt(TimeSpan time) =>
    time.Hours * 10000 + time.Minutes * 100 + time.Seconds;

  /// <summary>
  /// Gets a numeric value from a time value. 
  /// For eg: if time value is 1:10:25p.m., the function returns 131025.
  /// </summary>
  /// <param name="time">a time value.</param>
  /// <returns>a numerical value for the time.</returns>
  public static int TimeToInt(TimeSpan? time) =>
    TimeToInt(time.GetValueOrDefault());

  /// <summary>
  /// Gets a time of day for the specified value. 
  /// </summary>
  /// <param name="value">a DateTime value.</param>
  /// <returns>a time of day or null, if the input value is null.</returns>
  public static TimeSpan? TimeOfDay(DateTime? value) =>
    value == null ? null : TimeOfDay(value.Value);

  /// <summary>
  /// Gets a time of day, including hh:mm:ss, for the specified value . 
  /// </summary>
  /// <param name="value">a DateTime value.</param>
  /// <returns>a time of day, including hh:mm:ss.</returns>
  public static TimeSpan TimeOfDay(DateTime value) =>
    new(value.Hour, value.Minute, value.Second);

  /// <summary>
  /// Gets a TimeSpan value from numerical representation of a time.
  /// </summary>
  /// <param name="value">a numerical value of time.</param>
  /// <returns>
  /// a TimeSpan value from numerical representation of a time.
  /// </returns>
  public static TimeSpan IntToTime(int value)
  {
    var hours = (value / 10000) % 100;
    var minutes = (value / 100) % 100;
    var seconds = value % 100;

    if ((hours == 24) && (minutes == 0) && (seconds == 0))
    {
      hours = 0;
    }

    return new(hours, minutes, seconds);
  }

  /// <summary>
  /// Gets a DateTime value from valid string value. 
  /// Valid date formats: yyyy-mm-dd; mm/dd/yy; dd.mm.yyyy; yyyynnn;
  /// </summary>
  /// <param name="value">a string value of date.</param>
  /// <returns>a DateTime value</returns>
  public static DateTime? StringToDate(string value)
  {
    if (value == null)
    {
      return null;
    }

    value = value.Trim();

    if (value.Length < 7)
    {
      return null;
    }

    if ((value.Length == 19) || (value.Length == 26))
    {
      return Timestamp(value);
    }

    int year;
    int month = 0;
    int day;
    bool dayOfYear = false;

    if ((value[2] == '.') && (value[5] == '.'))
    {
      // date format will be dd.mm.yyyy
      day = int.Parse(value[..2]);
      month = int.Parse(value[3..5]);
      year = int.Parse(value[6..]);
    }
    else if ((value[2] == '/') && (value[5] == '/'))
    {
      // date format will be mm/dd/yy
      month = int.Parse(value[..2]);
      day = int.Parse(value[3..5]);
      year = int.Parse(value[6..]);

      // TODO: how to interpret 2 digit year?
      if (year < 100)
      {
        if (year < 70)
        {
          year += 2000;
        }
        else
        {
          year += 1900;
        }
      }
      // Otherwise year is considered to have a long value.
    }
    else if ((value[4] == '-') && (value[7] == '-'))
    {
      // date format will be yyyy-mm-dd
      year = int.Parse(value[..4]);
      month = int.Parse(value[5..7]);
      day = int.Parse(value[8..]);
    }
    else
    {
      // date format will be yyyynnn
      dayOfYear = true;
      year = int.Parse(value[..4]);
      day = int.Parse(value[4..]);
    }

    if ((year == 0) && (month == 0) && (day == 0))
    {
      return null;
    }

    try
    {
      if (dayOfYear)
      {
        return new DateTime(year, 1, 1).AddDays(day - 1);
      }
      else
      {
        return new(year, month, day);
      }
    }
    catch
    {
      // Invalid values.
      return null;
    }
  }

  /// <summary>
  /// Gets a TimeSpan value from string representation of a time value. 
  /// Valid time formats: HH.MM.SS; HH:MM AM or PM; HH:MM:SS; HH AM or PM;
  /// </summary>
  /// <param name="value">a string representation of time value.</param>
  /// <returns>a TimeSpan value.</returns>
  public static TimeSpan? StringToTime(string value)
  {
    value = Trim(value);

    if (value.Length == 0)
    {
      return null;
    }

    var p1 = 0;
    var p2 = 0;
    var c = '\0';

    while(p2 < value.Length)
    {
      c = value[p2];

      if ((c < '0') || (c > '9'))
      {
        break;
      }

      if (p2 - p1 > 2)
      {
        return null;
      }

      ++p2;
    }

    if (p2 == p1)
    {
      return null;
    }

    var hh = int.Parse(value[p1..p2]);
    var mm = 0;
    var ss = 0;

    if ((c == ':') || (c == '.'))
    {
      p1 = ++p2;

      while(p2 < value.Length)
      {
        c = value[p2];

        if ((c < '0') || (c > '9'))
        {
          break;
        }

        if (p2 - p1 > 2)
        {
          return null;
        }

        ++p2;
      }

      if (p2 == p1)
      {
        return null;
      }

      mm = int.Parse(value[p1..p2]);

      if ((c == ':') || (c == '.'))
      {
        p1 = ++p2;

        while(p2 < value.Length)
        {
          c = value[p2];

          if ((c < '0') || (c > '9'))
          {
            break;
          }

          if (p2 - p1 > 2)
          {
            return null;
          }

          ++p2;
        }

        if (p2 == p1)
        {
          return null;
        }

        ss = int.Parse(value[p1..p2]);
      }
    }

    var am_pm = value[p2..].Trim();

    return (am_pm.Length == 0) || (string.Compare(am_pm, "AM", true) == 0) ?
      new(hh, mm, ss) :
      string.Compare(am_pm, "PM", true) == 0 ?
      new(hh + 12, mm, ss) :
      null;
  }

  /// <summary>
  /// Truncates value's decimal digits after a specified number of 
  /// decimal places.
  /// </summary>
  /// <param name="value">A decimal to truncate.</param>
  /// <param name="decimals">
  /// A value that specifies the number of decimal places to truncate to.
  /// </param>
  /// <returns>A truncated value.</returns>
  public static decimal Truncate(decimal value, byte decimals)
  {
    decimal result = decimal.Round(value, decimals);
    bool negative = decimal.Compare(value, 0) < 0;
    int c = decimal.Compare(value, result);

    if (negative ? c <= 0 : c >= 0)
    {
      return result;
    }

    return decimal.Subtract(result, new(1, 0, 0, negative, decimals));
  }

  /// <summary>
  /// Truncates value's decimal digits after a specified number of 
  /// decimal places.
  /// </summary>
  /// <param name="value">A nullable decimal to truncate.</param>
  /// <param name="decimals">
  /// A value that specifies the number of decimal places to truncate to.
  /// </param>
  /// <returns>A truncated value.</returns>
  public static decimal? Truncate(decimal? value, byte decimals) =>
    value == null ? null : Truncate(value.Value, decimals);

  /// <summary>
  /// Gets a string representation of a number. If the number 
  /// of characters in string representation is less than value of 
  /// width parameter then zeroes are appended to the left of string.
  /// </summary>
  /// <param name="value">a value to convert.</param>
  /// <param name="width">a width of resultant string.</param>
  /// <returns>a string representation of the value.</returns>
  public static string NumberToString(long value, int width)
  {
    var result = Math.Abs(value).ToString();

    if (result.Length > 15)
    {
      result = result[^15..];
    }

    return result.Length == width ? result :
      result.Length >= width ? result[^width..] :
      result.PadLeft(Math.Min(15, width), '0').PadLeft(width);
  }

  /// <summary>
  /// Gets a string representation of a number and extracts a substring.
  /// If the number of characters in string representation is less than 
  /// value of width parameter then zeroes are appended to the left of string.
  /// </summary>
  /// <param name="value">a value to convert.</param>
  /// <param name="position">
  /// an index of string from where substring starts.
  /// </param>
  /// <param name="length">
  /// a number of characters in the substring.
  /// </param>
  /// <returns>a string representation of the value.</returns>
  public static string NumberToString(
    long value,
    int position,
    int length) =>
    Substring(NumberToString(value, 15), position, length);

  /// <summary>
  /// Gets a string representation of a number. If the number 
  /// of characters in string representation is less than value of 
  /// width parameter then zeroes are appended to the left of string.
  /// </summary>
  /// <param name="value">a value to convert.</param>
  /// <param name="width">a width of resultant string.</param>
  /// <returns>a string representation of the value.</returns>
  public static string NumberToString(long? value, int width) =>
    value == null ? "" : NumberToString(value, width);

  /// <summary>
  /// Gets a number value of the given string.
  /// </summary>
  /// <param name="value">a string representing a number.</param>
  /// <returns>a value of the string.</returns>
  public static long StringToNumber(string value)
  {
    if (value == null)
    {
      return 0;
    }

    value = value.Trim();

    if (value.Length == 0)
    {
      return 0;
    }

    var pos = value.IndexOf(".");

    if (pos >= 0)
    {
      value = value.Remove(pos, 1);
    }

    var result = long.Parse(value);

    return result >= 0 ? result : -result;
  }

  /// <summary>
  /// Edits a number for display, using a supplied format string containing 
  /// appropriate edit characters.
  /// </summary>
  /// <param name="number">A number that will be prepared for display.</param>
  /// <param name="posFormat">
  /// A format for display of number. 
  /// This Format will be used if number is greater than zero.
  /// </param>
  /// <param name="negFormat">
  /// A format for display of number.
  /// This Format will be used if number is less than zero.
  /// </param>
  /// <param name="zeroFormat">
  /// A format for display of number.
  /// This Format will be used if number is equal to zero.
  /// </param>
  /// <param name="nullFormat">
  /// A format for display of Number.
  /// This Format will be used only if number is null.</param>
  /// <returns>
  /// The function returns a string containing the formatted number.
  /// If an error occurs, the function returns an empty string "".
  /// The maximum display format length is 80 characters.
  /// </returns>
  /// <remarks>
  /// <para>
  /// Note:
  /// If no formats are given, the function performs a straight-forward
  /// conversion.  For example, if the input number is "-123.45", and 
  /// the input formats are "" (no value), the output is "-123.45".
  /// </para>
  /// 
  /// <para>
  /// Notes on Display Patterns
  /// "Z" - display as space if position digit is zero and digits to the 
  ///   left of this position are zero. There must not be any 9's to
  ///   the left of this position.
  /// "9" - display digit "as is" always. There must not be any Z's to the
  ///   right of this position.
  /// "." (period) - display if there is a displayable digit to the right.
  ///   There may be multiple occurrences of this symbol.
  /// "," (comma) - display if there is a displayable digit to the left,
  ///   otherwise print a space character if there is no floating $ character.
  /// "*" (asterisk) - fill symbol. Display as * if the position digit is 
  ///   zero and digits to the left of this position are zero.
  /// "$" - floating monetary symbol. Can represent a space, a 
  ///   position digit, or a monetary symbol. The monetary symbol displays
  ///   differently  depending on its position relative to the decimal point
  ///   in the number. This symbol can also replace a comma under certain 
  ///   conditions. For monetary symbols in the format occurring before 
  ///   the decimal in the number (this includes whole numbers or numbers
  ///   with no decimal in them), displays a space if the position digit is
  ///   zero and digits to the left of this position are zero. The rightmost
  ///   space will be displayed as a $ (or other monetary symbol) if any such
  ///   spaces exist. For monetary symbols in the format occurring after the
  ///   decimal in the number, displays the symbol if the current position
  ///   digit is zero and the next format character to the right of this 
  ///   symbol is not a monetary symbol.
  /// "+" (plus sign) - this symbol displays if the display number 
  ///   is positive. May appear as either the first or last character in 
  ///   a format string. There may be at most one positive symbol in each
  ///   display pattern.
  /// "-" (minus sign) - for negative display numbers only.
  ///   If this symbol is either (exclusively) the first or last character
  ///   of the display pattern and there is only one occurrence of this 
  ///   symbol, the negative symbol is displayed. If there are multiple 
  ///   occurrences of this symbol, or this symbol is not at the 
  ///   beginning or end of the display pattern, acts as the non-numeric
  ///   hyphen described below.
  /// "DR" - same as + except may only be at the end. May also be dr.
  /// "CR" - same as - except can only be at the end. May also be cr.
  /// "B" - display only if there is a displayable digit to the left of
  ///   this symbol (may also be a space).
  /// "-" (hypen) - display only if there is a displayable digit to the
  ///   immediate left of this symbol, otherwise display a space.
  /// ":" (colon) - display only if there is a displayable digit to the
  ///   immediate left of this symbol, otherwise display a space.
  /// "/" (foreslash) - display only if there is a displayable digit to the
  ///   immediate left of this symbol, otherwise display a space.
  /// "(" (left paren) - display only if there is a displayable digit to the
  ///   right of this symbol, otherwise display a space.
  /// ")" (right paren) - display only if there is a displayable digit to the
  ///   left of this symbol, otherwise display a space. 
  /// </para>
  /// </remarks>
  public static string NumberFormat(
    decimal? number,
    string posFormat,
    string negFormat,
    string zeroFormat,
    string nullFormat)
  {
    var pattern = zeroFormat;

    if (number == null)
    {
      if (Length(nullFormat) == 0)
      {
        return null;
      }
      else
      {
        pattern = nullFormat;
        number = 0M;
      }
    }

    try
    {
      var value = number.Value;
      var strNumber = number.ToString();
      var negativeValue = false;

      if (value < 0)
      {
        negativeValue = true;
        pattern = negFormat;
      }
      else if (value > 0)
      {
        pattern = posFormat;
      }
      // else continue

      if (Length(pattern) == 0)
      {
        return strNumber;
      }

      if (CustomNumericFormarString.IsMatch(pattern))
      {
        // the pattern suits for java.text.DecimalFormat
        var chars = pattern.ToCharArray();

        for(var i = 0; i < chars.Length; i++)
        {
          switch(chars[i])
          {
            case 'Z':
            case 'z':
            {
              chars[i] = '#';

              break;
            }
            case '9':
            {
              chars[i] = '0';

              break;
            }
            case '$':
            {
              chars[i] = '\u00A4';

              break;
            }
          }
        }

        var prefix = "";
        var suffix = "";

        if ((chars[0] == '+') || (chars[0] == '-'))
        {
          pattern = new(chars, 1, chars.Length - 1);
          prefix = negativeValue ? "-" : "+";

          if (chars[0] != prefix[0])
          {
            prefix = "";
          }
        }
        else if ((chars[^1] == '+') || (chars[^1] == '-'))
        {
          pattern = new(chars, 0, chars.Length - 1);
          suffix = negativeValue ? "-" : "+";

          if (chars[^1] != suffix[0])
          {
            suffix = "";
          }
        }
        else if (pattern.EndsWith("CR") || pattern.EndsWith("cr"))
        {
          suffix = negativeValue ? pattern[(chars.Length - 2)..] : "";
          pattern = new(chars, 0, chars.Length - 2);
        }
        else if (pattern.EndsWith("DB") || pattern.EndsWith("db"))
        {
          suffix = negativeValue ? "" : pattern[(chars.Length - 2)..];
          pattern = new(chars, 0, chars.Length - 2);
        }
        else
        {
          pattern = new(chars);
        }

        if (negativeValue)
        {
          value = -value;
        }

        strNumber =
          prefix + value.ToString(pattern, CultureInfo.InvariantCulture) + suffix;

        int diff = chars.Length - strNumber.Length;

        if (diff > 0)
        {
          strNumber = String("", diff) + strNumber;
        }
      }
      else
      {
        // TODO: correct implementation of MF edit pattern
      }

      return strNumber;
    }
    catch
    {
      // If an error occurs, the function returns an empty string "".
      return "";
    }
  }

  /// <summary>
  /// Gets a character from a value at a specified position.
  /// </summary>
  /// <param name="value">A value to get characted from.</param>
  /// <param name="position">
  /// An index of string from where substring starts.
  /// </param>
  /// <returns>A result character.</returns>
  public static char CharAt(string value, int position) =>
    string.IsNullOrEmpty(value) || (position > value.Length) ?
      ' ' : value[position > 0 ? position - 1 : 0];

  /// <summary>
  /// Converts a string value to a char.
  /// </summary>
  /// <param name="value">A value to convert.</param>
  /// <returns>A result character</returns>
  /// <exception cref="SystemRuntimeException">
  /// In case of value is a sting longer than one character.
  /// </exception>
  public static char AsChar(string value) =>
    string.IsNullOrEmpty(value) ? ' ' :
      value.Length == 1 ? value[0] :
      throw new ArgumentException(
        "Value is expected to be an empty or single character string.",
        nameof(value));

  /// <summary>
  /// Gets part of the string starting at a specified position and having a
  /// certain length. If the length value is more than range, the substring
  /// returned will be till end of the given string. For eg:
  /// substr("000000000001234", 12, 3) returns 123.
  /// </summary>
  /// <param name="value">A given string.</param>
  /// <param name="position">
  /// An index of string from where substring starts.
  /// </param>
  /// <param name="length">
  /// a number of characters in the substring.
  /// </param>
  /// <returns>a substring from the given string.</returns>
  public static string Substring(string value, int position, int length) =>
    Substring(value, 0, position, length);

  /// <summary>
  /// Pads string to a specified width and then calls Substring().
  /// </summary>
  /// <param name="value">A given string.</param>
  /// <param name="width">A width to pad to.</param>
  /// <param name="position">
  /// An index of string from where substring starts.
  /// </param>
  /// <param name="length">
  /// A number of characters in the substring.
  /// </param>
  /// <returns>A substring from the given string.</returns>
  public static string Substring(
    string value,
    int width,
    int position,
    int length)
  {
    if (value == null)
    {
      value = "";
    }

    var valueLength = value.Length;
    var start = position - 1;

    if (start < 0)
    {
      start = 0;
    }

    if ((length < 1) || (start > valueLength))
    {
      length = 0;
    }

    var end0 = start + length;
    var end = end0 > valueLength ? valueLength : end0;
    var end2 = end0 > width ? width : end0;
    var result = start >= end ? "" : value[start..end];

    return (end2 > end) && (end2 > start) ? 
      String(result, end2 - start) : result;
  }

  /// <summary>
  /// Gets part of the string starting at a specified position and having a
  /// certain length. If the length value is more than range, the substring
  /// returned will be till end of the given string. For eg:
  /// substr("000000000001234", 12, 3) returns 123.
  /// </summary>
  /// <param name="value">a given string.</param>
  /// <param name="position">
  /// an index of string from where substring starts.
  /// </param>
  /// <param name="length">
  /// a number of characters in the substring.
  /// </param>
  /// <returns>a substring from the given string.</returns>
  public static string Substring(string value, int? position, int? length) =>
    (position == null) || (length == null) ? "" :
      Substring(value, position.Value, length.Value);

  /// <summary>
  /// Retrieves a string value with the specified length. The origin string 
  /// value is padded right with spaces, if it's null or its length is less 
  /// than the specified number of characters.
  /// </summary>
  /// <param name="value">a given string.</param>
  /// <param name="length">
  /// a number of characters in the resulting string.
  /// </param>
  /// <returns>a string with the specified length.</returns>
  public static string String(string value, int length)
  {
    if (value == null)
    {
      value = "";
    }

    int valueLength = value.Length;

    return valueLength == length ? value :
      valueLength == 0 ? Spaces(length) :
      valueLength > length ? value[..length] :
      value.PadRight(length, ' ');
  }

  /// <summary>
  /// Gets a string of spaces.
  /// </summary>
  /// <param name="length">A string length.</param>
  /// <returns> string of spaces.</returns>
  public static string Spaces(int length)
  {
    return length switch
    {
      0 => "",
      1 => " ",
      2 => "  ",
      3 => "   ",
      4 => "    ",
      5 => "     ",
      6 => "      ",
      7 => "       ",
      8 => "        ",
      9 => "         ",
      10 => "          ",
      11 => "           ",
      12 => "            ",
      13 => "             ",
      14 => "              ",
      15 => "               ",
      16 => "                ",
      17 => "                 ",
      18 => "                  ",
      19 => "                   ",
      20 => "                    ",
      21 => "                     ",
      22 => "                      ",
      23 => "                       ",
      24 => "                        ",
      25 => "                         ",
      26 => "                          ",
      27 => "                           ",
      28 => "                            ",
      29 => "                             ",
      30 => "                              ",
      31 => "                               ",
      32 => "                                ",
      _ => new(' ', length)
    };
  }

  /// <summary>
  /// Gets part of a byte array starting at a specified position and having a
  /// certain length. If the length value is more than range, the substring
  /// returned will be till end of the given byte array. For eg:
  /// substr("000000000001234", 12, 3) returns 123.
  /// </summary>
  /// <param name="value">a given byte array.</param>
  /// <param name="position">
  /// an index of byte array from where substring starts.
  /// </param>
  /// <param name="length">
  /// a number of bytes in the substring.
  /// </param>
  /// <returns>a substring from the given byte array.</returns>
  public static byte[] Substring(byte[] value, int position, int length)
  {
    if (value == null)
    {
      return Array.Empty<byte>();
    }

    int start = position - 1;

    // according with bug #855
    if (start < 0)
    {
      start = 0;
    }

    if ((length < 1) || (start > value.Length))
    {
      return Array.Empty<byte>();
    }

    int end = start + length;

    if (end > value.Length)
    {
      end = value.Length;
    }

    if (start == end)
    {
      return Array.Empty<byte>();
    }

    var result = new byte[end - start];

    for(var i = 0; i < result.Length; ++i)
    {
      result[i] = value[start + i];
    }

    return result;
  }

  /// <summary>
  /// Gets part of a byte array starting at a specified position and having a
  /// certain length. If the length value is more than range, the substring
  /// returned will be till end of the given byte array. For eg:
  /// substr("000000000001234", 12, 3) returns 123.
  /// </summary>
  /// <param name="value">a given byte array.</param>
  /// <param name="position">
  /// an index of byte array from where substring starts.
  /// </param>
  /// <param name="length">
  /// a number of bytes in the substring.
  /// </param>
  /// <returns>a substring from the given byte array.</returns>
  public static byte[] Substring(byte[] value, int? position, int? length) =>
    (position == null) || (length == null) ? Array.Empty<byte>() :
      Substring(value, position.Value, length.Value);

  /// <summary>
  /// Retrieves a byte array with the specified length. The origin byte array
  /// value is padded right with 0, if it's null or its length is less 
  /// than the specified number of bytes.
  /// </summary>
  /// <param name="value">a given byte array.</param>
  /// <param name="length">
  /// a number of bytes in the resulting byte array.
  /// </param>
  /// <returns>a byte array with the specified length.</returns>
  public static byte[] String(byte[] value, int length)
  {
    if (length <= 0)
    {
      return Array.Empty<byte>();
    }
    else if (value == null)
    {
      return new byte[length];
    }
    else if (value.Length == length)
    {
      return value;
    }
    else if (value.Length > length)
    {
      return Substring(value, 0, length);
    }
    else
    {
      var result = new byte[length];

      for(var i = 0; i < value.Length; ++i)
      {
        result[i] = value[i];
      }

      return result;
    }
  }

  /// <summary>
  /// Concatenates two byte arrays.
  /// </summary>
  /// <param name="first">A first byte array.</param>
  /// <param name="second">A second byte array.</param>
  /// <returns>concatenated byte array.</returns>
  public static byte[] Concat(byte[] first, byte[] second)
  {
    if ((first == null) || (first.Length == 0))
    {
      if (second == null)
      {
        return Array.Empty<byte>();
      }

      return second;
    }

    if ((second == null) || (second.Length == 0))
    {
      return first;
    }

    var result = new byte[first.Length + second.Length];

    for(int i = 0; i < first.Length; ++i)
    {
      result[i] = first[i];
    }

    for(int i = 0; i < second.Length; ++i)
    {
      result[first.Length + i] = second[i];
    }

    return result;
  }

  /// <summary>
  /// Returns an index of search string within value string.
  /// Index is 1 based. If searched value is not found then 0 is returned.
  /// </summary>
  /// <param name="value">A value to look into.</param>
  /// <param name="search">A value to search.</param>
  /// <returns>
  /// One based index of searched string, or zero if value is not found.
  /// </returns>
  public static int Find(string value, string search) =>
    string.IsNullOrEmpty(value) || string.IsNullOrEmpty(search) ? 0 :
      value.IndexOf(search) + 1;

  /// <summary>
  /// Removes leading and trailing spaces from a string. 
  /// </summary>
  /// <param name="value">a value to trim.</param>
  /// <returns>A trimmed value.</returns>
  public static string Trim(string value) =>
    value?.Trim(Space) ?? "";

  /// <summary>
  /// Removes leading spaces from a string. 
  /// </summary>
  /// <param name="value">a value to trim.</param>
  /// <returns>A trimmed value.</returns>
  public static string TrimStart(string value) =>
    value?.TrimStart(Space) ?? "";

  /// <summary>
  /// Removes trailing spaces from a string. 
  /// </summary>
  /// <param name="value">a value to trim.</param>
  /// <returns>a trimmed value.</returns>
  public static string TrimEnd(string value) =>
    value?.TrimEnd(Space) ?? "";

  /// <summary>
  /// Removes leading and trailing whitespace characters from a string. 
  /// </summary>
  /// <param name="value">a value to trim.</param>
  /// <returns>a trimmed value.</returns>
  public static string TrimWhitespaces(string value) =>
    value?.Trim() ?? "";

  /// <summary>
  /// Removes leading whitespace characters from a string. 
  /// </summary>
  /// <param name="value">a value to trim.</param>
  /// <returns>a trimmed value.</returns>
  public static string TrimWhitespacesStart(string value) =>
    value?.TrimStart() ?? "";

  /// <summary>
  /// Removes trailing whitespace characters from a string. 
  /// </summary>
  /// <param name="value">a value to trim.</param>
  /// <returns>a trimmed value.</returns>
  public static string TrimWhitespacesEnd(string value) =>
    value?.TrimEnd() ?? "";

  /// <summary>
  /// Returns the number of characters up to the rightmost 
  /// non-whitespace character in the given string. 
  /// </summary>
  /// <param name="value">a value to get length for.</param>
  /// <returns>a length of the right-trimmed string.</returns>
  public static int TrimLength(string value)
  {
    if (value != null)
    {
      for(var i = value.Length; i-- > 0;)
      {
        if (!char.IsWhiteSpace(value[i]))
        {
          return i + 1;
        }
      }
    }

    return 0;
  }

  /// <summary>
  /// Returns the number of characters up to the rightmost non-space character
  /// in the given string. 
  /// </summary>
  /// <param name="value">a value to get length for.</param>
  /// <returns>a length of the right-trimmed string.</returns>
  public static int TrimSpacesLength(string value)
  {
    if (value != null)
    {
      for(var i = value.Length; i-- > 0;)
      {
        if (value[i] != ' ')
        {
          return i + 1;
        }
      }
    }

    return 0;
  }

  /// <summary>
  /// Returns a length of the specified string value.
  /// </summary>
  /// <param name="value">a value to get length for.</param>
  /// <returns>
  /// a length of the string or 0 when the specified value is null.
  /// </returns>
  public static int Length(string value) => value?.Length ?? 0;

  /// <summary>
  /// Tests whether a first string is like a search pattern.
  /// An sql LIKE pattern is used for a search pattern.
  /// </summary>
  /// <param name="value">a source string value being tested.</param>
  /// <param name="search">a string used for testing string.</param>
  /// <returns>
  /// true when string look like search string, and false otherwise.
  /// </returns>
  public static bool Like(string value, string search)
  {
    if ((value == null) || (search == null))
    {
      return false;
    }

    var builder = new StringBuilder();
    var length = search.Length;

    var i = 0;
    var escapeCharacter = '\x0';

    while(i < length)
    {
      var c = search[i];

      if (c == escapeCharacter)
      {
        i++;
        c = search[i];
        builder.Append(c);
      }
      else if (c == '%')
      {
        builder.Append(".*");
      }
      else if (c == '_')
      {
        builder.Append('.');
      }
      else
      {
        builder.Append(c);
      }

      i++;
    }

    var regex = new Regex(builder.ToString());

    return regex.IsMatch(value);
  }

  /// <summary>
  /// Given two input strings, confirms that any character in the first
  /// string is also present in the second string. If a character in 
  /// the first string is not in the second string, the position of 
  /// this character is returned. If all characters in the first string 
  /// are also in the second string, a zero is returned.
  /// </summary>
  /// <param name="firstString">a string to verify.</param>
  /// <param name="secondString">a pattern string.</param>
  /// <returns>
  /// the position of a character in first string which is not found in
  /// second string. If all characters in first string are present in second
  /// string, returns zero.
  /// </returns>
  public static int Verify(string firstString, string secondString)
  {
    firstString = TrimEnd(firstString);

    if (firstString.Length == 0)
    {
      firstString = " ";
    }

    if (string.IsNullOrEmpty(secondString))
    {
      return 1;
    }

    for(var i = 0; i < firstString.Length; ++i)
    {
      if (!secondString.Contains(firstString[i]))
      {
        return i + 1;
      }
    }

    return 0;
  }

  /// <summary>
  /// Finds a character or string within a given source string and replaces
  /// the occurrence with another character or string.  If the string 
  /// is not found, the original source string is returned.  
  /// The string comparisons are case-sensitive. 
  /// </summary>
  /// <param name="source">
  /// the string in which you intend to replace a character or string.
  /// </param>
  /// <param name="find">
  /// the character or string for which you are looking in  the given source.
  /// </param>
  /// <param name="replace">
  /// the character or string with which to replace the find value.
  /// </param>
  /// <returns>
  /// the modified source string or the original source string, 
  /// if the find string was not found. the function returns 
  /// an empty string "" if an error occurs. 
  /// </returns>
  public static string Replace(string source, string find, string replace) =>
    source == null ? "" :
      (find == null) || (replace == null) ? source :
      source.Replace(find, replace);

  /// <summary>
  /// Returns first argument if value is different from null value,
  /// otherwise returns null.
  /// </summary>
  /// <typeparam name="T">A value type.</typeparam>
  /// <param name="value">A value.</param>
  /// <param name="nullValue">A null value.</param>
  /// <returns>A value or null.</returns>
  public static T NullIf<T>(T value, T nullValue)
    where T : class, IEquatable<T> =>
    (value == null) || value.Equals(nullValue) ? null : value;

  /// <summary>
  /// Returns first argument if value is different from null value,
  /// otherwise returns null.
  /// </summary>
  /// <param name="value">A value.</param>
  /// <param name="nullValue">A null value.</param>
  /// <returns>A value or null.</returns>
  public static string NullIf (string value, string nullValue) =>
    Equal(value, nullValue) ? null : value;

  /// <summary>
  /// <para>Concatinates strings.</para>
  /// <para>Every argument with the exception of strings containing ONLY white space
  /// characters (both literal strings and string views), trailing white space
  /// characters are removed from all strings before concatenation.</para>
  /// </summary>
  /// <param name="args">An array of strings to cancatinate.</param>
  /// <returns>A result string.</returns>
  public static string Add(params string[] args) =>
    args == null ? "" : string.Join(
      "",
      args.
        Select(item => IsEmpty(item) ? item : TrimWhitespacesEnd(item)));

  /// <summary>
  /// Returns a copy of the specified string value converted to
  /// upper case.
  /// </summary>
  /// <param name="value">A string value to convert.</param>
  /// <returns>A string value converted to upper case.</returns>
  public static string ToUpper(string value) => value?.ToUpper() ?? "";

  /// <summary>
  /// Returns a copy of the specified string value converted to
  /// lower case.
  /// </summary>
  /// <param name="value">A string value to convert.</param>
  /// <returns>A string value converted to lower case.</returns>
  public static string ToLower(string value) => value?.ToLower() ?? "";

  /// <summary>
  /// Converts boolean to string.
  /// </summary>
  /// <param name="value">A value to convert.</param>
  /// <returns>A string value.</returns>
  public static string ToString(bool value) => value ? "True" : "False";

  /// <summary>
  /// Converts boolean to string.
  /// Synonym of <see cref="ToString(bool)"/>.
  /// </summary>
  /// <param name="value">A value to convert.</param>
  /// <returns>A string value.</returns>
  public static string AsString(bool value) => ToString(value);

  /// <summary>
  /// Converts string value to boolean.
  /// </summary>
  /// <param name="value">A value to convert.</param>
  /// <returns>A boolean value.</returns>
  public static bool ToBoolean(string value) =>
    !string.IsNullOrWhiteSpace(value) &&
      (string.Compare(value, "False", true) != 0) &&
      (value != "0");

  /// <summary>
  /// Checks length of integer part of the specified numeric value. 
  /// Throws an exception in order when an absolute value of the specified 
  /// number is bigger then allowed.
  /// <b>Note:</b> the function will be used for debug purposes for 
  /// directly generated code only.  
  /// </summary>
  /// <param name="number">a numeric value to check.</param>
  /// <param name="length">
  /// a length of integer part of the specified value.
  /// </param>
  public static void CheckOverflow(object number, int length)
  {
    if (number is not IConvertible convertibleValue)
    {
      return;
    }

    decimal value = convertibleValue.ToDecimal(null);

    if (value < 0)
    {
      value = -value;
    }

    if (value >= DecimalValues[length])
    {
      throw new ArithmeticException(
        "Numeric overflow: the current value " +
        convertibleValue.ToString() +
        " is bigger than or equalt to " +
        DecimalValues[length]);
    }
  }

  /// <summary>
  /// Verifies that the value is valid for a specified type and property.
  /// If value is not valid then <see cref="ProhibitedValueException"/> 
  /// is thrown.
  /// </summary>
  /// <typeparam name="T">An instance type.</typeparam>
  /// <param name="name">A property name.</param>
  /// <param name="value">a value to test.</param>
  public static void CheckValid<T>(string name, object value) =>
    CheckValid(typeof(T), name, value);

  /// <summary>
  /// Verifies that the value is valid for a specified type and property.
  /// If value is not valid then <see cref="ProhibitedValueException"/> 
  /// is thrown.
  /// </summary>
  /// <param name="instance">an instance or class type.</param>
  /// <param name="name">a property name.</param>
  /// <param name="value">a value to test.</param>
  public static void CheckValid(object instance, string name, object value)
  {
    if (!IsValid(instance, name, value))
    {
      throw new ProhibitedValueException(name);
    }
  }

  /// <summary>
  /// Tests whether the value is valid for a specified type and property.
  /// </summary>
  /// <typeparam name="T">An instance type.</typeparam>
  /// <param name="name">A property name.</param>
  /// <param name="value">A value to test.</param>
  /// <returns>true if value is valid, and false otherwise.</returns>
  public static bool IsValid<T>(string name, object value) =>
    IsValid(typeof(T), name, value);

  /// <summary>
  /// Tests whether the value is valid for a specified instance and property.
  /// </summary>
  /// <param name="instance">An instance or class type.</param>
  /// <param name="name">A property name.</param>
  /// <param name="value">A value to test.</param>
  /// <returns>true if value is valid, and false otherwise.</returns>
  public static bool IsValid(object instance, string name, object value)
  {
    if ((instance == null) || (name == null))
    {
      return false;
    }

    var type = instance as Type;
    var properties = type != null ? TypeDescriptor.GetProperties(type) :
      TypeDescriptor.GetProperties(instance);
    var property = properties[name];

    if (property == null)
    {
      return false;
    }

    return ValueAttribute.IsValid(property, value);
  }

  /// <summary>
  /// Gets an implicit value for a specified type and property.
  /// </summary>
  /// <typeparam name="T">An instance type.</typeparam>
  /// <typeparam name="R">A value type.</typeparam>
  /// <param name="name">A property name.</param>
  /// <returns>An implicit value.</returns>
  public static R GetImplicitValue<T, R>(string name) =>
    GetImplicitValue<R>(typeof(T), name);

  /// <summary>
  /// Gets an implicit value for a specified type and property.
  /// </summary>
  /// <typeparam name="R">A value type.</typeparam>
  /// <param name="instance">an instance or class type.</param>
  /// <param name="name">a property name.</param>
  /// <returns>An implicit value.</returns>
  public static R GetImplicitValue<R>(object instance, string name)
  {
    if ((instance == null) || (name == null))
    {
      return default;
    }

    var type = instance as Type;
    var properties = type != null ? TypeDescriptor.GetProperties(type) :
      TypeDescriptor.GetProperties(instance);
    var property = properties[name];

    if (property == null)
    {
      return default;
    }

    var value = ImplicitValueAttribute.GetValue(property);

    return value == null ? default :
      (R)ValueAttribute.Convert(value, typeof(R));
  }

  /// <summary>
  /// Parses command line arguments.
  /// </summary>
  /// <param name="args">an arguments string.</param>
  /// <returns>an array of arguments.</returns>
  public static string[] ParseCommandLine(string args)
  {
    if (IsEmpty(args))
    {
      return Array.Empty<string>();
    }

    var result = new List<string>();
    var arg = new StringBuilder();
    var quote = false;

    foreach(var c in args)
    {
      if (c == '"')
      {
        quote = !quote;
      }
      else if (!quote && (c == ' '))
      {
        if (arg.Length > 0)
        {
          result.Add(arg.ToString());
          arg.Length = 0;
        }
      }
      else
      {
        arg.Append(c);
      }
    }

    if (arg.Length > 0)
    {
      result.Add(arg.ToString());
    }

    return result.ToArray();
  }

  /// <summary>
  /// Gets a dictionary of named object as a enumerable.
  /// </summary>
  /// <typeparam name="T">An item type.</typeparam>
  /// <param name="map">A dictionary instance.</param>
  /// <returns>A enumerable of named objects.</returns>
  public static IEnumerable<T> ToEnumerable<T>(IDictionary<string, T> map)
    where T : INamed =>
    map == null ? Array.Empty<T>() : map.Values;

  /// <summary>
  /// Gets a dictionary of named objects from a enumerable.
  /// </summary>
  /// <typeparam name="T">An item type.</typeparam>
  /// <param name="items">An item enumerable.</param>
  /// <returns>A dictionary of named objects.</returns>
  public static Dictionary<string, T> ToDictionary<T>(IEnumerable<T> items)
    where T : INamed
  {
    if (items == null)
    {
      return null;
    }

    var map = items.ToDictionary(item => item.Name);

    return map.Count == 0 ? null : map;
  }

  /// <summary>
  /// Disposes resources, if any.
  /// </summary>
  /// <param name="values">optional resources to dispose.</param>
  public static void Dispose<V>(IEnumerable<V> resources)
  {
    if (resources != null)
    {
      List<Exception> exceptions = null;

      foreach(V resource in resources)
      {
        if (resource is IDisposable disposable)
        {
          try
          {
            disposable.Dispose();
          }
          catch(Exception e)
          {
            exceptions ??= new(1);
            exceptions.Add(e);
          }
        }
      }

      if (exceptions != null)
      {
        throw exceptions.Count == 1 ?
          exceptions[0] :
          new AggregateException(exceptions);
      }
    }
  }

  /// <summary>
  /// Disposes attributes, if any.
  /// </summary>
  /// <param name="attributes">optional attributes map.</param>
  public static void Dispose<K, V>(IDictionary<K, V> attributes)
  {
    if (attributes != null)
    {
      Dispose(attributes.Values);
      attributes.Clear();
    }
  }

  /// <summary>
  /// Produces up to a 9 digit Color from individual Red, Green, and Blue 
  /// color components.
  /// <para>Note: this function implements ColorRefRGB CA:GEN function.
  /// </summary>
  /// <param name="red">a number in the range 0 to 255 specifying 
  /// a red color intensity.</param>
  /// <param name="green">a number in the range 0 to 255 specifying 
  /// a green color intensity.</param>
  /// <param name="blue">a number in the range 0 to 255 specifying 
  /// a blue color intensity.</param>
  /// <returns>a number containing a 9-digit aggregate color that is appropriate 
  /// for the other supported color functions.If all three components are
  /// zero(000), the resulting color is Black.If all three components are
  /// 255 (255, 255, 255), the resulting color is White.</returns>
  public static int Rgb(int red, int green, int blue) =>
    ((blue & 0xFF) << 16) + ((green & 0xFF) << 8) + (red & 0xFF);

  /// <summary>
  /// Converts a hex string value in RGB format to an integer value that 
  /// represents color in BGR format.
  /// </summary>
  /// <param name="color">A hex string value that represents color to convert.</param>
  /// <returns>An integer value that represents color.</returns>
  public static int? FromColor(string color)
  {
    if (string.IsNullOrEmpty(color))
    {
      return null;
    }

    try
    {
      var value = int.Parse(color, NumberStyles.HexNumber);

      return ((value & 0xff) << 16) +
        (value & 0xff00) +
        ((value & 0xff0000) >> 16);
    }
    catch
    {
      return null;
    }
  }

  /// <summary>
  /// Returns a string with the indicated number of substrings. 
  /// A substring can be an individual character.
  /// </summary>
  /// <param name="substring">
  /// a string to be repeated by the number of occurrences. 
  /// For example, if substring is the text value "Abc" and 
  /// the number of occurrences is 2, the result is "AbcAbc".
  /// </param>
  /// <param name="number">a number of times the Substring will be repeated.</param>
  /// <returns>a string containing the result.</returns>
  public static string Repeat(string substring, int number)
  {
    if (string.IsNullOrEmpty(substring))
    {
      return substring;
    }

    if (number < 1)
    {
      return "";
    }

    if (substring.Length == 1)
    {
      return new(substring[0], number);
    }

    var result = new StringBuilder(substring);

    for(int i = 1; i < number; i++)
    {
      result.Append(substring);
    }

    return result.ToString();
  }

  /// <summary>
  /// Returns a string with the indicated number of substrings. 
  /// A substring can be an individual character.
  /// </summary>
  /// <param name="substring">
  /// a string to be repeated by the number of occurrences. 
  /// For example, if substring is the text value "Abc" and 
  /// the number of occurrences is 2, the result is "AbcAbc".
  /// </param>
  /// <param name="number">a number of times the Substring will be repeated.</param>
  /// <returns>a string containing the result.</returns>
  public static string Repeat(char substring, int number) => new(substring, number);

  /// <summary>
  /// Removes the specified number of characters from the end of a string.
  /// </summary>
  /// <param name="value">
  /// a string from which the ending number of chars is removed.
  /// </param>
  /// <param name="number">
  /// a number of characters to be removed from the end of the value.
  /// </param>
  /// <returns>
  /// a string containing the remaining text after this function is complete.
  /// If the number of chars exceeds the length of value, the returning string is empty.
  /// </returns>
  public static string RightRemove(string value, int number) =>
    string.IsNullOrEmpty(value) ? value :
      value.Length < number ? "" : value[..^number];

  /// <summary>
  /// Removes the specified number of characters from the beginning of a string.
  /// </summary>
  /// <param name="value">
  /// a string from which the beginning number of chars is removed.
  /// </param>
  /// <param name="number">
  /// a number of characters to be removed from the beginning of the value.
  /// </param>
  /// <returns>
  /// a string containing the remaining text after this function is complete.
  /// If the number of chars exceeds the length of value, the returning string is empty.
  /// </returns>
  public static string LeftRemove(string value, int number) =>
    string.IsNullOrEmpty(value) ? value :
      value.Length < number ? "" : value[number..];

  /// <summary>
  /// Returns index of first item, starting from a specified index 
  /// that satisfies a predicate.
  /// </summary>
  /// <typeparam name="T">An item type.</typeparam>
  /// <param name="items">Items to test.</param>
  /// <param name="start">A start index.</param>
  /// <param name="predicate">A test predicate.</param>
  /// <returns>Index of item, or -1 if no item is found.</returns>
  public static int Find<T>(
    IList<T> items,
    int start,
    Predicate<T> predicate)
  {
    if (items == null)
    {
      return -1;
    }

    if (start < 0)
    {
      start = 0;
    }

    for(int i = start; i < items.Count; ++i)
    {
      var item = items[i];

      if ((item != null) && predicate(item))
      {
        return i;
      }
    }

    return -1;
  }

  /// <summary>
  /// Calls setter upon list item.
  /// </summary>
  /// <typeparam name="T">An item type.</typeparam>
  /// <typeparam name="V">A value type.</typeparam>
  /// <param name="items">Items to set value for.</param>
  /// <param name="index">An item index.</param>
  /// <param name="value">A value to set.</param>
  /// <param name="setter">A setter operation.</param>
  public static void Set<T, V>(
    IList<T> items,
    int index,
    V value,
    Action<T, V> setter)
  {
    if ((items != null) && (index >= 0) && (index < items.Count))
    {
      var item = items[index];

      if (item != null)
      {
        setter(item, value);
      }
    }
  }

  /// <summary>
  /// Tests whether a specified selector indicates a highlighted value.
  /// </summary>
  /// <param name="selectable">A <see cref="ISelectable"/> instance.</param>
  /// <returns>
  /// <c>true</c> for the highlighted, and <c>false</c> otherwise.
  /// </returns>
  public static bool Highlighted(object selectable)
  {
    var value = CharAt((selectable as ISelectable)?.Selector, 1);

    return (value == '*') || (value == '+') || (value == '>');
  }

  /// <summary>
  /// Tests whether a specified selector indicates a clicked value.
  /// </summary>
  /// <param name="selectable">A <see cref="ISelectable"/> instance.</param>
  /// <returns>
  /// <c>true</c> for the clicked, and <c>false</c> otherwise.
  /// </returns>
  public static bool Clicked(object selectable) =>
    Highlighted(selectable);

  /// <summary>
  /// Marks <see cref="ISelectable"/> instance as higlighted or unhiglighted.
  /// </summary>
  /// <param name="selectable">A <see cref="ISelectable"/> instance.</param>
  /// <param name="value">
  /// <c>true</c> for highlight, and <c>false</c> for unhighlight.
  /// </param>
  public static void Highlight(object selectable, bool value)
  {
    var selectableInstance = selectable as ISelectable;
    var selector = CharAt(selectableInstance?.Selector, 1);
    var highlighted =
      (selector == '*') || (selector == '+') || (selector == '>');

    if ((value != highlighted) && (selector != 'H'))
    {
      selectableInstance.Selector =
        value ?
          selector == '<' ? ">" : "*" :
          selector == '>' ? "<" : " ";
    }
  }

  /// <summary>
  /// Tests whether a specified selector indicates a visible value.
  /// </summary>
  /// <param name="selectable">A <see cref="ISelectable"/> instance.</param>
  /// <returns>
  /// <c>true</c> for the visible, and <c>false</c> otherwise.
  /// </returns>
  public static bool Visible(object selectable) =>
    CharAt((selectable as ISelectable)?.Selector, 1) != 'H';

  /// <summary>
  /// Marks <see cref="ISelectable"/> instance as visible or hidden.
  /// </summary>
  /// <param name="selectable">A <see cref="ISelectable"/> instance.</param>
  /// <param name="value">
  /// <c>true</c> for visible, and <c>false</c> for hidden.
  /// </param>
  public static void Visible(object selectable, bool value)
  {
    if ((selectable is ISelectable selectableInstance) &&
      (Visible(selectable) != value))
    {
      selectableInstance.Selector = value ? " " : "H";
    }
  }

  /// <summary>
  /// Returns index of first highlighted item, starting from 
  /// a specified index.
  /// </summary>
  /// <typeparam name="T">An item type.</typeparam>
  /// <param name="items">Items to test.</param>
  /// <param name="start">A start index.</param>
  /// <returns>Index of item, or -1 if no item is found.</returns>
  public static int Highlighted<T>(IList<T> items, int start) =>
    Find(items, start, item => Highlighted(item));

  /// <summary>
  /// Returns index of first clicked item, starting from a specified index.
  /// </summary>
  /// <typeparam name="T">An item type.</typeparam>
  /// <param name="items">Items to test.</param>
  /// <param name="start">A start index.</param>
  /// <returns>Index of item, or -1 if no item is found.</returns>
  public static int Clicked<T>(IList<T> items, int start) =>
    Find(items, start, item => Clicked(item));

  /// <summary>
  /// Marks item at index as higlighted or unhiglighted.
  /// </summary>
  /// <typeparam name="T">An item type.</typeparam>
  /// <param name="items">Items to set value for.</param>
  /// <param name="index">An item index.</param>
  /// <param name="value">A value to set.</param>
  public static void Highlight<T>(IList<T> items, int index, bool value) =>
    Set(items, index, value, (item, v) => Highlight(item, v));

  /// <summary>
  /// Returns index of first visible item, starting from a specified index.
  /// </summary>
  /// <typeparam name="T">An item type.</typeparam>
  /// <typeparam name="T">An item type.</typeparam>
  /// <param name="items">Items to test.</param>
  /// <param name="start">A start index.</param>
  /// <returns>Index of item, or -1 if no item is found.</returns>
  public static int Visible<T>(IList<T> items, int start) =>
    Find(items, start, item => Visible(item));

  /// <summary>
  /// Marks item at index as visible or hidden.
  /// </summary>
  /// <typeparam name="T">An item type.</typeparam>
  /// <param name="items">Items to set value for.</param>
  /// <param name="index">An item index.</param>
  /// <param name="value">A value to set.</param>
  public static void Visible<T>(IList<T> items, int index, bool value) =>
    Set(items, index, value, (item, v) => Visible(item, v));

  /// <summary>
  /// Marks items either visible or hidden depending of predicate.
  /// </summary>
  /// <typeparam name="T">An item type.</typeparam>
  /// <param name="items">Items to filter.</param>
  /// <param name="predicate">A test predicate.</param>
  public static void Filter<T>(IList<T> items, Predicate<T> predicate)
  {
    if (items != null)
    {
      foreach(var item in items)
      {
        if (predicate(item))
        {
          Visible(item, false);
        }
      }
    }
  }

  /// <summary>
  /// Marks items either visible or hidden depending of predicate.
  /// </summary>
  /// <typeparam name="T">An item type.</typeparam>
  /// <param name="items">Items to filter.</param>
  /// <param name="expression">an expression for the filter.</param>
  /// <param name="getter">An attribute getter.</param>
  public static void Filter<T>(
    IList<T> items,
    string expression,
    Func<T, int, object> getter) =>
    Filter(items, GetFilter(expression, getter));

  /// <summary>
  /// Marks items as visible.
  /// </summary>
  /// <typeparam name="T">An item type.</typeparam>
  /// <param name="items">Items to unfilter.</param>
  public static void Unfilter<T>(IList<T> items)
  {
    if (items != null)
    {
      foreach(var item in items)
      {
        Visible(item, true);
      }
    }
  }

  /// <summary>
  /// Gets a filter predicate function.
  /// </summary>
  /// <typeparam name="T">
  /// A type of class whose properties are resolved.
  /// </typeparam>
  /// <param name="expression">an expression for the filter.</param>
  /// <param name="getter">An attribute getter.</param>
  /// <returns>
  /// A function that returns true if value is accepted, and false otherwise.
  /// </returns>
  /// <see cref="FilterExpression{T}"/>
  public static Predicate<T> GetFilter<T>(
    string expression,
    Func<T, int, object> getter) =>
    new FilterExpression<T>(expression, getter).Test;

  /// <summary>
  /// Sorts the elements in the entire Array(Of T) 
  /// using the specified comparer.
  /// </summary>
  /// <typeparam name="T">An item type.</typeparam>
  /// <param name="items">An array to sort.</param>
  /// <param name="comparer">A sort comparer.</param>
  public static void Sort<T>(Array<T> items, IComparer<T> comparer)
    where T : class, new() =>
    items?.Sort(comparer);

  /// <summary>
  /// Sorts the elements in the entire Array(Of T) dynamic comparer.
  /// </summary>
  /// <typeparam name="T">An item type.</typeparam>
  /// <param name="items">An array to sort.</param>
  /// <param name="expression">an expression for the filter.</param>
  /// <param name="getter">An attribute getter.</param>
  /// <param name="collator">Optional string collator.</param>
  public static void Sort<T>(
    Array<T> items,
    string expression,
    Func<T, int, object> getter,
    IComparer<string> collator = null)
    where T : class, new() =>
    items?.Sort(GetComparer(expression, getter, collator));

  /// <summary>
  /// Gets a comparer for a sort expression.
  /// </summary>
  /// <typeparam name="T">
  /// A type of class whose properties are resolved.
  /// </typeparam>
  /// <param name="expression">
  ///   <para>a sort expression.</para>
  /// <param name="getter">An attribute getter.</param>
  /// <param name="collator">Optional string collator.</param>
  /// <returns>
  /// A <see cref="IComparer{T}"/> instance that could be used 
  /// in a sort operation.
  /// </returns>
  /// <see cref="SortExpression{T}"/>
  public static IComparer<T> GetComparer<T>(
    string expression,
    Func<T, int, object> getter,
    IComparer<string> collator = null) =>
    new SortExpression<T>(expression, getter, collator);

  /// <summary>
  /// Marks a specified item in the list for display at top.
  /// </summary>
  /// <typeparam name="T">An item type.</typeparam>
  /// <param name="items">A list of items.</param>
  /// <param name="index">An index of item to mark.</param>
  public static void Display<T>(IList<T> items, int index)
  {
    for(int i = 0; i < items.Count; ++i)
    {
      if (items[i] is not ISelectable item)
      {
        continue;
      }

      var selector = CharAt(item.Selector, 1);

      if (selector != 'H')
      {
        if (i == index)
        {
          if (selector != '>')
          {
            if ((selector == '*') || (selector == '+'))
            {
              item.Selector = ">";
            }
            else
            {
              item.Selector = "<";
            }
          }
        }
        else
        {
          if (selector == '>')
          {
            item.Selector = "*";
          }
          else if (selector == '<')
          {
            item.Selector = " ";
          }
          // No more cases.
        }
      }
    }
  }

  /// <summary>
  /// <para>
  /// Moves content of list within open range <code>[start, end)</code>. 
  /// <code>to</code> should belong to that range.
  /// </para>
  /// <para>
  /// <code>list[start + (to - start + i) mod (end - start)] = 
  /// list[start + i]</code>, 
  /// where i in range<code>[0, end - start)</ code >.
  /// </para>
  /// </summary>
  /// <typeparam name="T">An element type.</typeparam>
  /// <param name="list">A list to move data withing.</param>
  /// <param name="start">Start position, including.</param>
  /// <param name="end">End position, not incuding.</param>
  /// <param name="to">Target position.</param>
  public static void CircularMove<T>(
    IList<T> list,
    int start,
    int end,
    int to)
  {
    var size = end - start;
    var step = to - start;

    if ((size == 0) || (step == 0))
    {
      return;
    }

    var anchor = start;
    var pos = start;
    var item = list[pos];

    for(int i = 0; i < size; ++i)
    {
      pos += step;

      if (pos >= end)
      {
        pos -= size;
      }

      var next = list[pos];

      list[pos] = item;
      item = next;

      if (pos == anchor)
      {
        pos = ++anchor;

        if (pos >= end)
        {
          break;
        }

        item = list[pos];
      }
    }
  }

  /// <summary>
  /// A space char array.
  /// </summary>
  private static readonly char[] Space = { ' ' };

  /// <summary>
  /// An array of decimal values (power of 10).
  /// </summary>
  private static readonly decimal[] DecimalValues = {
      1M,
      10M,
      100M,
      1000M,
      10000M,
      100000M,
      1000000M,
      10000000M,
      100000000M,
      1000000000M,
      10000000000M,
      100000000000M,
      1000000000000M,
      10000000000000M,
      100000000000000M,
      1000000000000000M,
      10000000000000000M,
      100000000000000000M,
      1000000000000000000M,
      10000000000000000000M,
      100000000000000000000M,
      1000000000000000000000M,
      10000000000000000000000M,
      100000000000000000000000M,
      1000000000000000000000000M,
      10000000000000000000000000M,
      100000000000000000000000000M,
      1000000000000000000000000000M,
      10000000000000000000000000000M
    };

  /// <summary>
  /// A regular expression to check whether a pattern matches a custom numeric format string.
  /// </summary>
  private static readonly Regex CustomNumericFormarString =
    new("^([+$-]?[Z9,]+[.]?[9Z]*|[Z9,]+[.]?[9Z]*[+$-]?|[Z9,]+[.]?[9Z]*(CR|DB))$");
}
