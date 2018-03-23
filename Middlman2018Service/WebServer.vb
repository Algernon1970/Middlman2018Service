Imports System.Net
Imports System.Text
Imports System.IO
Imports System.ComponentModel

Public Class WebServer
    Const listenerPrefix As String = "http://*:1701/"
    Dim listener As HttpListener
    Dim cancelled As Boolean = False

    Public Sub StartServer()
        listener = New HttpListener
        listener.Prefixes.Add(listenerPrefix)
        listener.Start()
        listener.BeginGetContext(AddressOf RequestWait, Nothing)
    End Sub

    Public Sub StopServer()
        cancelled = True
        listener.Close()
        listener.Stop()
    End Sub

    Private Sub RequestWait(ByVal ar As IAsyncResult)
        If Not listener.IsListening Then
            listener.BeginGetContext(AddressOf RequestWait, Nothing)
        End If
        Dim formattedResponse As String = ""
        Dim c = listener.EndGetContext(ar)
        listener.BeginGetContext(AddressOf RequestWait, Nothing)
        Respond(c, HandleCommands(c))

    End Sub

    Private Sub Respond(ByRef c As HttpListenerContext, ByVal response As String)
        Dim log As EventLog = GetLogger()
        Try
            Dim buffer() As Byte = Encoding.Unicode.GetBytes(response)
            c.Response.ContentLength64 = buffer.Length
            Dim output As System.IO.Stream = c.Response.OutputStream
            output.Write(buffer, 0, buffer.Length)
            output.Flush()
            output.Close()
        Catch ex As Exception
            If log IsNot Nothing Then
                log.WriteEntry(String.Format("Respond:{0} - {1}", response, ex.Message))
            End If
        End Try

    End Sub

    Private Function HandleCommands(ByRef c As HttpListenerContext) As String
        Dim eLog As EventLog = GetLogger()
        Try
            If IsNothing(c.Request.QueryString("params")) Then
                eLog.WriteEntry(String.Format("HandleCommands: {0}", c.Request.QueryString("command")))
                Return CStr(CallByName(New CommandHandler, c.Request.QueryString("command"), Microsoft.VisualBasic.CallType.Method))
            Else
                eLog.WriteEntry(String.Format("HandleCommands: {0} & {1}", c.Request.QueryString("command"), c.Request.QueryString("params")))
                Return CStr(CallByName(New CommandHandler, c.Request.QueryString("command"), Microsoft.VisualBasic.CallType.Method, c.Request.QueryString("params")))
            End If

        Catch ex As Exception
            Return "Failed to call " & c.Request.QueryString("command")
            If eLog IsNot Nothing Then
                eLog.WriteEntry(String.Format("HandleCommands {0}: Failed {1}", c.Request.QueryString("command"), ex.Message))
            End If
        End Try
    End Function

End Class


