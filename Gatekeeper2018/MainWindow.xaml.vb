Imports System.ComponentModel
Imports System.IO
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
    Private Const WEB_GetGroups As String = "getusergroups"
    Private Const WEB_UnHookGatekeeper As String = "hookgatekeeper&params=off"
    Private Const WEB_HookGatekeeper As String = "hookgatekeeper&params=on"
    Private Const WEB_GPUpdate As String = "GPUpdate"

    Private ReadOnly PrintMapper As New BackgroundWorker()
    Private ReadOnly PrivUserMapper As New BackgroundWorker()
    Private ReadOnly OneDriveMapper As New BackgroundWorker()
    Private ReadOnly DriveMonitor As New BackgroundWorker()
    Private ReadOnly Explorer As New BackgroundWorker()

    Private PasswordHandler As New Password

    Dim cid As Integer
    Dim online As Boolean = False
    Dim monitoredDrives As String = "ZY"

    Private Sub GatekeeperMainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles GatekeeperMainWindow.Loaded
        Dim ret As String = ""

        Setup()
        AddHandler PrintMapper.DoWork, AddressOf MapPrinters
        AddHandler PrivUserMapper.DoWork, AddressOf HandlePrivUser
        AddHandler OneDriveMapper.DoWork, AddressOf HandleOneDriveMapper
        AddHandler DriveMonitor.DoWork, AddressOf HandleDriveMonitor
        AddHandler Explorer.DoWork, AddressOf HandleExplorerStart
        OneDriveMapper.RunWorkerAsync()
        DriveMonitor.RunWorkerAsync()

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
        Dim ret As String = WebLoader.Request(WEB_URL & WEB_GPUpdate)
        ret = WebLoader.Request(WEB_URL & WEB_IsPrivileged)
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

