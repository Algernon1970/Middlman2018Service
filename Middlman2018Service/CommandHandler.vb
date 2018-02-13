Imports System
Imports System.IO
Imports System.DirectoryServices
Imports System.Security.Principal
Imports AshbyTools
Imports AshbyTools.murrayju.ProcessExtensions
Imports Microsoft.Win32
Imports System.DirectoryServices.AccountManagement
Imports System.ServiceProcess

Public Class CommandHandler

    Dim eLog As EventLog = GetLogger()
    Private Sub WriteLog(ByRef message As String, ByRef type As EventLogEntryType)
        If eLog IsNot Nothing Then
            eLog.WriteEntry(message, type)
        End If
    End Sub

    Public Function GetVersion() As String
        Return "Version 2018.5"
    End Function

    Public Function Test(ByVal cmdline As String) As String
        Return "Test got " & cmdline
    End Function

    Public Function SetUser(ByVal params As String) As String
        SharedData.currentUser = params
        Dim pr As DataTable = SharedData.PersonTableAdapter.GetPersonBySam(SharedData.currentUser)
        If pr.Rows.Count = 0 Then
            pr = CreateUserRecord(SharedData.currentUser)
        End If
        SharedData.currentUserSid = pr(0).Field(Of String)("SID")
        Return "Name = " & params
    End Function

    Public Function GetUser() As String
        Return SharedData.currentUser
    End Function

    Public Function ForceLogout() As String
        Dim t As Single
        Dim objWMIService, objComputer As Object

        objWMIService = GetObject("Winmgmts:{impersonationLevel=impersonate,(Debug,Shutdown)}")
        For Each objComputer In objWMIService.InstancesOf("Win32_OperatingSystem")
            t = objComputer.Win32Shutdown(0, 0)
        Next
        Return "Daisy..... Daisy....."
    End Function

    Public Function Execute(ByVal cmdline As String) As String
        Dim cmd() As String
        Dim success As Boolean = False
        If cmdline.Contains(",") Then
            cmd = cmdline.Split(",", 2, StringSplitOptions.RemoveEmptyEntries)
            success = ProcessExtensions.StartProcessAsCurrentUser(cmd(0), cmd(1))
        Else
            success = ProcessExtensions.StartProcessAsCurrentUser(cmdline)
        End If
        WriteLog(String.Format("Execute: {0}", cmdline), EventLogEntryType.Warning)
        Return If(success, "Succeeded", "Failed")
    End Function

    Public Function CheckOnline() As String
        Return SharedData.online
    End Function

    Public Function RecheckOnline() As String
        Dim ret As String = AmIOnDomain()
        WriteLog(String.Format("RecheckOnline: {0}", ret), EventLogEntryType.Information)
        Return If(ret = "Offline", "Offline", "Online")
    End Function

    Public Function GetComputerID() As String
        Dim cid As Integer = 0
        Dim cr As DataTable = SharedData.ComputerTableAdapter.GetComputerByName(My.Computer.Name)
        If cr.Rows.Count = 0 Then
            If SharedData.ComputerTableAdapter.CreateComputer(My.Computer.Name, "0", "0", "", "", 0, 0, False, 1, 1, "0", "0", Now.ToShortDateString, 0) <> 0 Then
                cr = SharedData.ComputerTableAdapter.GetComputerByName(My.Computer.Name)
                If cr.Rows.Count = 0 Then Return "0"
            End If
        End If
        cid = cr.Rows(0).Field(Of Integer)("ComputerID")
        Return cid.ToString
    End Function

    Public Function GetPersonID() As String
        Dim pid As Integer = 0
        Dim pr As DataTable = SharedData.PersonTableAdapter.GetPersonBySam(SharedData.currentUser)
        If pr.Rows.Count = 0 Then
            pr = CreateUserRecord(SharedData.currentUser)
        End If
        pid = pr.Rows(0).Field(Of Integer)("PersonID")
        Return pid.ToString
    End Function

    Public Function GetUserGroups() As String
        Dim grpList As String = ""
        Dim usr As UserPrincipal = getUserPrincipalbyUsername(userCTX, SharedData.currentUser)
        Dim usrGrps As PrincipalSearchResult(Of Principal) = usr.GetGroups()
        For Each grp As Principal In usrGrps
            If grpList.Length > 0 Then
                grpList = String.Format("{0},{1}", grpList, grp.Name)
            Else
                grpList = grp.Name
            End If
        Next
        Return grpList
    End Function

    Private Function CreateUserRecord(ByVal sam As String) As DataTable
        Dim upx As UserPrincipalex = getUserPrincipalexbyUsername(userCTX, sam)
        SharedData.PersonTableAdapter.CreatePerson(upx.GivenName, upx.Surname, sam, upx.Sid.ToString)
        Return SharedData.PersonTableAdapter.GetPersonBySam(sam)
    End Function

    Public Function GetPrinterList() As String
        Dim pList As String = ","
        Dim cid As Integer = Integer.Parse(GetComputerID())
        Dim pidTable As DataTable = SharedData.PrinterLinkTableAdapter.GetPIDsByComputerID(cid)
        Dim pid As Integer
        If pidTable.Rows.Count > 0 Then
            For Each row As DataRow In pidTable.Rows
                pid = row.Field(Of Integer)("PrinterID")
                If row.Field(Of Boolean)("isDefaultPrinter") Then
                    pList = String.Format("{0},*{1}", pList, pid)
                Else
                    pList = String.Format("{0},{1}", pList, pid)
                End If

            Next
        End If
        If pList.Length = 1 Then
            pList = "0"
        Else
            pList = pList.Substring(1)
        End If
        Return pList
    End Function

    Public Function GetPrinterName(ByRef pid As String) As String
        Dim printTable As DataTable = SharedData.PrinterTableAdapter.GetPrinterByID(Integer.Parse(pid))
        If printTable.Rows.Count > 0 Then
            Return printTable.Rows(0).Field(Of String)("Name")
        Else
            Return "No Printer"
        End If
    End Function

    Public Function GetPrinterConnection(ByRef pid As String) As String
        Dim printTable As DataTable = SharedData.PrinterTableAdapter.GetPrinterByID(Integer.Parse(pid))
        If printTable.Rows.Count > 0 Then
            Return printTable.Rows(0).Field(Of String)("ConnectionString")
        Else
            Return "No Printer"
        End If
    End Function

    Public Function LockWorkstation() As String
        If Environment.Is64BitOperatingSystem Then
            ProcessExtensions.StartProcessAsCurrentUser("C:\Program Files (x86)\Ashby School\MiddlemanInstaller\userutilities2018.exe", "Utilities LOCK", "C:\Program Files (x86)\Ashby School\MiddlemanInstaller", True)
        Else
            ProcessExtensions.StartProcessAsCurrentUser("C:\Program Files\Ashby School\MiddlemanInstaller\userutilities2018.exe", "Utilities LOCK", "C:\Program Files\Ashby School\MiddlemanInstaller", True)
        End If
        Return "OK"
    End Function

    Public Function GPUpdate() As String
        If Environment.Is64BitOperatingSystem Then
            ProcessExtensions.StartProcessAsCurrentUser("C:\Program Files (x86)\Ashby School\MiddlemanInstaller\userutilities2018.exe", "Utilities GPUPDATE", "C:\Program Files (x86)\Ashby School\MiddlemanInstaller", False)
        Else
            ProcessExtensions.StartProcessAsCurrentUser("C:\Program Files\Ashby School\MiddlemanInstaller\userutilities2018.exe", "Utilities GPUPDATE", "C:\Program Files\Ashby School\MiddlemanInstaller", False)
        End If
        Return "OK"
    End Function

    Public Function MusicRedirect() As String
        Dim ret As String = ""
        Dim applocation As String = "Standard"
        Dim op As New RegInfo
        Dim music As Boolean = AD.CheckGroup("AS MusicTech Computers")
        If music Then
            If IO.Directory.Exists("D:\") Then
                applocation = "D:\" & SharedData.currentUser & "\appData"
                ret = "AppData D"
            Else
                applocation = "C:\Program Files\Ashby School\" & SharedData.currentUser & "\appData"
                ret = "AppData C"
            End If
            If Not IO.Directory.Exists(applocation) Then
                IO.Directory.CreateDirectory(applocation)
            End If

            WriteReg("HKLM\Software\Policies\Microsoft\Windows\Group Policy\{35378EAC-683F-11D2-A89A-00C04FBBCFA2}\NoBackGroundPolicy=1|dword")
            WriteReg("HKLM\Software\Policies\Microsoft\Windows\Group Policy\{35378EAC-683F-11D2-A89A-00C04FBBCFA2}\NoGPOListChanges=1|dword")
            op.name = "appdata"
            op.type = RegistryValueKind.String
            op.value = applocation
            op.hive = "hkcu"
            op.path = "Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders"
            RegEdit.WriteReg(op)
            op.path = "Software\Microsoft\Windows\CurrentVersion\Explorer\Volatile Environment"
            RegEdit.WriteReg(op)
            op.path = "Software\Microsoft\Windows\CurrentVersion\Explorer\Environment"
            RegEdit.WriteReg(op)
        Else
            WriteReg("HKLM\Software\Policies\Microsoft\Windows\Group Policy\{35378EAC-683F-11D2-A89A-00C04FBBCFA2}\NoBackGroundPolicy=0|dword")
            WriteReg("HKLM\Software\Policies\Microsoft\Windows\Group Policy\{35378EAC-683F-11D2-A89A-00C04FBBCFA2}\NoGPOListChanges=0|dword")
        End If
        WriteLog(If(music, applocation, "Ordinary Redirect"), EventLogEntryType.Information)
        Return "OK"
    End Function

    Public Function HookGatekeeper(ByRef opt As String) As String
        Dim ret As String
        If opt.ToLower.Equals("on") Then
            ret = WriteReg("hklm\software\microsoft\windows nt\currentversion\winlogon\shell=C:\Program Files (x86)\Ashby School\MiddlemanInstaller\gatekeeper2018.exe|String")
        Else
            ret = WriteReg("hklm\software\microsoft\windows nt\currentversion\winlogon\shell=explorer.exe|String")
        End If
        Return ret
    End Function

    ''' <summary>
    ''' Is Z Drive mapped and ready?
    ''' </summary>
    ''' <returns>Yes/No</returns>
    Public Function Gotz() As String
        If SharedData.mappedDrives.ToUpper.Contains("Z") Then
            Return "GOTZ"
        Else
            Return "NOTZ"
        End If
    End Function

    Public Function RecordDrives(ByRef driveList As String)
        SharedData.mappedDrives = driveList
        Return "OK"
    End Function

    Public Function ReadReg(ByRef path As String) As String
        Try
            Dim reg As RegInfo = ParseRegPath(path)
            reg = RegEdit.ReadReg(reg)
            Return reg.value.ToString
        Catch ex As RegistryException
            Return ex.errorSource & " - " & ex.Message
        End Try
    End Function

    Public Function WriteReg(ByRef path As String) As String
        Try
            Dim pathbits As String() = path.Split("="c)
            If pathbits.Count <> 2 Then
                Return ("Incorrect format to writereg " & path)
            End If

            Dim reg As RegInfo = ParseRegPath(pathbits(0))
            reg = ParseValueObject(pathbits(1), reg)
            If Not reg.returnMessage.StartsWith("Error") Then
                reg = RegEdit.WriteReg(reg)
            End If

            Return reg.returnMessage
        Catch ex As RegistryException
            Return ex.errorSource & " - " & ex.Message
        End Try
    End Function

    Public Function DelRegValue(ByRef path As String) As String
        Try
            Dim reg As RegInfo = ParseRegPath(path)
            reg = RegEdit.DeleteRegValue(reg)
            Return reg.returnMessage.ToString
        Catch ex As RegistryException
            Return ex.errorSource & " - " & ex.Message
        End Try
    End Function

    Public Function DelRegKey(ByRef path As String) As String
        Try
            Dim reg As RegInfo = ParseRegPath(path)
            reg = RegEdit.DeleteRegKey(reg)
            Return reg.returnMessage.ToString
        Catch ex As RegistryException
            Return ex.errorSource & " - " & ex.Message
        End Try
    End Function

    ''' <summary>
    ''' Add/Remove user to local admin
    ''' </summary>
    ''' <param name="param">Add or Remove</param>
    ''' <returns></returns>
    Public Function LocalAdmin(ByRef param As String) As String
        Dim username As String = SharedData.currentUser
        Dim pcname As String = My.Computer.Name
        Try
            Dim LCL As New DirectoryEntry("WinNT://" & pcname & ",computer")
            Dim DOM As New DirectoryEntry("WinNT://" & "as")
            Dim DOMUSR As DirectoryEntry = DOM.Children.Find(username, "user")
            Dim LCLGRP As DirectoryEntry = LCL.Children.Find("administrators", "group")
            LCLGRP.Invoke(param, New Object() {DOMUSR.Path.ToString})
            LCLGRP.CommitChanges()
            LCLGRP.Close()
        Catch ex As Exception
            WriteLog(String.Format("LocalAdmin: {0}", ex.Message), EventLogEntryType.Error)
            Return "Error - " & ex.Message
        End Try
        WriteLog(String.Format("LocalAdmin: {0}", username), EventLogEntryType.SuccessAudit)
        Return "OK"
    End Function

    Public Function IsPrivUser() As String
        Dim cid As Integer = Integer.Parse(GetComputerID())
        Dim pid As Integer = Integer.Parse(GetPersonID())
        Dim count As Integer = SharedData.PersonLinkTableAdapter.PrivExists(cid, pid)
        If count > 0 Then
            Return True
        Else
            Return False
        End If


    End Function

    Public Function GetPriv() As String
        'get priv file from DB and store local
        Dim FileTable As New ZuulDataSetTableAdapters.Tbl_FilesTableAdapter
        Dim tbl As DataTable = FileTable.GetDataByTitle("Priv")
        If tbl.Rows.Count > 0 Then
            Dim row As DataRow = tbl.Rows(0)
            Dim dataItem As Byte() = row.Field(Of Byte())("Data")
            Dim path As String = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) & "\Ashby School\privsfile.txt"
            Using fs As New FileStream(path, FileMode.Create)
                fs.Write(dataItem, 0, dataItem.Length)
            End Using
            Return True
            FilePermissions.GrantEveroneReadAccess(path)
        End If
        Return False
    End Function

    Public Function GetMOTD() As String
        Try
            Dim FileTable As New ZuulDataSetTableAdapters.Tbl_FilesTableAdapter
            Dim tbl As DataTable = FileTable.GetDataByTitle("MOTD")
            If tbl.Rows.Count > 0 Then
                Dim row As DataRow = tbl.Rows(0)
                Dim dataItem As Byte() = row.Field(Of Byte())("Data")
                Dim path As String = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) & "\Ashby School\motd.rtf"
                Using fs As New FileStream(path, FileMode.Create)
                    fs.Write(dataItem, 0, dataItem.Length)
                End Using
                FilePermissions.GrantEveroneReadAccess(path)
                Return True
            End If
        Catch ex As Exception
            WriteLog(String.Format("GetMOTD: {0}", ex.Message), EventLogEntryType.Error)
        End Try

        Return False
    End Function

    Public Function SetPriv() As String
        Dim path As String = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) & "\Ashby School\privsfile.txt"
        Dim line As String = ""
        Dim regvalue As String = ""
        Dim op As RegInfo = New RegInfo
        If File.Exists(path) Then
            Using fs As New StreamReader(path)
                Do While fs.Peek() <> -1
                    line = fs.ReadLine()
                    If line.StartsWith("KeyName:") Then
                        op.hive = "hkcu"
                        op.path = line.Split(":", 2, StringSplitOptions.None)(1)
                    ElseIf line.StartsWith("ValueName:") Then
                        op.name = line.Split(":", 2, StringSplitOptions.None)(1)
                    ElseIf line.StartsWith("Value:") Then
                        regvalue = line.Split(":", 2, StringSplitOptions.None)(1)
                        Select Case op.type
                            Case RegistryValueKind.Binary
                                op.value = System.Text.Encoding.Unicode.GetBytes(regvalue)
                            Case RegistryValueKind.DWord
                                regvalue = regvalue.Substring(2).TrimStart("0")
                                If regvalue.Equals("") Then regvalue = "0"
                                op.value = Convert.ToInt32(regvalue, 16)
                            Case RegistryValueKind.String
                                op.value = regvalue
                        End Select
                        EnactPriv(op)
                    ElseIf line.StartsWith("ValueType:") Then
                        Dim regtype As String = line.Split(":", 2, StringSplitOptions.None)(1)
                        Select Case regtype.ToLower
                            Case "binary"
                                op.type = RegistryValueKind.Binary
                            Case "reg_dword"
                                op.type = RegistryValueKind.DWord
                            Case "expandstring"
                                op.type = RegistryValueKind.ExpandString
                                op.value = regvalue
                            Case "qword"
                                op.type = RegistryValueKind.QWord
                                op.value = Int64.Parse(regvalue)
                            Case "reg_sz"
                                op.type = RegistryValueKind.String

                            Case Else
                                WriteLog(String.Format("SetPriv: ParseValueObject - unknown type {0}", op.value), EventLogEntryType.Error)
                                Throw New RegistryException(System.Reflection.MethodInfo.GetCurrentMethod.Name.ToString, "Error ParseValueObject - unknown type " & op.value)
                        End Select
                    End If
                Loop
                fs.Close()
                fs.Dispose()

            End Using
        End If
        Return "OK"
    End Function

    Public Sub EnactPriv(ByRef op As RegInfo)
        If op.name.StartsWith("**del.") Then
            Try
                DeleteRegValue(op)
            Catch ex As Exception

            End Try

        ElseIf op.name.StartsWith("**delvals.") Then
            Try
                DeleteRegKey(op)
            Catch ex As Exception

            End Try

        Else
            Try
                RegEdit.WriteReg(op)
            Catch ex As Exception

            End Try

        End If
    End Sub

    Public Function FOAD(ByRef passwd As String)
        WriteLog("FOAD", EventLogEntryType.Warning)
        If passwd.ToLower.Equals("diediedie") Then
            WriteLog("KILLING", EventLogEntryType.Warning)
            Dim service As ServiceController = New ServiceController("Middleman2018")
            service.Stop()
        End If
        Return "Shucks"
    End Function

End Class

Public Structure Regop
    Dim keyname As String
    Dim valuename As String
    Dim valuetype As String
    Dim value As String
    Dim hive As String
End Structure

