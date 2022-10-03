using System;
using System.IO;

using Bphx.Cool.Events;

using static Bphx.Cool.Functions;

namespace Bphx.Cool.UI;

/// <summary>
/// A common dialog control.
/// </summary>
[Serializable]
public class UICommonDialog : UIControl
{
  /// <summary>
  /// A file name value.
  /// </summary>
  public string FileName
  {
    get => fileName;
    set
    {
      if (value != fileName)
      {
        fileName = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// A file title value.
  /// </summary>
  public string FileTitle
  {
    get => fileTitle;
    set
    {
      if (value != fileTitle)
      {
        fileTitle = value;
        SetDirty();
      }
    }
  }

  /// <summary>
  /// Shows open file dialog.
  /// </summary>
  public void ShowOpen()
  {
    var procedure = Owner?.Procedure;
    var context = Application?.Context;

    if (context != null)
    {
      context.Dialog.SessionManager.CurrentMessageBox = new()
      {
        Procedure = procedure,
        Type = "FileOpen",
        Attributes = { { nameof(FileName), FileName } }
      };
    }
  }

  /// <summary>
  /// Gets result of <see cref="ShowOpen"/> call.
  /// </summary>
  /// <returns>File name.</returns>
  public string GetShowOpenResult()
  {
    var e = Application?.Context?.CurrentEvent as CommandEvent;
    var eventObject = e?.EventObject;

    if (eventObject != null)
    {
      var fileName = eventObject.Get(nameof(FileName))?.Value;

      FileName = fileName;
      FileTitle = IsEmpty(fileName) ? fileName : Path.GetFileName(fileName);
    }

    return FileName;
  }

  /// <summary>
  /// Invokes a method of <see cref="UIObject"/> instance.
  /// </summary>
  /// <param name="name">A method name.</param>
  /// <param name="args">Method's arguments.</param>
  /// <returns>Result type.</returns>
  public override object Invoke(string name, params object[] args)
  {
    if (string.Compare(name, "ShowOpen", true) == 0)
    {
      ShowOpen();

      return null;
    }

    return base.Invoke(name, args);
  }

  /// <summary>
  /// A file name.
  /// </summary>
  protected string fileName;

  /// <summary>
  /// A file title.
  /// </summary>
  protected string fileTitle;
}
