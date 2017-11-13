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
        Dim buffer() As Byte = Encoding.Unicode.GetBytes(response)

        c.Response.ContentLength64 = buffer.Length
        Dim output As System.IO.Stream = c.Response.OutputStream
        output.Write(buffer, 0, buffer.Length)
        output.Flush()
        output.Close()
    End Sub

    Private Function HandleCommands(ByRef c As HttpListenerContext)
        Try
            If IsNothing(c.Request.QueryString("params")) Then
                Return CStr(CallByName(New CommandHandler, c.Request.QueryString("command"), Microsoft.VisualBasic.CallType.Method))
            Else
                Return CStr(CallByName(New CommandHandler, c.Request.QueryString("command"), Microsoft.VisualBasic.CallType.Method, c.Request.QueryString("params")))
            End If

        Catch ex As Exception
            Return "Failed to call " & c.Request.QueryString("command")
        End Try
    End Function

End Class


