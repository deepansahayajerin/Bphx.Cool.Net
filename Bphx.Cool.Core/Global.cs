namespace Bphx.Cool;

using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

using Bphx.Cool.Xml;

using static Bphx.Cool.Functions;

/// <summary>
/// A "GLOBAL" bean.
/// </summary>
[Serializable]
[XmlRoot(Namespace = Namespace, IsNullable = false)]
public class Global: ICloneable
{
  /// <summary>
  /// A <see cref="Global"/>'s namespace.
  /// </summary>
  public const string Namespace = "http://www.bphx.com/cool/xml";

  /// <summary>
  /// Default constructor.
  /// </summary>
  public Global() 
  {
    nextlocation = "";
    currentDialect = "DEFAULT";
    localSystemId = "";
    panelId = "";
    scrollInd = "";
    scrollAmt = "PAGE";
    scrollLoc = "";
    nexttran = "";
    PrinterTerminalId = "";
    userId = "";
    TerminalId = "";
    exitstate = "";
    trancode = "";
    pfkey = "";
    command = "";
    clientPassword = "";
    clientUserId = "";
    databaseSqlstate = "";
    DatabaseErrorMessage = "";
    MessageType = MessageType.None;
    TerminationAction = TerminationAction.Normal;
    ErrorDescription = "";
  }

  /// <summary>
  /// Copy constructor.
  /// </summary>
  /// <param name="that">another Global instance.</param>
  public Global(Global that) => Assign(that);

  /// <summary>
  /// Creates a copy of this object.
  /// </summary>
  /// <returns>A copy of this Global instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>
  /// Creates a copy of this object.
  /// </summary>
  /// <returns>A copy of this Global instance.</returns>
  public Global Clone() => new(this);

  /// <summary>
  /// Assigns value from another instance of Global.
  /// </summary>
  /// <param name="that">another Global instance.</param>
  /// <returns>this Global instance.</returns>
  public Global Assign(Global that)
  {
    this.nextlocation = that.nextlocation;
    this.currentDialect = that.currentDialect;
    this.localSystemId = that.localSystemId;
    this.panelId = that.panelId;
    this.scrollInd = that.scrollInd;
    this.scrollAmt = that.scrollAmt;
    this.scrollLoc = that.scrollLoc;
    this.nexttran = that.nexttran;
    this.PrinterTerminalId = that.PrinterTerminalId;
    this.userId = that.userId;
    this.TerminalId = that.TerminalId;
    this.exitstate = that.exitstate;
    this.ExitStateId = that.ExitStateId;
    this.trancode = that.trancode;
    this.pfkey = that.pfkey;
    this.Errmsg = that.Errmsg;
    this.command = that.command;
    this.clientPassword = that.clientPassword;
    this.clientUserId = that.clientUserId;
    this.TransactionRetryLimit = that.TransactionRetryLimit;
    this.TransactionRetryCount = that.TransactionRetryCount;
    this.DatabaseSqlcode = that.DatabaseSqlcode;
    this.databaseSqlstate = that.databaseSqlstate;
    this.DatabaseErrorMessage = that.DatabaseErrorMessage;
    this.TerminationAction = that.TerminationAction;
    this.MessageType = that.MessageType;
    this.ErrorNumber = that.ErrorNumber;
    this.ErrorDescription = that.ErrorDescription;
      
    return this;
  }

  /// <summary>
  /// Gets and sets "Next Location".
  /// </summary>
  [XmlElement("nextLocation"), JsonPropertyName("nextLocation")]
  [DefaultValue("")]
  public string Nextlocation
  {
    get => nextlocation;
    set => nextlocation = TrimEnd(value);
  }

  /// <summary>
  /// Gets and sets "Current Dialect".
  /// </summary>
  [XmlElement("currentDialect"), JsonPropertyName("currentDialect")]
  public string CurrentDialect
  {
    get => currentDialect;
    set => currentDialect = IsEmpty(value = TrimEnd(value)) ? 
      value : "DEFAULT";
  }

  /// <summary>
  /// Gets and sets "Local System Id".
  /// </summary>
      
  [DefaultValue("")]
  [XmlElement("localSystemId"), JsonPropertyName("localSystemId")]
  public string LocalSystemId
  {
    get => localSystemId;
    set => localSystemId = TrimEnd(value);
  }

  /// <summary>
  /// Gets and sets "Panel Id".
  /// </summary>
  [XmlElement("panelId"), JsonPropertyName("panelId")]
  [DefaultValue("")]
  public string PanelId
  {
    get => panelId;
    set => panelId = TrimEnd(value);
  }

