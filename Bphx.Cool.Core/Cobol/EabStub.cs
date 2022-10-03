using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Bphx.Cool.Cobol;

/// <summary>
/// Context related to execution of an EABs within a transaction.
/// </summary>
public interface IEabContext
{
  /// <summary>
  /// Runs EAB by name, with parameters passed as buffers.
  /// </summary>
  /// <param name="name">An EAB name.</param>
  /// <param name="args">Parameters.</param>
  /// <returns>Result code.</returns>
  int Execute(string name, byte[][] args);
}

/// <summary>
/// <see cref="IEabStub"/> implementation abstracting EAB call 
/// with <see cref="IEabContext"/>.
/// </summary>
public class EabStub: IEabStub
{
  /// <summary>
  /// Encoding to use when serializing and deserializing data 
  /// down to and from byte buffers. 
  /// </summary>
  public Encoding Encoding { get; init; }

  /// <summary>
  /// A factory for the <see cref="IEabContext"/>.
  /// </summary>
  public Func<IContext, IEabContext> EabContextFactory { get; init; }

  /// <summary>
  /// Implements the EAB call.
  /// </summary>
  /// <param name="name">A program name.</param>
  /// <param name="context"><see cref="IContext"/> instance.</param>
  /// <param name="import">An import instance.</param>
  /// <param name="export">An export instance.</param>
  /// <param name="options">Options defining how to pass parameters.</param>
  public void Execute(
    string name,
    IContext context,
    object import,
    object export,
    EabOptions options)
  {
    // Find owner context.
    while(context.Attributes.TryGetValue(Async.OwnerContext, out var owner))
    {
      context = (IContext)owner;
    }

    var serializer = new CobolSerializer(
      new CobolConverter(Encoding),
      (options & EabOptions.NoAS) != 0 ?
        CobolConverter.Options.AccessGroups :
        CobolConverter.Options.AccessFields);

    var inProperties = MemberAttribute.
      GetProperties(import.GetType()).
      Select(property => property.Name).
      ToArray();

    var outProperties = MemberAttribute.
      GetProperties(export.GetType()).
      Select(property => property.Name).
      ToArray();

    var dialog = context.Dialog;
    var procedure = context.Procedure;
    var transaction = context.Transaction;
    var attributes = procedure.Type == ProcedureType.Batch ?
      dialog.SessionManager.Attributes : transaction.Attributes;
    var key = GetType().FullName;
    var exists = attributes.TryGetValue(key, out var value);
    var eabContext = exists ? (IEabContext)value : EabContextFactory(context);

    if (!exists)
    {
      attributes[key] = eabContext;
      transaction.HasEnlistedResources = true;
    }

    context.Profiler.Value("external", "EAB", name);

    var returnCode = 0;
    var global = context.Global;

    global.ErrorNumber = 0;
    global.ErrorDescription = "";

    try
    {
      var globdata = null as byte[];
      var args = new List<byte[]>();

      if ((options & EabOptions.NoIefParams) == 0)
      {
        // If it is not CICS program set iefparams
        args.Add(new byte[16]);
        args.Add(new byte[16]);
      }

      if ((options & EabOptions.Hpvp) == 0)
      {
        if (inProperties.Length > 0)
        {
          var size = serializer.Size(import, inProperties);
          var buffer = new byte[size];

          try
          {
            serializer.Write(buffer, import, inProperties);
          }
          catch(Exception e)
          {
            Error(buffer, e, true, import);
          }

          //System.Diagnostics.Debug.WriteLine(Dump("Serialize ", buffer, import));

          args.Add(buffer);
        }

        if (outProperties.Length > 0)
        {
          var size = serializer.Size(export, outProperties);
          var buffer = new byte[size];

          try
          {
            serializer.Write(buffer, export, outProperties);
          }
          catch(Exception e)
          {
            Error(buffer, e, true, export);
          }

          //System.Diagnostics.Debug.WriteLine(Dump("Serialize ", buffer, export));

          args.Add(buffer);
        }

        args.Add(new byte[16]);
      }
      else
      {
        var action = context.Procedure.Name;

        // Initialize error message.
        global.InitErrMsg(
          context.Dialog.Resources,
          context.Procedure.ResourceID);

        globdata = new byte[Globdata.Size];

        Globdata.SetData(global, action, globdata, serializer.Converter);

        args.Add(globdata);

        foreach(var property in inProperties)
        {
          var size = serializer.Size(import, property);
          var buffer = new byte[size];

          try
          {
            serializer.Write(buffer, import, property);
          }
          catch(Exception e)
          {
            Error(buffer, e, true, import, property);
          }

          //System.Diagnostics.Debug.WriteLine(Dump("Serialize ", buffer, import, property));

          args.Add(buffer);
        }

        foreach(var property in outProperties)
        {
          var size = serializer.Size(export, property);
          var buffer = new byte[size];

          try
          {
            serializer.Write(buffer, export, property);
          }
          catch(Exception e)
          {
            Error(buffer, e, true, export, property);
          }

          //System.Diagnostics.Debug.WriteLine(Dump("Serialize ", buffer, export, property));

          args.Add(buffer);
        }
      }

      returnCode = eabContext.Execute(name, args.ToArray());

      global.ErrorNumber = returnCode;
      global.ErrorDescription = "";

      var index = (options & EabOptions.NoIefParams) == 0 ? 2 : 0;

      if ((options & EabOptions.Hpvp) == 0)
      {
        //if (inProperties.Length > 0)
        //{
        //  try
        //  {
        //    serializer.Read(args[index], import, inProperties);
        //  }
        //  catch(Exception e)
        //  {
        //    Error(args[0], e, false, import);
        //  }
        //
        //  //System.Diagnostics.Debug.WriteLine(Dump("Deserialize ", args[0], import));
        //}

        if (outProperties.Length > 0)
        {
          try
          {
            serializer.Read(args[index + 1], export, outProperties);
          }
          catch(Exception e)
          {
            Error(args[1], e, false, export);
          }

          //System.Diagnostics.Debug.WriteLine(Dump("Deserialize ", args[1], export));
        }
      }
      else
      {
        var status = Globdata.GetStatus(globdata, serializer.Converter);

        if (!string.IsNullOrWhiteSpace(status))
        {
          throw new AbortTransactionException(status);
        }

        //++index;
        index += 1 + inProperties.Length;

        //foreach(var property in inProperties)
        //{
        //  try
        //  {
        //    serializer.Read(args[i], import, property);
        //  }
        //  catch(Exception e)
        //  {
        //    Error(args[index], e, false, import, property);
        //  }
        //
        // //System.Diagnostics.Debug.WriteLine(Dump("Deserialize ", args[1], import, property));
        //
        //  ++index;
        //}

        foreach(var property in outProperties)
        {
          try
          {
            serializer.Read(args[index], export, property);
          }
          catch(Exception e)
          {
            Error(args[index], e, false, export, property);
          }

          //System.Diagnostics.Debug.WriteLine(Dump("Deserialize ", args[1], export, property));

          ++index;
        }
      }
    }
    catch(Exception e)
    {
      global.ErrorNumber = returnCode;
      global.ErrorDescription = e.Message;

      throw;
    }
  }

