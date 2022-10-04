using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bphx.Cool.Log;

/// <summary>
/// A writer of the log records.
/// </summary>
public interface ILogWriter: IDisposable
{
  /// <summary>
  /// Disposes log writer.
  /// </summary>
  void IDisposable.Dispose()
  { 
  }

  /// <summary>
  /// Converts a value to a form ready to write.
  /// </summary>
  /// <param name="value">A value</param>
  /// <returns>Write value.</returns>
  object Convert(object value);

  /// <summary>
  /// Writes a log record.
  /// </summary>
  /// <param name="timestamp">A record timestamp.</param>
  /// <param name="duration">A duration.</param>
  /// <param name="id">An id.</param>
  /// <param name="parent">A parent id.</param>
  /// <param name="type">A record type.</param>
  /// <param name="name">A record name.</param>
  /// <param name="value">A record value.</param>
  void Write(
    DateTime timestamp,
    long duration,
    long id,
    long parent,
    string type,
    string name,
    object value);
}

/// <summary>
/// A <see cref="IProfiler"/> implementation that writes everything 
/// into a database.
/// </summary>
[Serializable]
public class Logger: IProfiler
{
  /// <summary>Creates a <see cref="Logger"/> instance.</summary>
  /// <param name="writer">A log writer.</param>
  public Logger(ILogWriter writer)
  {
    storeId = Interlocked.Increment(ref lastStoreId);
    stores.GetOrAdd(storeId, new Store { writer = writer });
  }

  /// <summary>
  /// Flushes pending actions and closes the logger.
  /// </summary>
  public virtual void Dispose()
  {
    if (stores.TryRemove(storeId, out var store))
    {
      using(store)
      {
        Flush();
      }
    }
  }

  /// <summary>Flush all pending actions.</summary>
  public void Flush()
  {
    if (stores.TryGetValue(storeId, out _))
    {
      for(var i = 0; i < parallelism; ++i)
      {
        semaphore.WaitOne();
      }

      semaphore.Release(parallelism);
    }
  }

  /// <summary>
  /// <para>
  /// If <c><see cref="Playback"/> == true</c> then this method 
  /// returns next recorder <see cref="IProfiler"/> instance that 
  /// should have the same type and code. If no matching scope is 
  /// found then <c>null</c> is returned.
  /// </para>
  /// <para>
  /// If <c><see cref="Playback"/> == false</c> then this method creates 
  /// a scope within this <see cref="IProfiler"/> instance.
  /// </para>
  /// </summary>
  /// <param name="type">A scope type.</param>
  /// <param name="name">A scope name.</param>
  /// <returns>A <see cref="IProfiler"/> instance for the scope.</returns>
  public IProfiler GetScope(string type, string name)
  {
    return new Scope(storeId, 0, type, name);
  }

  /// <summary>
  /// <para>Logs a value.</para>
  /// <para>
  /// <b>Note:</b> this method may be called once, and only before
  /// <see cref="GetScope(string, string)"/>.
  /// </para>
  /// </summary>
  /// <param name="value">A value</param>
  /// <returns>A value.</returns>
  public object ScopeValue(object value)
  {
    return value;
  }

  /// <summary>
  ///  Logs an error.
  /// </summary>
  /// <param name="error">An error to log.</param>
  protected static void Log(Exception error)
  {
    Trace.TraceWarning(error.ToString());
  }

  /// <summary>
  /// A store id.
  /// </summary>    
  private readonly long storeId;

  /// <summary>
  /// A profiler scope.
  /// </summary>
  [Serializable]
  private class Scope: IProfiler
  {
    /// <summary>
    /// Creates a <see cref="Scope"/> instance.
    /// </summary>
    /// <param name="storeId">A store id.</param>
    /// <param name="parent">A parent id.</param>
    /// <param name="type">A scope type.</param>
    /// <param name="name">A scope name.</param>
    public Scope(
      long storeId,
      long parent,
      string type,
      string name)
    {
      if (!stores.TryGetValue(storeId, out var store))
      {
        throw new ArgumentException(nameof(storeId));
      }

      this.id = Interlocked.Increment(ref store.lastId);
      this.parent = parent;
      this.storeId = storeId;
      this.type = type;
      this.name = name;
      this.ticks = Stopwatch.GetTimestamp();
    }

    /// <summary>
    /// Disposes the scope.
    /// </summary>
    public void Dispose()
    {
      Flush(null);
    }

    /// <summary>
    /// A store id.
    /// </summary>
    readonly long storeId;

    /// <summary>
    /// A scope id.
    /// </summary>
    readonly long id;

    /// <summary>
    /// A parent id.
    /// </summary>      
    readonly long parent;

    /// <summary>
    /// A scope type.
    /// </summary>
    readonly string type;

    /// <summary>
    /// A scope name.
    /// </summary>
    readonly string name;

    /// <summary>
    /// A scope time source, in ticks.
    /// </summary>
    long ticks;

    /// <summary>
    /// ticks duration.
    /// </summary>      
    long duration;

    /// <summary>
    /// Flushed indicator.
    /// </summary>
    bool flushed;

