using System;

namespace Bphx.Cool;

/// <summary>
/// An attribute defining computed property.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class ComputedAttribute : System.Attribute
{
}
