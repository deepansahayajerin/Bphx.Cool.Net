namespace Bphx.Cool;

/// <summary>
/// Public interface for File functions.
/// </summary>
public interface IIO
{
  /// <summary>
  /// Creates and opens a file.
  /// </summary>
  /// <param name="fileName">
  /// Defines a valid name for the file to be created and opened.
  /// Long name support for Windows and path support is provided.
  /// You must provide a fully qualified path and filename.
  /// </param>
  /// <param name="accessMode">
  /// a string specifying the intended use of the file. Valid values are:
  /// <list type="bullet">
  ///   <item><term>Read</term></item>
  ///   <item><term>Write</term></item>
  /// </list>
  /// If the file is opened for <c>Write</c>, it must be closed before
  /// <c>Read</c> can occur.If the file is opened for <c>Read</c>, 
  /// it must be closed before <c>Write</c> can occur. The default access
  /// mode value is <c>Read</c>.
  /// </param>
  /// <param name="shareMode">
  ///  a string indicating the permitted simultaneous activity on this file.
  ///  Valid values are:
  ///  <list type="bullet">
  ///  <item><term>No_Sharing</term></item> 
  ///  <item><term>Read</term></item>
  ///  <item><term>Write</term></item> 
  ///  <item><term>Read_Write</term></item>
  ///  </list> 
  ///  The default share value is <c>No_Sharing</c>.
  /// </param>
  /// <returns>
  /// The function returns a unique file number, if the create and open 
  /// was successful. This file number is used for subsequent file operation.
  /// The function returns a negative number if an error occurs.
  /// </returns>
  int CreateTextFile(string fileName, string accessMode, string shareMode);

  /// <summary>
  /// Opens an existing file.
  /// </summary>
  /// <param name="openFileName">
  /// A string containing the name of the file that is to be opened. Valid name
  /// for the target operating system.Long name support for  Windows and path 
  /// support is provided.You must provide a fully qualified path and filename.
  /// </param>
  /// <param name="openAccessMode">
  /// A string specifying the intended use of the file.
  /// Valid values are:
  /// <list type="bullet">
  ///   <item><code>Read</code></item>
  ///   <item><code>Write_Append</code></item>
  ///   <item><code>Write_Replace</code></item>
  /// </list>
  /// <para>
  /// If the file is opened for write, it must be closed before read can occur.
  /// If the file is opened for read, it must be closed before write can occur. 
  /// The default value is <code>Read</code>.
  /// </para>
  /// <b>Notes:</b> <code>Write_Replace</code> erases all information in file
  /// when the open completes. <code>Write_Append</code> automatically positions
  /// the pointer at the end of file.
  /// </param>
  /// <param name="shareMode">
  /// a string indicating the permitted simultaneous activity on this file.
  /// Valid values are:
  /// <list type="bullet">
  ///   <item><code>No_Sharing</code></item>
  ///   <item><code>Read</code></item>
  ///   <item><code>Write</code></item>
  ///   <item><code>Read_Write</code></item>
  /// </list>
  /// The default value is <code>No_Sharing</code>.This mode is tested by OS.
  /// </param>
  /// <returns>
  /// The function returns a unique file number, if the create and open 
  /// was successful.This file number is used for subsequent file operation.
  /// The function returns a negative number if an error occurs.
  /// </returns>
  int OpenTextFile(string openFileName, string openAccessMode, string shareMode);

  /**
   * Closes a file opened using the 
   * {@link #openTextFile(String, String, String)} function. 
   * Terminates all processing and flushes all buffers associated with an 
   * individual file.
   * 
   * @param fileNumber a number returned from an {@link openTextFile()} or 
   * {@link createTextFile()} function. The fileNumber is associated internally 
   * with a file name.
   * @return the function returns a positive number if successful. 
   *   If it is not successful, the function returns a negative number. 
   *   If the file is not opened, the function returns a negative number.
   */
  int CloseFile(int fileNumber);

  /**
   * Makes an exact copy of the given file.
   * 
   * @param fromFileName a string containing the file name from which to copy. 
   *   If this file is opened, copy continues. The buffer will be flushed 
   *   before the copy begins.
   * @param toFileName a string containing the name of the new file created and 
   *   populated as a result of the copy. This function fails if this file 
   *   already exists. If the desired filename already exists, it can be 
   *   deleted using the {@link #deleteFile()} function.
   * @return the function returns a positive number if successful, and a 
   *   negative number if it is not successful.<br/>
   *   <b>Note:</b> If the file in fromFileName is open, <code>copyFile</code>
   *   will fail if fromFileName's share attribute does not include 
   *   <code>Read</code>.
   */
  int CopyFile(string fromFileName, string toFileName);

