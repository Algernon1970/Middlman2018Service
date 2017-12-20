Imports System.ComponentModel
Imports Hardcodet.Wpf.TaskbarNotification

Class MainWindow
    Private Const WEB_URL As String = "http://127.0.0.1:1701/?command="
    Private Const WEB_SetName As String = "setuser&params="
    Private Const WEB_CheckOnline As String = "recheckonline"
    Private Const WEB_GetComputerID As String = "getcomputerid"
    Private Const WEB_GetPrinterList As String = "getprinterlist"
    Private Const WEB_GetPrinterConnection As String = "getprinterconnection&params="
    Private Const WEB_IsPrivileged As String = "isprivuser"
    Private Const WEB_CopyPrivFile As String = "getpriv"
    Private Const WEB_SetPrivs As String = "setpriv"

    Private ReadOnly PrintMapper As New BackgroundWorker()
    Private ReadOnly PrivUserMapper As New BackgroundWorker()
    Private ReadOnly OneDriveMapper As New BackgroundWorker()

    Private PasswordHandler As New Password

    Dim cid As Integer
    Dim online As Boolean = False
    Private Sub GatekeeperMainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles GatekeeperMainWindow.Loaded
        Dim ret As String = ""

        Setup()
            AddHandler PrintMapper.DoWork, AddressOf MapPrinters
            AddHandler PrivUserMapper.DoWork, AddressOf HandlePrivUser
            AddHandler OneDriveMapper.DoWork, AddressOf HandleOneDriveMapper
            OneDriveMapper.RunWorkerAsync()

        If online Then
            PrintMapper.RunWorkerAsync()
            PrivUserMapper.RunWorkerAsync()
        End If
    End Sub

    Private Sub Setup()
        NotifyIcon.Icon = My.Resources.whitecloud
        Dim pwdString As String = "none"
        Try
            pwdString = PasswordHandler.LoadPW
            If Not PasswordHandler.CheckPW(pwdString) Then
                PasswordHandler.ShowDialog()
            End If
        Catch ex As Exception
            PasswordHandler.ShowDialog()
        End Try
        Dim ret As String = ""
        Try
            ret = WebLoader.Request(WEB_URL & WEB_SetName & Environment.UserName)
            ret = WebLoader.Request(WEB_URL & WEB_CheckOnline)
            statusLabel.Content = ret
            If ret.ToLower.Equals("online") Then online = True
            ret = WebLoader.Request(WEB_URL & WEB_GetComputerID)
        Catch ex As Exception
            DisplayBox.AppendText("Middleman not available. Contact Network Services." & vbCrLf)
            DisplayBox.Background = Brushes.Red
        End Try

    End Sub

    Private Sub HandlePrivUser(sender As Object, e As DoWorkEventArgs)
        Dim ret As String = WebLoader.Request(WEB_URL & WEB_IsPrivileged)
        If ret.Equals("True") Then
            If online Then WebLoader.Request(WEB_URL & WEB_CopyPrivFile)
            WebLoader.Request(WEB_URL & WEB_SetPrivs)

            Dim priv As Boolean = True
        Else
            Dim priv As Boolean = False
        End If
    End Sub

    Private Sub MapPrinters(sender As Object, e As DoWorkEventArgs)
        Dim plist As List(Of Pinfo) = GetPrinterConnectionList()
        Dim res As String
        For Each printer As Pinfo In plist
            If printer.isDefault Then
                res = PrinterMapper.MapPrinter(printer.connectionString)
                PrinterMapper.SetDefaultPrinter(printer.connectionString)
            Else
                res = PrinterMapper.MapPrinter(printer.connectionString)
            End If

        Next
    End Sub

    Private Sub HandleOneDriveMapper(sender As Object, e As DoWorkEventArgs)
        MapZ()
        MapY()
        MapV()

        While True
            Threading.Thread.Sleep(10000)
        End While
    End Sub

    Private Sub MapZ()
        Dim mapper As New AS365Cookie.Program
        Dim username As String = Environment.UserName & "@ashbyschool.org.uk"
        Dim passwd As String = PasswordHandler.LoadPW
        mapper.GetCookie365({"-s", "https://ashbyschool-my.sharepoint.com", "-u", username, "-p", passwd, "-mount", "z:", "-homedir"})
    End Sub

    Private Sub MapY()
        Dim mapper As New AS365Cookie.Program
        Dim username As String = Environment.UserName & "@ashbyschool.org.uk"
        Dim passwd As String = PasswordHandler.LoadPW
        mapper.GetCookie365({"-s", "https://ashbyschool-my.sharepoint.com", "-u", username, "-p", passwd, "-mount", "y:", "-map", "https://ashbyschool.sharepoint.com/StudentShared"})
    End Sub

    Private Sub MapV()
        Dim mapper As New AS365Cookie.Program
        Dim username As String = Environment.UserName & "@ashbyschool.org.uk"
        Dim passwd As String = PasswordHandler.LoadPW
        mapper.GetCookie365({"-s", "https://ashbyschool-my.sharepoint.com", "-u", username, "-p", passwd, "-mount", "v:", "-map", "https://ashbyschool.sharepoint.com/StaffShared"})
    End Sub

    Private Sub MapS()
        Dim mapper As New AS365Cookie.Program
        Dim username As String = Environment.UserName & "@ashbyschool.org.uk"
        Dim passwd As String = PasswordHandler.LoadPW
        mapper.GetCookie365({"-s", "https://ashbyschool-my.sharepoint.com", "-u", username, "-p", passwd, "-mount", "s:", "-map", "https://ashbyschool.sharepoint.com/SLT/SLTDocs"})
    End Sub

    Private Function GetPrinterConnectionList() As List(Of Pinfo)
        Dim ret As String
        Dim plist As New List(Of Pinfo)
        Dim cs As String
        Dim pdefault As Boolean
        ret = WebLoader.Request(WEB_URL & WEB_GetPrinterList)
        For Each pid As String In ret.Split(",".ToCharArray)
            If pid.Length > 0 Then
                pdefault = False
                If pid.StartsWith("*") Then
                    pid = pid.Substring(1)
                    pdefault = True
                End If
                cs = WebLoader.Request(WEB_URL & WEB_GetPrinterConnection & pid)
                Dim pi As New Pinfo With {
                    .connectionString = cs,
                    .isDefault = pdefault
                }
                plist.Add(pi)
            End If

        Next
        Return plist
    End Function

    Private Sub MapDrivesButton_Click(sender As Object, e As RoutedEventArgs) Handles MapDrivesButton.Click
        DisplayBox.AppendText("Map Drives" & vbCrLf)
    End Sub

    Private Sub PasswordButton_Click(sender As Object, e As RoutedEventArgs) Handles PasswordButton.Click
        PasswordHandler.ShowDialog()

        If PasswordHandler.Status Then
            DisplayBox.AppendText(vbCrLf & "Got PW " & PasswordHandler.LoadPW & vbCrLf)
            NotifyIcon.Icon = My.Resources.greencloud
        Else
            NotifyIcon.Icon = My.Resources.redcloud
        End If
    End Sub

    Private Sub GatekeeperMainWindow_Closed(sender As Object, e As EventArgs) Handles GatekeeperMainWindow.Closed

    End Sub
End Class

Structure Pinfo
    Public connectionString As String
    Public isDefault As Boolean
End Structure
