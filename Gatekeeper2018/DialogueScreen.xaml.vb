Public Class DialogueScreen

    Public Sub New(ByVal title As String, ByVal content As String)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        DialogueTitle.Content = title
        DialogueText.Document.Blocks.Clear()
        DialogueText.Document.Blocks.Add(New Paragraph(New Run(content)))
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub
End Class
