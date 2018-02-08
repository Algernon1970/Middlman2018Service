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
    Private Const WEB_CopyMOTD As String = "GetMOTD"
    Private Const WEB_RecordDrives As String = "RecordDrives&params="
    Private Const WEB_LocalAdmin As String = "LocalAdmin&params="

    Private ReadOnly PrintMapper As New BackgroundWorker()
    Private ReadOnly PrivUserMapper As New BackgroundWorker()
    Private ReadOnly OneDriveMapper As New BackgroundWorker()
    Private ReadOnly DriveMonitor As New BackgroundWorker()
    Private ReadOnly Explorer As New BackgroundWorker()

    Private PasswordHandler As New Password
    Private logging As Boolean = False
    Private log As New EventLog

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
        Try
            If Not EventLog.SourceExists("GK2018") Then
                EventLog.CreateEventSource("GK2018", "GK2018")
                log.Source = "GK2018"
            Else
                logging = True
                log.Source = "GK2018"
            End If
        Catch ex As Exception
            logging = False
        End Try

        NotifyIcon.Icon = My.Resources.whitecloud
        Dim pwdString As String = "none"
        Try
            pwdString = PasswordHandler.LoadPW
            If Not PasswordHandler.CheckPW(pwdString) Then
                PasswordHandler.WindowStartupLocation = WindowStartupLocation.CenterScreen
                PasswordHandler.ShowDialog()
            End If
        Catch ex As Exception
            PasswordHandler.WindowStartupLocation = WindowStartupLocation.CenterScreen
            PasswordHandler.ShowDialog()
        End Try
        Dim ret As String = ""
        Try
            ret = WebLoader.Request(WEB_URL & WEB_SetName & Environment.UserName)
            ret = WebLoader.Request(WEB_URL & WEB_CheckOnline)
            statusLabel.Content = ret
            If ret.ToLower.Equals("online") Then
                online = True
                ret = WebLoader.Request(WEB_URL & WEB_GetComputerID)
                ret = WebLoader.Request(WEB_URL & WEB_CopyMOTD)
            End If
            Dim path As String = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) & "\Ashby School\motd.rtf"
            If File.Exists(path) Then
                Dim range As TextRange
                range = New TextRange(DisplayBox.Document.ContentStart, DisplayBox.Document.ContentEnd)
                Using fstream As New FileStream(path, FileMode.Open)
                    range.Load(fstream, DataFormats.Rtf)
                End Using
            End If
        Catch ex As Exception
            DisplayBox.AppendText("Middleman not available. Contact Network Services." & vbCrLf)
            DisplayBox.Background = Brushes.Red
            If logging Then
                log.WriteEntry("SETUP: " & ex.Message, EventLogEntryType.Error)
            End If
        End Try

    End Sub

    Private Sub HandlePrivUser(sender As Object, e As DoWorkEventArgs)
        Dim ret As String = WebLoader.Request(WEB_URL & WEB_GPUpdate)
        ret = WebLoader.Request(WEB_URL & WEB_IsPrivileged)
        If ret.Equals("True") Then
            If online Then WebLoader.Request(WEB_URL & WEB_CopyPrivFile)
            WebLoader.Request(WEB_URL & WEB_LocalAdmin & "add")
            WebLoader.Request(WEB_URL & WEB_SetPrivs)

            Dim priv As Boolean = True
            log.WriteEntry("HandlePrivUser: Privileged", EventLogEntryType.Information)
        Else
            Dim priv As Boolean = False
            WebLoader.Request(WEB_URL & WEB_LocalAdmin & "remove")
            log.WriteEntry("HandlePrivUser: Not Privileged", EventLogEntryType.Information)
        End If
    End Sub


