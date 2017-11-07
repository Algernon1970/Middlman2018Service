Public Class Middleman2018
    Dim ws As New WebServer()

    Protected Overrides Sub OnStart(ByVal args() As String)
        SharedData.test = 10
        ws.StartServer()
    End Sub

    Protected Overrides Sub OnStop()
        ws.StopServer()
    End Sub

End Class
