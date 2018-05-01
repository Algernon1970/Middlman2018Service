Public Class NoServiceWindow
    Private Sub LogoutButton_Click(sender As Object, e As RoutedEventArgs) Handles LogoutButton.Click
        WebLoader.Request(WEB_URL & WEB_Logout)
    End Sub
End Class
