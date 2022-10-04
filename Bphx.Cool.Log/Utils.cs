using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

using static Bphx.Cool.Functions;
using System.Text.Encodings.Web;
using Bphx.Cool.Impl;

namespace Bphx.Cool.Log;

/// <summary>
/// Utility API used in logging.
/// </summary>
public static class Utils
{
  /// <summary>
  /// Makes string valid according xml 1.0 spec.
  /// Invalid characters are replaced with spaces.
  /// </summary>
  /// <param name="value">A value to convert.</param>
  /// <returns>Converted value.</returns>
  public static string ToXmlValue(string value)
  {
    if (value == null)
    {
      return null;
    }

    for(var i = 0; i < value.Length; i++)
    {
      var c = value[i];

      // http://www.w3.org/TR/xml/#charsets
      // Character Range
      // [2] Char ::= #x9 | #xA | #xD | 
      //              [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]
      //
      // Note that we have implemented weaker conditions.
      if (((c < ' ') && (c != '\r') && (c != '\n') && (c != '\t')) ||
        (c == '&') ||
        (c == '<'))
      {
        var result = new StringBuilder(value)
        {
          Length = i
        };

        for(; i < value.Length; ++i)
        {
          c = value[i];

          if ((c < ' ') && (c != '\r') && (c != '\n') && (c != '\t'))
          {
            result.Append(' ');
          }
          else if (c == '&')
          {
            result.Append("&amp;");
          }
          else if (c == '<')
          {
            result.Append("&lt;");
          }
          else
          {
            result.Append(c);
          }
        }

        return result.ToString();
      }
    }

    return value;
  }

  /// <summary>
  /// Unescapes xml values escaped with <see cref="ToXmlValue(string)"/>
  /// </summary>
  /// <param name="value">A value to convert.</param>
  /// <returns>Converted value.</returns>
  public static string FromXmlValue(string value)
  {
    if (value == null)
    {
      return null;
    }

    for(int i = 0; i < value.Length; i++)
    {
      var c = value[i];

      if (c == '&')
      {
        var result = new StringBuilder(value[..i]);

        while(true)
        {
          if (c != '&')
          {
            result.Append(c);
            ++i;
          }
          else if (StartsWith(value, "amp;", i + 1))
          {
            result.Append('&');
            i += 5;
          }
          else if (StartsWith(value, "lt;", i + 1))
          {
            result.Append('<');
            i += 4;
          }
          else if (StartsWith(value, "#13;", i + 1))
          {
            result.Append('\r');
            i += 5;
          }
          else if (StartsWith(value, "#10;", i + 1))
          {
            result.Append('\n');
            i += 5;
          }
          else if (StartsWith(value, "#44;", i + 1))
          {
            result.Append(',');
            i += 5;
          }
          else
          {
            result.Append('&');
            ++i;
          }

          if (i >= value.Length)
          {
            break;
          }

          c = value[i];
        }

        return result.ToString();
      }
    }

    return value;
  }

  /// <summary>
  /// Converts value to an xml string.
  /// </summary>
  /// <param name="value">A value to convert.</param>
  /// <param name="serializer">A binary serializer.</param>
  /// <returns>Converted value.</returns>
  public static string ToXml(object value, ISerializer serializer = null)
  {
    if (value == null)
    {
      return null;
    }
    else if (value is string stringValue)
    {
      return ToXmlValue(stringValue);
    }
    else if ((value is int) ||
      (value is long) ||
      (value is double) ||
      (value is decimal) ||
      (value is float) ||
      (value is short))
    {
      return value.ToString();
    }
    else if (value is bool boolValue)
    {
      return boolValue ? "true" : "false";
    }
    else if (value is DateTime date)
    {
      return XmlConvert.ToString(
        date,
        XmlDateTimeSerializationMode.Unspecified);
    }
    else if (value is TimeSpan time)
    {
      return XmlConvert.ToString(time);
    }
    else if (value is Exception error)
    {
      var stream = new MemoryStream();

      (serializer ?? new Serializer()).Serilize(error, stream);

      value = new Error
      {
        ExceptionMessage = error.Message,
        ExceptionType = error.GetType().FullName,
        StackTrace = error.StackTrace,
        Data = stream.ToArray()
      };
    }
    else if (value is object[] items)
    {
      value = new Values 
      { 
        Items = items.
          Select(item => item is TimeSpan time ? (XmlTime)time : item).
          ToArray()
      };
    }
    else if (value is IDictionary map)
    {
      value = new Map
      {
        Items = map.Keys.
          OfType<string>().
          Select(name =>
          {
            var value = map[name];

            return new Item 
            { 
              Name = name, 
              Value = value is TimeSpan time ? (XmlTime)time : value
            };
          }).
          ToArray()
      };
    }
    // No more cases

    var type = value.GetType();
    var writer = new StringWriter();
    var xmlWriter = XmlWriter.Create(writer, xmlWriterSettings);
    var xmlSerializer = new XmlSerializer(type);

    xmlSerializer.Serialize(xmlWriter, value);

    return writer.ToString();
  }

