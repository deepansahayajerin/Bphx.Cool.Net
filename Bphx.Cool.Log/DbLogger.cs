using System;
using System.Data;
using System.Text;

namespace Bphx.Cool.Log;

/// <summary>
/// A <see cref="IProfiler"/> implementation that writes everything 
/// into a database.
/// </summary>
[Serializable]
public class DbLogger: Logger
{
  /// <summary>
  /// <para>Creates a <see cref="DbLogger"/> instance.</para>
  /// </summary>
  /// <param name="connectionFactory">A database connection factory.</param>
  /// <param name="sessionId">A session ID.</param>
  /// <param name="serializer">A binary serializer.</param>
  public DbLogger(
    Func<IDbConnection> connectionFactory, 
    string sessionId, 
    ISerializer serializer):
    base(new LogWriter
    {
      connectionFactory = connectionFactory,
      sessionId = sessionId,
      serializer = serializer
    })
  {
  }

  /// <summary>
  /// Tests whether session already exists.
  /// </summary>
  /// <param name="connectionFactory">A database connection factory.</param>
  /// <param name="sessionId">A session ID.</param>
  /// <returns></returns>
  public static bool HasTrace(
    Func<IDbConnection> connectionFactory, 
    string sessionId)
  {
    using var connection = connectionFactory();

    connection.Open();

    using var command = connection.CreateCommand();

    command.CommandType = CommandType.Text;

    command.CommandText = 
      @"select top 1 1 from Trace where SessionID = @sessionId";

    SetParameter(command, "@sessionID", sessionId);

    return command.ExecuteScalar() != null;
  }

  /// <summary>
  /// Writes the record.
  /// </summary>
  [Serializable]
  private class LogWriter: ILogWriter
  {
    /// <summary>
    /// A database connection factory.
    /// </summary>
    public Func<IDbConnection> connectionFactory;

    /// <summary>
    /// A session id.
    /// </summary>
    public string sessionId;

    /// <summary>
    /// A binary serializer.
    /// </summary>
    public ISerializer serializer;

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
      using var connection = connectionFactory();

      connection.Open();

      using var command = connection.CreateCommand();

      command.CommandType = CommandType.Text;

      command.CommandText =
@"insert into Trace
(
  SessionID, Timestamp, TraceDuration, ID, ParentID, Type, Name, Value
)
values
(
  @sessionID, @timestamp, @duration, @id, @parent, @type, @name, @value
)";

      SetParameter(command, "@sessionID", sessionId);
      SetParameter(command, "@timestamp", timestamp);
      SetParameter(command, "@duration", duration);
      SetParameter(command, "@id", id);
      SetParameter(command, "@parent", parent);
      SetParameter(command, "@type", type);
      SetParameter(command, "@name", name);
      SetParameter(command, "@value", Escape(value?.ToString()));

      command.ExecuteNonQuery();
    }

    /// <summary>
    /// Escapes a value, if required. 
    /// </summary>
    /// <param name="value">A value to escape.</param>
    private static string Escape(string value)
    {
      if (value != null)
      {
        for(var i = 0; i < value.Length; ++i)
        {
          var c = value[i];

          if ((c <= ' ') || (c == ','))
          {
            var builder = new StringBuilder(value.Length + 4);

            if (i > 0)
            {
              builder.Append(value.AsSpan(0, i));
            }

            for(; i < value.Length; ++i)
            {
              c = value[i];

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

            return builder.ToString();
          }
        }
      }

      return value;
    }
  }

  /// <summary>
  /// Sets command parameters.
  /// </summary>
  /// <param name="command">A command instance.</param>
  /// <param name="name">A command name.</param>
  /// <param name="value">A command value.</param>
  private static void SetParameter(
    IDbCommand command, 
    string name, 
    object value)
  {
    var parameter = command.CreateParameter();

    parameter.ParameterName = name;
    parameter.Value = value == null ? DBNull.Value : value;

    command.Parameters.Add(parameter);
  }
}
