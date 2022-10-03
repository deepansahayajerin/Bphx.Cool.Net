namespace Bphx.Cool.Cobol;

using System;

using Bphx.Cool.Xml;

/// <summary>
/// An API to access Cobol GLOBDATA structure.
/// </summary>
public sealed class Globdata
{
  /// <summary>
  /// Populates data from <see cref="Global"/> instance.
  /// </summary>
  /// <param name="global">A <see cref="Global"/> instance.</param>
  /// <param name="action">An action name.</param>
  /// <param name="data">A data span.</param>
  /// <param name="converter">
  /// A <see cref="CobolConverter"/> instance.
  /// </param>
  public static void SetData(
    Global global,
    string action,
    Span<byte> data,
    CobolConverter converter)
  {
    converter.WriteString(
      data[PsmgrIefCommandOffset..], 
      global.Command, 
      PsmgrIefCommandSize, 
      0);
    
    converter.WriteString(
      data[PsmgrIefTrancodeOffset..], 
      global.TranCode, 
      PsmgrIefTrancodeSize, 
      0);
    
    converter.WriteLong(
      data[PsmgrExitStateOffset..], 
      global.ExitStateId, 
      PsmgrExitInfomsgSize, 
      0);

    converter.WriteString(
      data[PsmgrExitInfomsgOffset..], 
      global.Errmsg, 
      PsmgrExitInfomsgSize, 
      0);

    converter.WriteString(
      data[PsmgrUserIdOffset..], 
      global.UserId, 
      PsmgrUserIdSize, 
      0);

    converter.WriteString(
      data[PsmgrTerminalIdOffset..], 
      global.TerminalId, 
      PsmgrTerminalIdSize, 
      0);

    converter.WriteString(
      data[PsmgrPrinterIdOffset..], 
      global.PrinterTerminalId, 
      PsmgrPrinterIdSize, 
      0);

    converter.WriteString(
      data[PsmgrFuncErrmsgOffset..], 
      "", 
      PsmgrFuncErrmsgSize, 
      0);

    converter.WriteString(
      data[PsmgrFuncNameOffset..], 
      "", 
      PsmgrFuncNameSize, 
      0);
    
    var messageType = global.MessageType;
    
    converter.WriteString(
      data[PsmgrExitMsgtypeOffset..], 
      messageType == MessageType.Info ? "I" :
        messageType == MessageType.Warning ? "W" :
        messageType == MessageType.Error ? "E" :
        "N", 
      PsmgrExitMsgtypeSize, 
      0);

    converter.WriteString(
      data[ErrorEncounteredSwOffset..],
      "",
      ErrorEncounteredSwSize,
      0);

    converter.WriteString(
      data[ViewOverflowSwOffset..],
      "",
      ViewOverflowSwSize + 1,
      0);

    converter.WriteString(
      data[StatusFlagOffset..], 
      "", 
      StatusFlagSize, 
      0);
  
    converter.WriteString(
      data[CurAbNameOffset..], 
      action, 
      CurAbNameSize, 
      0);

    converter.WriteString(
      data[PsmgrActiveDialectOffset..], 
      global.CurrentDialect, 
      PsmgrActiveDialectSize, 
      0);
    
    converter.WriteString(
      data[ClientUseridOffset..], 
      global.ClientUserId, 
      ClientUseridSize, 
      0);
  }

  /// <summary>
  /// Gets status field out of the GLOBDATA structure.
  /// </summary>
  /// <param name="global">A <see cref="Global"/> instance.</param>
  /// <param name="converter">
  /// A <see cref="CobolConverter"/> instance.
  /// </param>
  /// <returns>A status value.</returns>
  public static string GetStatus(Span<byte> data, CobolConverter converter) =>
    converter.ReadString(data[StatusFlagOffset..], StatusFlagSize, 0);

  /**
    * Gets an exit state id out of the GLOBDATA structure.
    * @param data a buffer containing GLOBDATA.
    * @param offset an offset of the GLOBDATA within buffer.
    * @param converter a {@link CobolConverter} instance.
    * @return an exit state id value.
    */
  public static int GetExitStateId(
    Span<byte> data,
    CobolConverter converter) => 
    (int)converter.ReadLong(
      data[PsmgrExitStateOffset..],
      PsmgrExitInfomsgSize,
      0);

  /**
    * GLOBDATA minimum size.
    */
  public const int Size = 0xd34;
  
  // Offsets and sizes.
  