  /// <summary>
  /// Gets and sets "Scroll Indicator Msg".
  /// </summary>
  [XmlElement("scrollInd"), JsonPropertyName("scrollInd")]
  [DefaultValue("")]
  public string ScrollInd
  {
    get => scrollInd;
    set => scrollInd = TrimEnd(value);
  }

  /// <summary>
  /// Gets and sets "Scroll Amount Msg".
  /// </summary>
  [XmlElement("scrollAmt"), JsonPropertyName("scrollAmt")]
  [DefaultValue("PAGE")]
  public string ScrollAmt
  {
    get => scrollAmt;
    set => scrollAmt = IsEmpty(value = TrimEnd(value)) ? value : "PAGE";
  }

  /// <summary>
  /// Gets and sets "Scroll Location Msg".
  /// </summary>
  [XmlElement("scrollLoc"), JsonPropertyName("scrollLoc")]
  [DefaultValue("")]
  public string ScrollLoc
  {
    get => scrollLoc;
    set => scrollLoc = TrimEnd(value);
  }

  /// <summary>
  /// Gets and sets "Next transaction".
  /// </summary>
  [XmlElement("nexttran"), JsonPropertyName("nexttran")]
  [DefaultValue("")]
  public string NextTran
  {
    get => nexttran;
    set => nexttran = TrimEnd(value);
  }

  /// <summary>
  /// Gets and sets "Printer terminal id".
  /// </summary>
  [XmlElement("printerTerminalId"), JsonPropertyName("printerTerminalId")]
  public string PrinterTerminalId { get; set; }

  /// <summary>
  /// Gets and sets "User id".
  /// </summary>
  [XmlElement("userId"), JsonPropertyName("userId")]
  public string UserId
  {
    get => userId;
    set => userId = TrimEnd(value);
  }

  /// <summary>
  /// Gets and sets "Terminal id".
  /// </summary>
  [XmlElement("terminalId"), JsonPropertyName("terminalId")]
  public string TerminalId { get; set; }

  /// <summary>
  /// Gets and sets "Exit State".
  /// </summary>     
  [XmlElement("exitstate"), JsonPropertyName("exitstate")]
  [DefaultValue("")]
  public string Exitstate
  {
    get => exitstate;
    set => exitstate = TrimEnd(value);
  }

  /// <summary>
  /// Gets and sets "Transaction Code".
  /// </summary>  
  [XmlElement("trancode"), JsonPropertyName("trancode")]
  [DefaultValue("")]
  public string TranCode
  {
    get => trancode;
    set => trancode = TrimEnd(value);
  }

  /// <summary>
  /// Gets and sets "Program Function Keys".
  /// </summary>
  [XmlElement("pfkey"), JsonPropertyName("pfkey")]
  [DefaultValue("")]
  public string Pfkey
  {
    get => pfkey;
    set => pfkey = TrimEnd(value);
  }

  /// <summary>
  /// Gets and sets "System Error Message".
  /// </summary>
  [XmlElement("errmsg"), JsonPropertyName("errmsg")]
  public string Errmsg { get; set; }

  /// <summary>
  /// Gets and sets "Command Area".
  /// </summary>
  [XmlElement("command"), JsonPropertyName("command")]
  [DefaultValue("")]
  public string Command
  {
    get => command;
    set => command = TrimEnd(value).ToUpper();
  }

  /// <summary>
  /// Gets and sets "Client Password".
  /// </summary>
  [XmlElement("clientPassword"), JsonPropertyName("clientPassword")]
  [DefaultValue("")]
  public string ClientPassword
  {
    get => clientPassword;
    set => clientPassword = TrimEnd(value);
  }

  /// <summary>
  /// Gets and sets "Client user id".
  /// </summary>
  [XmlElement("clientUserId"), JsonPropertyName("clientUserId")]
  [DefaultValue("")]
  public string ClientUserId
  {
    get => clientUserId;
    set => clientUserId = TrimEnd(value);
  }

  /// <summary>
  /// Gets and sets "Transaction Retry Limit".
  /// </summary>
  [XmlIgnore, JsonIgnore]
  public int TransactionRetryLimit { get; set; }
    
  /// <summary>
  /// Gets and sets "Transaction Retry Count".
  /// </summary>
  [XmlIgnore, JsonIgnore]
  public int TransactionRetryCount { get; set; }
    