  /// <summary>
  /// Converts xml string value into instance.
  /// </summary>
  /// <param name="type">A target type.</param>
  /// <param name="value">A string value.</param>
  /// <param name="serializer">A binary serializer.</param>
  /// <returns>An object instance.</returns>
  public static object FromXml(
    Type type,
    string value,
    ISerializer serializer = null)
  {
    var declaredType = type;

    if (value == null)
    {
      return null;
    }
    else if (type == typeof(string))
    {
      return FromXmlValue(value);
    }
    else if (type == typeof(bool))
    {
      return (value == "true") || (value == "1");
    }
    else if (type == typeof(int))
    {
      return int.Parse(value);
    }
    else if (type == typeof(long))
    {
      return long.Parse(value);
    }
    else if (type == typeof(double))
    {
      return double.Parse(value);
    }
    else if (type == typeof(decimal))
    {
      return decimal.Parse(value);
    }
    else if (type == typeof(float))
    {
      return float.Parse(value);
    }
    else if (type == typeof(short))
    {
      return short.Parse(value);
    }
    else if (type == typeof(DateTime))
    {
      return XmlConvert.ToDateTime(
        value,
        XmlDateTimeSerializationMode.Unspecified);
    }
    else if (type == typeof(TimeSpan))
    {
      return XmlConvert.ToTimeSpan(value);
    }
    else if (typeof(Exception).IsAssignableFrom(type))
    {
      declaredType = typeof(Error);
    }
    else if (type.IsArray)
    {
      declaredType = typeof(Values);
    }
    else if (typeof(IDictionary).IsAssignableFrom(type))
    {
      declaredType = typeof(Map);
    }
    // No more cases

    var xmlSerializer = new XmlSerializer(declaredType);
    var result = xmlSerializer.Deserialize(new StringReader(value));

    if (result == null)
    {
      return null;
    }

    if (declaredType != type)
    {
      if (declaredType == typeof(Values))
      {
        var values = (Values)result;

        result = values.Items.
          Select(item => item is XmlTime time ? (TimeSpan)time : item).
          ToArray();
      }
      else if (declaredType == typeof(Map))
      {
        var values = (Map)result;

        result = values.Items?.
          ToDictionary(
            item => item.Name, 
            item =>
            {
              var value = item.Value;

              return value is XmlTime time ? (TimeSpan)time : value;
            });
      }
      else if (declaredType == typeof(Error))
      {
        var error = (Error)result;
        var stream = new MemoryStream(error.Data);

        result = 
          (serializer ?? new Serializer()).Deserilize<Exception>(stream);
      }
      // No more cases
    }

    return result;
  }

  /// <summary>
  /// Converts value to an json string.
  /// </summary>
  /// <param name="value">A value to convert.</param>
  /// <param name="serializer">A binary serializer.</param>
  /// <returns>Converted value.</returns>
  public static string ToJson(object value, ISerializer serializer = null)
  {
    if (value == null)
    {
      return null;
    }
    else if (value is string stringValue)
    {
      return ToXmlValue(stringValue);
    }
    else if ((value is int) ||
      (value is long) ||
      (value is double) ||
      (value is decimal) ||
      (value is float) ||
      (value is short))
    {
      return value.ToString();
    }
    else if (value is bool boolValue)
    {
      return boolValue ? "true" : "false";
    }
    else if (value is DateTime date)
    {
      return XmlConvert.ToString(
        date,
        XmlDateTimeSerializationMode.Unspecified);
    }
    else if (value is TimeSpan time)
    {
      return XmlConvert.ToString(time);
    }
    else if (value is Exception error)
    {
      var stream = new MemoryStream();

      (serializer ?? new Serializer()).Serilize(error, stream);

      value = new Error
      {
        ExceptionMessage = error.Message,
        ExceptionType = error.GetType().FullName,
        StackTrace = error.StackTrace,
        Data = stream.ToArray()
      };
    }
    // No more cases

    return JsonSerializer.Serialize(value, jsonOptions);
  }

