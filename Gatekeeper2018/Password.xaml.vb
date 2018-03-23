Imports System.IO
Imports System.DirectoryServices.AccountManagement
Imports GatekeeperTools

Public Class Password
    Private Const WEB_URL As String = "http://127.0.0.1:1701/?command="
    Private Const WEB_Logout As String = "ForceLogout"
    Public Status As Boolean = False
    Private Sub CheckPwdButton_Click(sender As Object, e As RoutedEventArgs) Handles CheckPwdButton.Click
        ProcessPWBox()
    End Sub

    Private Sub PasswordText_KeyUp(sender As Object, e As KeyEventArgs) Handles PasswordText.KeyUp
        If e.Key.Equals(Key.Return) Then
            ProcessPWBox()
        End If
    End Sub

    Private Sub ProcessPWBox()
        Dim pwd As String = PasswordText.Password
        Dim enc As New Simple3Des("A$hbySchool1")
        If CheckPW(pwd) Then
            Status = True
            StorePW(enc.EncryptData(pwd))
            Me.Visibility = Visibility.Hidden
            PasswordText.Clear()
        Else
            StatusText.Content = "Password is wrong."
            Status = False
            PasswordText.Clear()
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

    ''' <summary>
    ''' Return the password DECODED!
    ''' </summary>
    ''' <returns></returns>
    Public Function LoadPW() As String
        If File.Exists("N:\\My Settings\\Ashby School\\cpd.ash") Then
            Dim w As New StreamReader(File.Open("N:\\My Settings\\Ashby School\\cpd.ash", FileMode.Open))
            Dim encoded As String = w.ReadLine()
            Dim decoder As New Simple3Des("A$hbySchool1")
            w.Close()
            w.Dispose()

            Return decoder.DecryptData(encoded)
        End If
        Return "NOPWD"
    End Function

    ''' <summary>
    ''' return the password encoded.
    ''' </summary>
    ''' <returns></returns>
    Public Function LoadPWE() As String
        If File.Exists("N:\\My Settings\\Ashby School\\cpd.ash") Then
            Dim w As New StreamReader(File.Open("N:\\My Settings\\Ashby School\\cpd.ash", FileMode.Open))
            Dim encoded As String = w.ReadLine()
            w.Close()
            w.Dispose()

            Return encoded
        End If
        Return "NOPWD"
    End Function

    Private Sub Logout_Click(sender As Object, e As RoutedEventArgs) Handles Logout.Click
        WebLoader.Request(WEB_URL & WEB_Logout)
    End Sub
End Class

