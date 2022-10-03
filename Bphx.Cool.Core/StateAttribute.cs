using System;

namespace Bphx.Cool;

/// <summary>An attribute defining a state property.</summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple=false)]
public class StateAttribute: System.Attribute
{
  /// <summary>
  /// Creates a <see cref="StateAttribute"/> instance.
  /// </summary>
  /// <param name="name">Optional name.</param>
  public StateAttribute(string name = null) 
  {
    Name = name;
  }

  /// <summary>
  /// A state name. Default is null meaning that property name is used.
  /// </summary>
  public string Name { get; set; } = null;

  /// <summary>
  /// Indicates that the property used for a ready state (default is true).
  /// </summary>
  public bool Read { get; set; } = true;

  /// <summary>
  /// Indicates that the property used for a write state (default is false).
  /// </summary>
  public bool Write { get; set; } = false;
}
