Imports System
Imports System.DirectoryServices
Imports System.Security.Principal
Imports AshbyTools
Imports AshbyTools.murrayju.ProcessExtensions

Public Class CommandHandler

    Public Function GetVersion() As String
        Return "Version 2018.2"
    End Function

    Public Function Test(ByVal cmdline As String) As String
        Return "Test got " & cmdline
    End Function

    Public Function SetUser(ByVal params As String) As String
        SharedData.currentUser = params
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
        Return If(success, "Succeeded", "Failed")
    End Function

    Public Function CheckOnline() As String
        Return SharedData.online
    End Function

    Public Function RecheckOnline() As String
        Dim ret As String = AmIOnDomain()
        Return If(ret = "Offline", "Offline", "Online")
    End Function

    Public Function GetComputerID() As String
        Dim cid As Integer = 0
        Dim cr As DataTable = SharedData.ComputerTableAdapter.GetComputerByName(My.Computer.Name)
        If cr.Rows.Count = 0 Then
            If SharedData.ComputerTableAdapter.CreateComputer(My.Computer.Name, "0", "0", "0", "0", 0, 0, 0, 1, 1, 1, "Unknown", "Unknown") Then
                cr = SharedData.ComputerTableAdapter.GetComputerByName(My.Computer.Name)
                If cr.Rows.Count = 0 Then Return "0"
            End If
        End If
        cid = cr.Rows(0).Field(Of Integer)("ComputerID")
        Return cid.ToString
    End Function

    Public Function GetPrinterList() As String
        Dim pList As String = ""
        Dim cid As String = GetComputerID()
        Dim pidTable As DataTable = SharedData.PLinkTableAdapter.GetPIDsByComputerID(cid)
        Dim pid As Integer
        If pidTable.Rows.Count > 0 Then
            For Each row As DataRow In pidTable.Rows
                pid = row.Field(Of Integer)("PrinterID")
                pList = String.Format("{0}{1},", pList, pid)
            Next
        End If
        If pList.Length = 0 Then
            pList = "No Printers"
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
            ProcessExtensions.StartProcessAsCurrentUser("C:\Program Files (x86)\Ashby School\MiddlemanInstaller\userutilities2018.exe", "Utilities GPUPDATE", "C:\Program Files (x86)\Ashby School\MiddlemanInstaller", True)
        Else
            ProcessExtensions.StartProcessAsCurrentUser("C:\Program Files\Ashby School\MiddlemanInstaller\userutilities2018.exe", "Utilities GPUPDATE", "C:\Program Files\Ashby School\MiddlemanInstaller", True)
        End If
        Return "OK"
    End Function

    ''' <summary>
    ''' Is Z Drive mapped and ready?
    ''' </summary>
    ''' <returns>Yes/No</returns>
    Public Function Gotz() As String
        Return "YES"
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
            Dim pathbits As String() = path.Split("=")
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
        Dim username As String = My.User.Name
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
            Return "Error - " & ex.Message
        End Try
        Return "OK"
    End Function

End Class