#Region "Drives"
    Private Sub MapDrivesButton_Click(sender As Object, e As RoutedEventArgs) Handles MapDrivesButton.Click
        log.WriteEntry("MapDrives: Manual selected", EventLogEntryType.Information)
        DoMapDrives()
    End Sub

    Private Sub HandleOneDriveMapper(sender As Object, e As DoWorkEventArgs)
        log.WriteEntry("MapDrivesButton: Autorun", EventLogEntryType.Information)
        DoMapDrives()
    End Sub

    Private Sub DoMapDrives()
        MapZ()
        log.WriteEntry("DoMapDrives: Map Z", EventLogEntryType.Information)
        MapY()
        log.WriteEntry("DoMapDrives: Map Y", EventLogEntryType.Information)

        Try
            Dim glist As String = WebLoader.Request(WEB_URL & WEB_GetGroups)
            If glist.ToLower.Contains("staff") Then
                MapV()
                monitoredDrives = monitoredDrives & "V"
                log.WriteEntry("DoMapDrives: Map V", EventLogEntryType.Information)
            End If
            If glist.ToLower.Contains("slt") Then
                MapS()
                monitoredDrives = monitoredDrives & "S"
                log.WriteEntry("DoMapDrives: Map S", EventLogEntryType.Information)
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
        Dim rlist As String = ""
        Dim drive As DriveInfo
        For Each letter As Char In monitoredDrives
            drive = New DriveInfo(letter)
            If drive.IsReady Then
                rlist = rlist & letter
            Else
                Try
                    Dim flist = Directory.EnumerateFiles(letter & ":")
                Catch ex As Exception

                End Try

            End If
        Next
        WebLoader.Request(WEB_URL & WEB_RecordDrives & rlist)
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
        If (Not worked) And logging Then
            log.WriteEntry("MAPZ: " & err, EventLogEntryType.Warning)
        End If
        DoMap(New Uri("https://ashbyschool-my.sharepoint.com"), "z:", username, passwd, True)
    End Sub

    Private Sub MapY()
        Dim p As New ProcessStartInfo
        Dim username As String = Environment.UserName & "@ashbyschool.org.uk"
        Dim passwd As String = PasswordHandler.LoadPW
        p.FileName = "C:\Program Files (x86)\Ashby School\MiddlemanInstaller\ASCookieIntegrated.exe"
        p.WindowStyle = ProcessWindowStyle.Hidden
        p.UseShellExecute = False
        p.RedirectStandardOutput = True
        p.RedirectStandardError = True
        p.CreateNoWindow = True
        p.Arguments = String.Format("-s https://ashbyschool.sharepoint.com/StudentShared -u {0} -p {1} -mount y:", username, passwd)
        Dim Cookie As Process = Process.Start(p)
        Dim worked As Boolean = Cookie.WaitForExit(30000)
        Dim outline As String = Cookie.StandardOutput.ReadToEnd
        Dim err As String = Cookie.StandardError.ReadToEnd
        If (Not worked) And logging Then
            log.WriteEntry("MAPY: " & err, EventLogEntryType.Warning)
        End If
        DoMap(New Uri("https://ashbyschool.sharepoint.com/StudentShared"), "y:", username, passwd, False)
    End Sub

    Private Sub MapV()
        Dim p As New ProcessStartInfo
        Dim username As String = Environment.UserName & "@ashbyschool.org.uk"
        Dim passwd As String = PasswordHandler.LoadPW
        p.FileName = "C:\Program Files (x86)\Ashby School\MiddlemanInstaller\ASCookieIntegrated.exe"
        p.WindowStyle = ProcessWindowStyle.Hidden
        p.UseShellExecute = False
        p.RedirectStandardOutput = True
        p.RedirectStandardError = True
        p.CreateNoWindow = True
        p.Arguments = String.Format("-s https://ashbyschool.sharepoint.com/StaffShared -u {0} -p {1} -mount v:", username, passwd)
        Dim Cookie As Process = Process.Start(p)
        Dim worked As Boolean = Cookie.WaitForExit(30000)
        Dim outline As String = Cookie.StandardOutput.ReadToEnd
        Dim err As String = Cookie.StandardError.ReadToEnd
        If (Not worked) And logging Then
            log.WriteEntry("MAPV: " & err, EventLogEntryType.Warning)
        End If
        DoMap(New Uri("https://ashbyschool.sharepoint.com/StaffShared"), "v:", username, passwd, False)
    End Sub

    Private Sub MapS()
        Dim p As New ProcessStartInfo
        Dim username As String = Environment.UserName & "@ashbyschool.org.uk"
        Dim passwd As String = PasswordHandler.LoadPW
        p.FileName = "C:\Program Files (x86)\Ashby School\MiddlemanInstaller\ASCookieIntegrated.exe"
        p.WindowStyle = ProcessWindowStyle.Hidden
        p.UseShellExecute = False
        p.RedirectStandardOutput = True
        p.RedirectStandardError = True
        p.CreateNoWindow = True
        p.Arguments = String.Format("-s https://ashbyschool.sharepoint.com/SLT/SLTDocs -u {0} -p {1} -mount s:", username, passwd)
        Dim Cookie As Process = Process.Start(p)
        Dim worked As Boolean = Cookie.WaitForExit(30000)
        Dim outline As String = Cookie.StandardOutput.ReadToEnd
        Dim err As String = Cookie.StandardError.ReadToEnd
        If (Not worked) And logging Then
            log.WriteEntry("MAPS: " & err, EventLogEntryType.Warning)
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
        Process.StartInfo = New System.Diagnostics.ProcessStartInfo("cmd", cmdArgs) With {
            .CreateNoWindow = True,
            .RedirectStandardOutput = True,
            .RedirectStandardError = True,
            .UseShellExecute = False
        }
        Process.Start()

        Process.WaitForExit()

        Dim err As String = Process.StandardError.ReadToEnd
        Dim output As String = Process.StandardOutput.ReadToEnd()
        If logging Then
            log.WriteEntry("DoMap:Output: " & output, EventLogEntryType.Information)
            log.WriteEntry("DoMap:Error: " & err, EventLogEntryType.Information)
        End If
        Console.WriteLine(output)
    End Sub
