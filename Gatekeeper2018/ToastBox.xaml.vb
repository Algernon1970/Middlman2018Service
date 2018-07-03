Imports System.Windows.Media.Animation

Public Class ToastBox
    Dim sb As Storyboard
    Private _title As String
    Private _message As String

    Public Property ToastTitle As String
        Get
            Return _title
        End Get
        Set(value As String)
            _title = value
        End Set
    End Property
    Public Property Message As String
        Get
            Return _message
        End Get
        Set(value As String)
            _message = value
            ToastMessage.Text = _message
        End Set
    End Property

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        Me.Topmost = True
        Me.Focusable = False
        Dim WA = System.Windows.SystemParameters.WorkArea
        Me.Left = WA.Right - Me.Width
        Me.Top = WA.Bottom - Me.Height
    End Sub

    Public Overloads Sub Show()
        Me.Visibility = Visibility.Visible
        Dim sb As Storyboard = CType(FindResource("AttentionGrabber"), Storyboard)
        sb.Begin()
    End Sub

    Public Overloads Sub Show(ByVal title As String, ByVal msg As String)
        ToastTitle = title
        Message = msg
        Show()
    End Sub

    Private Sub Storyboard_Completed(sender As Object, e As EventArgs)
        Me.Visibility = Visibility.Hidden
    End Sub
End Class
