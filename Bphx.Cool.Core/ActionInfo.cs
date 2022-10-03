using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;

using Bphx.Cool.Xml;

namespace Bphx.Cool;

using ExecuteMethod = Func<IContext, object, object, object>;

/// <summary>
/// An action info.
/// </summary>
public class ActionInfo
{
  /// <summary>
  /// An entry record.
  /// </summary>
  /// <param name="name">An action name.</param>
  /// <param name="action">An action.</param>
  /// <param name="anEvent">Optional event attribute.</param>
  public record struct Entry(
    string name, 
    ExecuteMethod action, 
    EventAttribute anEvent = null);

  /// <summary>
  /// Creates a <see cref="ActionInfo"/> instance.
  /// </summary>
  /// <param name="type">An action type.</param>
  /// <param name="rule">An optional <see cref="Rule"/> instance.</param>
  /// <param name="resourceID">A resource ID hint.</param>
  public ActionInfo(Type type, Rule rule = null, int resourceID = 0)
  {
    ActionType = type ?? throw new ArgumentNullException(nameof(type));

    var procedureStep =
      type.GetCustomAttribute<ProcedureStepAttribute>();

    if (procedureStep != null)
    {
      ProcedureType = (procedureStep.Type == ProcedureType.Server) &&
        (procedureStep.Role == "client") ?
        ProcedureType.Window : procedureStep.Type;
      ParticipateInTransaction = procedureStep.ParticipateInTransaction;
      ProcedureRole = procedureStep.Role;
    }
    else
    {
      ProcedureType = ProcedureType.Default;
    }

    var entries = new Dictionary<string, Entry>();
    MemberInfo entry = null;
    MemberInfo execute = null;

    // lookup an entry point and events handlers for the procedure step
    foreach(var member in
      ActionType.GetMembers(
        BindingFlags.Static | 
        BindingFlags.Public | 
        BindingFlags.GetField | 
        BindingFlags.GetProperty | 
        BindingFlags.InvokeMethod))
    {
      switch(member.MemberType)
      {
        case MemberTypes.Field:
        case MemberTypes.Method:
        case MemberTypes.Property:
          break;
        default:
          continue;
      }

      foreach(object attribute in member.GetCustomAttributes(false))
      {
        if (attribute is EntryAttribute)
        {
          if (entry != null)
          {
            throw new InvalidOperationException(
              "Too many entry points have been found in \"" +
              type.FullName + "\".");
          }

          entry = member;
        }
        else if (attribute is EventAttribute e)
        {
          var action = GetExecute(member);

          if (action == null)
          {
            continue;
          }

          var eventID = Events.Event.GetKey(
            e.Type?.Trim().ToUpper() ?? "",
            e.Window ?? "",
            e.Component ?? "");

          // caches an event handler
          entries[eventID] = new Entry(member.Name, action, e);
        }
        // No more cases
      }

      if ("Execute".Equals(
        member.Name, 
        StringComparison.InvariantCultureIgnoreCase))
      {
        execute = member;
      }
    }

    entry ??= execute;

    // caches a main entry point
    entries["Execute"] = new Entry(
      entry.Name, 
      GetExecute(entry) ??
        throw new InvalidOperationException(
          $"No entry point was found in \"{type.FullName}\"."));

    // Retrieve types of import and export.
    var parameters = entry is FieldInfo field ?
      field.FieldType.GetMethod("Invoke").GetParameters() :
      entry is PropertyInfo property ? 
        property.PropertyType.GetMethod("Invoke").GetParameters() :
      entry is MethodInfo method ? method.GetParameters() : null;

    if (parameters?.Length != 3)
    {
      throw new InvalidOperationException(
        "Invalid entry point in \"" + type.FullName + "\".");
    }

    ImportType = parameters[1].ParameterType;
    ExportType = parameters[2].ParameterType;
    Entries = entries;
    Rule = rule;
    ResourceID = resourceID;
  }

  /// <summary>
  /// Gets the action's type.
  /// </summary>
  public Type ActionType { get; }

  /// <summary>
  /// A procedure definition.
  /// </summary>
  public Rule Rule { get; }

  /// <summary>
  /// A resource ID hint.
  /// </summary>
  public int ResourceID { get; }

