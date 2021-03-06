﻿Imports System.ComponentModel
Imports System.Drawing.Printing
Imports System.IO
Imports GatekeeperTools
Imports Hardcodet.Wpf.TaskbarNotification
Imports System.Media.SoundPlayer

Class MainWindow

#Region "Callbacks"
    Dim tn As New ToastBox

    Private Sub Toast(ByVal title As String, ByVal msg As String)
        Application.Current.Dispatcher.Invoke(Sub()
                                                  tn.Show("Alarm", msg)
                                              End Sub)
    End Sub

    Private Function IsAlarmAllowed() As Boolean?
        Return Application.Current.Dispatcher.Invoke(Function()
                                                         Return AlarmsCheck.IsChecked
                                                     End Function)
    End Function
#End Region

#Region "Threads"
    Private ReadOnly PrintMapper As New BackgroundWorker()
    Private ReadOnly PrivUserMapper As New BackgroundWorker()
    Private ReadOnly OneDriveMapper As New BackgroundWorker()
    Private ReadOnly DriveMonitor As New BackgroundWorker()
    Private ReadOnly AlarmMonitor As New BackgroundWorker()
    Private ReadOnly Explorer As New BackgroundWorker()
    Private WithEvents AutoLogoutTimer As New BackgroundWorker()

#End Region
#Region "State"
    Private PasswordHandler As New Password
    Private logging As Boolean = False

    Dim cid As Integer
    Dim online As Boolean = False
    Dim monitoredDrives As String = "ZY"
    Dim timerOn As Boolean = True
    Dim onlinestatus As String = "none"
    Dim alarmsList As New List(Of AlarmInfo)
    Dim alarmsArray As AlarmInfo()
    Dim toastWindow As New ToastBox()

#End Region

    Private Sub BGProgress(ByVal sender As Object, e As ProgressChangedEventArgs) Handles AutoLogoutTimer.ProgressChanged
        If e.ProgressPercentage = 100 Then
            WebLoader.Request(WEB_URL & WEB_Logout)
        End If
        AutoLogoutBar.Value = 100 - e.ProgressPercentage
    End Sub

#Region "Startup"
    Private Sub GatekeeperMainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles GatekeeperMainWindow.Loaded
        Dim ret As String = ""

        Setup()
        AddHandler PrintMapper.DoWork, AddressOf MapPrinters
        AddHandler PrivUserMapper.DoWork, AddressOf HandlePrivUser
        AddHandler OneDriveMapper.DoWork, AddressOf HandleOneDriveMapper
        AddHandler OneDriveMapper.RunWorkerCompleted, AddressOf MapDrivesComplete
        AddHandler DriveMonitor.DoWork, AddressOf HandleDriveMonitor
        AddHandler AlarmMonitor.DoWork, AddressOf HandleAlarmMonitor
        AddHandler Explorer.DoWork, AddressOf HandleExplorerStart
        AddHandler AutoLogoutTimer.DoWork, AddressOf AutoLogoutTimerThread

        AutoLogoutTimer.WorkerReportsProgress = True
        AutoLogoutTimer.RunWorkerAsync()
        OneDriveMapper.RunWorkerAsync()
        DriveMonitor.RunWorkerAsync()
        AlarmMonitor.RunWorkerAsync()

        If online Then
            PrintMapper.RunWorkerAsync()
            PrivUserMapper.RunWorkerAsync()
        End If
    End Sub

    Private Sub ReportBroken()
        Dim brokenWindow As New NoServiceWindow
        Me.Hide()
        brokenWindow.Show()
    End Sub

    Private Sub Setup()
        MapDrivesButton.Header = "Mapping Drives..."
        MapDrivesButton.IsEnabled = False
        PasswordButton.Header = "Offline"
        PasswordButton.IsEnabled = False
        AddPrinterButton.Visibility = Visibility.Hidden
        AddPrinterButton.IsEnabled = False
        NotifyIcon.Icon = My.Resources.whitecloud
        Dim pwdString As String = "none"
        pwdString = PasswordHandler.LoadPW
        Dim ret As String = ""
        Try
            ret = WebLoader.Request(WEB_URL & WEB_GetVersion)
            If ret.Equals("ConnectionError") Then
                ReportBroken()
            End If
            VersionLabel.Content = String.Format("{0}", ret)
            Dim cversion As String = WebLoader.Request(WEB_URL & WEB_GetCurrentVersion)

            VersionDisplay.Content = String.Format("{0}{1}Latest {2}", ret, vbCrLf, cversion)
            ret = WebLoader.Request(WEB_URL & WEB_CheckOnline)
            statusLabel.Content = ret
            onlinestatus = ret
            If ret.ToLower.Equals("ashby domain") Then
                Log("Setup: Showing online", EventLogEntryType.Warning)
                online = True
                PasswordButton.IsEnabled = True
                PasswordButton.Header = "Cloud Drives Password"
                HandlePassword(pwdString)
                ret = WebLoader.Request(WEB_URL & WEB_GetComputerID)
                ret = WebLoader.Request(WEB_URL & WEB_CopyMOTD)
            Else
                Log("Setup: Showing OFFLINE", EventLogEntryType.Error)
            End If
            ret = WebLoader.Request(WEB_URL & WEB_SetName & Environment.UserName)
            Dim path As String = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) & "\Ashby School\motd.rtf"
            If File.Exists(path) Then
                Dim range As TextRange
                range = New TextRange(DisplayBox.Document.ContentStart, DisplayBox.Document.ContentEnd)
                Using fstream As New FileStream(path, FileMode.Open)
                    range.Load(fstream, DataFormats.Rtf)
                End Using
            End If

            LoadAlarms()
        Catch ex As Exception
            DisplayBox.AppendText("Middleman not available. Contact Network Services." & vbCrLf)
            AcceptButton.IsEnabled = False
            DisplayBox.Background = Brushes.Red
            Log("SETUP: " & ex.Message, EventLogEntryType.Error)
        End Try
    End Sub
