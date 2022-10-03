namespace Bphx.Cool;

/// <summary>
/// Defines an execution state of a procedure step.
/// </summary>
public enum ExecutionState
{
  /// <summary>
  /// Initial execution state.
  /// When procedure executed with "execute first", its execution state
  /// is set to this value.
  /// </summary>
  Initial,

  /// <summary>
  /// A procedure step is ready for execution.
  /// On any procedure execution except "execute first", its execution state
  /// is set to this value.
  /// </summary>
  BeforeRun,

  /// <summary>
  /// A procedure is being executed.
  /// </summary>
  Running,

  /// <summary>
  /// A procedure step has been executed successfully.
  /// </summary>
  AfterRun,

  /// <summary>
  /// A procedure step has been finished, 
  /// and it cannot continue its run.
  /// </summary>
  Terminated,

  /// <summary>
  /// Display screen before a logic execution. 
  /// A procedure step waits for user input.
  /// </summary>
  WaitForUserInputDisplayFirst,

  /// <summary>
  /// A procedure step waits for user input.
  /// </summary>
  WaitForUserInput,

  /// <summary>
  /// A critical error has occured, a procedure step 
  /// has been terminated.
  /// </summary>
  CriticalError,

  /// <summary>
  /// A recoverable error has occured, 
  /// it's possible to restart a procedure step and 
  /// to continue its execution.
  /// </summary>
  RecoverableError,

  /// <summary>
  /// An intention to close procedure on first opportunity.
  /// </summary>
  Closing,

  /// <summary>
  /// A primary window or dialog of the corresponding 
  /// procedure step was closed.
  /// </summary>
  Closed
}
