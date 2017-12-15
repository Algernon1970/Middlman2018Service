Imports System.ComponentModel

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

    Dim cid As Integer
    Dim online As Boolean = False
    Private Sub GatekeeperMainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles GatekeeperMainWindow.Loaded
        Dim ret As String = ""
        Try
            Setup()
            If online Then
                AddHandler PrintMapper.DoWork, AddressOf MapPrinters
                AddHandler PrivUserMapper.DoWork, AddressOf HandlePrivUser
                PrintMapper.RunWorkerAsync()
                PrivUserMapper.RunWorkerAsync()
            End If

        Catch ex As Exception
            statusLabel.Content = "Error"
            DisplayBox.AppendText(vbCrLf & ex.Message)
        End Try
    End Sub

    Private Sub Setup()
        Dim ret As String = ""
        ret = WebLoader.Request(WEB_URL & WEB_SetName & Environment.UserName)
        ret = WebLoader.Request(WEB_URL & WEB_CheckOnline)
        statusLabel.Content = ret
        If ret.ToLower.Equals("online") Then online = True
        ret = WebLoader.Request(WEB_URL & WEB_GetComputerID)
        DisplayBox.AppendText("ComputerID = " & ret & vbCrLf)
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
End Class

Structure Pinfo
    Public connectionString As String
    Public isDefault As Boolean
End Structure
