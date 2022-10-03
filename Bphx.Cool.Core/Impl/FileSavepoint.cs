using System;
using System.IO;

namespace Bphx.Cool;

/// <summary>
/// A savepoint state.
/// </summary>
public class FileSavepoint: ISavepoint
{
  /// <summary>
  /// A savepoint path.
  /// </summary>
  public string Path { get; }

  /// <summary>
  /// A savepoint serializer.
  /// </summary>
  public ISerializer Serializer { get; }

  /// <summary>
  /// Creates a <see cref="FileSavepoint"/> instance.
  /// </summary>
  /// <param name="path">A file path to the safepoint file.</param>
  /// <param name="serializer">A serializer instance.</param>
  public FileSavepoint(string path, ISerializer serializer)
  {
    Path = path ?? throw new ArgumentNullException(nameof(path));
    Serializer = serializer ??
      throw new ArgumentNullException(nameof(serializer));
  }

  /// <summary>
  /// Gets a procedure from a savepoint, or <c>null</c> 
  /// if no procedure is available in a savepoint.
  /// </summary>
  /// <returns>A <see cref="IProcedure"/> instance or <c>null</c>.</returns>
  public IProcedure Get()
  {
    if(!File.Exists(Path))
    {
      return null;
    }

    // restore a procedure step's state from the specified file
    using var stream = File.OpenRead(Path);

    return Serializer.Deserilize<IProcedure>(stream);
  }

  /// <summary>
  /// Sets a procedure into the savepoint. 
  /// </summary>
  /// <param name="procedure">
  /// A <see cref="IProcedure"/> to set, or <c>null</c> to
  /// reset savepoint state.
  /// </param>
  public void Set(IProcedure procedure)
  {
    if(procedure != null)
    {
      using var stream = File.OpenWrite(Path);

      Serializer.Serilize(procedure, stream);
    }
    else
    {
      File.Delete(Path);
    }
  }
}
