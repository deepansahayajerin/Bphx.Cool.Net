using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Resources;
using System.Diagnostics;

using System.Runtime.Caching;
using System.Runtime.CompilerServices;

using System.Data.Common;

using Bphx.Cool.Impl;
using Bphx.Cool.Data;
using Bphx.Cool.Events;
using Bphx.Cool.UI;

using static Bphx.Cool.Functions;

namespace Bphx.Cool;

/// <summary>
/// A base class for the action implementation.
/// </summary>
[Serializable]
public class Action
{
  /// <summary>
  /// Application execution context.
  /// </summary>
  public readonly IContext context;

  /// <summary>
  /// An instance of "GLOBAL" bean.
  /// </summary>
  public readonly Global global;

  /// <summary>
  /// Creates an <see cref="Action"/> instance.
  /// </summary>
  /// <param name="context">An application context.</param>
  public Action(IContext context)
  {
    this.context = context;
    global = context.Global;
  }

  /// <summary>
  /// An <see cref="IEnvironment"/> instance.
  /// </summary>
  protected IEnvironment Environment => Dialog.Environment;

  /// <summary>
  /// An <see cref="IResources"/> instance.
  /// </summary>
  protected IResources Resources => Dialog.Resources;

  /// <summary>
  /// A <see cref="IProcedure"/> instance.
  /// </summary>
  protected IProcedure Procedure => context.Procedure;

  /// <summary>
  /// An <see cref="IDialogManager"/> instance.
  /// </summary>
  protected IDialogManager Dialog => context.Dialog;

  /// <summary>
  /// Gets a current event object.
  /// </summary>
  protected EventObject EventObject => context.CurrentEvent?.EventObject;

  /// <summary>
  /// Gets <see cref="ITimers"/> service.
  /// </summary>
  protected ITimers Timers => Procedure.Timers;

  /// <summary>
  /// Queries a specified service.
  /// </summary>
  /// <typeparam name="T">a service type.</typeparam>
  /// <returns>
  /// An instance of a specified service, 
  /// or null if a service is not available.
  /// </returns>
  public T GetService<T>() => context.GetService<T>();

  /// <summary>
  /// Gets "Current Timestamp" value.
  /// </summary>
  protected DateTime Now() =>
    context.Profiler.Value(
      "now", 
      null, 
      Environment.Clock(Dialog.SessionManager));

  /// <summary>
  /// Gets <see cref="Async"/> instance.
  /// </summary>
  protected Async Async(IContext context = null) =>
    new(context ?? this.context);

  /// <summary>
  /// Calls action function.
  /// </summary>
  /// <typeparam name="I">An import type.</typeparam>
  /// <typeparam name="E">An export type.</typeparam>
  /// <param name="action">An action function.</param>
  /// <param name="import">An import.</param>
  /// <param name="export">An export.</param>
  [MethodImpl(
    MethodImplOptions.AggressiveInlining |
    MethodImplOptions.AggressiveOptimization)]
  protected void Call<I, E>(
    Action<IContext, I, E> action, 
    I import, 
    E export) =>
    context.Call(action, import, export);

  /// <summary>
  /// Calls action function.
  /// </summary>
  /// <typeparam name="I">An import type.</typeparam>
  /// <typeparam name="E">An export type.</typeparam>
  /// <param name="factory">An instance factory.</param>
  /// <param name="action">An action function.</param>
  /// <param name="import">An import.</param>
  /// <param name="export">An export.</param>
  /// <returns>A continuation.</returns>
  [MethodImpl(
    MethodImplOptions.AggressiveInlining |
    MethodImplOptions.AggressiveOptimization)]
  public IEnumerable<bool> Call<I, E>(
    Func<IContext, I, E, IEnumerable<bool>> action,
    I import,
    E export) =>
    context.Call(action, import, export);

  /// <summary>
  /// Gets handle for the <see cref="UIObject"/>.
  /// </summary>
  /// <param name="value"><see cref="UIObject"/> instance.</param>
  /// <returns>A handle.</returns>
  protected int GetHandle(UIObject value) => context.GetHandle(value);

