Class MainWindow
    Private Sub GatekeeperMainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles GatekeeperMainWindow.Loaded
        Dim ret As String = ""
        Try
            ret = WebLoader.Request("http://127.0.0.1:1701/?command=setname&params=" & My.User.Name)
            ret = WebLoader.Request("http://127.0.0.1:1701/?command=recheckonline")
            statusLabel.Content = ret
            ret = WebLoader.Request("http://127.0.0.1:1701/?command=getcomputerid")
            DisplayBox.AppendText("ComputerID = " & ret & vbCrLf)
            ret = WebLoader.Request("http://127.0.0.1:1701/?command=getprinterlist")
            DisplayBox.AppendText("PrinterList = " & ret & vbCrLf)

        Catch ex As Exception
            statusLabel.Content = "Error"
            DisplayBox.AppendText(vbCrLf & ex.Message)
        End Try


    End Sub
End Class