  public const int PsmgrIefCommandOffset = 0x0;
  public const int PsmgrIefCommandSize = 0x50;
  public const int PsmgrIefTrancodeOffset = 0x50;
  public const int PsmgrIefTrancodeSize = 0x8;
  public const int PsmgrExitStateOffset = 0x58;
  public const int PsmgrExitStateSize = 0x6;
  public const int PsmgrExitInfomsgOffset = 0x5e;
  public const int PsmgrExitInfomsgSize = 0x50;
  public const int PsmgrUserIdOffset = 0xae;
  public const int PsmgrUserIdSize = 0x8;
  public const int PsmgrTerminalIdOffset = 0xb6;
  public const int PsmgrTerminalIdSize = 0x8;
  public const int PsmgrPrinterIdOffset = 0xbe;
  public const int PsmgrPrinterIdSize = 0x8;
  public const int PsmgrFunctionDataOffset = 0xd2;
  public const int PsmgrFuncErrmsgOffset = PsmgrFunctionDataOffset + 0x0;
  public const int PsmgrFuncErrmsgSize = 0x4;
  public const int PsmgrFuncNameOffset = PsmgrFunctionDataOffset + 0x4;
  public const int PsmgrFuncNameSize = 0x8;
  public const int PsmgrExitMsgtypeOffset = 0x16a;
  public const int PsmgrExitMsgtypeSize = 0x1;
  public const int PsmgrActiveDialectOffset = 0xc3d;
  public const int PsmgrActiveDialectSize = 0x8;
  public const int ClientUseridOffset = 0xc8b;
  public const int ClientUseridSize = 0x40;
  public const int PsmgrErrorDataOffset = 0x986;
  public const int ErrorEncounteredSwOffset = PsmgrErrorDataOffset + 0x20;
  public const int ErrorEncounteredSwSize = 0x1;
  public const int ViewOverflowSwOffset = ErrorEncounteredSwOffset + 0x1;
  public const int ViewOverflowSwSize = 0x1;
  public const int PsmgrDasgDataOffset = 0x9a8;
  public const int StatusFlagOffset = PsmgrDasgDataOffset + 0x14;
  public const int StatusFlagSize = 0x2;
  public const int PsmgrDebugDataOffset = 0xabf;
  public const int CurAbNameOffset = PsmgrDebugDataOffset + 0x18;
  public const int CurAbNameSize = 0x20;

    
  //  01  GLOBDATA.
  //  03  PSMGR-IEF-COMMAND.
  //    05  PSMGR-IEF-COMMAND-1    PIC X(8).
  //    05  PSMGR-IEF-COMMAND-2    PIC X(72).
  //*                             IEF-COMMAND
  //  03  PSMGR-IEF-TRANCODE       PIC X(8).
  //*                             IEF-TRANCODE.
  //  03  PSMGR-EXIT-STATE         PIC S9(11) COMP-3.
  //*                             EXIT STATE
  //  03  PSMGR-EXIT-INFOMSG       PIC X(80).
  //*                             EXIT INFOMSG
  //  03  PSMGR-USER-ID            PIC X(8).
  //*                             USER ID
  //  03  PSMGR-TERMINAL-ID        PIC X(8).
  //*                             TERMINAL ID
  //  03  PSMGR-PRINTER-ID         PIC X(8).
  //*                             PRINTER ID
  //  03  PSMGR-CURRENT-DATE       PIC S9(9) COMP.
  //*                             CURRENT DATE (YYYYMMDD)
  //  03  PSMGR-CURRENT-TIME       PIC S9(9) COMP.
  //*                             CURRENT TIME (HHMMSSTH)
  //  03  PSMGR-RUNTIME-TYPE       PIC X(4).
  //*                             CICS, IMS, BATC..ETC
  //  03  PSMGR-FUNCTION-DATA.
  //      05  PSMGR-FUNC-ERRMSG    PIC X(4).
  //*                             FUNCTION ERROR MSG NUMBER
  //      05  PSMGR-FUNC-NAME      PIC X(8).
  //*                             FUNCTION NAME
  //      05  PSMGR-FUNC-IN-DATE   PIC X(8).
  //*                             INPUT DATE FOR DURATIONS
  //      05  PSMGR-FUNC-IN-DDURA.
  //*                             INPUT LABELED DURATION
  //          07  DDURA-YEAR.
  //              09  DDURA-Y-MISS PIC X.
  //              09  DDURA-YYYY   PIC S9(9) COMP.
  //          07  DDURA-MONTH.
  //              09  DDURA-M-MISS PIC X.
  //              09  DDURA-MM     PIC S9(9) COMP.
  //          07  DDURA-DAY.
  //              09  DDURA-D-MISS PIC X.
  //              09  DDURA-DD     PIC S9(9) COMP.
  //      05  PSMGR-FUNC-OUT-DATE  PIC X(8).
  //*                             OUTPUT DATE FOR DURATIONS
  //      05  PSMGR-FUNC-IN-TIME   PIC X(6).
  //*                             INPUT TIME FOR DURATIONS
  //      05  PSMGR-FUNC-IN-TDURA.
  //*                             INPUT LABELED DURATION
  //          07  TDURA-HOUR.
  //              09  TDURA-H-MISS PIC X.
  //              09  TDURA-HH     PIC S9(9) COMP.
  //          07  TDURA-MINUTE.
  //              09  TDURA-M-MISS PIC X.
  //              09  TDURA-MM     PIC S9(9) COMP.
  //          07  TDURA-SECOND.
  //              09  TDURA-S-MISS PIC X.
  //              09  TDURA-SS     PIC S9(9) COMP.
  //      05  PSMGR-FUNC-OUT-TIME  PIC X(6).
  //*                             OUTPUT TIME FOR DURATIONS
  //  03  FILLER                   PIC X(2).
  //*                             GLOBDATA RELEASE
  //  03  PSMGR-IEF-NEXTTRAN       PIC X(80).
  //*                             IEF-NEXTTRAN
  //  03  PSMGR-EXIT-MSGTYPE       PIC X(1).
  //*                             EXIT INFOMSG TYPE
  //  03  FILLER                   PIC X(11).
  //*                             GROWTH ROOM
  //  03  PSMGR-IEF-DEBUG-FLAGS.
  //      05  PSMGR-IEF-DEBUG      PIC X.
  //          88  PSMGR-DEBUG-ON         VALUE 'Y'.
  //*                             DEBUG FLAG
  //      05  FILLER               PIC X(15).
  //*                             FILLER FOR DEBUG
  //  03  PSMGR-ENVIRONMENT-DATA.
  //*
  //      05 PSMGR-PCB-CNT         PIC S9(9) COMP SYNC.
  //      05 PSMGR-PCB-ENTRY             OCCURS 255.
  //         07 PSMGR-PCB-ADR      PIC S9(9) COMP SYNC.
  //*           07 PSMGR-PCB-PTR            REDEFINES
  //*                             PSMGR-PCB-ADR
  //*                             POINTER.
  //*
  //  03  PSMGR-EAB-DATA.
  //*
  //      05 PSMGR-EABPCB-CNT      PIC S9(9) COMP SYNC.
  //      05 PSMGR-EABPCB-ENTRY          OCCURS 255.
  //         07 PSMGR-EABPCB-ADR   PIC S9(9) COMP SYNC.
  //*           07 PSMGR-EABPCB-PTR         REDEFINES
  //*                             PSMGR-EABPCB-ADR
  //*                             POINTER.
  //*
  //  03  PSMGR-ERROR-DATA.
  //*
  //      05 ERROR-ACTION-NAME     PIC X(32).
  //      05 ERROR-ENCOUNTERED-SW  PIC X.
  //      05 VIEW-OVERFLOW-SW      PIC X.
  //*
  //  03  PSMGR-DASG-DATA.
  //*
  //      05 ACTION-ID-X.
  //         07 ACTION-ID          PIC 9(10).
  //      05 ATTRIBUTE-ID-X.
  //         07 ATTRIBUTE-ID       PIC 9(10).
  //      05 STATUS-FLAG           PIC XX.
  //         88 WHEN-SUCCESSFUL-SF          VALUE SPACES.
  //         88 FATAL-ERROR-SF              VALUE 'FE'.
  //         88 DB-ERROR-SF                 VALUE 'DB'.
  //         88 ABORT-SHOW-DBMS-MSG-SF      VALUE 'AB'.
  //         88 ABORT-SHOW-USER-MSG-SF      VALUE 'AM'.
  //         88 RETRY-TRAN-REQUESTED-SF     VALUE 'RT'.
  //         88 PSTEP-USE-FAILURE           VALUE 'PU'.
  //         88 ASYNC-WHEN-AVAILABLE-SF     VALUE SPACES.
  //         88 ASYNC-WHEN-ACCEPTED-SF      VALUE SPACES.
  //         88 ASYNC-WHEN-NOT-ACCEPTED-SF  VALUE 'NA'.
  //         88 ASYNC-WHEN-INVALID-ID-SF    VALUE 'II'.
  //         88 ASYNC-WHEN-PENDING-SF       VALUE 'PE'.
  //         88 ASYNC-WHEN-SERVER-ERROR-SF  VALUE 'SS'.
  //         88 ASYNC-WHEN-COMM-ERROR-SF    VALUE 'CE'.
  //      05 LAST-STATUS           PIC XX.
  //         88 DB-ERROR-FL-LS              VALUE 'DB'.
  //         88 DB-DEADLOCK-TIMOUT-FL-LS    VALUE 'DT'.
  //         88 DUPLICATE-FOUND-FL-LS       VALUE 'DF'.
  //         88 INVALID-DATAA-FL-LS         VALUE 'IA'.
  //         88 INVALID-DATAB-TYPE-FL-LS    VALUE 'BT'.
  //         88 INVALID-DATAB-PERM-FL-LS    VALUE 'BP'.
  //         88 FATAL-ERROR-FL-LS           VALUE 'FE'.
  //         88 NOT-FOUND-FL-LS             VALUE 'NF'.
  //         88 NOT-UNIQUE-FL-LS            VALUE 'NU'.
  //         88 IEF-FUNCTION-ERROR-FL-LS    VALUE 'IE'.
  //         88 IEF-DURATION-ERROR-FL-LS    VALUE 'DE'.
  //         88 USED-PSTEP-NOT-FOUND        VALUE 'PO'.
  //         88 USED-PSTEP-ROUTING-ERR      VALUE 'PX'.
  //         88 USED-PSTEP-SND-FMT-ERR      VALUE 'PF'.
  //         88 USED-PSTEP-ENCRYPT-ERR      VALUE 'PN'.
  //         88 USED-PSTEP-SND-BFR-ERR      VALUE 'PS'.
  //         88 USED-PSTEP-RCV-BFR-ERR      VALUE 'PR'.
  //         88 USED-PSTEP-RCV-FMT-ERR      VALUE 'PU'.
  //         88 USED-PSTEP-TIRSECR-ERR      VALUE 'PE'.
  //         88 USED-PSTEP-TOKEN-ERR        VALUE 'PT'.
  //         88 USED-PSTEP-SEND-MAX-SIZE    VALUE 'PM'.
  //         88 USED-PSTEP-SECG-ERR         VALUE 'PB'.
  //         88 USED-PSTEP-ALLOC-ERR        VALUE 'PA'.
  //         88 USED-PSTEP-CONNECT-ERR      VALUE 'PC'.
  //         88 USED-PSTEP-XERR             VALUE 'PD'.
  //         88 USED-PSTEP-RCV-UA-ERR       VALUE 'PH'.
  //         88 USED-PSTEP-RCV-ES-ERR       VALUE 'PI'.
  //         88 USED-PSTEP-XFAL             VALUE 'PJ'.
  //         88 USED-PSTEP-SETOA-ERR        VALUE 'PK'.
  //         88 USED-PSTEP-RCV-VIEW-ERR     VALUE 'PL'.
  //         88 USED-PSTEP-DECRYPT-ERR      VALUE 'PP'.
  //      05 SAVE-SQLCA            PIC X(255).
  //*
  //  03  PSMGR-DEBUG-DATA.
  //*
  //      05 PSMGR-TRACE-ADR       PIC S9(9) COMP SYNC.
  //*        05 PSMGR-TRACE-PTR             REDEFINES
  //*                             PSMGR-TRACE-ADR
  //*                             POINTER.
  //      05 LAST-STATEMENT-X.
  //         07 LAST-STATEMENT-NUM PIC 9(10).
  //      05 CUR-AB-ID             PIC X(10).
  //      05 CUR-AB-NAME           PIC X(32).
  //*
  //  03  PSMGR-TIRDATE-SAVEAREA  PIC X(12).
  //  03  PSMGR-TIRDATE-CMCB.
  //    05  PSMGR-TIRDATE-DATE  PIC S9(9) COMP.
  //*            DATE INPUT / SYSTEM DATE RETURNED
  //    05  PSMGR-TIRDATE-TIME  PIC S9(9) COMP.
  //*            TIME INPUT / SYSTEM TIME RETURNED
  //    05  FILLER  PIC S9(18) COMP.
  //*            TIRDATE INTERNAL USE
  //    05  PSMGR-TIRDATE-INC  PIC S9(9) COMP.
  //*            INCREMENT OF DAYS TO BE ADDED TO DATE
  //    05  PSMGR-TIRDATE-RC  PIC S9(4) COMP.
  //      88  PSMGR-TIRDATE-OK       VALUE +0.
  //      88  PSMGR-TIRDATE-WARNING  VALUE +4.
  //      88  PSMGR-TIRDATE-ERROR    VALUE +8.
  //    05  PSMGR-TIRDATE-REQ  PIC 9(1).
  //*            0=VALIDATE INPUT DATE AND/OR TIME
  //*            1=RETURN SYSTEM DATE AND TIME
  //*            2=ADD INCREMENT OF DAYS TO INPUT DATE
  //*            3=ADD INCREMENT OF DAYS TO SYSTEM DATE
  //*            4=CONVERT  CYYMMDD TO TIRDATE_DATEF FORMAT
  //*            5=CONVERT YYYYMMDD TO TIRDATE_DATEF FORMAT
  //*            6=VALIDATE INPUT TIMESTAMP
  //*            7=RETURN SYSTEM TIMESTAMP
  //    05  PSMGR-TIRDATE-DATEF  PIC 9(1).
  //*            DATE FORMAT 0=YYYYMMDD, 1=CYYMMDD, 2=YYMMDD
  //    05  PSMGR-TIRDATE-TIMEF  PIC 9(1).
  //*            TIME FORMAT 0=HHMMSSTH, 1=HHMMSST, 2=HHMMSS
  //    05  PSMGR-TIRDATE-RETMSG  PIC X(60).
  //*            ERROR MESSAGE IF RC > 0
  //    05  PSMGR-TIRDATE-TSTAMP  PIC X(20).
  //*            TIMESTAMP FORMAT = YYYYMMDDHHMISSTHNNNN
  //    05  FILLER  REDEFINES  PSMGR-TIRDATE-TSTAMP.
  //      07  PSMGR-TIRDATE-DATE-Z  PIC 9(8).
  //      07  PSMGR-TIRDATE-TIME-Z  PIC 9(8).
  //      07  FILLER  PIC X(4).
  //    05  PSMGR-TIRDATE-SKIP-VAL  PIC X(1).
  //*            Y=DON'T VALIDATE INPUT DATE AND/OR TIME YET;
  //*              THIS TIRDATE CALL IS FOR AN INTERMEDIATE
  //*              RESULT, AND ANOTHER CALL FOLLOWS
  //    05  FILLER  PIC X(95).
  //*
  //  03  PSMGR-ROLLBACK-RQSTED    PIC X.
  //      88 ROLLBACK-RQSTED             VALUE 'R'.
  //      88 ABEND-RQSTED                VALUE 'A'.
  //      88 TERMINATE-RQSTED            VALUE 'T'.
  //*
  //  03  TIRTRCE-SAVE-AREA.
  //      05  TOP-INDX             PIC S9(9) COMP.
  //      05  BOTTOM-INDX          PIC S9(9) COMP.
  //      05  END-INDX             PIC S9(9) COMP.
  //      05  LAST-STMT            PIC 9(9) COMP.
  //      05  TOP-OF-CALL          PIC S9(9) COMP.
  //      05  TRACE-BREAK-POINT    PIC S9(9) COMP.
  //      05  TRACE-BREAK-POINT-STATUS  PIC X(3).
  //      05  LAST-AB-NAME         PIC X(32).
  //      05  COLOR                PIC X(15).
  //      05  COLORT               PIC X(15).
  //      05  HILITE               PIC X(15).
  //      05  TRACE-ON-OFF         PIC X(3).
  //*
  //  03  CASCADE-DELETE-FLAGS.
  //      05  V1PRESENT            PIC X.
  //      05  V2PRESENT            PIC X.
  //      05  CASCADE1             PIC X.
  //      05  CASCADE2             PIC X.
  //      05  PROCESSQ-FLAG        PIC X.
  //*
  //  03  PSMGR-ACTIVE-DIALECT.
  //      05  DIALECT-NAME         PIC X(8).
  //      05  MESSAGE-TABLE-NAME   PIC X(8).
  //      05  TRANSLATE-TABLE-NAME PIC X(8).
  //  03  PSMGR-FUNCTION-DATA-EXT.
  //      05  PSMGR-FUNC-IN-TIMESTAMP PIC X(20).
  //*                             INPUT TIMESTAMP FOR DURATIONS
  //      05  PSMGR-FUNC-IN-TSDURA.
  //*                             INPUT LABELED DURATION
  //          07  TSDURA-MICROSECOND.
  //              09  TSDURA-M-MISS PIC X.
  //              09  TSDURA-MS    PIC S9(9) COMP.
  //      05  PSMGR-FUNC-OUT-TIMESTAMP  PIC X(20).
  //*                             OUTPUT TIMESTAMP FOR DURATIONS
  //  03  FILLER                   PIC X(8).
  //*                             RESERVED
  //  03  PSMGR-CICS-FAIL-SW            PIC X(1).
  //      88 INHIBIT-CICS-RECEIVE       VALUE 'I'.
  //*                             TIRFAIL PARAMETER FOR CICS
  //  03  CLIENT-USERID                 PIC X(64).
  //*                             CLIENT USER ID
  //  03  CLIENT-PASSWORD               PIC X(64).
  //*                             CLIENT PASSWORD
  //  03  LOAD-MODULE-NAME              PIC X(8).
  //*                             CURRENT LOAD MODULE NAME
  //  03  INSTRUMENT-CODE               PIC S9(9) COMP.
  //*                             INSTRUMENT CODE BIT-MASK
  //  03  TX-RETRY-LIMIT                PIC S9(9) COMP.
  //*                             TRANSACTION RETRY LIMIT
  //  03  TX-TIMEOUT                    PIC S9(9) COMP.
  //*                             PROC STEP TRANSACTION
  //*                             TIMEOUT
  //  03  PSMGR-EXTRA-ERRINFO.
  //*                             EXTRA ERROR INFO SECTION
  //      05  ERRINFO-BUF-SIZE          PIC S9(9) COMP.
  //*                             TOTAL BUFFER SIZE
  //      05  ERRINFO-MSG-SIZE          PIC S9(9) COMP.
  //*                             MSG SIZE IN BUFFER
  //      05  ERRINFO-BUF-ADDR          PIC X(8).
  //*                             BUFFER ADDRESS, 64 BITS
  //  03  TX-RETRY-COUNT                PIC S9(9) COMP.
  //*                             TRANSACTION RETRY COUNT
  //  03  TX-USER-RETRY-ALLOWED         PIC X.
  //*                             'Y' IF COOL:GEN 4.2 OR
  //*                             LATER
  //  03  FILLER                   PIC X(3).
  //  03  PSMGR-PSTEP-USE-PTRS.
  //      05  PSTEP-FAIL-MSG-PTR        POINTER.
  //      05  PSTEP-GURB-REST-PTR       POINTER.
  //      05  PSTEP-LIPS-PTR            POINTER.
  //      05  PSTEP-TBL-PTR             POINTER.
  //      05  PSTEP-DDF-PTR             POINTER.
  //      05  PSTEP-COMM-ID             PIC X(08).
  //      05  PSTEP-APPL-LIST-PTR       POINTER.
  //      05  PSTEP-CURR-PST-PTR        POINTER.
  //*
  //  03  PSMGR-PSTEP-USE-SYSFLDS.
  //*                             PSTEP USE TP FIELDS
  //      05  PSMGR-EIBERRCD            PIC X(04).
  //*                             CICS EIBERRCD
  //      05  PSMGR-EIBFN               PIC X(02).
  //*                             CICS EIBFN
  //      05  PSMGR-EIBRESP             PIC X(08).
  //*                             CICS EIBRESP
  //      05  PSMGR-EIBRESP2            PIC X(08).
  //*                             CICS EIBRESP2
  //  03  PSMGR-ASYNC-DATA.
  //      05  ASYNC-MODE                 PIC X.
  //          88  FLOW-TYPE-SYNC             VALUE SPACE.
  //          88  FLOW-TYPE-ASYNC            VALUE 'A'.
  //          88  FLOW-TYPE-NO-RESPONSE      VALUE 'N'.
  //      05  FILLER                     PIC X.
  //      05  ASYNC-FLOW-STATUS-TABLE-PTR    POINTER.
  //      05  ASYNC-SERVER-LOC-TABLE-PTR     POINTER.
  //  03  FILLER                   PIC X(189).
}
