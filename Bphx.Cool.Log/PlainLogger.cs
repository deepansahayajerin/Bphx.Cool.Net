using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

namespace Bphx.Cool.Log;

/// <summary>
/// A <see cref="IProfiler"/> implementation that writes everything 
/// into a writer in CSV format.
/// </summary>
[Serializable]
public class PlainLogger: Logger
{
  /// <summary>
  /// <para>Creates a <see cref="PlainLogger"/> instance.</para>
  /// <para>Log is created in folder <c>path</c>.</para>
  /// </summary>
  /// <param name="path">A root folder for the logs.</param>
  /// <param name="sessionId">A session ID.</param>
  /// <param name="serializer">A binary serializer.</param>
  public PlainLogger(string path, string sessionId, ISerializer serializer):
    base(new LogWriter
    {
      path = path,
      sessionId = sessionId,
      serializer = serializer
    })
  {
  }

  /// <summary>
  /// Creates a <see cref="PlainLogger"/> instance.
  /// </summary>
  /// <param name="writer">
  /// A <see cref="StreamWriter"/> instance to write trace to. 
  /// <b>Note:</b> <see cref="PlainLogger"/> takes the ownership of the
  /// <see cref="StreamWriter"/>, and closes it when 
  /// the <see cref="PlainLogger"/> is closed.
  /// </param>
  /// <param name="sessionId">A session ID.</param>
  /// <param name="serializer">A binary serializer.</param>
  public PlainLogger(
    StreamWriter writer,
    string sessionId,
    ISerializer serializer):
    base(new LogWriter
    {
      writer = writer,
      sessionId = sessionId,
      serializer = serializer
    })
  {
  }

  /// <summary>
  /// Writes the record.
  /// </summary>
  [Serializable]
  private class LogWriter: ILogWriter
  {
    /// <summary>
    /// A file path for the log.
    /// </summary>
    public string path;

    /// <summary>
    /// A <see cref="StreamWriter"/> instance.
    /// </summary>
    public StreamWriter writer;

    /// <summary>
    /// A session id.
    /// </summary>
    public string sessionId;

    /// <summary>
    /// A binary serializer.
    /// </summary>
    public ISerializer serializer;

    /// <summary>
    /// Disposes the scope.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Dispose()
    {
      closed = true;

      if (writer != null)
      {
        try
        {
          writer.Close();
        }
        catch(Exception e)
        {
          Log(e);
        }

        writer = null;
      }
    }

    /// <summary>
    /// Converts a value to a form ready to write.
    /// </summary>
    /// <param name="value">A value</param>
    /// <returns>Write value.</returns>
    public object Convert(object value) => Utils.ToJson(value, serializer);

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
    public void Write(
      DateTime timestamp, 
      long duration, 
      long id, 
      long parent, 
      string type, 
      string name, 
      object value)
    {
      var row = new StringBuilder();

      Append(row, sessionId);
      row.Append(',');
      row.Append(
        XmlConvert.ToString(
          timestamp,
          XmlDateTimeSerializationMode.Unspecified));
      row.Append(',');
      row.Append(duration);
      row.Append(',');
      row.Append(id);
      row.Append(',');
      row.Append(parent);
      row.Append(',');
      Append(row, type);
      row.Append(',');
      Append(row, name);
      row.Append(',');
      Append(row, value?.ToString());
      row.Append("\r\n");

      Write(row.ToString());
      Flush();
    }

    /// <summary>
    /// Writes the row down to store.
    /// </summary>
    /// <param name="row">A row to write.</param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Write(string row)
    {
      if (closed)
      {
        return;
      }

      try
      {
        if (!initialized)
        {
          if (writer == null)
          {
            writer = GetWriter(path);
          }

          initialized = true;

          writer.Write(
            "SessionID,Timestamp,TraceDuration,ID," +
            "ParentID,Type,Name,Value\r\n");
        }

        writer.Write(row);
      }
      catch(Exception e)
      {
        Log(e);
      }
    }

    /// <summary>
    /// Flushes the writer's buffer.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Flush()
    {
      if (!closed && (writer != null))
      {
        try
        {
          writer.Flush();
        }
        catch(Exception e)
        {
          Log(e);
        }
      }
    }

    /// <summary>
    /// Closed indicator.
    /// </summary>
    private bool closed;

    /// <summary>
    /// Initialized  indicator.
    /// </summary>
    private bool initialized;

    /// <summary>
    /// Returns an output stream where the current trace info 
    /// will be written to.
    /// </summary>
    /// <param name="path">A folder path to store the log.</param>
    /// <returns>A <see cref="StreamWriter"/> instance.</returns>
    private static StreamWriter GetWriter(string path)
    {
      Directory.CreateDirectory(Path.GetDirectoryName(path));

      return File.CreateText(path);
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
}
