Imports System.IO
Public Class Middleman2018
    Dim ws As New WebServer()

    Protected Overrides Sub OnStart(ByVal args() As String)
        Dim eLog As EventLog = GetLogger()
        Try
            Dim path As String = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) & "\Ashby School\"
            If Not Directory.Exists(path) Then
                Directory.CreateDirectory(path)
            End If

            Dim ch As New CommandHandler
            ch.HookGatekeeper("on")
        Catch ex As Exception
            If eLog IsNot Nothing Then
                eLog.WriteEntry(String.Format("OnStart: {0}", ex.Message), EventLogEntryType.Error)
            End If
        End Try
        If eLog IsNot Nothing Then
            eLog.WriteEntry("Starting Webserver", EventLogEntryType.Information)
        End If
        ws.StartServer()
    End Sub

    Protected Overrides Sub OnStop()
        If eLog IsNot Nothing Then
            eLog.WriteEntry("Stopping Webserver", EventLogEntryType.Information)
        End If
        ws.StopServer()
    End Sub

End Class