  /// <summary>
  /// Gets and sets "Database SQLCODE".
  /// </summary>
  [XmlIgnore, JsonIgnore]    
  public long DatabaseSqlcode { get; set; }
    
  /// <summary>
  /// Gets and sets "Database SQLSTATE".
  /// </summary>
  [XmlIgnore, JsonIgnore]    
  public string DatabaseSqlstate
  {
    get => databaseSqlstate;
    set => databaseSqlstate = TrimEnd(value);
  }
    
  /// <summary>
  /// Gets and sets "Database error message".
  /// </summary>
  [XmlIgnore, JsonIgnore]    
  public string DatabaseErrorMessage { get; set; }
    
  /// <summary>
  /// Gets and sets exit state id.
  /// </summary>
  [DefaultValue(0)]
  [XmlElement("exitStateId"), JsonPropertyName("exitStateId")]
  public int ExitStateId { get; set; }

  /// <summary>
  /// Gets and sets exit state termination action.
  /// </summary>
  [DefaultValue(TerminationAction.Normal)]
  [XmlElement("terminationAction"), JsonPropertyName("terminationAction")]
  public TerminationAction TerminationAction { get; set; }

  /// <summary>
  /// Gets and sets exit state message type.
  /// </summary>
  [DefaultValue(MessageType.None)]
  [XmlElement("messageType"), JsonPropertyName("messageType")]
  public MessageType MessageType { get; set; }

  /// <summary>
  /// The last function error number, if an error occurred.
  /// </summary>
  /// <remarks>
  /// <para>Some error codes:</para>
  /// <list type="table">
  /// <listheader>
  ///   <term>Error Number</term>
  ///  <description>Description</description>
  /// </listheader>
  /// <item>
  ///   <term>301</term>
  ///   <description>Invalid file name given.</description>
  /// </item>
  /// </list>
  /// <item>
  ///   <term>302</term>
  ///   <description>File already exists. Cannot create.</description>
  /// </item>
  /// <item>
  ///   <term>303</term>
  ///   <description>File does not exist. Cannot open.</description>
  /// </item>
  /// <item>
  ///   <term>305</term>
  ///   <description>File does not exist. Cannot open.</description>
  /// </item>
  /// <item>
  ///   <term>306</term>
  ///   <description>Invalid input value.</description>
  /// </item>
  /// <item>
  ///   <term>307</term>
  ///   <description>
  ///     File specified is open for read operations only.
  ///   </description>
  /// </item>
  /// <item>
  ///   <term>308</term>
  ///   <description>
  ///     File specified is open for write operations only.
  ///   </description>
  /// </item>
  /// <item>
  ///   <term>309</term>
  ///   <description>File write operation failed. Check file.</description>
  /// </item>
  /// <item>
  ///   <term>310</term>
  ///   <description>An internal file operation failed.</description>
  /// </item>
  /// <item>
  ///   <term>311</term>
  ///   <description>File does not exist.</description>
  /// </item>
  /// <item>
  ///   <term>312</term>
  ///   <description>File is opened.</description>
  /// </item>
  /// <item>
  ///   <term>312</term>
  ///   <description>Attempted to read past the end of file.</description>
  /// </item>
  /// <item>
  ///   <term>314</term>
  ///   <description>
  ///     File read operation failed. Check file attributes.
  ///   </description>
  /// </item>
  /// <item>
  ///   <term>315</term>
  ///   <description>
  ///     File seek operation failed. Check file attributes.
  ///   </description>
  /// </item>
  /// <para>
  /// See also COOL:GEN <a href="https://docops.ca.com/ca-gen/8-6/en/reference/messages/toolset-function-error-messages/file-function-error-messages-300-499"> documentation</a>.
  /// </para>
  /// </remarks>
  [XmlIgnore, JsonIgnore]
  public int ErrorNumber { get; set; }

  /// <summary>
  /// A brief text message describing the last function error,
  /// if an error occurred.
  /// </summary>
  [XmlIgnore, JsonIgnore]
  public string ErrorDescription { get; set; }

  /// <summary>
  /// Sets an exit state.
  /// </summary>
  /// <param name="exitState">An exit state name.</param>
  /// <param name="resources">
  /// <see cref="IResources"/> to resolve exit state.
  /// </param>
  /// <param name="resourceID">A resource ID hint.</param>
  public void SetExitState(
    string exitState, 
    IResources resources,
    int resourceID)
  {
    if (!Equal(exitstate, exitState))
    {
      SetExitState(resources.GetCheckedExitState(exitState, resourceID));
    }
  }

