using Microsoft.VisualStudio.TestTools.UnitTesting;

using static Bphx.Cool.Functions;

namespace Bphx.Cool.Tests;

[TestClass]
public class FilterTests
{

  [TestMethod]
  public void Test()
  {
    var items = new Array<Import>(10);

    Filter(items, "1 = 1", Getter);
    Sort(items, "1 A", Getter);
  }

  private object Getter(Import import, int index) =>
    index switch
    {
      1 => import?.Value,
      _ => null,
    };

  private class Import
  {
    public int Value { get; set; }
  }
}