  /**
   * Changes the name of the provided file to the provided name.
   * 
   * @param fromFileName a string containing a fully qualified path and file 
   *   name of a file to be renamed. This file must not currently be opened 
   *   by any application.
   * @param toFileName a string containing the new filename of the file.
   * @return the function returns a positive number if the rename operation is 
   *    successful. The function returns a negative number if the fromFileName 
   *    is open or does not exist, or if file in the toFileName argument 
   *    already exists.
   */
  int RenameFile(string fromFileName, string toFileName);

  /**
   * Deletes a file using the supplied file name. A failure occurs if the file 
   * does not exist or if the file is open or currently in use.
   * 
   * @param fileName a fully qualified path and file name conforming to the 
   *   targeted operating system.
   * @return the function returns a positive number if the delete is successful. 
   *   The function returns a negative number if the delete fails. A delete may 
   *   fail if the file is in use or open, or if the file does not exist.
   */
  int DeleteFile(string fileName);

  /**
   * Verifies that a file exists using the supplied filename.
   * 
   * @param fileName a string containing the fully qualified path and 
   *   file name to check. 
   * @return the function returns a positive number if the file exists, 
   *   zero (0) if the file does not exist, and a negative number if an 
   *   error occurred.
   */
  int FileExists(string fileName);

  /**
   * Obtains the length of the file, in bytes, of the provided file name. 
   * Note that the Carriage Return (CR) and Line Feed (LF) characters are 
   * included in the count.
   * 
   * @param fileName a string containing a fully qualified path and file name.
   * @return the function returns a positive number containing the length of 
   *   the file, if the file exists. If the file does not exist, the function 
   *   returns a negative number.
   */
  int FileLength(string fileName);

  /**
   * Adjusts the current position of the file pointer using the 
   * <code>newPosition</code> indicator. This function is valid in 
   * <code>Read</code> mode only.
   * 
   * @param fileNumber a number returned from an {@link openTextFile()} or 
   *   {@link createTextFile()} function. The <code>fileNumber</code> is 
   *   associated internally with a file name.
   * @param newPosition a string containing prompted values. Valid values are:
   * <ul>
   *  <li><code>BEGIN</code> - moves the current position to the beginning of 
   *  the file.</li>
   *  <li><code>END</code> - moves the current file position to the end of 
   *  the file.</li>
   *  <li><code>NEXT</code> - moves the current file position forward by one 
   *  line.</li>
   *  <li><code>PREVIOUS</code> - moves the current file position backward by 
   *  one line.</li>
   * </ul>
   *  A line is determined by the existence of a Carriage Return and Line Feed 
   *  (CR/LF) pair.
   * @return the function returns a positive number representing the new file 
   *   position if successful. If the file does not exist, the function returns 
   *   a negative number. If the function is attempted while the file is in 
   *   <code>Write</code> mode, the function returns a negative number.
   */
  long PositionFile(int fileNumber, string newPosition);

  /**
   * Writes a line to a text file. A Carriage Return (CR) followed by 
   * a Line Feed (LF) is appended to this string as it is written to disk. 
   * The <code>putText()</code> function appends a CR/LF pair to the end 
   * of each line that it writes. The <code>getText()</code> function removes 
   * the CR/LF pair from the end of each line that it reads.
   * 
   * @param fileNumber a number returned from an {@link openTextFile()} or 
   *   {@link createTextFile()} function. The <code>fileNumber</code> is 
   *   associated internally with a file name.
   * @param textData a string containing the data to be written. If this is an
   *   View, a fixed length string (with trailing spaces, if any are present) 
   *   is written to the file. The length is determined from the length of the 
   *   view. (Length of view is determined by the original length property for 
   *   the attribute used in the view.) Using the trim function to eliminate 
   *   trailing spaces may reduce the amount of data written to a file. You can 
   *   also use a literal. In order to ensure a Line designation, a CR/LF pair 
   *   is appended to each string written to the opened file. This appended 
   *   value is removed on subsequent <code>getText()</code> functions.
   * @return the function returns a number containing the number of bytes 
   *   actually written. This number will not contain count for the CR/LF pair. 
   *   The function returns a negative number if a failure occurs.
   */
  int PutText(int fileNumber, string textData);

  /**
   * Reads a line from the designated text file starting at the current position 
   * of the file pointer. All lines end with a carriage return - line feed 
   * (CR/LF) pair. The <code>putText()</code> function appends a CR/LF pair to 
   * the end of each line that it writes.
   * @param fileNumber a number returned from an {@link openTextFile()} or 
   *   {@link createTextFile()} function. The <code>fileNumber</code> is 
   *   associated internally with a file name.
   * @return the function returns a string read from the designated file. 
   *   If the function fails, it returns a null string "".
   */
  string GetText(int fileNumber);
}
