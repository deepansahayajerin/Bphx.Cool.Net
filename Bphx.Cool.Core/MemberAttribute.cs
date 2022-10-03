using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Caching;

namespace Bphx.Cool;

/// <summary>
/// An attribute defining data member.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class MemberAttribute : System.Attribute
{
  /// <summary>
  /// Creates MemberAttribute instance.
  /// </summary>
  public MemberAttribute()
  {
    AccessFields = true;
  }

  /// <summary>
  /// Member index, which defines member order during binary serialization.
  /// </summary>
  public int Index { get; set; }

  /// <summary>
  /// Defines member type.
  /// </summary>
  public MemberType Type { get; set; }

  /// <summary>
  /// Defines type length.
  /// </summary>
  public int Length { get; set; }

  /// <summary>
  /// Defines type precision.
  /// </summary>
  public int Precision { get; set; }

  /// <summary>
  /// Defines whether the value is optional.
  /// </summary>
  public bool Optional { get; set; }

  /// <summary>
  /// Indicates whether to serialize access fields.
  /// </summary>
  [DefaultValue(true)]
  public bool AccessFields { get; set; }

  /// <summary>
  /// Defines included nested members.
  /// </summary>
  public string[] Members { get; set; }

  /// <summary>
  /// Gets an array of properties with MemberAttribute for a type.
  /// </summary>
  /// <param name="type">A type to get properties for.</param>
  /// <returns>An array of properties.</returns>
  public static PropertyDescriptor[] GetProperties(Type type)
  {
    var key = "MemberAttributeProperties:" + type.AssemblyQualifiedName;
    var cache = MemoryCache.Default;

    if(cache.Get(key) is not PropertyDescriptor[] properties)
    {
      properties = TypeDescriptor.GetProperties(type).
        Cast<PropertyDescriptor>().
        Where(p => p.Attributes[typeof(MemberAttribute)] != null).
        OrderBy(
          p =>
          {
            var member =
              p.Attributes[typeof(MemberAttribute)] as MemberAttribute;

            return member.Index;
          }).
        ToArray();

      cache.Add(key, properties, cachePolicy);
    }

    return properties;
  }

  /// <summary>
  /// A cache policy.
  /// </summary>
  private static readonly CacheItemPolicy cachePolicy =
    new() { SlidingExpiration = new TimeSpan(0, 10, 0) };
}
