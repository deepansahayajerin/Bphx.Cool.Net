using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using Bphx.Cool.Impl;

namespace Bphx.Cool.Tests;

[TestClass]
public class BinarySerializerTest
{
  [Serializable]
  public class T
  {
    public string s;
    public int i;
    public decimal d;

    public IEnumerable<string> GetItems()
    {
      yield return "1";
      yield return "2";
      yield return "3";
    }
  }

  [TestMethod]
  public void Test()
  {
    var t1 = new T { s = "s", i = 1, d = 1.1m };

    var stream = new MemoryStream();
    var formatter = new BinaryFormatter();

#pragma warning disable SYSLIB0011 // Type or member is obsolete
    formatter.Serialize(stream, t1);
#pragma warning restore SYSLIB0011 // Type or member is obsolete

    stream.Position = 0;

#pragma warning disable SYSLIB0011 // Type or member is obsolete
    var t2 = (T)formatter.Deserialize(stream);
#pragma warning restore SYSLIB0011 // Type or member is obsolete

    Assert.IsTrue(t1.s == t2.s && t1.i == t2.i && t1.d == t2.d);
  }

  [TestMethod]
  public void Test2()
  {
    var t1 = new T { s = "s", i = 1, d = 1.1m };

    var e1 = t1.GetItems().GetEnumerator();

    e1.MoveNext();

    Assert.AreEqual(e1.Current, "1");

    var stream = new MemoryStream();
    var formatter = new BinaryFormatter();

#pragma warning disable SYSLIB0011 // Type or member is obsolete
    formatter.Serialize(stream, new EnumeratorWrapper(e1));
#pragma warning restore SYSLIB0011 // Type or member is obsolete

    stream.Position = 0;

#pragma warning disable SYSLIB0011 // Type or member is obsolete
    var e2 = ((EnumeratorWrapper)formatter.Deserialize(stream)).
      Enumerator as IEnumerator<string>;
#pragma warning restore SYSLIB0011 // Type or member is obsolete

    Assert.AreEqual(e2.Current, "1");

    e2.MoveNext();

    Assert.AreEqual(e2.Current, "2");
  }
}

