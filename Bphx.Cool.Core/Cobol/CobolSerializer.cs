namespace Bphx.Cool.Cobol;

using System;
using System.ComponentModel;

using Options = CobolConverter.Options;

/// <summary>
/// A serializer to read and write classes marked with MemberAttribute 
/// to and from a stream in Cool:GEN COBOL format.
/// </summary>
public class CobolSerializer
{
  /// <summary>
  /// Creates a CobolSerializer instance.
  /// </summary>
  /// <param name="converter">A <see cref="CobolConverter"/> instance.</param>
  /// <param name="options">A serialization options.</param>
  public CobolSerializer(CobolConverter converter, Options options)
  {
    Converter = converter ??
      throw new ArgumentNullException(nameof(converter));
    Options = options;
  }

  /// <summary>
  /// An encoding used during a serialization.
  /// </summary>
  public CobolConverter Converter { get; private set; }

  /// <summary>
  /// A serialization options.
  /// </summary>
  public Options Options { get; private set; }

  /// <summary>
  /// Gets the data buffer size.
  /// </summary>
  /// <param name="instance">An instance to get size for.</param>
  /// <param name="properties">
  /// Optional array of properties to read.
  /// <c>null</c> or empty array means all properties.
  /// </param>
  /// <returns>Required data buffer size.</returns>
  public int Size(object instance, params string[] properties)
  {
    if (instance == null)
    {
      return 0;
    }

    var reader = new ReaderWriter() { converter = Converter };

    return reader.Size(instance, properties, Options);
  }

  /// <summary>
  /// Populates an instance, and optional screen fields from a data span.
  /// </summary>
  /// <param name="data">A data span.</param>
  /// <param name="instance">An instance to populate.</param>
  /// <param name="properties">
  /// Optional array of properties to read. 
  /// <c>null</c> or empty array means all properties.
  /// </param>
  /// <returns>A number of bytes read.</returns>
  public int Read(
    Span<byte> data, 
    object instance, 
    params string[] properties)
  {
    if (instance == null)
    {
      return 0;
    }

    var reader = new ReaderWriter() { converter = Converter };

    if ((Options & Options.VideoAttributes) != 0)
    {
      reader.nullScreenField = new();
    }

    return reader.Read(data, instance, properties, Options, 0);
  }

  /// <summary>
  /// Writes an instance to a data span.
  /// </summary>
  /// <param name="data">A data span.</param>
  /// <param name="instance">An instance to write.</param>
  /// <param name="screenFields">
  /// An optional screen fields. If value is specified than 
  /// it is assumed that binary data contain screen fields, otherwise 
  /// no screen fields are present in the binary data.
  /// </param>
  /// <param name="properties">
  /// Optional array of properties to read. 
  /// <c>null</c> or empty array means all properties.
  /// </param>
  /// <returns>A number of bytes written.</returns>
  public int Write(
    Span<byte> data,
    object instance, 
    params string[] properties)
  {
    if (instance == null)
    {
      return 0;
    }

    var writer = new ReaderWriter() { converter = Converter };

    if ((Options & Options.VideoAttributes) != 0)
    {
      writer.nullScreenField = new ScreenField();
    }

    return writer.Write(data, instance, properties, Options, 0);
  }

  /// <summary>
  /// A serializer's reader/writer.
  /// </summary>
  private class ReaderWriter
  {
    /// <summary>
    /// Gets the data buffer size.
    /// </summary>
    /// <param name="instance">an instance to populate.</param>
    /// <param name="properties">a properties to read.</param>
    /// <param name="options">A serialization options.</param>
    /// <returns>Required data buffer size.</returns>
    public int Size(object instance, string[] properties, Options options)
    {
      var size = 0;

      if ((properties == null) || (properties.Length == 0))
      {
        foreach(var descriptor in
          MemberAttribute.GetProperties(instance.GetType()))
        {
          size += Size(instance, descriptor, options);
        }
      }
      else
      {
        var descriptions = TypeDescriptor.GetProperties(instance.GetType());

        foreach(var property in properties)
        {
          size += Size(instance, descriptions[property], options);
        }
      }

      return size;
    }

