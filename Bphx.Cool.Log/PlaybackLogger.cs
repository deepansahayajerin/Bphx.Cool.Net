using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

using static Bphx.Cool.Functions;

namespace Bphx.Cool.Log;

/// <summary>
/// An implementation of <see cref="IProfiler"/> that playbacks 
/// logs written by the <see cref="PlainLogger"/>.
/// </summary>
[Serializable]
public class PlaybackLogger: IProfiler
{
  /// <summary>
  /// <para>Creates a <see cref="PlaybackLogger"/> instance.</para>
  /// <param name="path">A log file path.</param>
  /// <param name="bypass">
  /// Optional comma separated list of call ids to bypass.
  /// </param>
  /// <param name="serializer">A binary serializer.</param>
  /// <param name="json">
  /// true for a json value format, and false for xml format.
  /// </param>
  public PlaybackLogger(
    ISerializer serializer, 
    string path, 
    string bypass = null,
    bool json = false)
  {
    root.path = path;
    root.bypass = string.IsNullOrWhiteSpace(bypass) ? null :
      bypass.Split(",").
        Select(id => long.Parse(id.Trim())).
        ToHashSet();

    stores.AddOrUpdate(
      path,
      k => new() 
      { 
        path = k, 
        serializer = serializer, 
        json = json 
      },
      (k, v) =>
      {
        Interlocked.Increment(ref v.refCount);

        return v;
      });
  }

