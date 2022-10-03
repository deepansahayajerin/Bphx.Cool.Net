using System;

namespace Bphx.Cool;

/// <summary>
/// An attribute to provide application resources.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module)]
public class ResourcesAttribute: System.Attribute
{
  /// <summary>
  /// Creates a ResourcesAttribute instance.
  /// </summary>
  /// <param name="type">A resource type.</param>
  public ResourcesAttribute(Type type)
  {
    if (type == null)
    {
      throw new ArgumentNullException(nameof(type));
    }

    if (!typeof(IResources).IsAssignableFrom(type) || type.IsAbstract)
    {
      throw new ArgumentException(
        "Type must implement IResources, " +
        "should have a default constructor.");
    }

    Type = type;
  }

  /// <summary>
  /// Gets and sets a reference to a resource implementation.
  /// </summary>
  public Type Type { get; }
}
