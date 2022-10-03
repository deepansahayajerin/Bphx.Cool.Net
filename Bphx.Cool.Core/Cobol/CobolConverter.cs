namespace Bphx.Cool.Cobol;

using System;
using System.Text;

using static Bphx.Cool.Functions;

/// <summary>
/// An utility for conversion to/from COBOL data types.
/// </summary>
public class CobolConverter
{	 
  /// <summary>
  /// A serialization options.
  /// </summary>
  [Flags]
  public enum Options
  {
    /// <summary>
    /// Little endian option.
    /// </summary>
    LittleEndian = 0x1,

    /// <summary>
    /// A binary number format.
    /// </summary>
    Binary = 0x2,

    /// <summary>
    /// A packed decimal number format.
    /// </summary>
    PackedDecimal = 0x4,

    /// <summary>
    /// Indicates unsigned number.
    /// </summary>
    Unsigned = 0x20,

    /// <summary>
    /// Read/write screen attributes.
    /// </summary>
    VideoAttributes = 0x100,

    /// <summary>
    /// Read/write access fields for the attributes.
    /// </summary>
    AccessAttributes = 0x200,

    /// <summary>
    /// Read/write access fields for the groups.
    /// </summary>
    AccessGroups = 0x0400,

    /// <summary>
    /// Read/write access fields.
    /// </summary>
    AccessFields = AccessAttributes | AccessGroups,
  }

  /// <summary>
  /// ASCII indicator.
  /// </summary>
  public readonly bool ascii;

  /// <summary>
  /// The decimal 0.
  /// </summary>
  public readonly byte zero;

  /// <summary>
  /// A fill character.
  /// </summary>
  public readonly byte fillChar;

  /// <summary>
  /// Creates a CobolConverter instance.
  /// </summary>
  /// <param name="encoding">An encoding to use in conversion.</param>
  public CobolConverter(Encoding encoding)
  {
    Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
    zero = encoding.GetBytes("0")[0];
    ascii = zero == 0x30;
    fillChar = (byte)(ascii ? 0x20 : 0x40);
  }

  /// <summary>
  /// Encoding used during conversion.
  /// </summary>
  public Encoding Encoding { get; }

  /// <summary>
  /// Get a length of a field.
  /// </summary>
  /// <param name="type">a member type.</param>
  /// <param name="length">a member length.</param>
  /// <param name="precision">a member precision.</param>
  /// <param name="options">a serialization options.</param>
  /// <returns>a length of a field.</returns>
  public static int GetLength(
    MemberType type, 
    int length, 
    int precision, 
    Options options)
  {
    switch(type)
    {
      case MemberType.Char:
      case MemberType.Binary:
      {
        return (length <= 0) || (length > 65535) ? -1 : length;
      }
      case MemberType.Varchar:
      case MemberType.Varbinary:
      {
        return (length <= 0) || (length > 65535) ? -1 : length + 2;
      }
      case MemberType.Date:
      {
        return 8;
      }
      case MemberType.Time:
      {
        return 6;
      }
      case MemberType.Timestamp:
      {
        return 20;
      }
      case MemberType.BinaryNumber:
      {
        options |= Options.Binary;

        goto case MemberType.Number;
      }
      case MemberType.PackedDecimal:
      {
        options |= Options.PackedDecimal;

        goto case MemberType.Number;
      }
      case MemberType.Number:
      {
        return (length <= 0) || (precision < 0) || (precision > length) ? -1 :
          (options & Options.Binary) != 0 ?
            precision != 0 ? 8 :
            length <= 4 ? 2 :
            length <= 9 ? 4 :
            length <= 18 ? 8 : -1 :
          (options & Options.PackedDecimal) != 0 ?
            length / 2 + 1 :
            length;
      }
      default:
      {
        return -1;
      }
    }
  }