    /// <summary>
    /// Gets the data buffer size.
    /// </summary>
    /// <param name="instance">An instance to populate.</param>
    /// <param name="descriptor">A property descriptor.</param>
    /// <param name="options">A serialization options.</param>
    /// <returns>Required data buffer size.</returns>
    public int Size(
      object instance,
      PropertyDescriptor descriptor,
      Options options)
    {
      var size = 0;

      if(descriptor.Attributes[typeof(MemberAttribute)] 
        is not MemberAttribute member)
      {
        return -1;
      }

      var type = member.Type;
      var memberOptions = options;
        //member.AccessFields ? options : options & ~Options.AccessFields;

      if (type != MemberType.Object)
      {
        var length = member.Length;
        var precision = member.Precision;

        if ((instance is View) &&
          ((memberOptions & Options.VideoAttributes) != 0))
        {
          size += VideoAttributesLength;
        }

        if ((memberOptions & Options.AccessAttributes) != 0)
        {
          size += 1;
        }

        var dataLength =
          CobolConverter.GetLength(type, length, precision, memberOptions);

        size += dataLength;
      }
      else
      {
        var memberType = descriptor.PropertyType;
        var memberValue = descriptor.GetValue(instance);

        if (memberValue == null)
        {
          memberValue = Activator.CreateInstance(memberType);
        }

        if (memberValue is IArray array)
        {
          var capacity = array.Capacity;

          int itemSize = 
            Size(array.NewElement(), member.Members, memberOptions);

          if ((memberOptions & Options.AccessGroups) != 0)
          {
            ++itemSize;
          }

          size += (capacity < 10000 ? 4 : 8) + capacity * itemSize;
        }
        else
        {
          size += Size(memberValue, member.Members, memberOptions);
        }
      }

      return size;
    }

    /// <summary>
    /// Populates an instance, and optional screen fields from a data span.
    /// </summary>
    /// <param name="data">A data span.</param>
    /// <param name="instance">An instance to populate.</param>
    /// <param name="properties">A properties to read.</param>
    /// <param name="options">A serialization options.</param>
    /// <param name="offset">A current data offset.</param>
    /// <returns>A new offset.</returns>
    public int Read(
      Span<byte> data,
      object instance, 
      string[] properties,
      Options options, 
      int offset)
    {
      if ((properties == null) || (properties.Length == 0))
      {
        foreach(var descriptor in 
          MemberAttribute.GetProperties(instance.GetType()))
        {
          offset = Read(data, instance, descriptor, options, offset);
        }
      }
      else
      {
        var descriptions = TypeDescriptor.GetProperties(instance.GetType());

        foreach(var property in properties)
        {
          offset =
            Read(data, instance, descriptions[property], options, offset);
        }
      }

      return offset;
    }

