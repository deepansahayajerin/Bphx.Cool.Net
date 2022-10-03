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
  public class DateTimeToStringConverter: JsonConverter<DateTime>
  {
    public override DateTime Read(
      ref Utf8JsonReader reader,
      Type typeToConvert,
      JsonSerializerOptions options) => 
      XmlConvert.ToDateTime(
        reader.GetString(), 
        XmlDateTimeSerializationMode.Unspecified);

    public override void Write(
      Utf8JsonWriter writer,
      DateTime value,
      JsonSerializerOptions options)
    {
      if (value.TimeOfDay.Ticks != 0)
      {
        writer.WriteStringValue(
          XmlConvert.ToString(
            value,
            XmlDateTimeSerializationMode.Unspecified));
      }
      else
      {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
      }
    }
  }
}