#End Region

#Region "Printers"
    Private Sub MapPrinters(sender As Object, e As DoWorkEventArgs)
        Dim plist As List(Of Pinfo) = GetPrinterConnectionList()
        Dim res As String
        For Each printer As Pinfo In plist
            If printer.isDefault Then
                res = PrinterMapper.MapPrinter(printer.connectionString)
                PrinterMapper.SetDefaultPrinter(printer.connectionString)
                If Not res.Equals("OK") Then
                    log.WriteEntry(String.Format("MapPrinters: Default {0} - {1}", printer.connectionString, res), EventLogEntryType.FailureAudit)
                Else
                    log.WriteEntry("MapPrinters: Default " & printer.connectionString, EventLogEntryType.SuccessAudit)
                End If

            Else
                res = PrinterMapper.MapPrinter(printer.connectionString)
                If Not res.Equals("OK") Then
                    log.WriteEntry(String.Format("MapPrinters: {0} - {1}", printer.connectionString, res), EventLogEntryType.FailureAudit)
                Else
                    log.WriteEntry("MapPrinters:  " & printer.connectionString, EventLogEntryType.SuccessAudit)
                End If
            End If

        Next
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
#End Region

    Private Sub PasswordButton_Click(sender As Object, e As RoutedEventArgs) Handles PasswordButton.Click
        PasswordHandler.WindowStartupLocation = WindowStartupLocation.CenterScreen
        PasswordHandler.ShowDialog()

        If PasswordHandler.Status Then

            log.WriteEntry("PasswordHandler: Correct Password stored", EventLogEntryType.SuccessAudit)
        Else
            log.WriteEntry("PasswordHandler: Password not validated", EventLogEntryType.FailureAudit)
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
        log.WriteEntry("Starting Explorer", EventLogEntryType.Information)
        Threading.Thread.Sleep(5000)
        ret = WebLoader.Request(WEB_URL & WEB_HookGatekeeper)
    End Sub

End Class

Structure Pinfo
    Public connectionString As String
    Public isDefault As Boolean
End Structure
