using System;
using System.Collections.Generic;
using System.Data;

namespace Bphx.Cool.Data;

/// <smmary>
/// Default data converter implementation.
/// </summary>
public class DataConverter
{
  /// <summary>
  /// Defines a null date value, if any. 
  /// Default is <see cref="DateTime.MinValue"/>.
  /// </summary>
  public virtual DateTime? NullDate { get; } = DateTime.MinValue;

  /// <summary>
  /// Indicates whether to use ANSI strings. Default is false.
  /// </summary>
  public virtual bool AnsiStrings { get; } = false;

  /// <summary>
  /// Indicates whether to convert empty string value "" to " ".
  /// Specific to Oracle.
  /// </summary>
  public virtual bool EmptyStringAsSpace { get; } = false;

  /// <summary>
  /// Indicates whether to implement Time type as timestamp in SQL. 
  /// Specific to Oracle.
  /// </summary>
  public virtual bool TimeAsTimestamp { get; } = false;

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
  public virtual IDbCommand CreateCommand(
    IDbConnectionProvider connectionProvider,
    string sql,
    string name,
    object caller)
  {
    if (connectionProvider == null)
    {
      return null;
    }

    var command = connectionProvider.Connection.CreateCommand();

    command.Transaction = connectionProvider.Transaction;
    command.CommandType = CommandType.Text;
    command.CommandText = sql;

    return command;
  }

  /// <summary>
  /// Executes the SQL query using <see cref="IDbCommand"/> instance.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="behavior">A command behavior options.</param>
  /// <returns>An <see cref="IDataReader"/> instance.</returns>
  public virtual IDataReader ExecuteReader(
    IDbCommand command,
    CommandBehavior behavior) => 
    command?.ExecuteReader(behavior);

  /// <summary>
  /// Executes the SQL statement using <see cref="IDbCommand"/> instance.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <returns>Number of rows, if any, affected.</returns>
  public virtual int ExecuteNonQuery(IDbCommand command) => 
    command?.ExecuteNonQuery() ?? 0;

  /// <summary>
  /// Moves the <see cref="IDataReader"/> one row from its current position.
  /// </summary>
  /// <param name="reader">A <see cref="IDataReader"/> instance.</param>
  /// <returns>
  ///   <c>true</c> if the new current row is valid;
  ///   <c>false</c> if there are no more rows.
  /// </returns>
  public virtual bool Next(IDataReader reader) => reader?.Read() == true;

  /// <summary>
  /// Initializes a command.
  /// </summary>
  /// <param name="initialize">
  /// A function used to initialize the command.
  /// </param>
  /// <param name="command">A <see cref="IDbCommand"/> to initialize.</param>
  public virtual void Initialize(
    Action<DataConverter, IDbCommand> initialize,
    IDbCommand command) => 
    initialize?.Invoke(this, command);

  /// <summary>
  /// Populates data from a <see cref="IDataReader"/>.
  /// </summary>
  /// <param name="populate">A populate function.</param>
  /// <param name="reader">a <see cref="IDataReader"/> to populate from.</param>
  /// <returns><c>true</c> to continue, and <c>false</c> to break.</returns>
  public virtual bool Populate(
    Func<DataConverter, IDataReader, bool> populate,
    IDataReader reader) => 
    populate?.Invoke(this, reader) != false;

  /// <summary>
  /// Sets a named value to the specified command.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="name">A parameter name to set.</param>
  /// <param name="value">A parameter value.</param>
  /// <returns>A <see cref="IDbDataParameter"/> instance.</returns>
  public virtual IDbDataParameter SetInt32(
    IDbCommand command,
    string name,
    int value) => 
    AddParameter(command, name, DbType.Int32, value);

  /// <summary>
  /// Sets a named value to the specified command.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="name">A parameter name to set.</param>
  /// <param name="value">A parameter value.</param>
  /// <returns>A <see cref="IDbDataParameter"/> instance.</returns>
  public virtual IDbDataParameter SetNullableInt32(
    IDbCommand command,
    string name,
    int? value) => 
    AddParameter(command, name, DbType.Int32, value);

