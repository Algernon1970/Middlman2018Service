Imports System.IO
Imports System.DirectoryServices.AccountManagement

Public Class Password
    Public Status As Boolean = False
    Private Sub CheckPwdButton_Click(sender As Object, e As RoutedEventArgs) Handles CheckPwdButton.Click
        Dim pwd As String = PasswordText.Password
        Dim enc As New Simple3Des("A$hbySchool1")
        If CheckPW(pwd) Then
            Status = True
            StorePW(enc.EncryptData(pwd))
            Me.Visibility = Visibility.Hidden
        Else
            StatusText.Content = "Password is wrong."
            Status = False
        End If
    End Sub

    Public Function CheckPW(ByRef pw As String) As Boolean
        Dim res As Boolean = False
        Using pc As PrincipalContext = New PrincipalContext(ContextType.Domain, "as.internal")
            res = pc.ValidateCredentials(Environment.UserName, pw)
        End Using
        Return res
    End Function

    Public Sub StorePW(ByRef pw As String)
        If Not Directory.Exists("N:\My Settings\Ashby School\") Then
            Directory.CreateDirectory("N:\My Settings\Ashby School")
        End If

        Using w As New StreamWriter(File.Open("N:\\My Settings\\Ashby School\\cpd.ash", FileMode.Create))
            w.WriteLine(pw)
        End Using
    End Sub

    Public Function LoadPW() As String
        Dim w As New StreamReader(File.Open("N:\\My Settings\\Ashby School\\cpd.ash", FileMode.Open))
        Dim encoded As String = w.ReadLine()
        Dim decoder As New Simple3Des("A$hbySchool1")
        w.Close()
        w.Dispose()

        Return decoder.DecryptData(encoded)
    End Function

End Class