  /// <summary>
  /// Gets default alignment for a field.
  /// </summary>
  /// <param name="type">a member type.</param>
  /// <param name="length">a member length.</param>
  /// <param name="precision">a member precision.</param>
  /// <param name="options">a serialization options.</param>
  /// <returns>an alignment value.</returns>
  public static int GetAlignment(
    MemberType type, 
    int length, 
    int precision, 
    Options options)
  {
    switch(type)
    {
      case MemberType.BinaryNumber:
      {
        options |= Options.Binary;

        goto case MemberType.Number;
      }
      case MemberType.PackedDecimal:
      {
        options |= Options.PackedDecimal;

        goto case MemberType.Number;
      }
      case MemberType.Number:
      {
        // TODO: account Options.Binary and Options.PackedDecimal

        if (precision == 0)
        {
          if ((options & Options.Binary) != 0)
          {
            if (length <= 4)
            {
              return 2;
            }
            else if (length <= 9)
            {
              return 4;
            }
            else if (length <= 18)
            {
              return 8;
            }
          }
        }
        else
        {
          if ((options & Options.Binary) != 0)
          {
            return 8;
          }
        }

        break;
      }
    }
    
    return 1;
  }

  /// <summary>
  /// Writes a byte array into a data span.
  /// </summary>
  /// <param name="data">A data span.</param>
  /// <param name="value">A value to write.</param>
  /// <param name="length">A number of bytes to write.</param>
  /// <param name="options">A serialization options.</param>
  public static void WriteBytes(
    Span<byte> data, 
    byte[] value, 
    int length, 
    Options options)
  {
    if ((length <= 0) || (length > 65535)) 
    {
      throw new ArgumentException("Invalid buffer length", nameof(length));
    }
    
    var valueLength = value == null ? 0 : value.Length;
    
    if (valueLength > 0)
    {
      var source = new Span<byte>(
        value, 
        0, 
        valueLength > length ? length : valueLength);
        
      source.TryCopyTo(data);
    }

    if (valueLength < length)
    {
      data[valueLength..length].Fill(0);
    }
  }

  /// <summary>
  /// Reads a byte array from a data span.
  /// </summary>
  /// <param name="data">A data span.</param>
  /// <param name="length">A length of output byte array.</param>
  /// <param name="options">A serialization options.</param>
  /// <returns>A read byte array.</returns>
  public static byte[] ReadBytes(Span<byte> data, int length, Options options)
  {
    if ((length <= 0) || (length > 65535)) 
    {
      throw new ArgumentException("Invalid length", nameof(length));
    }

    return data.Slice(0, length).ToArray();
  }

  /// <summary>
  /// Writes a string to a data span.
  /// </summary>
  /// <param name="data">A data span.</param>
  /// <param name="value">A value to write.</param>
  /// <param name="length">A length of an alphanumeric field in COBOL.</param>
  /// <param name="options">A serialization options.</param>
  public void WriteString(
    Span<byte> data, 
    string value, 
    int length, 
    Options options) 
  { 
    // NOTE: potential speedup through use of unsafe code.
    var buffer = IsEmpty(value) ? null : Encoding.GetBytes(value);
    var bufferLength = buffer == null ? 0 : buffer.Length;

    if (bufferLength > 0)
    {
      var source = new Span<byte>(
        buffer,
        0,
        bufferLength > length ? length : bufferLength);

      source.TryCopyTo(data);
    }

    if (bufferLength < length)
    {
      data[bufferLength..length].Fill(fillChar);
    }
  }

  /// <summary>
  /// Reads a string form a data span.
  /// </summary>
  /// <param name="data">A data span.</param>
  /// <param name="length">an alphanumeric field's length.</param>
  /// <param name="options">a serialization options.</param>
  /// <returns>a read string.</returns>
  public string ReadString(Span<byte> data, int length, Options options) 
  {
    if ((length <= 0) || (length > 65535)) 
    {
      throw new ArgumentException("Invalid string legnth", nameof(length));
    }

    // NOTE: potential speedup through use of unsafe code.
    return Encoding.GetString(data[..length].ToArray());
  }