  /// <summary>
  /// Sets a named value to the specified command.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="name">A parameter name to set.</param>
  /// <param name="value">A parameter value.</param>
  /// <returns>A <see cref="IDbDataParameter"/> instance.</returns>
  public virtual IDbDataParameter SetInt16(
    IDbCommand command,
    string name,
    short value) => 
    AddParameter(command, name, DbType.Int16, value);

  /// <summary>
  /// Sets a named value to the specified command.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="name">A parameter name to set.</param>
  /// <param name="value">A parameter value.</param>
  /// <returns>A <see cref="IDbDataParameter"/> instance.</returns>
  public virtual IDbDataParameter SetNullableInt16(
    IDbCommand command,
    string name,
    short? value) => 
    AddParameter(command, name, DbType.Int16, value);

  /// <summary>
  /// Sets a named value to the specified command.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="name">A parameter name to set.</param>
  /// <param name="value">A parameter value.</param>
  /// <returns>A <see cref="IDbDataParameter"/> instance.</returns>
  public virtual IDbDataParameter SetInt64(
    IDbCommand command,
    string name,
    long value) => 
    AddParameter(command, name, DbType.Int64, value);

  /// <summary>
  /// Sets a named value to the specified command.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="name">A parameter name to set.</param>
  /// <param name="value">A parameter value.</param>
  /// <returns>A <see cref="IDbDataParameter"/> instance.</returns>
  public virtual IDbDataParameter SetNullableInt64(
    IDbCommand command,
    string name,
    long? value) => 
    AddParameter(command, name, DbType.Int64, value);

  /// <summary>
  /// Sets a named value to the specified command.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="name">A parameter name to set.</param>
  /// <param name="value">A parameter value.</param>
  /// <returns>A <see cref="IDbDataParameter"/> instance.</returns>
  public virtual IDbDataParameter SetDouble(
    IDbCommand command,
    string name,
    double value) => 
    AddParameter(command, name, DbType.Double, value);

  /// <summary>
  /// Sets a named value to the specified command.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="name">A parameter name to set.</param>
  /// <param name="value">A parameter value.</param>
  /// <returns>A <see cref="IDbDataParameter"/> instance.</returns>
  public virtual IDbDataParameter SetNullableDouble(
    IDbCommand command,
    string name,
    double? value) => 
    AddParameter(command, name, DbType.Double, value);

  /// <summary>
  /// Sets a named value to the specified command.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="name">A parameter name to set.</param>
  /// <param name="value">A parameter value.</param>
  /// <returns>A <see cref="IDbDataParameter"/> instance.</returns>
  public virtual IDbDataParameter SetDecimal(
    IDbCommand command,
    string name,
    decimal value) => 
    AddParameter(command, name, DbType.Decimal, value);

  /// <summary>
  /// Sets a named value to the specified command.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="name">A parameter name to set.</param>
  /// <param name="value">A parameter value.</param>
  /// <returns>A <see cref="IDbDataParameter"/> instance.</returns>
  public virtual IDbDataParameter SetNullableDecimal(
    IDbCommand command,
    string name,
    decimal? value) => 
    AddParameter(command, name, DbType.Decimal, value);

  /// <summary>
  /// Sets a named value to the specified command.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="name">A parameter name to set.</param>
  /// <param name="value">A parameter value.</param>
  /// <returns>A <see cref="IDbDataParameter"/> instance.</returns>
  public virtual IDbDataParameter SetString(
    IDbCommand command,
    string name,
    string value) => 
    AddParameter(
      command,
      name,
      AnsiStrings ? DbType.AnsiString : DbType.String,
      !string.IsNullOrEmpty(value) ? value :
        EmptyStringAsSpace ? " " : value);