  /// <summary>
  /// Converts json string value into instance.
  /// </summary>
  /// <typeparam name="T">A target type.</typeparam>
  /// <param name="value">A string value.</param>
  /// <param name="serializer">A binary serializer.</param>
  /// <returns>An object instance.</returns>
  public static T FromJson<T>(
    string value, 
    ISerializer serializer = null) =>
    (T)FromJson(typeof(T), value, serializer);

  /// <summary>
  /// Converts json string value into instance.
  /// </summary>
  /// <param name="type">A target type.</param>
  /// <param name="value">A string value.</param>
  /// <param name="serializer">A binary serializer.</param>
  /// <returns>An object instance.</returns>
  public static object FromJson(
    Type type,
    string value,
    ISerializer serializer = null)
  {
    var declaredType = type;

    if (value == null)
    {
      return null;
    }
    else if (type == typeof(string))
    {
      return FromXmlValue(value);
    }
    else if (type == typeof(bool))
    {
      return (value == "true") || (value == "1");
    }
    else if (type == typeof(int))
    {
      return int.Parse(value);
    }
    else if (type == typeof(long))
    {
      return long.Parse(value);
    }
    else if (type == typeof(double))
    {
      return double.Parse(value);
    }
    else if (type == typeof(decimal))
    {
      return decimal.Parse(value);
    }
    else if (type == typeof(float))
    {
      return float.Parse(value);
    }
    else if (type == typeof(short))
    {
      return short.Parse(value);
    }
    else if (type == typeof(DateTime))
    {
      return XmlConvert.ToDateTime(
        value,
        XmlDateTimeSerializationMode.Unspecified);
    }
    else if (type == typeof(TimeSpan))
    {
      return XmlConvert.ToTimeSpan(value);
    }
    else if (typeof(Exception).IsAssignableFrom(type))
    {
      declaredType = typeof(Error);
    }
    // No more cases

    var result = JsonSerializer.Deserialize(value, type, jsonOptions);

    if (result == null)
    {
      return null;
    }

    if (declaredType == typeof(Error))
    {
      var error = (Error)result;
      var stream = new MemoryStream(error.Data);

      result = (serializer ?? new Serializer()).Deserilize<Exception>(stream);
    }

    return result;
  }

