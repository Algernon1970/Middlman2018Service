Module SharedModule

    Public Const SessionSet As String = "Add Printer (Session)"
    Public Const SessionUnSet As String = "Remove Printer (Session)"
    Public Const MappedPrinter As String = "Mapped Always"
    Public Const AllocateSet As String = "Add Printer"
    Public Const AllocateUnSet As String = "Remove Printer"
    Public Const SetColour As String = "#FF158CE6"
    Public Const UnSetColour As String = "#FF7AB4E0"
    Public Const EncryptionPW As String = "A$hbySchool1"

#Region "Web Comms Constants"
    Public Const WEB_URL As String = "http://127.0.0.1:1701/?command="
    Public Const WEB_SetName As String = "setuser&params="
    Public Const WEB_CheckOnline As String = "recheckonline"
    Public Const WEB_GetComputerID As String = "getcomputerid"
    Public Const WEB_GetPrinterList As String = "getprinterlist"
    Public Const WEB_GetPrinterConnection As String = "getprinterconnection&params="
    Public Const WEB_IsPrivileged As String = "isprivuser"
    Public Const WEB_CopyPrivFile As String = "getpriv"
    Public Const WEB_SetPrivs As String = "setpriv"
    Public Const WEB_GetGroups As String = "getusergroups"
    Public Const WEB_UnHookGatekeeper As String = "hookgatekeeper&params=off"
    Public Const WEB_HookGatekeeper As String = "hookgatekeeper&params=on"
    Public Const WEB_GPUpdate As String = "GPUpdate"
    Public Const WEB_CopyMOTD As String = "GetMOTD"
    Public Const WEB_RecordDrives As String = "RecordDrives&params="
    Public Const WEB_LocalAdmin As String = "LocalAdmin&params="
    Public Const WEB_MusicRedirect As String = "MusicRedirect"
    Public Const WEB_Logout As String = "ForceLogout"
    Public Const WEB_GetVersion As String = "GetVersion"
    Public Const WEB_GetCurrentVersion As String = "GetCurrentVersion"
    Public Const WEB_GetAllPrinters As String = "GetAllPrinters"
    Public Const Web_GetPID As String = "GetPrinterID&params="
    Public Const WEB_GetPrinterNameByConnection As String = "GetPrinterNameByConnection&params="
    Public Const WEB_GetPrinterNameByID As String = "GetPrinterName&params="
    Public Const WEB_AddPrinter As String = "AddPrinter&params="
    Public Const WEB_DeletePrinter As String = "DeletePrinter&params="
    Public Const WEB_UpdateDefaultPrinter As String = "UpdateDefaultPrinter&params="
    Public Const WEB_RemoveDefaultByComputer As String = "RemoveDefaultPrinter&params="
    Public Const WEB_IsLaptop As String = "IsLaptop"
    Public Const WEB_LoadAlarms As String = "LoadAlarms"
#End Region

    Public eLog As EventLog = Nothing
    Public Function GetLogger() As EventLog
        If eLog Is Nothing Then
            eLog = New EventLog
        End If
        Try
            If Not EventLog.SourceExists("GK2018") Then
                EventLog.CreateEventSource("GK2018", "GK2018")
            End If
        Catch ex As Exception
            Return Nothing
        End Try
        eLog.Source = "GK2018"
        Return eLog
    End Function

    Public Sub Log(ByVal message As String, ByVal type As EventLogEntryType)
        If eLog Is Nothing Then
            eLog = GetLogger()
            If eLog Is Nothing Then
                Return
            Else
            End If
        End If
        eLog.WriteEntry(message, type)
    End Sub
End Module