  /// <summary>
  /// Sets a named value to the specified command.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="name">A parameter name to set.</param>
  /// <param name="value">A parameter value.</param>
  /// <returns>A <see cref="IDbDataParameter"/> instance.</returns>
  public virtual IDbDataParameter SetNullableString(
    IDbCommand command,
    string name,
    string value) => 
    AddParameter(
      command,
      name,
      AnsiStrings ? DbType.AnsiString : DbType.String,
      EmptyStringAsSpace && (value != null) && (value.Length == 0) ?
        " " : value);

  /// <summary>
  /// Sets a named value to the specified command.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="name">A parameter name to set.</param>
  /// <param name="value">A parameter value.</param>
  /// <returns>A <see cref="IDbDataParameter"/> instance.</returns>
  public virtual IDbDataParameter SetBytes(
    IDbCommand command,
    string name,
    byte[] value) =>
    AddParameter(command, name, DbType.Binary, value ?? Array.Empty<byte>());

  /// <summary>
  /// Sets a named value to the specified command.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="name">A parameter name to set.</param>
  /// <param name="value">A parameter value.</param>
  /// <returns>A <see cref="IDbDataParameter"/> instance.</returns>
  public virtual IDbDataParameter SetNullableBytes(
    IDbCommand command,
    string name,
    byte[] value) =>
    AddParameter(command, name, DbType.Binary, value);

  /// <summary>
  /// Sets a named value to the specified command.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="name">A parameter name to set.</param>
  /// <param name="value">A parameter value.</param>
  /// <returns>A <see cref="IDbDataParameter"/> instance.</returns>
  public virtual IDbDataParameter SetDate(
    IDbCommand command,
    string name,
    DateTime? value) => 
    AddParameter(command, name, DbType.Date, value ?? NullDate);

  /// <summary>
  /// Sets a named value to the specified command.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="name">A parameter name to set.</param>
  /// <param name="value">A parameter value.</param>
  /// <returns>A <see cref="IDbDataParameter"/> instance.</returns>
  public virtual IDbDataParameter SetNullableDate(
    IDbCommand command,
    string name,
    DateTime? value) =>
    AddParameter(command, name, DbType.Date, value ?? NullDate);

  /// <summary>
  /// Sets a named value to the specified command.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="name">A parameter name to set.</param>
  /// <param name="value">A parameter value.</param>
  /// <returns>A <see cref="IDbDataParameter"/> instance.</returns>
  public virtual IDbDataParameter SetTimeSpan(
    IDbCommand command, 
    string name, 
    TimeSpan value) =>
    AddParameter(command, name, DbType.Time, value);

  /// <summary>
  /// Sets a named value to the specified command.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="name">A parameter name to set.</param>
  /// <param name="value">A parameter value.</param>
  /// <returns>A <see cref="IDbDataParameter"/> instance.</returns>
  public virtual IDbDataParameter SetNullableTimeSpan(
    IDbCommand command,
    string name,
    TimeSpan? value) =>
    AddParameter(command, name, DbType.Time, value);

  /// <summary>
  /// Sets a named value to the specified command.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="name">A parameter name to set.</param>
  /// <param name="value">A parameter value.</param>
  /// <returns>A <see cref="IDbDataParameter"/> instance.</returns>
  public virtual IDbDataParameter SetDateTime(
    IDbCommand command,
    string name,
    DateTime? value) =>
    AddParameter(command, name, DbType.DateTime2, value ?? NullDate);

  /// <summary>
  /// Sets a named value to the specified command.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="name">A parameter name to set.</param>
  /// <param name="value">A parameter value.</param>
  /// <returns>A <see cref="IDbDataParameter"/> instance.</returns>
  public virtual IDbDataParameter SetNullableDateTime(
    IDbCommand command,
    string name,
    DateTime? value) =>
    AddParameter(command, name, DbType.DateTime2, value ?? NullDate);

  /// <summary>
  /// Gets a column's value.
  /// </summary>
  /// <param name="reader">A <see cref="IDataReader"/> instance.</param>
  /// <param name="column">A column index to read.</param>
  /// <returns>A column value.</returns>
  public virtual int GetInt32(IDataReader reader, int column)
  {
    var value = GetValue(reader, column);

    return value == null ? 0 : Convert.ToInt32(value);
  }