#Region "Drives"
    Private Sub MapDrivesButton_Click(sender As Object, e As RoutedEventArgs) Handles MapDrivesButton.Click
        DisplayBox.AppendText("Map Drives" & vbCrLf)
        DoMapDrives()
    End Sub

    Private Sub HandleOneDriveMapper(sender As Object, e As DoWorkEventArgs)
        DoMapDrives()
    End Sub

    Private Sub DoMapDrives()
        MapZ()
        MapY()

        Try
            Dim glist As String = WebLoader.Request(WEB_URL & WEB_GetGroups)
            If glist.ToLower.Contains("staff") Then
                MapV()
                monitoredDrives = monitoredDrives & "V"
            End If
            If glist.ToLower.Contains("slt") Then
                MapS()
                monitoredDrives = monitoredDrives & "S"
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub HandleDriveMonitor(sender As Object, e As DoWorkEventArgs)
        While True
            MonitorDrives()
            Threading.Thread.Sleep(2000)
        End While

    End Sub

    Private Sub MonitorDrives()
        'Dim allDrives() As DriveInfo = DriveInfo.GetDrives
        Dim rlist As String = ""
        Dim drive As DriveInfo
        For Each letter As Char In monitoredDrives
            drive = New DriveInfo(letter)
            If drive.IsReady Then
                rlist = rlist & letter
            End If
        Next
        If rlist.Length = monitoredDrives.Length Then
            NotifyIcon.Icon = My.Resources.greencloud
            NotifyIcon.HideBalloonTip()
        Else
            NotifyIcon.Icon = My.Resources.redcloud
            NotifyIcon.ShowBalloonTip("Drive Mapper", "Not all drives mapped.", BalloonIcon.Error)
        End If
    End Sub

    Private Sub MapZ()
        Dim outline As String = ""
        Dim p As New ProcessStartInfo
        Dim username As String = Environment.UserName & "@ashbyschool.org.uk"
        Dim passwd As String = PasswordHandler.LoadPW
        p.FileName = "C:\Program Files (x86)\Ashby School\MiddlemanInstaller\ASCookieIntegrated.exe"
        p.UseShellExecute = False
        p.RedirectStandardOutput = True
        p.RedirectStandardError = True
        p.CreateNoWindow = True
        p.Arguments = String.Format("-s https://ashbyschool-my.sharepoint.com -u {0} -p {1} mount z: -homedir", username, passwd)
        Dim Cookie As New Process With {
            .StartInfo = p
        }
        Cookie.Start()
        Dim worked As Boolean = Cookie.WaitForExit(30000)
        outline = Cookie.StandardOutput.ReadToEnd
        Dim err As String = Cookie.StandardError.ReadToEnd
        If Not worked Then
            Dim arse As Boolean = True
            'fucked
        End If
        DoMap(New Uri("https://ashbyschool-my.sharepoint.com"), "z:", username, passwd, True)
    End Sub

    Private Sub MapY()
        Dim p As New ProcessStartInfo
        Dim username As String = Environment.UserName & "@ashbyschool.org.uk"
        Dim passwd As String = PasswordHandler.LoadPW
        p.FileName = "C:\Program Files (x86)\Ashby School\MiddlemanInstaller\ASCookieIntegrated.exe"
        p.WindowStyle = ProcessWindowStyle.Hidden
        p.Arguments = String.Format("-s https://ashbyschool.sharepoint.com/StudentShared -u {0} -p {1} -mount y:", username, passwd)
        Dim Cookie As Process = Process.Start(p)
        Dim worked As Boolean = Cookie.WaitForExit(30000)
        If Not worked Then
            Dim arse As Boolean = True
            'fucked
        End If
        DoMap(New Uri("https://ashbyschool.sharepoint.com/StudentShared"), "y:", username, passwd, False)
    End Sub

    Private Sub MapV()
        Dim p As New ProcessStartInfo
        Dim username As String = Environment.UserName & "@ashbyschool.org.uk"
        Dim passwd As String = PasswordHandler.LoadPW
        p.FileName = "C:\Program Files (x86)\Ashby School\MiddlemanInstaller\ASCookieIntegrated.exe"
        p.WindowStyle = ProcessWindowStyle.Hidden
        p.Arguments = String.Format("-s https://ashbyschool.sharepoint.com/StaffShared -u {0} -p {1} -mount v:", username, passwd)
        Dim Cookie As Process = Process.Start(p)
        Dim worked As Boolean = Cookie.WaitForExit(30000)
        If Not worked Then
            Dim arse As Boolean = True
            'fucked
        End If
        DoMap(New Uri("https://ashbyschool.sharepoint.com/StaffShared"), "v:", username, passwd, False)
    End Sub

    Private Sub MapS()
        Dim p As New ProcessStartInfo
        Dim username As String = Environment.UserName & "@ashbyschool.org.uk"
        Dim passwd As String = PasswordHandler.LoadPW
        p.FileName = "C:\Program Files (x86)\Ashby School\MiddlemanInstaller\ASCookieIntegrated.exe"
        p.WindowStyle = ProcessWindowStyle.Hidden
        p.Arguments = String.Format("-s https://ashbyschool.sharepoint.com/SLT/SLTDocs -u {0} -p {1} -mount s:", username, passwd)
        Dim Cookie As Process = Process.Start(p)
        Dim worked As Boolean = Cookie.WaitForExit(30000)
        If Not worked Then
            Dim arse As Boolean = True
            'fucked
        End If
        DoMap(New Uri("https://ashbyschool.sharepoint.com/SLT/SLTDocs"), "s:", username, passwd, False)
    End Sub

    Private Sub DoMap(sharepointUri As Uri, disk As String, username As String, passwd As String, mount As Boolean)
        Dim homedir As String = ""
        If mount Then
            Dim user As String = username.Split(CType("@", Char()))(0)
            Dim domain As String = username.Split(CType("@", Char()))(1)
            homedir = "DavWWWRoot\\personal\\" + user + "_" + domain.Split(CType(".", Char()))(0) + "_" + domain.Split(CType(".", Char()))(1) + "_" + domain.Split(CType(".", Char()))(2) + "\\Documents"
        End If


        Dim cmdArgs As String = "/c net use " + disk + " \\\\" + sharepointUri.Host + "@ssl" + sharepointUri.PathAndQuery.Replace("/", "\\") + homedir
        cmdArgs = cmdArgs.Replace("\\", "\")

        Dim Process As Process = New System.Diagnostics.Process()
        Process.StartInfo = New System.Diagnostics.ProcessStartInfo("cmd", cmdArgs)
        Process.StartInfo.CreateNoWindow = True
        Process.StartInfo.RedirectStandardOutput = True
        Process.StartInfo.RedirectStandardError = True
        Process.StartInfo.UseShellExecute = False
        'Process.StartInfo.CreateNoWindow = true
        Process.Start()

        Process.WaitForExit()
        Dim err As String = Process.StandardError.ReadToEnd
        Dim output As String = Process.StandardOutput.ReadToEnd()
        Console.WriteLine(output)
    End Sub
#End Region

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

    Private Sub AcceptButton_Click(sender As Object, e As RoutedEventArgs) Handles AcceptButton.Click
        Me.Visibility = Visibility.Hidden
        Explorer.RunWorkerAsync()
    End Sub

    Private Sub HandleExplorerStart(sender As Object, e As DoWorkEventArgs)
        Dim ret As String = ""
        ret = WebLoader.Request(WEB_URL & WEB_UnHookGatekeeper)
        Threading.Thread.Sleep(1000)
        Process.Start("C:\windows\explorer.exe")
        Threading.Thread.Sleep(5000)
        ret = WebLoader.Request(WEB_URL & WEB_HookGatekeeper)
    End Sub

End Class

Structure Pinfo
    Public connectionString As String
    Public isDefault As Boolean
End Structure