  /// <summary>
  /// <para>Gets <see cref="UI.UIObject"/> by handle.</para>
  /// </para>
  /// <see cref="UINullControl"/> is returned if no instance is found 
  /// for the handle.
  /// </para>
  /// </summary>
  /// <param name="handle">An object handle.</param>
  /// <returns>A <see cref="UI.UIObject"/> instance.</returns>
  protected UIObject UIObject(int handle) => context.GetUIObject(handle);

  /// <summary>
  /// Gets a collection of windows for the current procedure.
  /// </summary>
  /// <returns>A collection of windows for the current procedure.</returns>
  protected UICollection<UIWindow> GetWindows()
  {
    var windows = new UICollection<UIWindow>();

    windows.List.AddRange(
      Dialog.SessionManager.Procedures.
        Where(procedure => procedure.Type == ProcedureType.Window).
        SelectMany(procedure => procedure.Windows).
        Where(window => window.WindowState != WindowState.Closed));
    
    return windows;
  }

  /// <summary>
  /// Gets a window view by name.
  /// </summary>
  /// <param name="name">
  /// Optional, a window or dialog name.
  /// If value is not specified, or null, the active window is returned.
  /// </param>
  /// <returns>
  /// a WindowView instance if available, or null otherwise.
  /// </returns>
  protected UIWindow GetWindow(string name = null)
  {
    try
    {
      return Procedure.GetWindow(Dialog, name, true);
    }
    catch
    {
      // No window is found.
      return null;
    }
  }

  /// <summary>
  /// Gets a Screen feld
  /// </summary>
  /// <param name="view">A <see cref="View"/> instance.</param>
  /// <param name="name">A field name.</param>
  /// <returns>A <see cref="ScreenField"/> instance.</returns>
  public ScreenField GetField(View view, string name)
  {
    var screenField = view.GetField(name, true);

    screenField.ScreenFields = Procedure.GetScreenFields();

    return screenField;
  }

  /// <summary>
  /// Enables or disables a command.
  /// </summary>
  /// <param name="name">A command name.</param>
  /// <param name="enabled">
  /// <c>true</c> to enable, and <c>false</c> to disable command.
  /// </param>
  protected void EnableCommand(string name, bool enabled)
  {
    foreach(var window in Procedure.Windows)
    {
      if (window.ControlsWithCommands.TryGetValue(name, out var controls))
      {
        foreach(var control in controls)
        {
          if (control.Command == name)
          {
            control.Enabled = enabled;
          }
        }
      }
    }
  }

  /// <summary>
  /// Marks or unmarks a command.
  /// </summary>
  /// <param name="name">A command name.</param>
  /// <param name="mark">
  /// <c>true</c> to mark, and <c>false</c> to unmark command.
  /// </param>
  protected void MarkCommand(string name, bool mark)
  {
    foreach(var window in Procedure.Windows)
    {
      if (window.ControlsWithCommands.TryGetValue(name, out var controls))
      {
        foreach(var control in controls)
        {
          if ((control.Command == name) && (control is UIMenu menu))
          {
            menu.Marked = mark;
          }
        }
      }
    }
  }

  /// <summary>
  /// Queues an event for a further execution.
  /// </summary>
  /// <param name="anEvent">an window event to queue.</param>
  protected void QueueEvent(Event anEvent) =>
    context.QueueEvent(anEvent);

  /// <summary>
  /// Queues an event for a further execution.
  /// </summary>
  /// <param name="type">an event type to queue.</param>
  /// <param name="onRun">Optional on run handler.</param>
  protected void QueueEvent(string type, Func<IContext, bool> onRun = null)
  {
    if (!IsEmpty(type))
    {
      QueueEvent(new ProcedureEvent(Procedure, type.ToUpper(), null, onRun));
    }
  }

  /// <summary>
  /// Queues an event for a further execution.
  /// </summary>
  /// <param name="type">an event type to queue.</param>
  /// <param name="control">Optional control instance.</param>
  protected void QueueEvent(string type, UIObject control)
  {
    if (IsEmpty(type))
    {
      return;
    }

    if (control == null)
    {
      return;
    }

    var window = control as UIWindow ?? control.Owner;

    if (window == null)
    {
      return;
    }

    var procedure = window.Procedure;

    if (procedure == null)
    {
      return;
    }

    var eventObject = new EventObject { Type = type };

    eventObject.Window = window.Name;

    if (control != window)
    {
      eventObject.Component = control.Name;
    }

    QueueEvent(Event.Create(procedure, eventObject));
  }

