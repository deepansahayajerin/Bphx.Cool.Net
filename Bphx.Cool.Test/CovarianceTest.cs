using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bphx.Cool.Tests;

[TestClass]
public class CovarianceTest
{
  [TestMethod]
  public void TestMethod1()
  {
    var x = new C { ValueA = 1, ValueB = 2, ValueC = 3 };
    var y = new C();

    y.Assign(x);

    var z = new C(y);

    Assert.AreEqual(x.ValueA, 1);
    Assert.AreEqual(x.ValueB, 2);
    Assert.AreEqual(x.ValueC, 3);
    Assert.AreEqual(z.ValueA, 1);
    Assert.AreEqual(z.ValueB, 2);
    Assert.AreEqual(z.ValueC, 3);
  }

  public class A : ICloneable
  {
    public A() { }

    public A(A that)
    {
      Assign(that);
    }

    public A Clone()
    {
      return new A(this);
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    public virtual void Assign(A that)
    {
      this.ValueA = that.ValueA;
    }

    public int ValueA { get; set; }
  }

  public class B : A, ICloneable
  {
    public B() { }

    public B(B that): base(that)
    {
    }

    public new B Clone()
    {
      return new B(this);
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    public override void Assign(A that)
    {
      Assign((B)that);
    }

    public void Assign(B that)
    {
      base.Assign(that);
      this.ValueB = that.ValueB;
    }

    public int ValueB { get; set; }
  }

  public class C : B, ICloneable
  {
    public C() { }

    public C(C that) : base(that)
    {
    }

    public new C Clone()
    {
      return new C(this);
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    public override void Assign(A that)
    {
      Assign((C)that);
    }

    public void Assign(C that)
    {
      base.Assign(that);
      this.ValueC = that.ValueC;
    }

    public int ValueC { get; set; }
  }
}