    /// <summary>
    /// <para>
    /// If <c><see cref="Playback"/> == true</c> then this method 
    /// returns next recorder <see cref="IProfiler"/> instance that 
    /// should have the same type and code. If no matching scope is 
    /// found then <c>null</c> is returned.
    /// </para>
    /// <para>
    /// If <c><see cref="Playback"/> == false</c> then this method creates 
    /// a scope within this <see cref="IProfiler"/> instance.
    /// </para>
    /// </summary>
    /// <param name="type">A scope type.</param>
    /// <param name="name">A scope name.</param>
    /// <returns>A <see cref="IProfiler"/> instance for the scope.</returns>
    public IProfiler GetScope(string type, string name)
    {
      Flush(null);

      return new Scope(storeId, id, type, name);
    }

    /// <summary>
    /// <para>Logs a value.</para>
    /// <para>
    /// <b>Note:</b> this method may be called once, and only before
    /// <see cref="GetScope(string, string)"/>.
    /// </para>
    /// </summary>
    /// <param name="value">A value</param>
    /// <returns>A value.</returns>
    public object ScopeValue(object value)
    {
      try
      {
        if (flushed)
        {
          throw new InvalidOperationException("Value is already set.");
        }

        if (!stores.TryGetValue(storeId, out var store))
        {
          return value;
        }

        var start = Stopwatch.GetTimestamp();
        var recordValue = store.writer.Convert(value);

        duration += Stopwatch.GetTimestamp() - start;
        ticks = start;
        Flush(recordValue);
      }
      catch(Exception e)
      {
        Log(e);
      }

      return value;
    }

    /// <summary>
    /// Flushes the data.
    /// </summary>
    /// <param name="value">A value to write.</param>
    private void Flush(object value)
    {
      if (!flushed)
      {
        flushed = true;

        long start = Stopwatch.GetTimestamp();
        bool acquired = false;

        try
        {
          semaphore.WaitOne();
          acquired = true;
          duration += Stopwatch.GetTimestamp() - start;

          Task.Run(() =>
          {
            try
            {
              Write(value);
            }
            finally
            {
              semaphore.Release();
            }
          });
        }
        catch(Exception e)
        {
          if (acquired)
          {
            semaphore.Release();
          }

          throw new InvalidOperationException("Critical error in logger", e);
        }
      }
    }

    /// <summary>
    /// Writes the log record. 
    /// </summary>
    /// <param name="value">A value to write.</param>
    private void Write(object value)
    {
      if (!stores.TryGetValue(storeId, out var store))
      {
        return;
      }

      store.writer.Write(
        store.timestamp.AddTicks(ticks - store.ticks),
        duration,
        id,
        parent,
        type,
        name,
        value);
    }

    /// <summary>
    /// Appends escaped string to a string builder. 
    /// </summary>
    /// <param name="builder">
    /// A <see cref="StringBuilder"/> to append to.
    /// </param>
    /// <param name="value">A string value to append.</param>
    private static void Append(StringBuilder builder, string value)
    {
      if (value != null)
      {
        var escape = false;

        for(var i = 0; i < value.Length; ++i)
        {
          var c = value[i];

          if ((c <= ' ') || (c == ','))
          {
            escape = true;

            break;
          }
        }

        if (escape)
        {
          for(var i = 0; i < value.Length; ++i)
          {
            var c = value[i];

            switch(c)
            {
              case '\r':
              {
                builder.Append("&#13;");

                break;
              }
              case '\n':
              {
                builder.Append("&#10;");

                break;
              }
              case '\t':
              {
                builder.Append('\t');

                break;
              }
              case ',':
              {
                builder.Append("&#44;");

                break;
              }
              default:
              {
                builder.Append(c < ' ' ? ' ' : c);

                break;
              }
            }
          }
        }
        else
        {
          builder.Append(value);
        }
      }
    }
  }

  /// <summary>
  /// A trace store.
  /// </summary>
  private class Store: IDisposable
  {
    /// <summary>
    /// A log writer instance.
    /// </summary>
    public ILogWriter writer;

    /// <summary>
    /// Last id value.
    /// </summary>
    public long lastId;

    /// <summary>
    /// A store creation timestamp.
    /// </summary>
    public DateTime timestamp = DateTime.Now;

    /// <summary>
    /// A store creation time source, in ticks.
    /// </summary>
    public long ticks = Stopwatch.GetTimestamp();

    /// <summary>
    /// Disposes the scope.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Dispose()
    {
      if (writer != null)
      {
        try
        {
          writer.Dispose();
        }
        catch(Exception e)
        {
          Log(e);
        }

        writer = null;
      }
    }
  }

  /// <summary>
  /// Stores by session id.
  /// </summary>
  private static readonly ConcurrentDictionary<long, Store> stores = new();

  /// <summary>
  /// Last store id used.
  /// </summary>
  private static long lastStoreId;

  /// <summary>
  /// Level of parallelism.
  /// </summary>
  private static readonly int parallelism = Environment.ProcessorCount;

  /// <summary>
  /// A write semaphore.
  /// </summary>  
  private static readonly Semaphore semaphore = new(parallelism, parallelism);
}