  /// <summary>
  /// Queues an open window event.
  /// </summary>
  /// <param name="window">a window to open.</param>
  protected void Open(string window)
  {
    var e = Event.Create(
      Procedure,
      new()
      {
        Type = OpenEvent.EventType,
        Window = window
      });

    e.Cancelable = false;
    QueueEvent(e);
  }

  /// <summary>
  /// Queues a close window event.
  /// </summary>
  /// <param name="window">A window to close.</param>
  protected void Close(string window)
  {
    var e = Event.Create(
      Procedure,
      new()
      {
        Type = CloseEvent.EventType,
        Window = window
      });

    e.Cancelable = false;
    QueueEvent(e);
  }

  /// <summary>
  /// Triggers a start of a given application. 
  /// The calling application remains active.
  /// </summary>
  /// <param name="args">An application arguments.</param>
  /// <returns>A <see cref="LaunchCommand"/> instance, if any.</returns>
  protected LaunchCommand Launch(string args) => context.Launch(args);

  /// <summary>
  /// Immediately terminates a procedure.
  /// </summary>
  protected void Terminate() => throw new TerminateException();

  /// <summary>
  /// Refreshes the view.
  /// </summary>
  protected void Refresh()
  {
    // Do nothing.
  }

  /// <summary>
  /// Queues a message box to be shown.
  /// </summary>
  /// <param name="text">a message displayed in the dialog box.</param>
  /// <param name="title">a caption or title in the dialog box.</param>
  /// <param name="buttons">a caption for the pushbutton(s).</param>
  /// <param name="defaultButton">a default pushbutton.</param>
  /// <param name="style">a display icon type.</param>
  protected void ShowMessageBox(
    string text,
    string title,
    string buttons,
    int defaultButton,
    string style) =>
    Dialog.SessionManager.CurrentMessageBox = new()
    { 
      Procedure = Procedure,
      Text = text?.Trim(),
      Title = title == "" ? " " : title,
      Buttons = buttons,
      DefaultButton = defaultButton,
      Style = style
    };

  /// <summary>
  /// Gets a message box result, or null when there is no a message box.
  /// </summary>
  protected string MessageBoxResult =>
    Dialog.SessionManager.CurrentMessageBox?.Result;

  /// <summary>
  /// Prints a window. 
  /// </summary>
  /// <param name="handle">A window handle.</param>
  /// <param name="target">A print target</param>
  protected void PrintWindow(int handle, string target)
  {
    if ((handle == 0 ? GetWindow() : UIObject(handle)) is UIWindow window)
    {
      var launch = Launch("print");

      launch.Get<int>("id").Value = window.Procedure.Id;
      launch.Get("procedure").Value = window.Procedure.Name;
      launch.Get("window").Value = window.Name;
      launch.Get("target").Value = target;
    }
  }

  /// <summary>
  /// Creates an object using the object name. 
  /// </summary>
  /// <param name="name">an object name.</param>
  /// <returns>A new object instance.</returns>
  protected UIObject CreateObject(string name) =>
    Environment.CreateObject(name, context) ??
      throw new NotSupportedException(
        "Cannot create object: \"" + name + "\".");

  /// <summary>
  /// Invokes an OLE method.
  /// </summary>
  /// <typeparam name="T">A return type.</typeparam>
  /// <param name="name">A method name.</param>
  /// <param name="parameters">Optional method parameters.</param>
  /// <returns>The method's return value.</returns>
  protected T Invoke<T>(string name, params object[] parameters)
  {
    try
    {
      return (T)Convert.ChangeType(
        Environment.Invoke(name, context, parameters), typeof(T));
    }
    catch(NotSupportedException)
    {
      Trace.Write(
        $@"Call {name} is not supported with parameters: [{
          string.Join(", ", parameters)}].",
        "DEBUG");

