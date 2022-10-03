using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;

namespace Bphx.Cool.Impl;

/// <summary>
/// A wrapper to serialize <see cref="IEnumerator"/> instance.
/// </summary>
[Serializable]
public class EnumeratorWrapper : ISerializable
{
  /// <summary>
  /// Creates a <see cref="EnumeratorWrapper"/> value.
  /// </summary>
  /// <param name="enumerator">A wrapped <see cref="IEnumerator"/>.</param>
  public EnumeratorWrapper(IEnumerator enumerator) =>
    Enumerator = enumerator ??
      throw new ArgumentNullException(nameof(enumerator));

  /// <ssummary>
  /// Wrapped <see cref="IEnumerator"/> instance.
  /// </summary>
  public IEnumerator Enumerator { get; }

  /// <summary>
  /// Constructor used during deserialization.
  /// </summary>
  /// <param name="info">A serialization info.</param>
  /// <param name="context">A serialization context.</param>
  private EnumeratorWrapper(
    SerializationInfo info,
    StreamingContext context)
  {
    var type = Type.GetType(info.GetString("type"));

    if (!typeof(IEnumerator).IsAssignableFrom(type))
    {
      throw new InvalidOperationException();
    }

    if (type.IsSerializable)
    {
      Enumerator = (IEnumerator)info.GetValue("value", type);
    }
    else
    {
      var enumerator =
        (IEnumerator)FormatterServices.GetUninitializedObject(type);

      foreach(var field in enumerator.GetType().
        GetFields(
          BindingFlags.Instance |
          BindingFlags.Public |
          BindingFlags.NonPublic))
      {
        field.SetValue(
          enumerator,
          info.GetValue("_" + field.Name, field.FieldType));
      }

      Enumerator = enumerator;
    }
  }

  /// <summary>
  /// Collects serialization data.
  /// </summary>
  /// <param name="info">A serialization info.</param>
  /// <param name="context">A serialization context.</param>
  public void GetObjectData(SerializationInfo info, StreamingContext context)
  {
    var type = Enumerator.GetType();

    info.AddValue("type", type.AssemblyQualifiedName);

    if (type.IsSerializable)
    {
      info.AddValue("value", Enumerator, type);
    }
    else
    {
      foreach(var field in type.GetFields(
        BindingFlags.Instance |
        BindingFlags.Public |
        BindingFlags.NonPublic))
      {
        info.AddValue(
          "_" + field.Name,
          field.GetValue(Enumerator),
          field.FieldType);
      }
    }
  }
}