  /// <summary>
  /// Writes a number to a data span.
  /// </summary>
  /// <param name="data">A data span.</param>
  /// <param name="value">A number to write.</param>
  /// <param name="length">
  /// A number of decimal digits that value contains.
  /// </param>
  /// <param name="options">A serialization options.</param>
  public void WriteLong(
    Span<byte> data, 
    long value, 
    int length, 
    Options options)
  {
    if (length <= 0) 
    {
      throw new ArgumentException(
        "Invalid number of digits", 
        nameof(length));
    }

    var negative = value < 0;

    if ((options & (Options.Binary | Options.PackedDecimal)) == 0)
    {
      if (negative)
      {
        value = -value;
      }

      byte d = (byte)(value % 10);
      value /= 10;

      data[length - 1] = (byte)((options & Options.Unsigned) != 0 ?
        d + zero :
        ascii ?
          d == 0 ?
            negative ? 0x7d : 0x7b :
            negative ? 0x49 + d : 0x40 + d :
          negative ? 0xd0 + d : 0xc0 + d);

      for(var i = length - 1; i-- > 0;)
      {
        d = (byte)(value % 10);
        value /= 10;
        data[i] = (byte)(d + zero);
      }

      if (value != 0)
      {
        throw new OverflowException("Decimal overflow.");
      }

      return;
    }
    else if ((options & Options.PackedDecimal) != 0)
    {
      WriteDecimal(data, value, length, 0, options);
    }
    else
    {
      if (length <= 4)
      {
        if ((value < -0x8000) || (value > 0x7FFF))
        {
          throw new OverflowException("Decimal overflow.");
        }

        if ((options & Options.LittleEndian) != 0)
        {
          data[0] = (byte)(value & 0xFF);
          data[1] = (byte)((value >> 8) & 0xFF);
        }
        else
        {
          data[0] = (byte)((value >> 8) & 0xFF);
          data[1] = (byte)(value & 0xFF);
        }

        return;
      }
      else if (length <= 9)
      {
        if ((value < -0x80000000) || (value > 0x7FFFFFFF))
        {
          throw new OverflowException("Decimal overflow.");
        }

        if ((options & Options.LittleEndian) != 0)
        {
          data[0] = (byte)(value & 0xFF);
          data[1] = (byte)((value >> 8) & 0xFF);
          data[2] = (byte)((value >> 16) & 0xFF);
          data[3] = (byte)((value >> 24) & 0xFF);
        }
        else
        {
          data[0] = (byte)((value >> 24) & 0xFF);
          data[0] = (byte)((value >> 16) & 0xFF);
          data[0] = (byte)((value >> 8) & 0xFF);
          data[0] = (byte)(value & 0xFF);
        }

        return;
      }
      else if (length <= 18)
      {
        if ((options & Options.LittleEndian) != 0)
        {
          data[0] = (byte)(value & 0xFF);
          data[0] = (byte)((value >> 8) & 0xFF);
          data[0] = (byte)((value >> 16) & 0xFF);
          data[0] = (byte)((value >> 24) & 0xFF);
          data[0] = (byte)((value >> 32) & 0xFF);
          data[0] = (byte)((value >> 40) & 0xFF);
          data[0] = (byte)((value >> 48) & 0xFF);
          data[0] = (byte)((value >> 56) & 0xFF);
        }
        else
        {
          data[0] = (byte)((value >> 56) & 0xFF);
          data[0] = (byte)((value >> 48) & 0xFF);
          data[0] = (byte)((value >> 40) & 0xFF);
          data[0] = (byte)((value >> 32) & 0xFF);
          data[0] = (byte)((value >> 24) & 0xFF);
          data[0] = (byte)((value >> 16) & 0xFF);
          data[0] = (byte)((value >> 8) & 0xFF);
          data[0] = (byte)(value & 0xFF);
        }

        return;
      }
    }
    
    throw new OverflowException("Invalid number.");
  }

