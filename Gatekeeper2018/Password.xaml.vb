Imports System.IO
Imports System.DirectoryServices.AccountManagement

Public Class Password
    Public Passwd As String = ""
    Public Status As Boolean = False
    Private Sub CheckPwdButton_Click(sender As Object, e As RoutedEventArgs) Handles CheckPwdButton.Click
        Dim pwd As String = PasswordText.Password
        Dim res As Boolean = CheckPW(pwd)
        Dim enc As New Simple3Des("A$hbySchool1")
        Dim cryptPassword As String = ""
        If res Then
            StatusText.Content = "Password is good"
            Status = True

            cryptPassword = enc.EncryptData(pwd)
            StorePW(cryptPassword)
        Else
            StatusText.Content = "Password is bad"
            Status = False
        End If

        Passwd = cryptPassword
    End Sub

    Private Function CheckPW(ByRef pw As String) As Boolean
        Dim res As Boolean = False
        Using pc As PrincipalContext = New PrincipalContext(ContextType.Domain, "as.internal")
            res = pc.ValidateCredentials(Environment.UserName, pw)
        End Using
        Return res
    End Function

    Private Sub StorePW(ByRef pw As String)
        If Not Directory.Exists("N:\My Settings\Ashby School\") Then
            Directory.CreateDirectory("N:\My Settings\Ashby School")
        End If

        Using w As New StreamWriter(File.Open("N:\\My Settings\\Ashby School\\cpd.ash", FileMode.Create))
            w.WriteLine(pw)
        End Using
    End Sub

    Public Function LoadPW() As String
        Using w As New StreamReader(File.Open("N:\\My Settings\\Ashby School\\cpd.ash", FileMode.Open))
            Dim encoded As String = w.ReadLine()
            Dim decoded As String
            Dim decoder As New Simple3Des("A$hbySchool1")
            decoded = decoder.DecryptData(encoded)
            Return decoded
        End Using
    End Function
End Class

