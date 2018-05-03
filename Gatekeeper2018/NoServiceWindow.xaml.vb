Public Class NoServiceWindow
    Private Sub LogoutButton_Click(sender As Object, e As RoutedEventArgs) Handles LogoutButton.Click
        System.Diagnostics.Process.Start("ShutDown", "/r /t 00")
    End Sub
End Class