  /// <summary>
  /// Flushes pending actions and closes the logger.
  /// </summary>
  public void Dispose()
  {
    if (stores.TryGetValue(root.path, out var store) &&
      (Interlocked.Decrement(ref store.refCount) == 0))
    {
      using(store)
      {
        stores.TryRemove(root.path, out _);
      }
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
  public IProfiler GetScope(string type, string name) => 
    root.GetScope(type, name);

  /// <summary>
  /// <para>Logs a value.</para>
  /// <para>
  /// <b>Note:</b> this method may be called once, and only before
  /// <see cref="GetScope(string, string)"/>.
  /// </para>
  /// </summary>
  /// <param name="value">A value</param>
  /// <returns>A value.</returns>
  public object ScopeValue(object value) => value;

  /// <summary>Logs an error.</summary>
  /// <param name="error">An error to log.</param>
  private static void Log(Exception error) => 
    Trace.TraceWarning(error.ToString());

  /// <summary>A root scope.</summary>
  private readonly Scope root = new();

  /// <summary>
  /// Record descriptor.
  /// </summary>
  private class Record
  {
    /// <summary>
    /// Record id.
    /// </summary>
    public long id = -1;

    /// <summary>
    /// Parent id.
    /// </summary>
    public long parent = -1;

    /// <summary>
    /// Following sibling record.
    /// </summary>
    public long followingSibling = -1;

    /// <summary>
    /// Child record.
    /// </summary>
    public long firstChild = -1;

    /// <summary>
    /// Record type.
    /// </summary>
    public string type;

    /// <summary>
    /// Record name.
    /// </summary>
    public string name;

    /// <summary>
    /// Payload data offset.
    /// </summary>
    public long offset;

    /// <summary>
    /// Payload data length.
    /// </summary>
    public int length;
  }

  /// <summary>
  /// A profile scope.
  /// </summary>
  [Serializable]
  private class Scope: IProfiler
  {
    public string path;
    public HashSet<long> bypass;
    public Scope parent;
    public long id;
    public long currentChild = -1;

    /// <summary>
    /// 0 - regular scope with no playback.
    /// 1 - call playback.
    /// 2 - sql playback.
    /// 3 - now playback.
    /// </summary>
    public int playbackType;

    public void Dispose()
    {
      if (playbackType == 0)
      {
        var record = GetRecord(id);
        var child = GetRecord(currentChild);
        var next =
          GetRecord(child == null ? record.firstChild : child.followingSibling);

        while(next != null)
        {
          if ((next.type == "external") || (next.type == "trace"))
          {
            next = GetRecord(next.followingSibling);
          }
          else
          {
            Mismatch(next, null, null);

            break;
          }
        }
      }
    }

    public IProfiler GetScope(string type, string name)
    {
      var record = GetRecord(id);

      if (((playbackType == 1) || (playbackType == 2)) &&
        (type == "error") &&
        IsEmpty(name))
      {
        for(Record item = GetRecord(record.firstChild);
          item != null;
          item = GetRecord(item.followingSibling))
        {
          if ((item.type == "error") && IsEmpty(item.name))
          {
            return new Scope
            {
              path = path,
              bypass = bypass,
              parent = this,
              id = item.id
            };
          }
        }

        if (parent == null)
        {
          Mismatch(null, type, name);
        }

        return null;
      }

      if (type == "trace")
      {
        return null;
      }

      var child = GetRecord(currentChild);
      var next =
        GetRecord(child != null ? child.followingSibling : record.firstChild);

      while(true)
      {
        if (next == null)
        {
          if (parent == null)
          {
            Mismatch(null, type, name);
          }

          return null;
        }

        if (((playbackType == 1) && (next.type == "external")) ||
          (next.type == "trace"))
        {
          next = GetRecord(next.followingSibling);

          continue;
        }

        if (((playbackType == 1) || (playbackType == 2)) &&
          (type == "error") &&
          IsEmpty(name))
        {
          next = GetRecord(next.followingSibling);

          continue;
        }

        break;
      }

      bool match = next.type == type;

      if (match && (next.name != name) && (type != "external"))
      {
        match = false;
      }

      if (!match)
      {
        if (parent == null)
        {
          Mismatch(next, type, name);
        }

        return null;
      }

      var scope = new Scope
      {
        path = path,
        bypass = bypass,
        parent = this,
        id = next.id
      };

      currentChild = next.id;

      if (next.type != null)
      {
        switch(next.type)
        {
          case "call":
          {
            if (bypass?.Contains(next.id) == true)
            {
              scope.playbackType = 1;
            }
            else
            {
              // Check if it's a call playback.
              for(var item = GetRecord(next.firstChild);
                item != null;
                item = GetRecord(item.followingSibling))
              {
                if(item.type == "external")
                {
                  if((item.name == null) || !item.name.StartsWith('#'))
                  {
                    scope.playbackType = 1;
                  }

                  break;
                }
              }
            }

            break;
          }
          case "sql":
          {
            scope.playbackType = 2;

            break;
          }
          case "now":
          {
            scope.playbackType = 3;

            break;
          }
        }
      }

      return scope;
    }

    public object ScopeValue(object value)
    {
      var record = GetRecord(id);

      if (record == null)
      {
        return value;
      }

      if (!stores.TryGetValue(path, out var store))
      {
        return value;
      }

      var type = value != null ? value.GetType() :
        (record.type == "error") && IsEmpty(record.name) ? typeof(Exception) :
        null;

      try
      {
        var recordString = store.GetValue(record);
          
        var recordedValue = type == null ? null : 
          store.json ?
            Utils.FromJson(type, recordString, store.serializer) :
            Utils.FromXml(type, recordString, store.serializer);

        // now playback.
        if (playbackType == 3)
        {
          return recordedValue;
        }

        if (parent != null)
        {
          // external playback.
          if ((parent.playbackType == 1) && (record.type == "output"))
          {
            if ((value != null) && (recordedValue != null))
            {
              DataUtils.Copy(recordedValue, value, false);
            }

            return value;
          }

          // sql playback.
          if ((parent.playbackType == 2) && (record.type == "row"))
          {
            return recordedValue;
          }

          if ((value == null) && (recordedValue is Exception))
          {
            return recordedValue;
          }
        }

        if (!Utils.Equal(value, recordedValue))
        {
          var builder = new StringBuilder();

          builder.Append("Value mismatch:\r");
          BuildPath(builder);
          builder.Append("\rexpected:\r-----\r");
          builder.Append(recordString);
          builder.Append("\r-----\rfound:\r-----\r");
          builder.Append(Utils.ToXml(value, store.serializer));
          builder.Append("\r-----\r");

          Trace.TraceWarning(builder.ToString());
        }
      }
      catch(Exception e)
      {
        Log(e);
      }

      return value;
    }

    /// <summary>
    /// A playback indicator.
    /// <c>true</c> for playback mode when some data are retrieved or 
    /// validated from <see cref="IProfiler"/>, and <c>false</c> for tracing.
    /// </summary>
    public bool Playback => playbackType != 0;

    /// <summary>
    /// Gets <see cref="Record"/> by id.
    /// </summary>
    /// <param name="id">A record id</param>
    /// <returns>A <see cref="Record"/> instance, if available.</returns>
    private Record GetRecord(long id) =>
      !stores.TryGetValue(path, out var store) ? null :
        store.GetRecords().TryGetValue(id, out var record) ? record : null;

    /// <summary>
    /// Reports scope mismatch.
    /// </summary>
    /// <param name="child">A child scope.</param>
    /// <param name="type">Expected record type.</param>
    /// <param name="name">Expected record name.</param>
    private void Mismatch(Record child, string type, string name)
    {
      var builder = new StringBuilder();

      builder.Append("A scope mismatch:\r");
      BuildPath(builder);

      builder.
        Append("expected type: ").
        Append(type).
        Append(", name: ").
        Append(name).
        Append('\r');

      if (child != null)
      {
        builder.
          Append("found record id: ").
          Append(child.id).
          Append(", type: ").
          Append(child.type).
          Append(", name: ").
          Append(child.name).
          Append('\r');
      }
      else
      {
        builder.Append("found null.\r");
      }

      Trace.TraceWarning(builder.ToString());
    }

    /// <summary>
    /// Builds a scope path.
    /// </summary>
    /// <param name="builder">A <see cref="StringBuilder"/> instance.</param>
    private void BuildPath(StringBuilder builder)
    {
      for(var scope = this;
        (scope != null) && (scope.id != 0);
        scope = scope.parent)
      {
        var record = GetRecord(scope.id);

        builder.
          Append("  at record id: ").
          Append(record.id).
          Append(", type: ").
          Append(record.type).
          Append(", name: ").
          Append(record.name).
          Append('\r');
      }
    }
  }

  /// <summary>
  /// A trace store.
  /// </summary>
  private class Store: IDisposable
  {
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Dispose()
    {
      if (stream != null)
      {
        try
        {
          stream.Close();
        }
        catch(Exception e)
        {
          Log(e);
        }

        stream = null;
      }
    }

    /// <summary>
    /// Gets a map of <see cref="Record"/> by id.
    /// </summary>
    /// <returns>A map of <see cref="Record"/></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Dictionary<long, Record> GetRecords()
    {
      if (records == null)
      {
        Debug.Assert(refCount != 0);
        Debug.Assert(stream == null);

        records = new();

        try
        {
          var buffer = new Buffer
          {
            stream = File.OpenRead(path)
          };

          buffer.ReadFirstLine();

          while(true)
          {
            var record = buffer.ReadRecord();

            if (record == null)
            {
              break;
            }

            records[record.id] = record;
          }
        }
        catch(Exception e)
        {
          Log(e);
        }

        var root = new Record { id = 0 };

        records[0] = root;

        long parent = 0;
        Record prev = null;

        foreach(var record in records.Values.OrderBy(r => (r.parent, r.id)))
        {
          if ((prev != null) && (parent == record.parent))
          {
            prev.followingSibling = record.id;
          }
          else
          {
            parent = record.parent;

            if (records.TryGetValue(parent, out var parentRecord))
            {
              parentRecord.firstChild = record.id;
            }
          }

          prev = record;
        }
      }

      return records;
    }

    /// <summary>
    /// Gets record value.
    /// </summary>
    /// <param name="record">A record to get value for.</param>
    /// <returns>A record value.</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public string GetValue(Record record)
    {
      if ((stream == null) || (record.length == 0))
      {
        return null;
      }

      stream.Seek(record.offset, SeekOrigin.Begin);

      var buffer = new byte[record.length];
      var position = 0;

      while(true)
      {
        var count = stream.Read(
          buffer,
          position,
          buffer.Length - position);

        if (count <= 0)
        {
          break;
        }

        position += count;
      }

      return Encoding.UTF8.GetString(buffer);
    }

    /// <summary>
    /// A file path.
    /// </summary>
    public string path;

    /// <summary>
    /// A binary serializer.
    /// </summary>
    public ISerializer serializer;

    /// <summary>
    /// true for json and false for xml format.
    /// </summary>
    public bool json;

    /// <summary>
    /// A log stream.
    /// </summary>
    public Stream stream;

    /// <summary>
    /// A reference count.
    /// </summary>
    public int refCount = 1;

    /// <summary>
    /// A map of records.
    /// </summary>
    public Dictionary<long, Record> records;
  }

  private class Buffer
  {
    public Stream stream;
    public MemoryStream data = new();
    public long p = -1;
    public int c;
    public Dictionary<string, string> values = new();

    public void Read()
    {
      ++p;
      c = stream.ReadByte();
    }

    public void ReadFirstLine()
    {
      int count = 1;

      while((c != -1) && (c != '\r') && (c != '\n'))
      {
        Read();

        if (c == ',')
        {
          ++count;
        }
      }

      while((c == '\r') || (c == '\n'))
      {
        Read();
      }
    }

    public int SkipToNextLine()
    {
      int count = 0;

      while((c != -1) && (c != '\r') && (c != '\n'))
      {
        Read();
        ++count;
      }

      while((c == '\r') || (c == '\n'))
      {
        Read();
      }

      return count;
    }

    public int SkipColumn()
    {
      int count = 0;

      while((c != -1) && (c != ','))
      {
        Read();
        ++count;
      }

      Read();

      return count;
    }

    public string ReadColumn()
    {
      while((c != -1) && (c != ','))
      {
        data.WriteByte((byte)c);
        Read();
      }

      Read();

      if (data.Length == 0)
      {
        return null;
      }

      string value =
        Encoding.UTF8.GetString(data.GetBuffer(), 0, (int)data.Length);

      data.SetLength(0);

      return value;
    }

    public string Cache(string value)
    {
      if (value == null)
      {
        return value;
      }

      if (!values.TryGetValue(value, out var cached))
      {
        cached = value;
        values[value] = value;
      }

      return cached;
    }

    public Record ReadRecord()
    {
      if (c == -1)
      {
        return null;
      }

      var record = new Record();

      SkipColumn();
      SkipColumn();
      SkipColumn();

      record.id = long.Parse(ReadColumn());
      record.parent = long.Parse(ReadColumn());
      record.type = Cache(ReadColumn());
      record.name = Cache(ReadColumn());
      record.offset = p;
      record.length = SkipToNextLine();

      return record;
    }
  }

  /// <summary>
  /// Stores by session id.
  /// </summary>
  private static readonly ConcurrentDictionary<string, Store> stores = new();
}