  /// <summary>
  /// Gets a column's value.
  /// </summary>
  /// <param name="reader">A <see cref="IDataReader"/> instance.</param>
  /// <param name="column">A column index to read.</param>
  /// <returns>A column value.</returns>
  public virtual int? GetNullableInt32(IDataReader reader, int column)
  {
    var value = GetValue(reader, column);

    return value == null ? null as int? : Convert.ToInt32(value);
  }

  /// <summary>
  /// Gets a column's value.
  /// </summary>
  /// <param name="reader">A <see cref="IDataReader"/> instance.</param>
  /// <param name="column">A column index to read.</param>
  /// <returns>A column value.</returns>
  public virtual short GetInt16(IDataReader reader, int column)
  {
    var value = GetValue(reader, column);

    return value == null ? (short)0 : Convert.ToInt16(value);
  }

  /// <summary>
  /// Gets a column's value.
  /// </summary>
  /// <param name="reader">A <see cref="IDataReader"/> instance.</param>
  /// <param name="column">A column index to read.</param>
  /// <returns>A column value.</returns>
  public virtual short? GetNullableInt16(IDataReader reader, int column)
  {
    var value = GetValue(reader, column);

    return value == null ? null as short? : Convert.ToInt16(value);
  }

  /// <summary>
  /// Gets a column's value.
  /// </summary>
  /// <param name="reader">A <see cref="IDataReader"/> instance.</param>
  /// <param name="column">A column index to read.</param>
  /// <returns>A column value.</returns>
  public virtual long GetInt64(IDataReader reader, int column)
  {
    var value = GetValue(reader, column);

    return value == null ? 0 : Convert.ToInt64(value);
  }

  /// <summary>
  /// Gets a column's value.
  /// </summary>
  /// <param name="reader">A <see cref="IDataReader"/> instance.</param>
  /// <param name="column">A column index to read.</param>
  /// <returns>A column value.</returns>
  public virtual long? GetNullableInt64(IDataReader reader, int column)
  {
    var value = GetValue(reader, column);

    return value == null ? null as long? : Convert.ToInt64(value);
  }

  /// <summary>
  /// Gets a column's value.
  /// </summary>
  /// <param name="reader">A <see cref="IDataReader"/> instance.</param>
  /// <param name="column">A column index to read.</param>
  /// <returns>A column value.</returns>
  public virtual double GetDouble(IDataReader reader, int column)
  {
    var value = GetValue(reader, column);

    return value == null ? 0 : Convert.ToDouble(value);
  }

  /// <summary>
  /// Gets a column's value.
  /// </summary>
  /// <param name="reader">A <see cref="IDataReader"/> instance.</param>
  /// <param name="column">A column index to read.</param>
  /// <returns>A column value.</returns>
  public virtual double? GetNullableDouble(IDataReader reader, int column)
  {
    var value = GetValue(reader, column);

    return value == null ? null as double? : Convert.ToDouble(value);
  }

  /// <summary>
  /// Gets a column's value.
  /// </summary>
  /// <param name="reader">A <see cref="IDataReader"/> instance.</param>
  /// <param name="column">A column index to read.</param>
  /// <returns>A column value.</returns>
  public virtual decimal GetDecimal(IDataReader reader, int column)
  {
    var value = GetValue(reader, column);

    return value == null ? 0M : Convert.ToDecimal(value);
  }

  /// <summary>
  /// Gets a column's value.
  /// </summary>
  /// <param name="reader"></param>
  /// <param name="column"></param>
  /// <returns></returns>
  public virtual decimal? GetNullableDecimal(IDataReader reader, int column)
  {
    var value = GetValue(reader, column);

    return value == null ? null as decimal? : Convert.ToDecimal(value);
  }

  /// <summary>
  /// Gets a column's value.
  /// </summary>
  /// <param name="reader">A <see cref="IDataReader"/> instance.</param>
  /// <param name="column">A column index to read.</param>
  /// <returns>A column value.</returns>
  public virtual string GetString(IDataReader reader, int column)
  {
    var value = GetValue(reader, column);

    return value == null ? "" : value.ToString();
  }