#End Region

    Public Sub LoadAlarms()
        Try
            alarmsList.Clear()
            Dim alarm As AlarmInfo
            Dim ret As String = WebLoader.Request(WEB_URL & WEB_LoadAlarms)
            Dim alarmFields() As String
            Dim alarmParts() As String = ret.Split(";"c)
            For Each individualAlarm In alarmParts
                alarm = New AlarmInfo
                alarmFields = individualAlarm.Split(","c)
                alarm.name = alarmFields(0)
                alarm.time = alarmFields(1)
                alarm.sounder = True
                alarm.soundfile = alarmFields(2)
                alarmsList.Add(alarm)
            Next
            alarmsArray = alarmsList.ToArray()
        Catch ex As Exception

        End Try

    End Sub

#Region "Handlers"
    Private Sub HandlePassword(pwdString As String)
        Try
            If Not PasswordHandler.CheckPW(pwdString) Then
                PasswordHandler.WindowStartupLocation = WindowStartupLocation.CenterScreen
                Me.Visibility = Visibility.Hidden
                PasswordHandler.ShowDialog()
                Me.Visibility = Visibility.Visible
            End If
        Catch ex As Exception
            PasswordHandler.WindowStartupLocation = WindowStartupLocation.CenterScreen
            Me.Visibility = Visibility.Hidden
            PasswordHandler.ShowDialog()
            Me.Visibility = Visibility.Visible
        End Try
    End Sub

    Private Sub AutoLogoutTimerThread(sender As Object, e As DoWorkEventArgs)
        For countdown = 0 To 100
            If Not timerOn Then
                Return
            End If
            AutoLogoutTimer.ReportProgress(countdown)
            Threading.Thread.Sleep(1000)
        Next
    End Sub

    Private Sub HandlePrivUser(sender As Object, e As DoWorkEventArgs)
        Dim ret As String = WebLoader.Request(WEB_URL & WEB_GPUpdate)
        ret = WebLoader.Request(WEB_URL & WEB_IsPrivileged)
        If ret.Equals("True") Then
            If online Then WebLoader.Request(WEB_URL & WEB_CopyPrivFile)
            WebLoader.Request(WEB_URL & WEB_LocalAdmin & "add")
            WebLoader.Request(WEB_URL & WEB_SetPrivs)

            Dim priv As Boolean = True
            Log("HandlePrivUser: Privileged", EventLogEntryType.Information)
        Else
            Dim priv As Boolean = False
            WebLoader.Request(WEB_URL & WEB_LocalAdmin & "remove")
            Log("HandlePrivUser: Not Privileged", EventLogEntryType.Information)
        End If
    End Sub

    Private Sub HandleExplorerStart(sender As Object, e As DoWorkEventArgs)
        Dim ret As String = ""
        ret = WebLoader.Request(WEB_URL & WEB_UnHookGatekeeper)
        Threading.Thread.Sleep(1000)
        Process.Start("C:\windows\explorer.exe")
        Log("Starting Explorer", EventLogEntryType.Information)
        Threading.Thread.Sleep(5000)
        ret = WebLoader.Request(WEB_URL & WEB_HookGatekeeper)
    End Sub
