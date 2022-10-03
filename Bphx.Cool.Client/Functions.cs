using System;
using System.Runtime.InteropServices;

namespace Bphx.Cool.Client
{
  /// <summary>
  /// A common functions.
  /// </summary>
  [ComVisible(false)]
  public class Functions
  {
    /// <summary>
    /// Minimal DateTime value for COM objects.
    /// </summary>
    private static DateTime MinDateTimeValue = new DateTime(100, 1, 1);

    /// <summary>
    /// OLE min date value for COM objects.
    /// </summary>
    private static DateTime OLENullDateValue = new DateTime(1899, 12, 30);

    /// <summary>
    /// Converts a DateTime value that less than 0100-01-01 to null.
    /// </summary>
    /// <param name="value">A nullable DateTime value to convert.</param>
    /// <returns>A converted DateTime value.</returns>
    public static DateTime? ConvertToDateTime(DateTime? value) => 
      (value.GetValueOrDefault() <= MinDateTimeValue) || 
      (value.GetValueOrDefault() == OLENullDateValue) ? 
        null : value;

    /// <summary>
    /// Converts a nullable DateTime value to a non nullable DateTime value.
    /// </summary>
    /// <param name="value">A value to convert.</param>
    /// <returns>A converted DateTime value.</returns>
    public static DateTime GetValueOrDefault(DateTime? value) =>
      value.HasValue && (value.Value >= MinDateTimeValue) ? 
        value.Value : OLENullDateValue;

    /// <summary>
    /// Converts a nullable string value to a non nullable string value.
    /// </summary>
    /// <param name="value">A value to convert.</param>
    /// <returns>A converted string value.</returns>
    public static string GetValueOrDefault(string value) =>
      value ?? string.Empty;

    /// <summary>
    /// Parses a string value in the specified date-time format.
    /// </summary>
    /// <param name="value">A sting value to parse.</param>
    /// <param name="format">
    /// A format to use in DateTime.ParseExact() method call.
    /// </param>
    /// <returns>A parsed DateTime value.</returns>
    public static DateTime ParseDateTime(string value, string format) => 
      string.IsNullOrEmpty(value) ?
        DateTime.MinValue : DateTime.ParseExact(value, format, null);

    /// <summary>
    /// Parses a string value in the specified date-time format.
    /// </summary>
    /// <param name="value">A sting value to parse.</param>
    /// <param name="format">
    /// A format to use in DateTime.ParseExact() method call.
    /// </param>
    /// <returns>A parsed DateTime value.</returns>
    public static DateTime? StringToTimestamp(string value) => 
      string.IsNullOrEmpty(value) || (value == "00000000000000000000") ?
        (DateTime?)null : DateTime.ParseExact(value, TimestampFormat, null);

    /// <summary>
    /// Converts timestamp value to a string.
    /// </summary>
    /// <param name="timestamp">A timestamp value to convert.</param>
    /// <returns>A string value.</returns>
    public static string TimestampToString(DateTime? timestamp) => 
      timestamp == null ?
        "00000000000000000000" : timestamp.Value.ToString(TimestampFormat);
  
    /// <summary>
    /// Timestamp format.
    /// </summary>
    public static readonly string TimestampFormat = "yyyyMMddHHmmssffffff";
  }
}