    /// <summary>
    /// Populates an instance, and optional screen fields from a data span.
    /// </summary>
    /// <param name="data">A data span.</param>
    /// <param name="instance">An instance to populate.</param>
    /// <param name="descriptor">A property descriptor.</param>
    /// <param name="options">A serialization options.</param>
    /// <param name="offset">A current data offset.</param>
    /// <returns>A new offset.</returns>
    public int Read(
      Span<byte> data,
      object instance, 
      PropertyDescriptor descriptor,
      Options options,
      int offset)
    {
      var ex = null as Exception;
      var member =
        descriptor.Attributes[typeof(MemberAttribute)] as MemberAttribute;

      if (member == null)
      {
        goto Error;
      }

      try
      {
        var type = member.Type;
        var memberOptions = options;
          //member.AccessFields ? options : options & ~Options.AccessFields;

        if (type != MemberType.Object)
        {
          var length = member.Length;
          var precision = member.Precision;

          if ((instance is View view) && 
            ((memberOptions & Options.VideoAttributes) != 0))
          {
            var screenField = ReadVideoAttributes(data[offset..]);

            if (screenField != null)
            {
              screenField.Name = descriptor.Name;
              view.ScreenFields[descriptor.Name] = screenField;
            }
            else
            {
              view.ScreenFields.Remove(descriptor.Name);
            }

            offset += VideoAttributesLength;
          }

          if ((memberOptions & Options.AccessAttributes) != 0)
          {
            ++offset;
          }

          var dataLength =
            CobolConverter.GetLength(type, length, precision, memberOptions);

          switch(type)
          {
            case MemberType.Char:
            {
              descriptor.SetValue(
                instance, 
                converter.ReadString(data[offset..], length, memberOptions));

              return offset += dataLength;
            }
            case MemberType.Varchar:
            {
              var count = 
                (int)converter.ReadLong(data[offset..], 2, Options.Binary);

              if (count < 0)
              {
                count = 0;
              }
              else if (count > length)
              {
                count = length;
              }
              // No more cases

              descriptor.SetValue(
                instance, 
                converter.ReadString(
                  data[(offset + 2)..], 
                  count, 
                  memberOptions));
            
              return offset += dataLength;
            }
            case MemberType.Binary:
            {
              descriptor.SetValue(
                instance,
                CobolConverter.ReadBytes(data[offset..], length, memberOptions));

              return offset += dataLength;
            }
            case MemberType.Varbinary:
            {
              var count = 
                (int)converter.ReadLong(data[offset..], 2,  Options.Binary);

              if (count < 0)
              {
                count = 0;
              }
              else if (count > length)
              {
                count = length;
              }
              // No more cases

              descriptor.SetValue(
                instance,
                CobolConverter.ReadBytes(data[(offset + 2)..], count, memberOptions));
            
              return offset += dataLength;
            }
            case MemberType.Date:
            {
              descriptor.
                SetValue(instance, converter.ReadDate(data[offset..], 0));
            
              return offset += dataLength;
            }
            case MemberType.Time:
            {
              descriptor.
                SetValue(instance, converter.ReadTime(data[offset..], 0));
            
              return offset += dataLength;
            }
            case MemberType.Timestamp:
            {
              descriptor.SetValue(
                instance, 
                converter.ReadTimestamp(data[offset..], 0));
            
              return offset += dataLength;
            }
            case MemberType.BinaryNumber:
            {
              memberOptions |= Options.Binary;

              goto case MemberType.Number;
            }
            case MemberType.PackedDecimal:
            {
              memberOptions |= Options.PackedDecimal;

              goto case MemberType.Number;
            }
            case MemberType.Number:
            {
              if (precision != 0)
              {
                descriptor.SetValue(
                  instance, 
                  Convert.ChangeType(
                    converter.ReadDecimal(
                      data[offset..], 
                      length, 
                      precision, 
                      memberOptions),
                    descriptor.PropertyType));
              }
              else
              {
                descriptor.SetValue(
                  instance,
                  Convert.ChangeType(
                    converter.ReadLong(data[offset..], length, memberOptions),
                    descriptor.PropertyType));
              }

              return offset += dataLength;
            }
          }
        }
        else
        {
          var memberType = descriptor.PropertyType;
          var memberValue = descriptor.GetValue(instance);
          var toSet = memberValue == null;
        
          if (toSet)
          {
            if (descriptor.IsReadOnly)
            {
              goto Error;
            }
            
            memberValue = Activator.CreateInstance(memberType);
          }

          if (memberValue is IArray array)
          {
            var capacity = array.Capacity;

            if ((capacity <= 0) || (capacity > 99999999))
            {
              throw new InvalidOperationException(
                "Unsupported array capacity: " + capacity + ".");
            }

            var sizeLength = capacity < 10000 ? 4 : 8;
            var size = 
              (int)converter.ReadLong(data[offset..], sizeLength, 0);
            var item = null as object;

            offset += sizeLength;

            if (size < 0)
            {
              size = 0;
            }
            else if (size > capacity)
            {
              size = capacity;
            }
            // No more cases.

            var accessFields = (memberOptions & Options.AccessGroups) != 0;
            var itemSize = 0;

            array.Clear();

            for(var i = 0; i < size; ++i)
            {
              if (accessFields)
              {
                ++offset;
              }

              item = array.NewElement();

              int nextOffset = 
                Read(data, item, member.Members, memberOptions, offset);

              itemSize = nextOffset - offset;
              offset = nextOffset;
              array.Add(item);
            }

            if (size < capacity)
            {
              if (item == null)
              {
                itemSize = 
                  Size(array.NewElement(), member.Members, memberOptions);
              }

              if (accessFields)
              {
                ++itemSize;
              }

              offset += (capacity - size) * itemSize;
            }
          }
          else
          {
            offset = 
              Read(data, memberValue, member.Members, memberOptions, offset);
          }

          if (toSet)
          {
            descriptor.SetValue(instance, memberValue);
          }
        
          return offset;
        }
      }
      catch (Exception e)
      {
        ex = e;
      }

Error:
      return Error(instance, descriptor, member, offset, ex);
    }

