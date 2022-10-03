using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Bphx.Cool.Impl;

/// <summary>
/// API to encapsulate serialization.
/// </summary>
public class Serializer: ISerializer
{
  /// <summary>
  /// Serializes data down to the stream.
  /// </summary>
  /// <param name="data">An object to serialize.</param>
  /// <param name="stream">A stream to write data to.</param>
  public void Serilize(object data, Stream stream)
  {
    var formatter = new BinaryFormatter();

#pragma warning disable SYSLIB0011 // Type or member is obsolete
    formatter.Serialize(stream, data);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
  }

  /// <summary>
  /// Deserializes object out of the stream.
  /// </summary>
  /// <param name="type">An object type.</param>
  /// <param name="stream">A stream to read from.</param>
  /// <returns></returns>
  public object Deserilize(Type type, Stream stream)
  {
    var formatter = new BinaryFormatter();

#pragma warning disable SYSLIB0011 // Type or member is obsolete
    var instance = formatter.Deserialize(stream);
#pragma warning restore SYSLIB0011 // Type or member is obsolete

    return (instance == null) || type.IsAssignableFrom(instance.GetType()) ?
      instance : throw new InvalidCastException();
  }
}
