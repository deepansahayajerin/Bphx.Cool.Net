using System;
using System.IO;

namespace Bphx.Cool;

/// <summary>
/// API to encapsulate serialization.
/// </summary>
public interface ISerializer
{
  /// <summary>
  /// Serializes data down to the stream.
  /// </summary>
  /// <param name="data">An object to serialize.</param>
  /// <param name="stream">A stream to write data to.</param>
  void Serilize(object data, Stream stream);

  /// <summary>
  /// Deserializes object out of the stream.
  /// </summary>
  /// <param name="type">An object type.</param>
  /// <param name="stream">A stream to read from.</param>
  /// <returns></returns>
  object Deserilize(Type type, Stream stream);
}

/// <summary>
/// Extension API to encapsulate serialization.
/// </summary>
public static class SerialierExtensions
{
  /// <summary>
  /// Serializes data down to the stream.
  /// </summary>
  /// <typeparam name="T">A type of data.</typeparam>
  /// <param name="serializer">A serializer instance.</param>
  /// <param name="data">A data to serialize.</param>
  /// <param name="stream">A stream to write data to.</param>
  public static void Serilize<T>(
    this ISerializer serializer,
    T data,
    Stream stream)
  {
    serializer.Serilize(data, stream);
  }

  /// <summary>
  /// Deserializes object out of the stream.
  /// </summary>
  /// <typeparam name="T">A type of data.</typeparam>
  /// <param name="serializer">A serializer instance.</param>
  /// <param name="stream">A stream to read from.</param>
  /// <returns></returns>
  public static T Deserilize<T>(this ISerializer serializer, Stream stream)
  {
    return (T)serializer.Deserilize(typeof(T), stream);
  }
}
