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
        Return "Name = " & params
    End Function

    Public Function ForceLogout() As String
        Dim t As Single
        Dim objWMIService, objComputer As Object

        'Now get some privileges
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
        Return SharedData.test
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
        For Each pid As String In pidTable.Rows(0).Field(Of String)("PrinterID")
            pList = String.Format("{0}{1},", pList, pid)
        Next
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
End Class