  /// <summary>
  /// Gets a column's value.
  /// </summary>
  /// <param name="reader">A <see cref="IDataReader"/> instance.</param>
  /// <param name="column">A column index to read.</param>
  /// <returns>A column value.</returns>
  public virtual string GetNullableString(IDataReader reader, int column)
  {
    var value = GetValue(reader, column);

    return value?.ToString();
  }

  /// <summary>
  /// Gets a column's value.
  /// </summary>
  /// <param name="reader">A <see cref="IDataReader"/> instance.</param>
  /// <param name="column">A column index to read.</param>
  /// <returns>A column value.</returns>
  public virtual byte[] GetBytes(IDataReader reader, int column)
  {
    var value = GetValue(reader, column);

    return value == null ? Array.Empty<byte>() : (byte[])value;
  }

  /// <summary>
  /// Gets a column's value.
  /// </summary>
  /// <param name="reader">A <see cref="IDataReader"/> instance.</param>
  /// <param name="column">A column index to read.</param>
  /// <returns>A column value.</returns>
  public virtual byte[] GetNullableBytes(IDataReader reader, int column)
  {
    var value = GetValue(reader, column);

    return (byte[])value;
  }

  /// <summary>
  /// Gets a column's value.
  /// </summary>
  /// <param name="reader">A <see cref="IDataReader"/> instance.</param>
  /// <param name="column">A column index to read.</param>
  /// <returns>A column value.</returns>
  public virtual DateTime? GetDate(IDataReader reader, int column)
  {
    var value = GetValue(reader, column);

    if (value == null)
    {
      return null;
    }

    var date = Convert.ToDateTime(value).Date;

    return date == NullDate ? null : date;
  }

  /// <summary>
  /// Gets a column's value.
  /// </summary>
  /// <param name="reader">A <see cref="IDataReader"/> instance.</param>
  /// <param name="column">A column index to read.</param>
  /// <returns>A column value.</returns>
  public virtual DateTime? GetNullableDate(IDataReader reader, int column) => 
    GetDate(reader, column);

  /// <summary>
  /// Gets a column's value.
  /// </summary>
  /// <param name="reader">A <see cref="IDataReader"/> instance.</param>
  /// <param name="column">A column index to read.</param>
  /// <returns>A column value.</returns>
  public virtual TimeSpan GetTimeSpan(IDataReader reader, int column)
  {
    var value = GetValue(reader, column);

    return value == null ? TimeSpan.Zero :
      Convert.ToDateTime(value).TimeOfDay;
  }

  /// <summary>
  /// Gets a column's value.
  /// </summary>
  /// <param name="reader">A <see cref="IDataReader"/> instance.</param>
  /// <param name="column">A column index to read.</param>
  /// <returns>A column value.</returns>
  public virtual TimeSpan? GetNullableTimeSpan(IDataReader reader, int column)
  {
    var value = GetValue(reader, column);

    return value == null ? null as TimeSpan? :
      Convert.ToDateTime(value).TimeOfDay;
  }

  /// <summary>
  /// Gets a column's value.
  /// </summary>
  /// <param name="reader">A <see cref="IDataReader"/> instance.</param>
  /// <param name="column">A column index to read.</param>
  /// <returns>A column value.</returns>
  public virtual DateTime? GetDateTime(IDataReader reader, int column)
  {
    var value = GetValue(reader, column);

    if (value == null)
    {
      return null;
    }

    var dateTime = Convert.ToDateTime(value);

    return dateTime == NullDate ? null : dateTime;
  }

  /// <summary>
  /// Gets a column's value.
  /// </summary>
  /// <param name="reader">A <see cref="IDataReader"/> instance.</param>
  /// <param name="column">A column index to read.</param>
  /// <returns>A column value.</returns>
  public virtual DateTime? GetNullableDateTime(
    IDataReader reader,
    int column) => 
    GetDateTime(reader, column);