  /// <summary>
  /// Creates a dump message.
  /// </summary>
  /// <param name="message">A dump title.</param>
  /// <param name="data">A data span.</param>
  /// <param name="instance">Optional related instance value.</param>
  /// <param name="property">Optional related property name.</param>
  /// <returns>A dump string.</returns>
  protected string Dump(
    string message,
    Span<byte> data,
    object instance,
    string property = null)
  {
    var builder =
      new StringBuilder($"{message} {instance?.GetType().FullName} {property}");

    builder.AppendLine();
    DataUtils.Dump(builder, data, Encoding);

    return builder.ToString();
  }

  /// <summary>
  /// Throws serialization error with dump in message.
  /// </summary>
  /// <param name="data">A data span.</param>
  /// <param name="e">Optional related exception.</param>
  /// <param name="serialize">
  /// <code>true</code> to serialize, and <code>false</code> for 
  /// deserialize operation.
  /// </param>
  /// <param name="instance">Optional related instance value.</param>
  /// <param name="property">Optional related property name.</param>
  /// <exception cref="InvalidOperationException">
  /// Unconditionally throws an exception.
  /// </exception>
  protected void Error(
    Span<byte> data,
    Exception e,
    bool serialize,
    object instance,
    string property = null) =>
    throw new InvalidOperationException(
      Dump(
        serialize ? "Cannot serialize" : "Cannot deserialize",
        data,
        instance,
        property),
      e);
}
