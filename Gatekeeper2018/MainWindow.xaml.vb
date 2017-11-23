Class MainWindow
    Private Const WEB_URL As String = "http://127.0.0.1:1701/?command="
    Private Const WEB_SetName As String = "setname&params="
    Private Const WEB_CheckOnline As String = "recheckonline"
    Private Const WEB_GetComputerID As String = "getcomputerid"
    Private Const WEB_GetPrinterList As String = "getprinterlist"
    Private Const WEB_GetPrinterConnection As String = "getprinterconnection&params="

    Dim cid As Integer
    Dim online As Boolean = False
    Private Sub GatekeeperMainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles GatekeeperMainWindow.Loaded
        Dim ret As String = ""
        Try
            Setup()
            If online Then
                MapPrinters()
            End If

        Catch ex As Exception
            statusLabel.Content = "Error"
            DisplayBox.AppendText(vbCrLf & ex.Message)
        End Try
    End Sub

    Private Sub Setup()
        Dim ret As String = ""
        ret = WebLoader.Request(WEB_URL & WEB_SetName & My.User.Name)
        ret = WebLoader.Request(WEB_URL & WEB_CheckOnline)
        statusLabel.Content = ret
        If ret.ToLower.Equals("online") Then online = True
        ret = WebLoader.Request(WEB_URL & WEB_GetComputerID)
        DisplayBox.AppendText("ComputerID = " & ret & vbCrLf)
    End Sub

    Private Sub MapPrinters()
        Dim plist As List(Of String) = GetPrinterConnectionList()
        For Each printer As String In plist
            DisplayBox.AppendText("Printer = " & printer & vbCrLf)
        Next
    End Sub

    Private Function GetPrinterConnectionList() As List(Of String)
        Dim ret As String
        Dim plist As New List(Of String)
        Dim cs As String
        ret = WebLoader.Request(WEB_URL & WEB_GetPrinterList)
        For Each pid As String In ret.Split(",")
            cs = WebLoader.Request(WEB_URL & WEB_GetPrinterConnection & pid)
            plist.Add(cs)
        Next
        Return plist
    End Function
End Class
