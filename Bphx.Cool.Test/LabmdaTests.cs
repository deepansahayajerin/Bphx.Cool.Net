using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Bphx.Cool.Tests;

[TestClass]
public class LabmdaTests
{
  private const int iterations = 10;
  private const long testCount = 500000L;

  [TestMethod]
  public void Test()
  {
    TestNew();
    TestOld();
  }

  public void TestOld()
  {
    var context = new Context();
    var import = new Import();
    var export = new Export();

    var stopwatch = new Stopwatch();
 
    for(int j = 0; j < iterations; ++j)
    {
      stopwatch.Start();

      for(long i = 0; i < testCount; ++i)
      {
        MyAction.ExecuteOld(context, import, export);
      }

      stopwatch.Stop();

      Trace.WriteLine(
        "MyAction.ExecuteOld() time: " + 
        stopwatch.ElapsedMilliseconds + "ms.");

      stopwatch.Reset();
    }

    Trace.WriteLine("----------");
  }

  public void TestNew()
  {
    var context = new Context();
    var import = new Import();
    var export = new Export();

    var stopwatch = new Stopwatch();

    for(int j = 0; j < iterations; ++j)
    {
      stopwatch.Start();

      for(long i = 0; i < testCount; ++i)
      {
        MyAction.ExecuteNew(context, import, export);
      }

      stopwatch.Stop();

      Trace.WriteLine(
        "MyAction.ExecuteNew() time: " +
        stopwatch.ElapsedMilliseconds + "ms.");

      stopwatch.Reset();
    }

    Trace.WriteLine("----------");
  }
}

public class Action
{
  protected Action(Context context)
  {
    this.context = context ??
      throw new ArgumentNullException(nameof(context));
  }

  public readonly Context context;
}

public class Context
{
  public bool Log { get; set; }

  public void Call<T, I, O>(
    Func<Context, I, O, T> factory, 
    Action<T> action,
    I import,
    O export,
    [CallerMemberName] string caller = null)
  {
    var instance = factory(this, import, export);

    if (!Log)
    {
      action(instance);
    }
    else
    {
      CallWithLog(instance, action, import, export, caller);
    }
  }

  public void CallWithLog<T, I, O>(
    T instance, 
    Action<T> action,
    I import,
    O export,
    string caller)
  {
    Trace.WriteLine(instance.GetType() + ", " + caller);

    Trace.WriteLine("Import: " + import +", export: " + export);
    Trace.WriteLine("Before call");

    try
    {
      action(instance);
    }
    finally
    {
      Trace.WriteLine("After call");
    }
  }
}

public class Import
{
  public int Value { get; set; }
}

public class Export
{
  public string Result { get; set; }
}

public class MyAction: Action
{
  public static void ExecuteOld(
    Context context, 
    Import import, 
    Export export)
  {
    var action = new MyAction(context, import, export);

    if (!context.Log)
    {
      action.Run();
    }
    else
    {
      action.ExecuteOldWithLog();
    }
  }

  public void ExecuteOldWithLog()
  {
    Trace.WriteLine(GetType() + ", ExecuteOld");
    Trace.WriteLine("Import: " + import +", export: " + export);
    Trace.WriteLine("Before call");

    try
    {
      Run();
    }
    finally
    {
      Trace.WriteLine("After call");
    }
  }

  public static void ExecuteNew(
    Context context, 
    Import import, 
    Export export)
  {
    context.Call(
      (c, i, e) => new MyAction(c, i, e), 
      p => p.Run(), 
      import, 
      export);
  }

  public MyAction(Context context, Import import, Export export):
    base(context)
  {
    this.import = import ?? 
      throw new ArgumentNullException(nameof(import));
    this.export = export ?? 
      throw new ArgumentNullException(nameof(export));
  }

  private void Run()
  {
    for (int i = 0; i < 50; ++i)
    {
      import.Value++;
      export.Result = "b";
      import.Value--;
      export.Result = "c";
    }
  }

  public readonly Import import;
  public readonly Export export;
}
