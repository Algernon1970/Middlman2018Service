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
End Class