  /// <summary>
  /// Sets an exit state.
  /// </summary>
  /// <param name="exitStateID">An exit state name ID.</param>
  /// <param name="resources">
  /// <see cref="IResources"/> to resolve exit state.
  /// </param>
  /// <param name="resourceID">A resource ID hint.</param>
  public void SetExitState(
    int exitStateID, 
    IResources resources,
    int resourceID)
  {
    if (ExitStateId != exitStateID)
    {
      SetExitState(resources.GetCheckedExitState(exitStateID, resourceID));
    }
  }

  /// <summary>
  /// Sets an exit state.
  /// </summary>
  /// <param name="exitState">An exit state instance.</param>
  public void SetExitState(ExitState exitState)
  {
    if (exitState != null)
    {
      Exitstate = exitState.Name;
      ExitStateId = exitState.Id;
      MessageType = exitState.Type;
      TerminationAction = exitState.Action;
    }
    else
    {
      Exitstate = null;
      ExitStateId = 0;
      MessageType = MessageType.None;
      TerminationAction = TerminationAction.Normal;
    }

    Errmsg = null;
  }

  /// <summary>
  /// Initializes an error message.
  /// </summary>
  /// <param name="resources">
  /// <see cref="IResources"/> to resolve exit state.
  /// </param>
  /// <param name="resourceID">A resource ID hint.</param>
  public void InitErrMsg(IResources resources, int resourceID)
  {
    if (Errmsg == null)
    {
      Errmsg = IsEmpty(Exitstate) ? "" :
        resources.GetExitStateMessage(
          Exitstate,
          TrimEnd(CurrentDialect),
          resourceID) ?? "";
    }
  }

  /// <summary>
  /// Compares two exit states.
  /// </summary>
  /// <param name="first">First exit state value.</param>
  /// <param name="second">Second exit state value.</param>
  /// <param name="resources">
  /// <see cref="IResources"/> to resolve exit state.
  /// </param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <returns>true if exit states are equal, and false otherwise.</returns>
  public static bool AreExitStatesEqual(
    string first,
    string second,
    IResources resources,
    int resourceID)
  {
    if (first == second)
    {
      return true;
    }

    if ((first == null) || (second == null))
    {
      return false;
    }

    var firstExitState = resources.GetExitState(first, resourceID);
    var secondExitState = resources.GetExitState(second, resourceID);

    if ((firstExitState == null) || (secondExitState == null))
    {
      return false;
    }

    return (firstExitState == secondExitState) ||
      ((firstExitState.Id != 0) &&
        (firstExitState.Id == secondExitState.Id));
  }

  /// <summary>
  /// Verifies whether an exit state matches to one of space separated 
  /// exit states.
  /// </summary>
  /// <param name="exitStates">A space separated exit states.</param>
  /// <param name="exitState">An exit state value.</param>
  /// <param name="resources">
  /// <see cref="IResources"/> to resolve exit state.
  /// </param>
  /// <param name="resourceID">A resource ID hint.</param>
  /// <returns>true for match, and false otherwise.</returns>
  public static bool IsMatchingExitState(
    string exitStates, 
    string exitState,
    IResources resources,
    int resourceID)
  {
    if (string.IsNullOrEmpty(exitStates) || string.IsNullOrEmpty(exitState))
    {
      return false;
    }

    int p = exitStates.IndexOf(' ');

    if (p == -1)
    {
      return AreExitStatesEqual(exitState, exitStates, resources, resourceID);
    }

    int s = 0;

    do
    {
      if (AreExitStatesEqual(
        exitState,
        exitStates[s..p],
        resources,
        resourceID))
      {
        return true;
      }

      s = p + 1;
      p = exitStates.IndexOf(' ', s);
    }
    while (p >= 0);

    return AreExitStatesEqual(
      exitState, 
      exitStates[s..], 
      resources,
      resourceID);
  }

  /// <summary>
  /// Display partial content of global structure for debu purposes.
  /// </summary>
  /// <returns>A debug string info.</returns>
  public override string ToString() =>
    $"exit-state: {exitstate}, command: {command}, nexttran: {nexttran}";

  private string nextlocation;
  private string currentDialect;
  private string localSystemId;
  private string panelId;
  private string scrollInd;
  private string scrollAmt;
  private string scrollLoc;
  private string nexttran;
  private string userId;
  private string exitstate;
  private string trancode;
  private string pfkey;
  private string command;
  private string clientPassword;
  private string clientUserId;
  private string databaseSqlstate;
}
