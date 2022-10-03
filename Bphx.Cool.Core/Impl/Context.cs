using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Bphx.Cool.Events;

namespace Bphx.Cool.Impl;

/// <summary>An implementation of <see cref="IContext"/> interface.</summary>
[Serializable]
public class Context: IContext
{
  /// <summary>
  /// Creates a Context instance, sets the current procedure step.
  /// </summary>
  /// <param name="procedure">A <see cref="IProcedure"/> instance.</param>
  public Context(IProcedure procedure)
  {
    Procedure = procedure ?? throw new ArgumentNullException(nameof(procedure));
    Global = procedure.Global;
  }

  /// <summary>
  /// A <see cref="IDialogManager"/> instance.
  /// </summary>
  public IDialogManager Dialog
  {
    get => dialog;
    set
    {
      dialog = value;
      Profiler = dialog?.SessionManager.Profiler;
    }
  }

  /// <summary>
  /// A <see cref="IProcedure"/> instance.
  /// </summary>
  public IProcedure Procedure { get; }

  /// <summary>
  /// A <see cref="Global"/> instance.
  /// </summary>
  public Global Global { get; }

  /// <summary>
  /// A <see cref="IProfiler"/> instance, if available.
  /// </summary>
  public IProfiler Profiler { get; set; }

  /// <summary>
  /// Encoding used to convert bytes to and from strings. 
  /// </summary>
  public Encoding Encoding
  {
    get
    {
      if (!encodingInitialized)
      {
        encodingInitialized = true;
        encoding = Dialog.Environment.Encoding;
      }

      return encoding;
    }
  }

  /// <summary>
  /// A string comparer, if available.
  /// </summary>
  public IComparer<string> Collator
  {
    get
    {
      if (!collatorInitialized)
      {
        collatorInitialized = true;

        if (Procedure.Type != ProcedureType.Window)
        {
          collator = Dialog.Environment.Collator;
        }
      }

      return collator;
    }
  }

  /// <summary>
  /// Gets current transaction or strats a new one, if need.
  /// </summary>
  public ITransaction Transaction { get; set; }

  /// <summary>
  /// Gets an attribute map.
  /// </summary>
  public Dictionary<string, object> Attributes => attributes ??= new();

  /// <summary>
  /// The current event object.
  /// </summary>
  public Event CurrentEvent { get; set; }

  /// <summary>
  /// Current state machine instance.
  /// </summary>
  public IEnumerator CurrentStateMachine
  {
    get => stateMachine?.Enumerator;
    set => stateMachine = value == null ?
      null : new EnumeratorWrapper(value);
  }

  /// <summary>
  /// Queries a specified service. 
  /// </summary>
  /// <param name="type">A service type.</param>
  /// <returns>
  /// An instance of a specified service, or null if a service 
  /// is not available.
  /// </returns>
  public virtual object GetService(Type type) => 
    Dialog?.ServiceProvider?.GetService(type);

  /// <summary>
  /// Provides an existing instance of the specified type or creates a new one.
  /// </summary>
  /// <param name="type">A data type.</param>
  /// <returns>an instance of the requested type.</returns>
  public object GetData(Type type)
  {
    var key = type.FullName;
    var procedure = Procedure;
    var attributes = procedure.Type == ProcedureType.Window ?
      procedure.Attributes : Attributes;
    var instance = attributes.Get(key);

    if (instance == null)
    {
      instance = Activator.CreateInstance(type);

      if (instance is IInitializable)
      {
        attributes[key] = instance;
      }
    }
    else
    {
      if (instance is IInitializable initializable)
      {
        initializable.Initialize();
      }
    }

    return instance;
  }

  /// <summary>
  /// Gets a factory for a type.
  /// </summary>
  /// <param name="type">A type to get factory for.</param>
  /// <returns>
  /// A delegate to create an instance of the type.
  /// Delegate is of the form <see cref="Func{T, TResult}"/>, 
  /// <see cref="Func{T1, T2, TResult}"/>, and so on.
  /// </returns>
  public Delegate GetFactory(Type type) => FactoryProvider.GetFactory(type);

  /// <summary>
  /// Gets <see cref="IFactoryProvider"/> implementation.
  /// </summary>
  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
  protected IFactoryProvider FactoryProvider =>
    factoryProvider ??= this.GetService<IFactoryProvider>() ??
      throw new InvalidOperationException("No factory provider is available.");

  /// <summary>
  /// A context attributes map. 
  /// </summary>
  private Dictionary<string, object> attributes;

  /// <summary>
  /// Indicates that data encoding is initialized.
  /// </summary>
  [NonSerialized]
  private bool encodingInitialized;

  /// <summary>
  /// A data encoding.
  /// </summary>
  [NonSerialized]
  private Encoding encoding;

  /// <summary>
  /// Indicates that a string collator is initialized.
  /// </summary>
  [NonSerialized]
  private bool collatorInitialized;

  /// <summary>
  /// A string collator.
  /// </summary>
  [NonSerialized]
  private IComparer<string> collator;

  /// <summary>
  /// A <see cref="IDialogManager"/> instance.
  /// </summary>
  [NonSerialized]
  private IDialogManager dialog;

  /// <summary>
  /// A factory provider.
  /// </summary>
  [NonSerialized]
  private IFactoryProvider factoryProvider;

  /// <summary>
  /// A state machine reference.
  /// </summary>
  private EnumeratorWrapper stateMachine;
}