  /// <summary>
  /// Writes a number to a data span.
  /// </summary>
  /// <param name="data">A data span.</param>
  /// <param name="value">A number to write.</param>
  /// <param name="length">
  /// A number of decimal digits that value contains. 
  /// </param>
  /// <param name="precision">
  /// A number of decimal digits after decimal point.
  /// </param>
  /// <param name="options">A serialization options.</param>
  public void WriteDecimal(
    Span<byte> data, 
    decimal value, 
    int length, 
    int precision,
    Options options)
  {
    if (length <= 0) 
    {
      throw new ArgumentException(
        "Invalid number of digits", 
        nameof(length));
    }

    if ((precision < 0) || (length < precision))
    {
      throw new ArgumentException("Invalid precision", nameof(precision));
    }

    value = Truncate(value, (byte)precision);

    var negative = value < 0;
    var bits = decimal.GetBits(value);
    var scaledValue = new decimal(bits[0], bits[1], bits[2], false, 0);

    if ((options & (Options.Binary | Options.PackedDecimal)) == 0)
    {
      for(var i = length; i > 0; i -= 18)
      {
        var remainder = (long)(scaledValue % 1000000000000000000L);

        scaledValue = (scaledValue - remainder) / 1000000000000000000L;

        for(var j = 0; j < 18; ++j)
        {
          var index = i - j - 1;

          if (index < 0)
          {
            break;
          }

          var digit = (byte)(remainder % 10);

          remainder /= 10;
          data[index] = (byte)(zero + digit);
        }
      }

      if (scaledValue != 0)
      {
        throw new OverflowException("Decimal overflow.");
      }

      byte d = (byte)(data[length - 1] - zero);

      data[length - 1] =
        (byte)(ascii ?
          d == 0 ?
            negative ? 0x7d : 0x7b :
            negative ? 0x49 + d : 0x40 + d :
          negative ? 0xd0 + d : 0xc0 + d);

      return;
    }
    else if ((options & (Options.PackedDecimal)) != 0)
    {
      data[length >> 1] = (byte)(negative ? 0x0d : 0x0c);

      var s = 1 - (length & 1);

      for(var i = length; i > 0; i -= 18)
      {
        var remainder = (long)(scaledValue % 1000000000000000000L);

        scaledValue = (scaledValue - remainder) / 1000000000000000000L;

        for(var j = 0; j < 18; ++j)
        {
          var index = i - j - 1;

          if (index < 0)
          {
            break;
          }

          var digit = (byte)(remainder % 10);

          remainder /= 10;

          var p = (s + index) >> 1;

          if ((j & 1) == 0)
          {
            data[p] |= (byte)(digit << 4);
          }
          else
          {
            data[p] = digit;
          }
        }
      }

      if (scaledValue != 0)
      {
        throw new OverflowException("Decimal overflow.");
      }

      return;
    }
    else if (precision == 0)
    {
      if (length <= 18)
      {
        WriteLong(data, (long)value, length, options);

        return;
      }
    }
    // No more cases
    
    throw new OverflowException("Invalid number.");
  }

