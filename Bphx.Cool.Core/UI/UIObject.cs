using System;
using System.Collections.Generic;

namespace Bphx.Cool.UI;

/// <summary>
/// An UI object.
/// </summary>
[Serializable]
public class UIObject: INamed, IAttributes, ISetAttribute, IInvocable
{
  /// <summary>
  /// Sets dirty state of the object.
  /// </summary>
  public virtual void SetDirty() => Owner?.SetDirty();

  /// <summary>
  /// Resets dirty state of the object.
  /// </summary>
  public virtual void ResetDirty()
  {
  }

  /// <summary>
  /// Locks dirty state.
  /// </summary>
  public virtual void LockDirty() => Owner?.LockDirty();

  /// <summary>
  /// Unlocks dirty state.
  /// </summary>
  public virtual void UnlockDirty() => Owner?.UnlockDirty();

  /// <summary>
  /// Gets object's name.
  /// </summary>
  /// <returns>The object's name.</returns>
  public override string ToString() => Name;

  /// <summary>
  /// An object name.
  /// </summary>
  public string Name { get; set; }

  /// <summary>
  /// Object's parent
  /// </summary>
  public string Parent
  {
    get => Owner?.Name;
    set { }
  }

  /// <summary>
  /// An object handle.
  /// </summary>
  public int Handle { get; set; }

  /// <summary>
  /// Gets a string containing the Interface Object's name.
  /// The descendants must override this method and return 
  /// the appropriate name.
  /// </summary>
  public virtual string ObjectType => GetType().Name;

  /// <summary>
  /// The root application object as a GUI Object. 
  /// </summary>
  public virtual UIApplication Application
  {
    get => application ?? Owner?.Application;
    set => application = value;
  }

  /// <summary>
  /// Owner UIWindow.
  /// </summary>
  public UIWindow Owner { get; private set; }

  /// <summary>
  /// Sets the owner window.
  /// </summary>
  /// <param name="window">An owner window.</param>
  public virtual void SetOwner(UIWindow window)
  {
    if (window != Owner)
    {
      Owner = window;
      handle = 0;
      SetDirty();
    }
  }

  #region IAttributes Members
  /// <summary>
  /// Gets attributes map.
  /// </summary>
  [State("attributes")]
  public Dictionary<string, object> Attributes => attributes ??= new();

  /// <summary>
  /// Gets an event attribute.
  /// </summary>
  /// <param name="name">An attribute name.</param>
  /// <returns>An attribute value.</returns>
  public object GetAttribute(string name)
  {
    return attributes?.Get(name);
  }

  /// <summary>
  /// Sets attribute value.
  /// </summary>
  /// <param name="name">An attribute name.</param>
  /// <param name="value">An attribute value.</param>
  public void SetAttribute(string name, object value)
  {
    if(value == null)
    {
      if(attributes == null)
      {
        return;
      }

      Attributes.Remove(name);
    }
    else
    {
      Attributes[name] = value;
    }

    SetDirty();
  }
  #endregion

  /// <summary>
  /// Invokes a method of <see cref="UIObject"/> instance.
  /// </summary>
  /// <param name="name">A method name.</param>
  /// <param name="args">Method's arguments.</param>
  /// <returns>Result value.</returns>
  public virtual object Invoke(string name, params object[] args)
  {
    if((string.Compare(name, "GetObject", true) == 0) &&
      ((args == null) || (args.Length == 0)))
    {
      return this;
    }

    throw new NotSupportedException($"Method {name} is not supported.");
  }

  /// <summary>
  /// An object handle.
  /// </summary>
  protected int handle;

  /// <summary>
  /// An application reference.
  /// </summary>
  protected UIApplication application;

  /// <summary>
  /// Attributes map.
  /// </summary>
  protected Dictionary<string, object> attributes;
}
