using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace Bphx.Cool.Data;

/// <smmary>
/// Default data converter implementation.
/// </summary>
public class ProfilingDataConverter: DataConverter
{
  /// <summary>
  /// Creates a <see cref="ProfilingDataConverter"/> instance.
  /// </summary>
  /// <param name="profiler">A <see cref="IProfiler"/> instance.</param>
  /// <param name="db">A <see cref="DataConverter"/> instance.</param>
  public ProfilingDataConverter(IProfiler profiler, DataConverter db)
  {
    this.profiler = profiler ??
      throw new ArgumentNullException(nameof(profiler));
    this.db = db ??
      throw new ArgumentNullException(nameof(db));
  }

  /// <summary>
  /// Defines a null date value, if any. 
  /// Default is <see cref="DateTime.MinValue"/>.
  /// </summary>
  public override DateTime? NullDate { get => db.NullDate; }

  /// <summary>
  /// Indicates whether to convert empty string value "" to " ".
  /// Specific to Oracle.
  /// </summary>
  public override bool EmptyStringAsSpace { get => db.EmptyStringAsSpace; }

  /// <summary>
  /// Indicates whether to implement Time type as timestamp in SQL. 
  /// Specific to Oracle.
  /// </summary>
  public override bool TimeAsTimestamp { get => db.TimeAsTimestamp; }

  /// <summary>
  /// Creates a <see cref="IDbCommand"/> instance.
  /// </summary>
  /// <param name="connectionProvider">
  /// A <see cref="IDbConnectionProvider"/> instance.
  /// </param>
  /// <param name="sql">An sql text.</param>
  /// <param name="name">A resource name.</param>
  /// <param name="caller">A caller instance.</param>
  /// <returns>A <see cref="IDbCommand"/> instance.</returns>
  public override IDbCommand CreateCommand(
    IDbConnectionProvider connectionProvider,
    string sql,
    string name,
    object caller)
  {
    Debug.Assert(scope == null);

    scope = profiler.Scope("sql", name);
    scope.Value(sql?.Trim());

    if (scope.Playback())
    {
      var error = scope.Value("error", null, null as Exception);

      if (error != null)
      {
        throw error;
      }

      return null;
    }
    else
    {
      try
      {
        return db.CreateCommand(connectionProvider, sql, name, caller);
      }
      catch(Exception e)
      {
        scope.Value("error", null, e);

        throw;
      }
    }
  }

  /// <summary>
  /// Executes the SQL query using <see cref="IDbCommand"/> instance.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="behavior">A command behavior options.</param>
  /// <returns>An <see cref="IDataReader"/> instance.</returns>
  public override IDataReader ExecuteReader(
    IDbCommand command, 
    CommandBehavior behavior)
  {
    Debug.Assert(scope != null);

    IDataReader reader;

    try
    {
      reader = db.ExecuteReader(command, behavior);
    }
    catch(Exception e)
    {
      scope.Value("error", null, e);

      throw;
    }

    if (scope.Playback())
    {
      var error = scope.Value("error", null, null as Exception);

      if (error != null)
      {
        throw error;
      }
    }

    return reader;
  }

  /// <summary>
  /// Executes the SQL statement using <see cref="IDbCommand"/> instance.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <returns>Number of rows, if any, affected.</returns>
  public override int ExecuteNonQuery(IDbCommand command)
  {
    Debug.Assert(scope != null);

    int result;

    try
    {
      result = db.ExecuteNonQuery(command);
    }
    catch(Exception e)
    {
      scope.Value("error", null, e);

      throw;
    }

    if (scope.Playback())
    {
      var error = scope.Value("error", null, null as Exception);

      if (error != null)
      {
        throw error;
      }
    }

    return result;
  }

  /// <summary>
  /// Moves the <see cref="IDataReader"/> one row from its current position.
  /// </summary>
  /// <param name="reader">A <see cref="IDataReader"/> instance.</param>
  /// <returns>
  ///   <c>true</c> if the new current row is valid;
  ///   <c>false</c> if there are no more rows.
  /// </returns>
  public override bool Next(IDataReader reader)
  {
    Debug.Assert(scope != null);

    if (rowScope != null)
    {
      rowScope.Dispose();
      rowScope = null;
    }

    var result = db.Next(reader);

    if ((reader != null) && !result)
    {
      return false;
    }

    rowScope = scope.Scope("row", null);

    return rowScope != null;
  }

  /// <summary>
  /// Initializes a command.
  /// </summary>
  /// <param name="initialize">
  /// A function used to initialize the command.
  /// </param>
  /// <param name="command">A <see cref="IDbCommand"/> to initialize.</param>
  public override void Initialize(
    Action<DataConverter, IDbCommand> initialize,
    IDbCommand command)
  {
    if (initialize == null)
    {
      return;
    }

    parameters = null;

    try
    {
      initialize(this, command);
    }
    finally
    {
      scope.Value("params", null, parameters);
      parameters = null;
    }
  }

  /// <summary>
  /// Populates data from a <see cref="IDataReader"/>.
  /// </summary>
  /// <param name="populate">A populate function.</param>
  /// <param name="reader">a <see cref="IDataReader"/> to populate from.</param>
  /// <returns><c>true</c> to continue, and <c>false</c> to break.</returns>
  public override bool Populate(
    Func<DataConverter, IDataReader, bool> populate,
    IDataReader reader)
  {
    Debug.Assert(rowScope != null);

    if (populate == null)
    {
      return true;
    }

    var playback = scope.Playback();

    row = playback ? rowScope.Value(Array.Empty<object>()) : null;

    try
    {
      return populate(this, reader);
    }
    finally
    {
      if (!playback)
      {
        rowScope.Value(row);
      }

      row = null;
    }
  }

  /// <summary>
  /// Adds command parameter.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="name">A parameter name to set.</param>
  /// <param name="type">A parameter type.</param>
  /// <param name="value">A parameter value.</param>
  /// <returns>A <see cref="IDbDataParameter"/> instance.</returns>
  public override IDbDataParameter AddParameter(
    IDbCommand command,
    string name,
    DbType type,
    object value)
  {
    parameters ??= new();

    if (scope.Playback())
    {
      value = parameters.TryGetValue(name, out var result) ? result : null;
    }
    else
    {
      parameters[name] = value;
    }

    return db.AddParameter(command, name, type, value);
  }

  /// <summary>
  /// Gets column value.
  /// </summary>
  /// <param name="reader">A <see cref="IDataReader"/> instance.</param>
  /// <param name="column">A clumn index.</param>
  /// <returns>A read value.</returns>
  public override object GetValue(IDataReader reader, int column)
  {
    if (row == null)
    {
      row = new object[column + 1];
    }
    else if (column >= row.Length)
    {
      Array.Resize(ref row, column + 1);
    }
    // No more cases.

    return scope.Playback() ? row[column] :
      (row[column] = db.GetValue(reader, column));
  }

  /// <summary>
  /// A <see cref="IProfiler"/> instance.
  /// </summary>
  private readonly IProfiler profiler;

  /// <summary>
  /// A <see cref="DataConverter"/> instance.
  /// </summary>
  private readonly DataConverter db;

  /// <summary>
  /// A current command profiler.
  /// </summary>
  private IProfiler scope;

  /// <summary>
  /// A current row profiler.
  /// </summary>
  private IProfiler rowScope;

  /// <summary>
  /// Row values being read.
  /// </summary>
  private object[] row;

  /// <summary>
  /// Parameters being populated.
  /// </summary>
  private Dictionary<string, object> parameters;
}