      return default;
    }
  }

  /// <summary>
  /// Invokes an OLE method returning <see cref="Bphx.Cool.UI.UIObject"/>.
  /// </summary>
  /// <param name="name">A method name.</param>
  /// <param name="parameters">Optional method parameters.</param>
  /// <returns>The method's return value.</returns>
  protected UIObject Invoke(string name, params object[] parameters)
  {
    try
    {
      return (UIObject)Environment.Invoke(name, context, parameters);
    }
    catch(NotSupportedException)
    {
      Trace.Write(
        $@"Call {name} is not supported with parameters: [{
          string.Join(", ", parameters)}].",
        "DEBUG");

      return default;
    }
  }

  /// <summary>
  /// <para>
  /// Copies source object into the target, 
  /// matching properties using MemberAttribute layout.
  /// </para>
  /// <para>
  /// This method is used for beans generated from different models, 
  /// which match structurally only.
  /// </para>
  /// </summary>
  /// <typeparam name="S">a type of a source instance.</typeparam>
  /// <typeparam name="T">a type of a target instance.</typeparam>
  /// <param name="source">a source instance.</param>
  /// <param name="target">a target instance.</param>
  /// <returns>a target instance.</returns>
  protected T Copy<S, T>(S source, T target)
    where S: class
    where T: class =>
    DataUtils.Copy(source, target, false);

  /// <summary>
  /// Gets interface for File IO.
  /// </summary>
  /// <returns><see cref="IIO"/> instance.</returns>
  protected IIO IO() => Environment.CreateIO(context) ??
    throw new NotSupportedException("No IO API is avaialable.");

  /// <summary>
  /// <para>Gets and sets "exit state" name.</para>
  /// <para>Note: the setter updates also appropriate properties according 
  /// with the specified name.</para>
  /// </summary>
  protected string ExitState
  {
    get => global.Exitstate;
    set
    {
      global.SetExitState(value, Resources, Procedure.ResourceID);
      context.Profiler.Value("trace", "exitState", global);
      Environment.GlobalTracer?.Invoke("exitState", global, this);
    }
  }

  /// <summary>
  /// An exit state message.
  /// </summary>
  protected string ExitStateMessage
  {
    get
    {
      global.InitErrMsg(Resources, Procedure.ResourceID);

      return global.Errmsg;
    }
    set => global.Errmsg = value;
  }

  /// <summary>
  /// <para>Gets and sets "exit state" Id.</para>
  /// <para>Note: the setter updates also appropriate properties according 
  /// with the specified id.</para>
  /// </summary>
  protected int ExitStateId
  {
    get => global.ExitStateId;
    set
    {
      global.SetExitState(value, Resources, Procedure.ResourceID);
      context.Profiler.Value("trace", "exitState", global);
      Environment.GlobalTracer?.Invoke("exitState", global, this);
    }
  }

  /// <summary>
  /// Tests whether the current exit state is like a specified value.
  /// </summary>
  /// <param name="value">A value to test.</param>
  /// <returns>
  /// true if exits state is like a specified value, and false otherwise.
  /// </returns>
  protected bool IsExitState(string value) =>
    Global.AreExitStatesEqual(
      ExitState, 
      value, 
      Resources,
      Procedure.ResourceID);

  /// <summary>
  /// Optional string collator.
  /// </summary>
  protected IComparer<string> Collator => context.Collator;

  /// <summary>
  /// <para>
  /// Tests whether the first string is less than the second string. 
  /// </para>
  /// <para>
  /// The method is equivalent of:
  /// <code>Compare(first, second, Collator) &lt; 0</code>
  /// </para>
  /// </summary>
  /// <param name="first">A first string to compare.</param>
  /// <param name="second">A second string to compare.</param>
  /// <returns>
  /// true if first string is less than the second string; and 
  /// false otherwise.
  /// </returns>
  protected bool Lt(string first, string second) =>
    Compare(first, second, Collator) < 0;

  /// <summary>
  /// <para>
  /// Tests whether the first buffer is less than the second buffer. 
  /// </para>
  /// <para>
  /// The method is equivalent of:
  /// <code>Compare(first, second) &lt; 0</code>
  /// </para>
  /// </summary>
  /// <param name="first">A first buffer to compare.</param>
  /// <param name="second">A second buffer to compare.</param>
  /// <returns>
  /// true if first buffer is less than the second buffer; and 
  /// false otherwise.
  /// </returns>
  protected bool Lt(byte[] first, byte[] second) =>
    Compare(first, second) < 0;

  /// <summary>
  /// <para>
  /// Tests whether the first value is less than the second value. 
  /// </para>
  /// <para>
  /// The method is equivalent of:
  /// <code>Compare(first, second) &lt; 0</code>
  /// </para>
  /// </summary>
  /// <param name="first">A first value to compare.</param>
  /// <param name="second">A second value to compare.</param>
  /// <returns>
  /// true if first value is less than the second value; and 
  /// false otherwise.
  /// </returns>
  protected bool Lt<T>(T first, T second) 
    where T: IComparable<T> =>
    Compare(first, second) < 0;

  /// <summary>
  /// <para>
  /// Tests whether the first value is less than the second value. 
  /// </para>
  /// <para>
  /// The method is equivalent of:
  /// <code>Compare(first, second) &lt; 0</code>
  /// </para>
  /// </summary>
  /// <param name="first">A first value to compare.</param>
  /// <param name="second">A second value to compare.</param>
  /// <returns>
  /// true if first value is less than the second value; and 
  /// false otherwise.
  /// </returns>
  protected bool Lt<T>(T? first, T? second)
    where T: struct, IComparable<T> =>
    Compare(first, second) < 0;


  /// <summary>
  /// <para>
  /// Tests whether the first date is less than the second date. 
  /// </para>
  /// <para>
  /// The method is equivalent of:
  /// <code>Compare(first, second) < 0</code>
  /// </para>
  /// </summary>
  /// <param name="first">A first date to compare.</param>
  /// <param name="second">A second date to compare.</param>
  /// <returns>
  /// true if first date is less than the second date; and 
  /// false otherwise.
  /// </returns>
  protected bool Lt(DateTime? first, DateTime? second) =>
    Compare(first, second) < 0;

  /// <summary>
  /// Gets an error code for a specified exception.
  /// </summary>
  /// <param name="exception">an exception to get error code for.</param>
  /// <returns>a ErrorCode value.</returns>
  /// <seealso cref="ErrorCode"/>
  protected ErrorCode GetErrorCode(Exception exception) =>
    (Environment.ErrorCodeResolver ??
      throw new InvalidOperationException(
        "No error code resolver is available.",
        exception)).
      Resolve(exception);

  /// <summary>
  /// Gets a database connection provider.
  /// </summary>
  protected IDbConnectionProvider DbConnectionProvider =>
    context.Transaction.GetService<IDbConnectionProvider>() ??
    throw new NotSupportedException("No database connection is available.");

  /// <summary>
  /// Gets a command text for a name.
  /// </summary>
  /// <param name="name">a command name.</param>
  /// <returns>A SQL command text.</returnsthro>
  protected string GetCommandText(string name)
  {
    if (name == null)
    {
      throw new ArgumentNullException(nameof(name));
    }

    var cache = MemoryCache.Default;
    var type = GetType();
    var key = type.AssemblyQualifiedName;

    if (cache[key] is not ResourceManager resources)
    {
      resources = new(type);
      cache.Add(key, resources, cachePolicy);
    }

    return resources.GetString(name);
  }

  /// <summary>
  /// Reads data from the database.
  /// </summary>
  /// <param name="name">A statement name.</param>
  /// <param name="initialize">A command initializer.</param>
  /// <param name="populate">An action to populate result.</param>
  /// <returns>true if there are data, and false otherwise.</returns>
  protected bool Read(
    string name,
    Action<DataConverter, IDbCommand> initialize,
    Action<DataConverter, IDataReader> populate) =>
    context.GetDataConverter().Read(
      c => c.CreateCommand(
        DbConnectionProvider,
        GetCommandText(name),
        name,
        this),
      initialize,
      (c, r) =>
      {
        populate?.Invoke(c, r);

        return true;
      });

  /// <summary>
  /// Creates a enumerator over data read from the database.
  /// </summary>
  /// <param name="name">A statement name.</param>
  /// <param name="initialize">A command initializer.</param>
  /// <param name="populate">An action to populate result.</param>
  /// <returns>
  /// A data enumerator, where each item correspond to a read row.
  /// </returns>
  protected IEnumerable<bool> ReadEach(
    string name,
    Action<DataConverter, IDbCommand> initialize,
    Func<DataConverter, IDataReader, bool> populate) =>
    context.GetDataConverter().ReadEach(
      c => c.CreateCommand(
        DbConnectionProvider,
        GetCommandText(name),
        name,
        this), 
      initialize, 
      populate);

  /// <summary>
  /// Updates data in the database.
  /// </summary>
  /// <param name="name">A statement name.</param>
  /// <param name="initialize">A command initializer.</param>
  /// <returns>Number of rows affected, if relevant.</returns>
  protected int Update(
    string name,
    Action<DataConverter, IDbCommand> initialize) =>
    context.GetDataConverter().Update(
      c => c.CreateCommand(
        DbConnectionProvider,
        GetCommandText(name),
        name,
        this), 
      initialize);

  /// <summary>
  /// Creates a <see cref="DataExpression"/> instance.
  /// </summary>
  /// <param name="message">An error message.</param>
  /// <param name="sqlState">Sql state</param>
  /// <returns>A <see cref="DataExpression"/> instance.</returns>
  protected DbException DataError(string message, string sqlState) =>
    new Data.DataException(message, sqlState);

  /// <summary>
  /// Converts byte array into string  using Environment encoding.
  /// </summary>
  /// <param name="value">A byte array to convert.</param>
  /// <returns>A result string.</returns>
  protected string ToString(byte[] value) =>
    value == null ? null : context.Encoding.GetString(value);

  /// <summary>
  /// Converts a string value into a byte array using Environment encoding.
  /// </summary>
  /// <param name="value">Input string.</param>
  /// <returns>Result byte array.</returns>
  protected byte[] ToBytes(string value) =>
    value == null ? null : context.Encoding.GetBytes(value);

  /// <summary>
  /// Returns the next token of the given string. A token is a (string) sequence 
  /// of characters delimited by some other character.This function returns one 
  /// token at a time.A call to this function with the Parse string set to
  /// an empty string returns the next token in the last (non-empty) string that 
  /// was passed in value string. Subsequent calls with a null or zero-length
  /// string will work through and tokenize the string until no tokens remain.
  /// </summary>
  /// <param name="value">a string to be tokenized.</param>
  /// <param name="delimiter">a string containing delimiting characters.</param>
  /// <returns>
  /// A string containing the token as determined by the delimiter. 
  /// If no delimiter is found, the entire remaining string becomes the token 
  /// and is returned.
  /// </returns>
  protected string Tokenize(string value, string delimiter)
  {
    var key = typeof(Action) + "." + nameof(Tokenize);

    if (string.IsNullOrEmpty(value))
    {
      if (context.Attributes.TryGetValue(key, out object attr))
      {
        value = attr as string;

        if (string.IsNullOrEmpty(value))
        {
          return value;
        }
      }

      return null;
    }

    int pos = value.IndexOf(delimiter);

    if (pos == -1)
    {
      context.Attributes.Remove(key);

      return value;
    }

    context.Attributes[key] = value[(pos + delimiter.Length)..];

    return value[..(pos - 1)];
  }

  /// <summary>
  /// Converts a string, representing a base 10 number, in a text
  /// attribute view into a number in a numeric attribute view.
  /// </summary>
  /// <param name="value">
  /// A string representing an integer or decimal number
  /// (at most 1 decimal). May contain a + or - as the first or 
  /// last character, but not both first and last characters.  
  /// The leading and trailing whitespace characters are removed 
  /// before conversion. 
  /// </param>
  /// <returns>
  /// A number equal to the number represented in the given string.
  /// </returns>
  protected decimal TextToNumber(string value)
  {
    // TODO: set error code.
    value = Trim(value);

    int length = value.Length;

    if ((length == 0) || (length > 16))
    {
      return -1;
    }
    // no more cases

    return decimal.TryParse(value, out var result) ? result : -1;
  }

  /// <summary>
  /// A cache policy.
  /// </summary>
  private static readonly CacheItemPolicy cachePolicy =
    new() { SlidingExpiration = new(0, 10, 0) };
}
