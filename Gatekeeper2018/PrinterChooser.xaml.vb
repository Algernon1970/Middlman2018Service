Option Strict Off
Public Class PrinterChooser
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

    End Sub

    Public Sub DisplayList(ByVal pString As String)
        Dim plist As New List(Of String)
        For Each printer As String In pString.Split(","c)
            If printer IsNot "" Then
                plist.Add(printer)
            End If

        Next
        PListBox.ItemsSource = plist
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        If sender.Content.tolower.contains("always") Then
            MsgBox("Always map " & sender.datacontext.ToString)
        Else
            MsgBox("This time map " & sender.datacontext.ToString)
        End If
    End Sub

End Class

