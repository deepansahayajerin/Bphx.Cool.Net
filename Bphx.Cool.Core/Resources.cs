using System;
using System.Linq;
using System.IO;
using System.Resources;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Xml.Serialization;
using System.Threading.Tasks;

using Bphx.Cool.Xml;

using static Bphx.Cool.Functions;

namespace Bphx.Cool;

/// <summary>
/// Implements assemble resources.
/// </summary>
public abstract class Resources: IResources
{
  /// <summary>
  /// Creates a <see cref="Resources"/> instance.
  /// </summary>
  public Resources()
  {
    ResourceManager = new(GetType());
  }

  /// <summary>
  /// Forces initialization.
  /// </summary>
  public void Init()
  {
    Task.Run(InitExitStates);
    Task.Run(InitProcedures);
  }

  /// <summary>
  /// A resource manager instance.
  /// </summary>
  public ResourceManager ResourceManager { get; }

  /// <summary>
  /// Gets an <see cref="NavigationRule"/> instance for a procedure name.
  /// </summary>
  /// <param name="name">A procedure name.</param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <returns>An <see cref="NavigationRule"/> instance</returns>
  public ActionInfo GetActionInfo(string name, int resourceID)
  {
    if (name == null)
    {
      return null;
    }

    var resourceKey = "Procedure_" + name;
    var cache = MemoryCache.Default;
    var key = resourceKey + ":" + GetType().AssemblyQualifiedName;

    if (cache[key] is not ActionInfo actionInfo)
    {
      var procedure = ResourceManager.GetString(resourceKey);

      if (string.IsNullOrEmpty(procedure))
      {
        actionInfo = null;
      }
      else
      {
        var serializer = new XmlSerializer(typeof(Rule));
        var rule = (Rule)serializer.Deserialize(new StringReader(procedure));

        actionInfo = new(
          GetType().Assembly.GetType(rule.Type, true),
          rule,
          resourceID);

        cache.Add(key, actionInfo, cachePolicy);
      }
    }

    return actionInfo;
  }

  /// <summary>
  /// Gets a procedure name by a transaction code. 
  /// </summary>
  /// <param name="transactionCode">A transaction code.</param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <returns>
  /// A procedure name, or null if there is no procedure for a specified 
  /// transaction name.
  /// </returns>
  public string GetProcedureByTransactionCode(
    string transactionCode, 
    int resourceID)
  {
    if (transactionCodes == null)
    {
      InitProcedures();
    }

    return transactionCodes.Get(transactionCode?.ToUpper());
  }

  /// <summary>
  /// Gets <see cref="ExitState"/> instance by name.
  /// </summary>
  /// <param name="exitState">An exit state name.</param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <returns>
  /// An <see cref="ExitState"/> instance, or null if no exitstate 
  /// is available for the name.
  /// </returns>
  public ExitState GetExitState(string exitState, int resourceID)
  {
    if (exitStates == null)
    {
      InitExitStates();
    }

    return exitStates.Count == 0 ? null : exitStates.Get(exitState);
  }

  /// <summary>
  /// Gets <see cref="ExitState"/> instance by ID.
  /// </summary>
  /// <param name="exitState">An exit state ID.</param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <returns>
  /// An <see cref="ExitState"/> instance, or null if no exitstate 
  /// is available for the ID.
  /// </returns>
  public ExitState GetExitState(int exitStateID, int resourceID)
  {
    if (exitStateID == 0)
    {
      return null;
    }

    if (exitStates == null)
    {
      InitExitStates();
    }

    return exitStateIDs.Count == 0 ? null : exitStateIDs.Get(exitStateID);
  }

  /// <summary>
  /// Gets a message for an exit state.
  /// </summary>
  /// <param name="name">An exit state name.</param>
  /// <param name="dialect">Optional, a dialect name.</param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <returns>An exit state message.</returns>
  public string GetExitStateMessage(
    string name, 
    string dialect,
    int resourceID)
  {
    return IsEmpty(name) ? null : ResourceManager.GetString(
      string.IsNullOrEmpty(dialect) ? 
        "S_" + name : 
        "S_" + dialect + "_" + name);
  }

  /// <summary>
  /// Initializes exit states.
  /// </summary>
  private void InitExitStates()
  {
    lock(exitStatesSync)
    {
      if (exitStatesError != null)
      {
        throw new IOException("Cannot load exit states.", exitStatesError);
      }

      if (exitStates != null)
      {
        return;
      }

      try
      {
        exitStates = new();
        exitStateIDs = new();

        var exitStatesValue = ResourceManager.GetString("ExitStates");

        if (!string.IsNullOrEmpty(exitStatesValue))
        {
          var serializer = new XmlSerializer(typeof(ExitStates));
          var resources = (ExitStates)serializer.
            Deserialize(new StringReader(exitStatesValue));

          foreach(var exitState in resources.ExitState)
          {
            if (exitState.Id != 0)
            {
              exitStateIDs[exitState.Id] = exitState;
            }

            exitStates[exitState.Name] = exitState;
          }
        }
      }
      catch(Exception e)
      {
        exitStatesError = e;

        throw;
      }
    }
  }

  /// <summary>
  /// Loads mappings of procedures names to an application roles and
  /// transaction codes.
  /// </summary>
  private void InitProcedures()
  {
    lock(proceduresSync)
    {
      if (transactionsError != null)
      {
        throw new IOException("Cannot load transactions.", transactionsError);
      }

      if (transactionCodes != null)
      {
        return;
      }

      try
      {
        var serializer = new XmlSerializer(typeof(Procedures));
        var xml = ResourceManager.GetString("Procedures");
        var procedures = string.IsNullOrEmpty(xml) ? null :
          (Procedures)serializer.Deserialize(new StringReader(xml));

        transactionCodes = procedures?.Procedure?.
            Where(item => item?.Transaction != null).
            ToDictionary(
              item => item.Transaction.ToUpperInvariant(),
              item => item.Name) ?? new();
      }
      catch(Exception e)
      {
        transactionsError = e;

        throw;
      }
    }
  }

  /// <summary>
  /// Error that has happened during loading of exit states.
  /// </summary>
  private Exception exitStatesError;

  /// <summary>
  /// Error that has happened during loading of transactions.
  /// </summary>
  private Exception transactionsError;

  /// <summary>
  /// A map of transaction codes.
  /// </summary>
  private Dictionary<string, string> transactionCodes;

  /// <summary>
  /// A map of exist states.
  /// </summary>
  private Dictionary<string, ExitState> exitStates;

  /// <summary>
  /// A map of exit states by id.
  /// </summary>
  private Dictionary<int, ExitState> exitStateIDs;

  /// <summary>
  /// Exit states sync object.
  /// </summary>
  private readonly object exitStatesSync = new();

  /// <summary>
  /// Procedures sync object.
  /// </summary>
  private readonly object proceduresSync = new();

  /// <summary>
  /// A cache policy.
  /// </summary>
  private static readonly CacheItemPolicy cachePolicy =
    new() { SlidingExpiration = new(0, 10, 0) };
}
