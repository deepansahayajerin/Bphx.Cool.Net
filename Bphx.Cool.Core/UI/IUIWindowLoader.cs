namespace Bphx.Cool.UI;

/// <summary>
/// A service to load <see cref="UIWindow"/> definition.
/// </summary>
public interface IUIWindowLoader
{
  /// <summary>
  /// Loads window definition.
  /// </summary>
  /// <param name="procedure">A <see cref="IProcedure"/> instance.</param>
  /// <param name="window">A window name.</param>
  /// <returns><see cref="UIWindow"/> instance.</returns>
  UIWindow Load(IProcedure procedure, string window);
}