#End Region

#Region "Drives"
    Private Sub MapDrivesButton_Click(sender As Object, e As RoutedEventArgs) Handles MapDrivesButton.Click
        MapDrivesButton.Header = "Mapping Drives..."
        MapDrivesButton.IsEnabled = False
        Log("MapDrives: Manual selected", EventLogEntryType.Information)
        OneDriveMapper.WorkerReportsProgress = True
        OneDriveMapper.RunWorkerAsync()
    End Sub

    Private Sub MapDrivesComplete(ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs)
        Log("MapDrives: Complete", EventLogEntryType.Information)
        MapDrivesButton.IsEnabled = True
        MapDrivesButton.Header = "Map Drives"
    End Sub

    Private Sub HandleOneDriveMapper(sender As Object, e As DoWorkEventArgs)
        Dim bg As BackgroundWorker = CType(sender, BackgroundWorker)
        Log("MapDrivesButton: Autorun", EventLogEntryType.Information)
        DoMapDrives()
    End Sub

    Private Sub DoMapDrives()
        If onlinestatus.ToLower.Contains("ashby domain") Then
            Dim passwd As String = PasswordHandler.LoadPW
            If passwd.Equals("NOPWD") Then
                Log("DoMapDrives: Skipping Mapping Drives.  No password specified (offline)", EventLogEntryType.Warning)
                Return
            End If
            MapZ()
            Log("DoMapDrives: Map Z", EventLogEntryType.Information)
            MapY()
            Log("DoMapDrives: Map Y", EventLogEntryType.Information)

            Try
                Dim glist As String
                If online Then
                    glist = WebLoader.Request(WEB_URL & WEB_GetGroups)
                    WriteGroups(glist)
                Else
                    glist = ReadGroups()
                End If

                If glist.ToLower.Contains("staff") Then
                    MapV()
                    monitoredDrives = monitoredDrives & "V"
                    Log("DoMapDrives: Map V", EventLogEntryType.Information)
                End If
                If glist.ToLower.Contains("slt") Then
                    MapS()
                    monitoredDrives = monitoredDrives & "S"
                    Log("DoMapDrives: Map S", EventLogEntryType.Information)
                End If
            Catch ex As Exception
                Log("DoMapDrives: Error " & ex.Message, EventLogEntryType.Error)
            End Try
        Else
            Log("MapDrivesButton: Not done, offline", EventLogEntryType.Information)
        End If
    End Sub

    Private Sub WriteGroups(ByRef glist As String)
        Dim path As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) & "\Ashby School\"
        Dim encoder As New Simple3Des(EncryptionPW)
        Dim encGList As String = encoder.EncryptData(glist)
        If Not Directory.Exists(path) Then
            Directory.CreateDirectory(path)
        End If

        Using w As New StreamWriter(File.Open(path & "groups.ash", FileMode.Create))
            w.WriteLine(encGList)
        End Using
    End Sub

    Private Function ReadGroups() As String
        Dim path As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) & "\Ashby School\"
        If File.Exists(path & "groups.ash") Then
            Dim w As New StreamReader(File.Open(path & "groups.ash", FileMode.Open))
            Dim encoded As String = w.ReadLine()
            Dim decoder As New Simple3Des(EncryptionPW)
            w.Close()
            w.Dispose()

            Return decoder.DecryptData(encoded)
        End If
        Return "NOPWD"
    End Function

    Private Sub HandleDriveMonitor(sender As Object, e As DoWorkEventArgs)
        While True
            MonitorDrives()

            Threading.Thread.Sleep(5000)
        End While
    End Sub

    Private Sub HandleAlarmMonitor(sender As Object, e As DoWorkEventArgs)
        Dim count As Integer = 0
        While True
            count = count + 1
            If count > 8 Then
                count = 0
                LoadAlarms()
            End If
            MonitorAlarms()
            Threading.Thread.Sleep(15000)
        End While

    End Sub

    Private Sub MonitorAlarms()
        Dim tNow As Date = Date.Parse(Now.ToShortTimeString)
        Dim cTime As Date
        Dim diff As TimeSpan
        Dim alarm As AlarmInfo

        For x As Integer = 0 To alarmsList.Count - 1
            alarm = alarmsArray(x)
            If alarm.sounder Then
                cTime = Date.Parse(alarm.time)
                If tNow = cTime Then
                    diff = tNow - cTime
                    PlayAlarm("Alarm", alarm.name, alarm.soundfile)
                    alarm.sounder = False
                    alarmsArray(x).sounder = False

                End If
            End If
        Next
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
            'NotifyIcon.ShowBalloonTip("Drive Mapper", "Not all drives mapped.", BalloonIcon.Error)
        End If
    End Sub

    Private Sub MapZ()
        Log("MAPZ: running", EventLogEntryType.Information)
        Dim outline As String = ""
        Dim p As New ProcessStartInfo
        Dim username As String = Environment.UserName & "@ashbyschool.org.uk"
        Dim passwd As String = PasswordHandler.LoadPWE
        If Environment.Is64BitOperatingSystem Then
            p.FileName = "C:\Program Files (x86)\Ashby School\MiddlemanInstaller\ASCookieIntegrated.exe"
        Else
            p.FileName = "C:\Program Files\Ashby School\MiddlemanInstaller\ASCookieIntegrated.exe"
        End If

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
            Log("MAPZ: " & err, EventLogEntryType.Warning)
        End If
        Log("MAPZ: Got Cookie", EventLogEntryType.Information)
        DoMap(New Uri("https://ashbyschool-my.sharepoint.com"), "z:", username, passwd, True)
        DoMap(New Uri("https://ashbyschool-my.sharepoint.com"), "z:", username, passwd, True)
        Log("MAPZ: Finished", EventLogEntryType.Information)
    End Sub

    Private Sub MapY()
        Log("MAPY: running", EventLogEntryType.Information)
        Dim p As New ProcessStartInfo
        Dim username As String = Environment.UserName & "@ashbyschool.org.uk"
        Dim passwd As String = PasswordHandler.LoadPWE
        If Environment.Is64BitOperatingSystem Then
            p.FileName = "C:\Program Files (x86)\Ashby School\MiddlemanInstaller\ASCookieIntegrated.exe"
        Else
            p.FileName = "C:\Program Files\Ashby School\MiddlemanInstaller\ASCookieIntegrated.exe"
        End If
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
            Log("MAPY: " & err, EventLogEntryType.Warning)
        End If
        DoMap(New Uri("https://ashbyschool.sharepoint.com/StudentShared"), "y:", username, passwd, False)
        Log("MAPY: Finished", EventLogEntryType.Information)
    End Sub

    Private Sub MapV()
        Log("MAPV: running", EventLogEntryType.Information)
        Dim p As New ProcessStartInfo
        Dim username As String = Environment.UserName & "@ashbyschool.org.uk"
        Dim passwd As String = PasswordHandler.LoadPWE
        If Environment.Is64BitOperatingSystem Then
            p.FileName = "C:\Program Files (x86)\Ashby School\MiddlemanInstaller\ASCookieIntegrated.exe"
        Else
            p.FileName = "C:\Program Files\Ashby School\MiddlemanInstaller\ASCookieIntegrated.exe"
        End If
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
            Log("MAPV: " & err, EventLogEntryType.Warning)
        End If
        DoMap(New Uri("https://ashbyschool.sharepoint.com/StaffShared"), "v:", username, passwd, False)
        Log("MAPV: Finished", EventLogEntryType.Information)
    End Sub

    Private Sub MapS()
        Log("MAPS: running", EventLogEntryType.Information)
        Dim p As New ProcessStartInfo
        Dim username As String = Environment.UserName & "@ashbyschool.org.uk"
        Dim passwd As String = PasswordHandler.LoadPWE
        If Environment.Is64BitOperatingSystem Then
            p.FileName = "C:\Program Files (x86)\Ashby School\MiddlemanInstaller\ASCookieIntegrated.exe"
        Else
            p.FileName = "C:\Program Files\Ashby School\MiddlemanInstaller\ASCookieIntegrated.exe"
        End If
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
            Log("MAPS: " & err, EventLogEntryType.Warning)
        End If
        DoMap(New Uri("https://ashbyschool.sharepoint.com/SLT/SLTDocs"), "s:", username, passwd, False)
        Log("MAPS: Finished", EventLogEntryType.Information)
    End Sub

    Private Sub DoMap(sharepointUri As Uri, disk As String, username As String, passwd As String, mount As Boolean)
        If logging Then
            Log(String.Format("DoMap: {0} running", disk), EventLogEntryType.Information)
        End If
        Dim homedir As String = ""
        If mount Then
            Dim user As String = username.Split(CType("@", Char()))(0)
            Dim domain As String = username.Split(CType("@", Char()))(1)
            homedir = "DavWWWRoot\\personal\\" + user + "_" + domain.Split(CType(".", Char()))(0) + "_" + domain.Split(CType(".", Char()))(1) + "_" + domain.Split(CType(".", Char()))(2) + "\\Documents"
        End If

        UnMap(disk)

        Dim cmdArgs As String = "/c net use " + disk + " \\\\" + sharepointUri.Host + "@ssl" + sharepointUri.PathAndQuery.Replace("/", "\\") + homedir
        cmdArgs = cmdArgs.Replace("\\", "\")

        Dim Process As Process = New Process With {
            .StartInfo = New System.Diagnostics.ProcessStartInfo("cmd", cmdArgs) With {
            .CreateNoWindow = True,
            .RedirectStandardOutput = True,
            .RedirectStandardError = True,
            .UseShellExecute = False
        }
        }
        Process.Start()

        Dim worked As Boolean = Process.WaitForExit(10000)
        If logging Then
            Log(String.Format("DoMap: {0}", If(worked, "Worked", "Didnt Work")), EventLogEntryType.Information)
        End If
        Dim err As String = Process.StandardError.ReadToEnd
        Dim output As String = Process.StandardOutput.ReadToEnd()
        If logging Then
            Log("DoMap:Output: " & output, EventLogEntryType.Information)
            Log("DoMap:Error: " & err, EventLogEntryType.Information)
        End If
    End Sub

    Private Sub UnMap(disk As String)
        Dim cmdArgs As String = "/c net use " + disk + " /delete"

        Dim Process As Process = New Process With {
            .StartInfo = New System.Diagnostics.ProcessStartInfo("cmd", cmdArgs) With {
            .CreateNoWindow = True,
            .RedirectStandardOutput = True,
            .RedirectStandardError = True,
            .UseShellExecute = False
        }
        }
        Process.Start()

        Process.WaitForExit()

        Dim err As String = Process.StandardError.ReadToEnd
        Dim output As String = Process.StandardOutput.ReadToEnd()
        If logging Then
            Log("UnMap:Output: " & output, EventLogEntryType.Information)
            Log("UnMap:Error: " & err, EventLogEntryType.Information)
        End If
    End Sub
#End Region

#Region "Printers"
    Private Sub MapPrinters(sender As Object, e As DoWorkEventArgs)
        'remove stale printers
        For Each installedPrinter As String In PrinterSettings.InstalledPrinters
            If installedPrinter.StartsWith("\\") Then
                UnMapPrinter(installedPrinter)
            End If
        Next
        Dim plist As List(Of Pinfo) = GetPrinterConnectionList()
        Dim res As String
        For Each printer As Pinfo In plist
            If printer.isDefault Then
                res = PrinterMapper.MapPrinter(printer.connectionString)
                PrinterMapper.SetDefaultPrinter(printer.connectionString)
                If Not res.Equals("OK") Then
                    Log(String.Format("MapPrinters: Default {0} - {1}", printer.connectionString, res), EventLogEntryType.FailureAudit)
                Else
                    Log("MapPrinters: Default " & printer.connectionString, EventLogEntryType.SuccessAudit)
                End If

            Else
                res = PrinterMapper.MapPrinter(printer.connectionString)
                If Not res.Equals("OK") Then
                    Log(String.Format("MapPrinters: {0} - {1}", printer.connectionString, res), EventLogEntryType.FailureAudit)
                Else
                    Log("MapPrinters:  " & printer.connectionString, EventLogEntryType.SuccessAudit)
                End If
            End If

        Next
    End Sub

    Private Function GetPrinterConnectionList() As List(Of Pinfo)
        Dim ret As String
        Dim plist As New List(Of Pinfo)
        Dim cs As String
        Dim pdefault As Boolean
        'Log("Calling GetPrinterList()", EventLogEntryType.Error)
        ret = WebLoader.Request(WEB_URL & WEB_GetPrinterList)
        'Log(String.Format("GetPrinterList finished: {0}", ret), EventLogEntryType.Information)
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

#Region "UI"
    Private Sub AddPrinters_Click(sender As Object, e As RoutedEventArgs) Handles AddPrinterButton.Click
        Dim PC As New PrinterChooser
        PC.Show()
    End Sub

    Private Sub PasswordButton_Click(sender As Object, e As RoutedEventArgs) Handles PasswordButton.Click
        Dim ret As String = WebLoader.Request(WEB_URL & WEB_CheckOnline)
        If ret.ToLower.Equals("ashby domain") Then
            PasswordHandler.WindowStartupLocation = WindowStartupLocation.CenterScreen
            PasswordHandler.ShowDialog()

            If PasswordHandler.Status Then
                Log("PasswordHandler: Correct Password stored", EventLogEntryType.SuccessAudit)
            Else
                Log("PasswordHandler: Password not validated", EventLogEntryType.FailureAudit)
            End If
        Else
            PasswordButton.Header = "Offline"
            PasswordButton.IsEnabled = False
        End If
    End Sub

    Private Sub GatekeeperMainWindow_Closed(sender As Object, e As EventArgs) Handles GatekeeperMainWindow.Closed
        Log("Gatekeeper Closed", EventLogEntryType.Error)
    End Sub

    Private Sub AcceptButton_Click(sender As Object, e As RoutedEventArgs) Handles AcceptButton.Click
        timerOn = False
        Dim ret As String
        Me.Visibility = Visibility.Hidden
        ret = WebLoader.Request(WEB_URL & WEB_MusicRedirect)
        ret = WebLoader.Request(WEB_URL & WEB_GetGroups)
        If ret.Contains("Domain Admins") Or (ret.Contains("Staff") And IsLaptop()) Then
            AddPrinterButton.Visibility = Visibility.Visible
            AddPrinterButton.IsEnabled = True
        End If
        Explorer.RunWorkerAsync()
    End Sub

    Private Sub DeclineButton_Click(sender As Object, e As RoutedEventArgs) Handles DeclineButton.Click
        timerOn = False
        WebLoader.Request(WEB_URL & WEB_Logout)
    End Sub
#End Region

    Private Function IsLaptop() As Boolean
        Dim ret As String = WebLoader.Request(WEB_URL & WEB_IsLaptop)
        Return If(ret.Equals("True"), True, False)

    End Function

    Private Sub PlayAlarm(ByVal alarmTitle As String, ByVal alarmName As String, ByVal alarmSound As String)
        If IsAlarmAllowed() Then
            Toast(alarmTitle, alarmName)
            Dim sp As New Media.SoundPlayer("C:\windows\media\alarm10.wav")
            sp.Play()
        End If
    End Sub
End Class

Structure Pinfo
    Public connectionString As String
    Public isDefault As Boolean
End Structure

Structure AlarmInfo
    Public name As String
    Public time As String
    Public sounder As Boolean
    Public soundfile As String
End Structure