    /// <summary>
    /// Writes an instance to a data span.
    /// </summary>
    /// <param name="data">A data span.</param>
    /// <param name="instance">an instance to write.</param>
    /// <param name="properties">a properties to write.</param>
    /// <param name="options">A serialization options.</param>
    /// <param name="offset">A current data offset.</param>
    /// <returns>A new offset.</returns>
    public int Write(
      Span<byte> data,
      object instance, 
      string[] properties, 
      Options options, 
      int offset)
    {
      if ((properties == null) || (properties.Length == 0))
      {
        foreach(var descriptor in 
          MemberAttribute.GetProperties(instance.GetType()))
        {
          offset = Write(data, instance, descriptor, options, offset);
        }
      }
      else
      {
        var descriptions = TypeDescriptor.GetProperties(instance.GetType());

        foreach(var property in properties)
        {
          offset = 
            Write(data, instance, descriptions[property], options, offset);
        }
      }

      return offset;
    }

    /// <summary>
    /// Writes an instance, and optional screen fields to a data span.
    /// </summary>
    /// <param name="data">A data span.</param>
    /// <param name="instance">An instance to write.</param>
    /// <param name="descriptor">A property descriptor.</param>
    /// <param name="options">A serialization options.</param>
    /// <param name="offset">A current data offset.</param>
    /// <returns>A new offset.</returns>
    public int Write(
      Span<byte> data,
      object instance, 
      PropertyDescriptor descriptor, 
      Options options,
      int offset)
    {
      var ex = null as Exception;
      var member =
        descriptor.Attributes[typeof(MemberAttribute)] as MemberAttribute;

      if (member == null)
      {
        goto Error;
      }

      try
      {
        var type = member.Type;
        var memberOptions = options;
          //member.AccessFields ? options : options & ~Options.AccessFields;

        if (type != MemberType.Object)
        {
          var length = member.Length;
          var precision = member.Precision;

          if ((instance is View view) && 
            ((memberOptions & Options.VideoAttributes) != 0))
          {
            view.ScreenFields.
              TryGetValue(descriptor.Name, out var screenField);
            WriteVideoAttributes(data[offset..], screenField);
            offset += VideoAttributesLength;
          }

          if ((memberOptions & Options.AccessAttributes) != 0)
          {
            data[offset++] = converter.fillChar;
          }

          var dataLength =
            CobolConverter.GetLength(type, length, precision, memberOptions);

          switch (type)
          {
            case MemberType.Char:
            {
              var value = (string)descriptor.GetValue(instance);
            
              converter.WriteString(
                data[offset..], 
                value, 
                length, 
                memberOptions);
            
              return offset += dataLength;
            }
            case MemberType.Varchar:
            {
              var value = (string)descriptor.GetValue(instance);
              var count = value == null ? 0 : value.Length;
            
              if (count > length)
              {
                count = length;
              }
            
              converter.WriteLong(
                data[offset..],
                count, 
                2, 
                Options.Binary | (memberOptions & Options.LittleEndian));

              converter.WriteString(
                data[(offset + 2)..], 
                value, 
                count, 
                memberOptions);

              return offset += dataLength;
            }
            case MemberType.Binary:
            {
              var value = (byte[])descriptor.GetValue(instance);

              CobolConverter.WriteBytes(
                data[offset..], 
                value, 
                length, 
                memberOptions);
            
              return offset += dataLength;
            }
            case MemberType.Varbinary:
            {
              var value = (byte[])descriptor.GetValue(instance);
              var count = value == null ? 0 : value.Length;
            
              if (count > length)
              {
                count = length;
              }
            
              converter.WriteLong(
                data[offset..], 
                count, 
                2, 
                Options.Binary | (memberOptions & Options.LittleEndian));

              CobolConverter.WriteBytes(
                data[(offset + 2)..], 
                value, 
                count, 
                memberOptions);

              return offset += dataLength;
            }
            case MemberType.Date:
            {
              var value = (DateTime?)descriptor.GetValue(instance);
            
              converter.WriteDate(data[offset..], value, memberOptions);

              return offset += dataLength;
            }
            case MemberType.Time:
            {
              var value = (TimeSpan?)descriptor.GetValue(instance);
            
              converter.WriteTime(data[offset..], value, memberOptions);
            
              return offset += dataLength;
            }
            case MemberType.Timestamp:
            {
              var value = (DateTime?)descriptor.GetValue(instance);
            
              converter.
                WriteTimestamp(data[offset..], value, memberOptions);
            
              return offset += dataLength;
            }
            case MemberType.BinaryNumber:
            {
              memberOptions |= Options.Binary;

              goto case MemberType.Number;
            }
            case MemberType.PackedDecimal:
            {
              memberOptions |= Options.PackedDecimal;

              goto case MemberType.Number;
            }
            case MemberType.Number:
            {
              if (precision != 0)
              {
                converter.WriteDecimal(
                  data[offset..], 
                  Convert.ToDecimal(descriptor.GetValue(instance)), 
                  length, 
                  precision, 
                  memberOptions);
              }
              else
              {
                converter.WriteLong(
                  data[offset..], 
                  Convert.ToInt64(descriptor.GetValue(instance)), 
                  length, 
                  memberOptions);
              }
            
              return offset += dataLength;
            }
          }
        }
        else
        {
          var memberType = descriptor.PropertyType;
          var memberValue = descriptor.GetValue(instance);
        
          if (memberValue == null)
          {
            memberValue = Activator.CreateInstance(memberType);
          }

          if (memberValue is IArray array)
          {
            var capacity = array.Capacity;
            var size = array.Count;

            if ((capacity <= 0) || (capacity > 99999999))
            {
              throw new InvalidOperationException(
                "Unsupported array capacity: " + capacity + ".");
            }

            var sizeLength = capacity < 10000 ? 4 : 8;

            if (size > capacity)
            {
              size = capacity;
            }

            converter.WriteLong(data[offset..], size, sizeLength, 0);
            offset += sizeLength;

            var accessFields = (memberOptions & Options.AccessGroups) != 0;

            for(var i = 0; i < size; ++i)
            {
              if (accessFields)
              {
                data[offset++] = converter.fillChar;
              }

              offset = 
                Write(data, array[i], member.Members, memberOptions, offset);
            }

            if (size < capacity)
            {
              var itemOffset = offset;

              if (accessFields)
              {
                ++offset;
              }

              offset = Write(
                data, 
                array.NewElement(), 
                member.Members, 
                memberOptions, 
                offset);

              var itemSize = offset - itemOffset;
              var item = data.Slice(itemOffset, itemSize);

              for(var i = size + 1; i < capacity; ++i)
              {
                item.TryCopyTo(data[offset..]);
                offset += itemSize;
              }
            }
          }
          else
          {
            offset = Write(
              data, 
              memberValue, 
              member.Members, 
              memberOptions, 
              offset);
          }

          return offset;
        }
      }
      catch (Exception e)
      {
        ex = e;
      }

Error:
      return Error(instance, descriptor, member, offset, ex);
    }

