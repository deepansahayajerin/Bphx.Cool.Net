using System;

namespace Bphx.Cool;

/// <summary>
/// An attribute defining entry point for the action.
/// </summary>
[AttributeUsage(
  AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field,
  Inherited = true, 
  AllowMultiple = false)]
public class EntryAttribute : System.Attribute
{
}