  /// <summary>
  /// Compares two instances.
  /// 
  /// All public properties are compared.
  /// For property values supporting <see cref="IComparable"/>, 
  /// the <see cref="IComparable.CompareTo(object?)"/> is called, 
  /// otherwise recursion is applied to compare nested properties.
  /// Lists and arrays are compared on element basis.
  /// 
  /// Note that result of this call is not necessary coincides with
  /// results of <see cref="object.Equals(object?)"/>, or with
  /// <see cref="IComparable.CompareTo(object?)"/> calls.
  /// 
  /// Note that <see cref="Equal(object, object)"/> call may potentially
  /// result in an infinitive recursion, thus it should be used with care.
  /// </summary>
  /// <param name="first">A first instance to compare.</param>
  /// <param name="second">A second instance to compare.</param>
  /// <returns>
  /// <c>true</c> if instances are equal, and <c>false</c> otherwise.
  /// </returns>
  public static bool Equal(object first, object second)
  {
    if (first == second)
    {
      return true;
    }

    if (first == null)
    {
      return second is string stringValue ? IsEmpty(stringValue) :
        second is byte[] bytes ? bytes.Length == 0 :
        second is IComparable comparable && IsNullOrZero(comparable);
    }

    if (second == null)
    {
      return first is string stringValue ? IsEmpty(stringValue) :
        first is byte[] bytes ? bytes.Length == 0 :
        first is IComparable comparable && IsNullOrZero(comparable);
    }

    if (first is ICollection firstCollection)
    {
      if (second is not ICollection secondCollection)
      {
        return false;
      }
      else if ((first is object[] firstItems) &&
        (second is object[] secondItems))
      {
        return (firstItems.Length == secondItems.Length) &&
          firstItems.Zip(secondItems).
          All(pair => pair.First?.ToString() == pair.Second?.ToString());
      }
      else if ((first is IDictionary<string, object> firstMap) &&
        (second is IDictionary<string, object> secondMap))
      {
        return firstMap.
          Where(entry => entry.Value != null).
          All(entry =>
            secondMap.TryGetValue(entry.Key, out var value) &&
            (value != null) &&
            entry.Value.ToString() == value.ToString()) &&
          secondMap.
            Where(entry => entry.Value != null).
            All(entry =>
              firstMap.TryGetValue(entry.Key, out var value) &&
              (value != null));
      }
      else
      {
        int size = firstCollection.Count;

        if (size != secondCollection.Count)
        {
          return false;
        }

        var firstEnumerator = firstCollection.GetEnumerator();
        var secondEnumerator = secondCollection.GetEnumerator();

        for(int i = 0; i < size; ++i)
        {
          if ((firstEnumerator.MoveNext() != secondEnumerator.MoveNext()) ||
            !Equal(firstEnumerator.Current, secondEnumerator.Current))
          {
            return false;
          }
        }

        return true;
      }
    }

    var type = first.GetType();

    if (type != second.GetType())
    {
      return false;
    }

    if (type.IsArray)
    {
      var firstArray = (Array)first;
      var secondArray = (Array)second;
      int size = firstArray.Length;

      if (size != secondArray.Length)
      {
        return false;
      }

      for(int i = 0; i < size; i++)
      {
        if (!Equal(firstArray.GetValue(i), secondArray.GetValue(i)))
        {
          return false;
        }
      }

      return true;
    }

    if (first is string firstString)
    {
      var secondString = (string)second;

      return Functions.Equal(firstString, secondString) ||
        (IsEmpty(firstString) && IsEmpty(secondString));
    }

    if (first is byte[] firstBytes)
    {
      return Functions.Equal(firstBytes, (byte[])second);
    }

    if (first is IComparable firstComparable)
    {
      var secondComparable = (IComparable)second;

      return firstComparable.CompareTo(secondComparable) == 0;
    }

    if (first is Exception firstError)
    {
      var secondError = (Exception)second;

      return Equal(firstError.Message, secondError.Message);
    }

    if (first is Global firstGlobal)
    {
      var secondGlobal = (Global)second;

      return (firstGlobal.Command == secondGlobal.Command) &&
        (firstGlobal.Exitstate == secondGlobal.Exitstate) &&
        (firstGlobal.ExitStateId == secondGlobal.ExitStateId) &&
        (firstGlobal.TranCode == secondGlobal.TranCode) &&
        (firstGlobal.NextTran == secondGlobal.NextTran) &&
        (firstGlobal.UserId == secondGlobal.UserId);
    }

    try
    {
      foreach(PropertyDescriptor property in
        TypeDescriptor.GetProperties(type))
      {
        if (property.Attributes[typeof(ComputedAttribute)] != null)
        {
          continue;
        }

        if (Equal(property.GetValue(first), property.GetValue(second)))
        {
          continue;
        }

        return false;
      }
    }
    catch
    {
      return false;
    }

    return true;
  }

  /// <summary>
  /// Test whether to consider value as empty for the purpose of comparison.
  /// </summary>
  /// <param name="value">A value to test.</param>
  /// <returns><c>true</c> if value is considered empty.</returns>
  private static bool IsEmpty(string value)
  {
    return string.IsNullOrWhiteSpace(value) ||
      (value == "0") ||
      (value == "0.0") ||
      (value == "00:00:00") ||
      (value == "false");
  }

  /// <summary>
  /// Tests whether value contains a search string a specified position.
  /// </summary>
  /// <param name="value">A value string.</param>
  /// <param name="search">A search string.</param>
  /// <param name="index">A first index to check.</param>
  /// <returns>
  /// <c>true</c> if search is found in value ar position index, and 
  /// <c>false</c> otherwise.
  /// </returns>
  private static bool StartsWith(string value, string search, int index)
  { 
    if (value.Length <= index + search.Length)
    {
      return false;
    }

    for(var i = 0; i < search.Length; ++i)
    {
      if (value[index + i] != search[i])
      {
        return false;
      }
    }

    return true;
  }

  /// <summary>
  /// Settings for xml serialization.
  /// </summary>
  private static readonly XmlWriterSettings xmlWriterSettings = new()
  {
    OmitXmlDeclaration = true,
    NewLineChars = "\n"
  };

  /// <summary>
  /// JSON serialization options.
  /// </summary>
  private static readonly JsonSerializerOptions jsonOptions = new()
  {
    WriteIndented = true,
    AllowTrailingCommas = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowReadingFromString,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
  };
}
