using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace Bphx.Cool.Client
{
  /// <summary>
  /// JSON converter for TimeSpan instances.
  /// </summary>
  /// <seealso cref="https://newbedev.com/net-core-3-0-timespan-deserialization-error-fixed-in-net-5-0"/>
  public class TimeSpanToStringConverter: JsonConverter<TimeSpan>
  {
    public override TimeSpan Read(
      ref Utf8JsonReader reader,
      Type typeToConvert,
      JsonSerializerOptions options) => 
      XmlConvert.ToTimeSpan(reader.GetString());

    public override void Write(
      Utf8JsonWriter writer, 
      TimeSpan value, 
      JsonSerializerOptions options) => 
      writer.WriteStringValue(XmlConvert.ToString(value));
  }
}