  /// <summary>
  /// Adds command parameter.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="name">A parameter name to set.</param>
  /// <param name="type">A parameter type.</param>
  /// <param name="value">A parameter value.</param>
  /// <returns>A <see cref="IDbDataParameter"/> instance.</returns>
  public virtual IDbDataParameter AddParameter(
    IDbCommand command,
    string name,
    DbType type,
    object value)
  {
    if (command == null)
    {
      return null;
    }

    IDbDataParameter parameter = command.CreateParameter();

    parameter.ParameterName = name;
    parameter.DbType = type;
    parameter.Value = value;
    command.Parameters.Add(parameter);

    return parameter;
  }

  /// <summary>
  /// Gets column value.
  /// </summary>
  /// <param name="reader">A <see cref="IDataReader"/> instance.</param>
  /// <param name="column">A clumn index.</param>
  /// <returns>A read value.</returns>
  public virtual object GetValue(IDataReader reader, int column)
  {
    if (reader == null)
    {
      return null;
    }

    var value = reader[column];

    return (value == null) || (value is DBNull) ? null : value;
  }
}

/// <summary>
/// Extension API for <see cref="DataConverter"/> interface.
/// </summary>
public static class DataConverterExtensions
{
  /// <summary>
  /// Reads data from the database.
  /// </summary>
  /// <param name="db">A <see cref="DataConverter"/> instance.</param>
  /// <param name="commandFactory">
  /// A factory of a <see cref="IDbCommand"/> instance.
  /// </param>
  /// <param name="initialize">A command initializer.</param>
  /// <param name="populate">An action to populate result.</param>
  /// <returns>true if there are data, and false otherwise.</returns>
  public static bool Read(
    this DataConverter db,
    Func<DataConverter, IDbCommand> commandFactory,
    Action<DataConverter, IDbCommand> initialize,
    Func<DataConverter, IDataReader, bool> populate)
  {
    using var command = commandFactory(db);

    db.Initialize(initialize, command);

    using var reader = db.ExecuteReader(
      command,
      CommandBehavior.SingleRow | CommandBehavior.SingleResult);

    if (!db.Next(reader))
    {
      return false;
    }

    db.Populate(populate, reader);

    return true;
  }

  /// <summary>
  /// Creates a enumerator over data read from the database.
  /// </summary>
  /// <param name="db">A <see cref="DataConverter"/> instance.</param>
  /// <param name="commandFactory">
  /// A factory of a <see cref="IDbCommand"/> instance.
  /// </param>
  /// <param name="initialize">A command initializer.</param>
  /// <param name="populate">An action to populate result.</param>
  /// <returns>
  /// A data enumerator, where each item correspond to a read row.
  /// </returns>
  public static IEnumerable<bool> ReadEach(
    this DataConverter db,
    Func<DataConverter, IDbCommand> commandFactory,
    Action<DataConverter, IDbCommand> initialize,
    Func<DataConverter, IDataReader, bool> populate)
  {
    using var command = commandFactory(db);

      db.Initialize(initialize, command);

    using var reader =
      db.ExecuteReader(command, CommandBehavior.SingleResult);

    while(db.Next(reader))
    {
      if (!db.Populate(populate, reader))
      {
        yield break;
      }

      yield return true;
    }
  }

  /// <summary>
  /// Updates data in the database.
  /// </summary>
  /// <param name="db">A <see cref="DataConverter"/> instance.</param>
  /// <param name="commandFactory">
  /// A factory of a <see cref="IDbCommand"/> instance.
  /// </param>
  /// <param name="initialize">A command initializer.</param>
  /// <returns>Number of rows affected, if relevant.</returns>
  public static int Update(
    this DataConverter db,
    Func<DataConverter, IDbCommand> commandFactory,
    Action<DataConverter, IDbCommand> initialize)
  {
    using var command = commandFactory(db);

    db.Initialize(initialize, command);

    return db.ExecuteNonQuery(command);
  }
}