  /// <summary>
  /// Reads a long value from a data span.
  /// </summary>
  /// <param name="data">A data span.</param>
  /// <param name="length">
  /// A number of decimal digits that value contains.
  /// </param>
  /// <param name="options">A serialization options.</param>
  /// <returns>A number value.</returns>
  public long ReadLong(Span<byte> data, int length, Options options)
  {
    if (length <= 0) 
    {
      throw new ArgumentException(
        "Invalid number of digits", 
        nameof(length));
    }

    if ((options & (Options.Binary | Options.PackedDecimal)) == 0)
    {
      var value = 0L;
      var negative = false;
      
      for(int i = 0; i < length; ++i)
      {
        var c = data[i];

        if (c == fillChar)
        {
          continue;
        }

        var digit = -1;

        if (i < length - 1)
        {
          digit = c - zero;
        }
        else
        {
          if (ascii)
          {
            if ((c >= 0x30) && (c <= 0x39))
            {
              digit = c - 0x30;
            }
            else if (c == 0x7b)
            {
              digit = 0;
            }
            else if (c == 0x7d)
            {
              digit = 0;
              negative = true;
            }
            else if ((c >= 0x41) && (c <= 0x49))
            {
              digit = c - 0x40;
            }
            else if ((c >= 0x4A) && (c <= 0x52))
            {
              digit = c - 0x49;
              negative = true;
            }
            // No more cases
          }
          else
          {
            digit = c & 0xf;
            negative = (c & 0xf0) == 0xd0;
          }
        }

        if ((digit < 0) || (digit > 9))
        {
          // Invalid number.
          throw new OverflowException("Invalid number.");
        }

        value = value * 10 + digit;
      }

      if (negative && ((options & Options.Unsigned) == 0))
      {
        value = -value;
      }

      return value;
    }
    if ((options & Options.PackedDecimal) != 0)
    {
      return (long)ReadDecimal(data, length, 0, options);
    }
    else
    {
      if (length <= 4)
      {
        var byte1 = data[0];
        var byte2 = data[1];

        return (options & Options.LittleEndian) != 0 ?
          byte1 | (byte2 << 8) :
          byte2 | (byte1 << 8);
      }
      else if (length <= 9)
      {
        var byte1 = data[0];
        var byte2 = data[1];
        var byte3 = data[2];
        var byte4 = data[3];

        return (options & Options.LittleEndian) != 0 ?
          byte1 | (byte2 << 8) | (byte3 << 16) | (byte4 << 24) :
          byte4 | (byte3 << 8) | (byte2 << 16) | (byte1 << 24);
      }
      else if (length <= 18)
      {
        var byte1 = data[0];
        var byte2 = data[1];
        var byte3 = data[2];
        var byte4 = data[3];
        var byte5 = data[4];
        var byte6 = data[5];
        var byte7 = data[6];
        var byte8 = data[7];

        return (options & Options.LittleEndian) != 0 ?
          (uint)(byte1 | (byte2 << 8) | (byte3 << 16) | (byte4 << 24)) |
            (long)(byte5 | (byte6 << 8) | (byte7 << 16) | (byte8 << 24)) << 32 :
          (uint)(byte8 | (byte7 << 8) | (byte6 << 16) | (byte5 << 24)) |
            (long)(byte4 | (byte3 << 8) | (byte2 << 16) | (byte1 << 24)) << 32;
      }
    }
    
    throw new OverflowException("Invalid number.");
  }

  /// <summary>
  /// Reads a decinal value from a data span.
  /// </summary>
  /// <param name="data">A data span.</param>
  /// <param name="length">
  /// A number of decimal digits that value contains.
  /// </param>
  /// <param name="precision">
  /// A number of decimal digits after decimal point.
  /// </param>
  /// <param name="options">A serialization options.</param>
  /// <returns>A number value.</returns>
  public decimal ReadDecimal(
    Span<byte> data, 
    int length, 
    int precision,
    Options options)
  {
    if ((precision == 0) && 
      (length < 19) && 
      (options & Options.PackedDecimal) == 0)
    {
      return ReadLong(data, length, options);
    }
    
    if (length <= 0) 
    {
      throw new ArgumentException(
        "Invalid number of digits", 
        nameof(length));
    }

    if ((precision < 0) || (length < precision))
    {
      throw new ArgumentException("Invalid precision", nameof(precision));
    }

    if ((options & (Options.Binary | Options.PackedDecimal)) == 0)
    {
      var result = 0M;
      var error = false;
      var negative = false;
      var c = data[length - 1];

      if (ascii)
      {
        if (c == 0x7b)
        {
          c = 0x30;
        }
        else if (c == 0x7d)
        {
          c = 0x30;
          negative = true;
        }
        else if ((c >= 0x41) && (c <= 0x49))
        {
          c = (byte)(c - 0x40 + 0x30);
        }
        else if ((c >= 0x4A) && (c <= 0x52))
        {
          c = (byte)(c - 0x49 + 0x30);
          negative = true;
        }
        else
        {
          error = true;
        }
      }
      else
      {
        negative = (c & 0xf0) == 0xd0;
        c |= 0xf0;
      }

      if (error)
      {
        throw new OverflowException("Invalid number.");
      }

      for(var i = length; i > 0; i -= 18)
      {
        var remainder = 0L;

        for(var j = 0; j < 18; ++j)
        {
          var index = i - j - 1;

          if (index < 0)
          {
            break;
          }

          var d = index == length - 1 ? c : data[index];
          var digit = d - zero;

          if ((digit < 0) || (digit > 9))
          {
            throw new OverflowException("Invalid number.");
          }

          remainder = remainder * 10 + digit;
        }

        result = result * 1000000000000000000L + remainder;
      }

      var bits = decimal.GetBits(result);

      return new(
        bits[0],
        bits[1],
        bits[2],
        negative,
        (byte)precision);
    }
    else if ((options & Options.PackedDecimal) != 0)
    {
      var result = 0M;
      var s = 1 - (length & 1);

      if ((s == 1) && ((data[0] & 0xff) != 0))
      {
        throw new NotSupportedException("PackedDecimal");
      }

      for(var i = 0; i < length; ++i)
      {
        var c = data[(s + i) >> 1];
        var digit = (byte)(((i & 1) == 0 ? (c >> 4) : c) & 0x0f);

        if (digit > 9)
        {
          throw new OverflowException("Invalid number.");
        }

        result = result * 10 + digit;
      }

      var sign = data[length >> 1] & 0x0f;
        
      var negative = (sign == 0x0d) || (sign == 0x07) ? true :
        (sign == 0x0c) || (sign == 0x0c) ? false : 
        null as bool?;

      if (negative == null)
      {
        throw new OverflowException("Invalid number.");
      }

      if (negative == true)
      {
        result = -result;
      }

      return result;
    }
    else if (precision == 0)
    {
      // Do nothing
    }
    else
    {
      if ((options & Options.Binary) != 0)
      {
        throw new NotSupportedException("BinaryDecimal");
      }
    }
    
    throw new OverflowException("Invalid number.");
  }