    /// <summary>
    /// Reads video attributes.
    /// </summary>
    /// <param name="data">A data span.</param>
    /// <returns>A ScreenField instance.</returns>
    public ScreenField ReadVideoAttributes(Span<byte> data) 
    {
      var nullField = nullScreenField;
      var screenField = null as ScreenField;
      var videoAttrs = converter.ReadString(
        data,
        VideoAttributesLength,
        0);

      var isProtected = null as bool?;
      
      // 0 Pos - Protection
      switch(videoAttrs[0])
      {
        case 'P':
        case 'p':
        {
          isProtected = true;
          
          break;
        }
        case 'U':
        case 'u':
        {
          isProtected = false;
          
          break;
        }
      }
      
      if ((screenField == null) && (nullField.Protected != isProtected))
      {
        screenField = new();
      }
      
      if (screenField != null)
      {
        screenField.Protected = isProtected;
      }
      
      // 1 Pos - Intensity
      var intensity = Intensity.Normal;
      
      switch(videoAttrs[1])
      {
        case 'N':
        case 'n':
        {
          intensity = Intensity.Normal;
          
          break;
        }
        case 'H':
        case 'h':
        {
          intensity = Intensity.High;
          
          break;
        }
        case 'B':
        case 'b':
        {
          intensity = Intensity.Dark;
          
          break;
        }
      }
      
      if ((screenField == null) && (nullField.Intensity != intensity))
      {
        screenField = new();
      }
      
      if (screenField != null)
      {
        screenField.Intensity = intensity;
      }
      
      // 2 Pos - Color
      var color = null as string;
      
      switch(videoAttrs[2])
      {
        case 'W':
        case 'w':
        {
          color = "white";
          
          break;
        }
        case 'C':
        case 'c':
        {
          color = "cyan";
          
          break;
        }
        case 'Y':
        case 'y':
        {
          color = "yellow";
          
          break;
        }
        case 'G': 
        case 'g': 
        {
          color = "green";
          
          break;
        }
        case 'P': 
        case 'p': 
        {
          color = "pink";
          
          break;
        }
        case 'R': 
        case 'r': 
        {
          color = "red";
          
          break;
        }
        case 'B':
        case 'b':
        {
          color = "blue";
          
          break;
        }
      }
      
      if ((screenField == null) && (nullField.Color != color))
      {
        screenField = new();
      }
      
      if (screenField != null)
      {
        screenField.Color = color;
      }
      
      // 3 Pos - Hilighting
      var highlight = Highlighting.Normal;
      
      switch(videoAttrs[3])
      {
        case 'N':
        case 'n':
        {
          highlight = Highlighting.Normal;
          
          break;
        }
        case 'U':
        case 'u':
        {
          highlight = Highlighting.Underscore;
          
          break;
        }
        case 'B':
        case 'b':
        {
          highlight = Highlighting.Blinking;
          
          break;
        }
        case 'R':
        case 'r':
        {
          highlight = Highlighting.ReverseVideo;
          
          break;
        }
      }
      
      if ((screenField == null) && (nullField.Highlighting != highlight))
      {
        screenField = new();
      }
      
      if (screenField != null)
      {
        screenField.Highlighting = highlight;
      }

      // 4 Pos - C(ontaining Cursor) or E(rror cursor)
      var focused = false;
      
      switch(videoAttrs[4])
      {
        case 'C':
        case 'c':
        {
          focused = true;
          
          break;
        }
      }
      
      if ((screenField == null) && (nullField.Focused != focused))
      {
        screenField = new();
      }
      
      if (screenField != null)
      {
        screenField.Focused = focused;
      }
      
      return screenField;
    }

