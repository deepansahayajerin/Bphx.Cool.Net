using System;

using Bphx.Cool.Tests.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using static Bphx.Cool.Functions;

namespace Bphx.Cool.Tests;

[TestClass]
public class FunctionsTests
{
  [TestMethod]
  public void IsValidTest()
  {
    CheckValid<CsePerson>("Type1", "C");
  }

  [TestMethod]
  [ExpectedException(typeof(ProhibitedValueException))]
  public void IsNotValidTest()
  {
    CheckValid<CsePerson>("Type1", "P");
  }

  [TestMethod]
  public void TimestampTest()
  {
    var timestamp = Timestamp("2022-04-29-09.37.52.233236");
    var expected = new DateTime(2022, 4, 29, 9, 37, 52).
      AddTicks(233236 * TimeSpan.TicksPerMillisecond / 1000);

    Assert.AreEqual(timestamp, expected);
  }
}