  /// <summary>
  /// Writes a date value to a data span.
  /// </summary>
  /// <param name="data">A data span.</param>
  /// <param name="date">A value to write.</param>
  /// <param name="options">A serialization options.</param>
  public void WriteDate(Span<byte> data, DateTime? date, Options options)
  {
    WriteLong(data, DateToInt(date), 8, 0);
  }

  /// <summary>
  /// Reads a date value from a data span.
  /// </summary>
  /// <param name="data">A data span.</param>
  /// <param name="options">A serialization options.</param>
  /// <returns>A date value.</returns>
  public DateTime? ReadDate(Span<byte> data, Options options)
  {
    return IntToDate((int)ReadLong(data, 8, 0));
  }

  /// <summary>
  /// Writes a time value to a data span.
  /// </summary>
  /// <param name="data">A data span.</param>
  /// <param name="time">A value to write.</param>
  /// <param name="options">A serialization options.</param>
  public void WriteTime(Span<byte> data, TimeSpan? time, Options options)
  {
    WriteLong(data, TimeToInt(time), 6, 0);
  }

  /// <summary>
  /// Reads a time value from a data span.
  /// </summary>
  /// <param name="data">A data span.</param>
  /// <param name="options">A serialization options.</param>
  /// <returns>A time value.</returns>
  public TimeSpan ReadTime(Span<byte> data, Options options)
  {
    return IntToTime((int)ReadLong(data, 6, 0));
  }

  /// <summary>
  /// Writes a timestamp value to a data span.
  /// </summary>
  /// <param name="data">A data span.</param>
  /// <param name="timestamp">A value to write.</param>
  /// <param name="options">A serialization options.</param>
  public void WriteTimestamp(
    Span<byte> data, 
    DateTime? timestamp, 
    Options options)
  {
    WriteLong(data, DateToInt(timestamp), 8, Options.Unsigned);
    WriteLong(data[8..], TimeToInt(timestamp?.TimeOfDay), 6, Options.Unsigned);
    WriteLong(data[14..], Microsecond(timestamp), 6, Options.Unsigned);
  }

  /// <summary>
  /// Reads a timestamp value from a data span.
  /// </summary>
  /// <param name="data">A data span.</param>
  /// <param name="options">A serialization options.</param>
  /// <returns>A timestamp value.</returns>
  public DateTime? ReadTimestamp(Span<byte> data, Options options)
  {
    var date = IntToDate((int)ReadLong(data, 8, Options.Unsigned));
    var time = IntToTime((int)ReadLong(data[8..], 6, Options.Unsigned));
    var microseconds = (int)ReadLong(data[14..], 6, Options.Unsigned);

    return AddMicroseconds(date + time, microseconds);
  }
}
