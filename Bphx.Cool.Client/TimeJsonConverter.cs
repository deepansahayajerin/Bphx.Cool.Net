using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace Bphx.Cool.Client
{
  public class TimeJsonConverter: JsonConverter<DateTime>
  {
    public override DateTime Read(
      ref Utf8JsonReader reader,
      Type typeToConvert,
      JsonSerializerOptions options) => 
      DateTime.MinValue + XmlConvert.ToTimeSpan(reader.GetString());

    public override void Write(
      Utf8JsonWriter writer,
      DateTime value,
      JsonSerializerOptions options) =>
      writer.WriteStringValue(XmlConvert.ToString(value.TimeOfDay));
  }
}