    /// <summary>
    /// Writes video attributes.
    /// </summary>
    /// <param name="data">A data span.</param>
    /// <param name="screenField">a ScreenField instance.</param>
    public void WriteVideoAttributes(
      Span<byte> data,
      ScreenField screenField) 
    {
      var videoAttrs = new[] { ' ', ' ', ' ', ' ', ' ', ' ' };
      
      if (screenField == null)
      {
        screenField = nullScreenField;
      }
      
      // 0 Pos - Protection
      var protectedField = screenField.Protected;
      
      videoAttrs[0] = protectedField.GetValueOrDefault() ? 'P' : 'U';

      // 1 Pos - Intensity
      switch(screenField.Intensity)
      {
        case Intensity.Normal:
        {
          videoAttrs[1] = 'N';
          
          break;
        }
        case Intensity.High:
        {
          videoAttrs[1] = 'H';
          
          break;
        }
        case Intensity.Dark:
        {
          videoAttrs[1] = 'B';
          
          break;
        }
      }
      
      // 2 Pos - Color
      var color = screenField.Color;
      
      videoAttrs[2] = string.IsNullOrEmpty(color) ? ' ' : 
        char.ToUpper(color[0]);
      
      // 3 Pos - Hilighting
      switch(screenField.Highlighting)
      {
        case Highlighting.Normal:
        {
          videoAttrs[3] = 'N';
          
          break;
        }
        case Highlighting.Underscore:
        {
          videoAttrs[3] = 'U';
          
          break;
        }
        case Highlighting.Blinking:
        {
          videoAttrs[3] = 'B';
          
          break;
        }
        case Highlighting.ReverseVideo:
        {
          videoAttrs[3] = 'R';

          break;
        }
      }
      
      // 4 Pos - C(ontaining Cursor) or E(rror cursor)
      videoAttrs[4] = screenField.Focused ? 'C' : ' ';
      videoAttrs[5] = ' ';

      converter.WriteString(
        data, 
        new(videoAttrs), 
        VideoAttributesLength, 
        0);
    }