  /// <summary>
  /// Import type.
  /// </summary>
  public Type ImportType { get; }

  /// <summary>
  /// Export type.
  /// </summary>
  public Type ExportType { get; }

  /// <summary>
  /// A procedure type.
  /// </summary>
  public ProcedureType ProcedureType { get; }

  /// <summary>
  /// Indicates whether the server procedure step should participate in
  /// an existing transaction formed by a caller procedure step.
  /// </summary>
  public bool ParticipateInTransaction { get; }

  /// <summary>
  /// A procedure role.
  /// </summary>
  public string ProcedureRole { get; }

  /// <summary>
  /// Gets a map of identifier to a method actions.
  /// </summary>
  public IDictionary<string, Entry> Entries { get; }

  /// <summary>
  /// Creates new instance of import data, if any.
  /// </summary>
  /// <returns>an import data object or null.</returns>
  public object CreateImport()
  {
    return Activator.CreateInstance(ImportType);
  }

  /// <summary>
  /// Creates new instance of export data, if any.
  /// </summary>
  /// <returns>an export data object or null.</returns>    
  public object CreateExport()
  {
    return Activator.CreateInstance(ExportType);
  }

  /// <summary>
  /// Executes an action creating import and export with 
  /// <see cref="CreateImport"/> and <see cref="CreateExport"/> methods,
  /// and then copying with <see cref="DataUtils.Copy{S, T}(S, T, bool)"/> 
  /// </summary>
  /// <param name="name">A method identifier.</param>
  /// <param name="context">A context object.</param>
  /// <param name="import">An import object.</param>
  /// <param name="export">An export object.</param>
  /// <param name="errorOnMismatch">
  /// Determines whether to throw an exception in case of 
  /// layout mismatch (true) or write a warning message to a log.
  /// </param>
  /// <returns>A return value.</returns>
  public object Execute(
    string name,
    IContext context,
    object import,
    object export,
    bool errorOnMismatch)
  {
    var calledIn = DataUtils.Copy(import, CreateImport(), errorOnMismatch);
    var calledOut = DataUtils.Copy(export, CreateExport(), errorOnMismatch);
    var result = Execute(name, context, calledIn, calledOut);

    DataUtils.Copy(calledIn, import, errorOnMismatch);
    DataUtils.Copy(calledOut, export, errorOnMismatch);

    return result;
  }

  /// <summary>
  /// Executes an action directly passing <c>import</c> and 
  /// <c>export</c> parameters.
  /// </summary>
  /// <param name="name">A method identifier.</param>
  /// <param name="context">A context object.</param>
  /// <param name="import">An import object.</param>
  /// <param name="export">An export object.</param>
  /// <returns>A return value.</returns>
  public object Execute(
    string name,
    IContext context,
    object import,
    object export) =>
    context.Call(Entries[name].action, import, export);

  /// <summary>
  /// Gets an execute delegate.
  /// </summary>
  /// <param name="member">A member info.</param>
  /// <returns>An execute delegate, if any.</returns>
  private static ExecuteMethod GetExecute(MemberInfo member)
  {
    if (member is FieldInfo field)
    {
      if (typeof(Delegate).IsAssignableFrom(field.FieldType))
      {
        return WrapExecute((Delegate)field.GetValue(null));
      }
    }
    else if (member is PropertyInfo property)
    {
      if (typeof(Delegate).IsAssignableFrom(property.PropertyType))
      {
        return WrapExecute((Delegate)property.GetValue(null));
      }
    }
    else if (member is MethodInfo method)
    {
      return WrapExecute((object c, object i, object e) => 
        method.Invoke(null, new[] { c, i, e }));
    }

    return null;
  }

  /// <summary>
  /// Wraps execute delegate to unwrap <see cref="TargetInvocationException"/> during the call.
  /// </summary>
  /// <param name="handler">A delegate to wrap.</param>
  /// <returns>Wrapped delegate.</returns>
  public static ExecuteMethod WrapExecute(Delegate handler) =>
    (IContext c, object i, object e) =>
    {
      try
      {
        return handler.DynamicInvoke(c, i, e);
      }
      catch(TargetInvocationException error) when(error.InnerException != null)
      {
        ExceptionDispatchInfo.Capture(error.InnerException).Throw();

        throw;
      }
    };
}