    /// <summary>
    /// Throws an error.
    /// </summary>
    /// <param name="instance">An instance being serialized.</param>
    /// <param name="descriptor">A property descripor.</param>
    /// <param name="member">A member attribute.</param>
    /// <param name="offset">Current offset.</param>
    /// <param name="ex">Optional inner exception.</param>
    /// <returns>Never returns.</returns>
    private static int Error(
      object instance,
      PropertyDescriptor descriptor,
      MemberAttribute member,
      int offset,
      Exception ex) => 
      throw new InvalidOperationException(
        ErrorMessage(instance, descriptor, member, offset),
        ex);

    /// <summary>
    /// Gets an error message. 
    /// </summary>
    /// <param name="instance">An instance being serialized.</param>
    /// <param name="descriptor">A property descripor.</param>
    /// <param name="member">A member attribute.</param>
    /// <param name="offset">Current offset.</param>
    /// <returns>An error message.</returns>
    private static string ErrorMessage(
      object instance,
      PropertyDescriptor descriptor,
      MemberAttribute member,
      int offset) => 
      "Invalid or unsupported member: " + descriptor.Name +
        ((member != null) && (member.Type != MemberType.Object) ? "" :
          " (type: " + member.Type +
          ", length: " + member.Length +
          ", precision: " + member.Precision +
          ")") +
        " of instance type: " + instance.GetType().FullName +
        " at offset: " + offset + ".";

    /// <summary>
    /// A cobol converter instance.
    /// </summary>
    public CobolConverter converter;
 
    /// <summary>
    /// Cached null screen field.
    /// </summary>
    public ScreenField nullScreenField;
  }

  /// <summary>
  /// A length of video attributes field.
  /// </summary>
  private const int VideoAttributesLength = 6;
}
