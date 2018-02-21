Imports System.Net.NetworkInformation

Module SharedData
    Public ComputerTableAdapter As New ZuulDataSetTableAdapters.Tbl_ComputerTableAdapter
    Public PrinterLinkTableAdapter As New ZuulDataSetTableAdapters.Lnk_ComputerPrinterTableAdapter
    Public PrinterTableAdapter As New ZuulDataSetTableAdapters.Tbl_PrinterTableAdapter
    Public PersonLinkTableAdapter As New ZuulDataSetTableAdapters.Lnk_ComputerPersonTableAdapter
    Public PersonTableAdapter As New ZuulDataSetTableAdapters.Tbl_PersonTableAdapter

    Public online As String = "Needs to be coded"
    Public currentUser As String = "Noone"
    Public currentUserSid As String = "0"
    Public mappedDrives As String = ""
    Public eLog As EventLog = Nothing

    Public Function GetLogger() As EventLog
        If eLog Is Nothing Then
            eLog = New EventLog
        End If
        Try
            If Not EventLog.SourceExists("Middle") Then
                EventLog.CreateEventSource("Middle", "GK2018")
            End If
        Catch ex As Exception
            Return Nothing
        End Try
        eLog.Source = "Middle"
        Return eLog
    End Function

    Public Function AmIOnDomain() As String
        Dim pinger As New Ping
        Try
            If IsNetworkAvailable() Then
                Dim reply As PingReply = pinger.Send("svr-dca", 2000)
                If reply.Status = IPStatus.Success Then
                    online = True
                    Return "Ashby Domain"
                Else
                    online = False
                    Return "Not Ashby Domain"

                End If
            Else
                online = False
                Return "No Network"
            End If

        Catch ex As Exception
            online = False
            Return "Failed check"
        End Try
    End Function

    Private Function IsNetworkAvailable() As Boolean
        Dim log As EventLog = GetLogger()
        log.WriteEntry("Starting IsNetworkAvailable", EventLogEntryType.Warning)
        If NetworkInterface.GetIsNetworkAvailable Then
            For Each face As NetworkInterface In NetworkInterface.GetAllNetworkInterfaces
                If face.OperationalStatus = OperationalStatus.Up Then
                    If (Not face.NetworkInterfaceType = NetworkInterfaceType.Tunnel) And (Not face.NetworkInterfaceType = NetworkInterfaceType.Loopback) Then
                        If face.GetIPv4Statistics.BytesReceived > 0 And face.GetIPv4Statistics.BytesSent > 0 Then
                            log.WriteEntry("Finished IsNetworkAvailable - true", EventLogEntryType.Warning)
                            Return True
                        End If
                    End If
                End If
            Next

        End If
        log.WriteEntry("Finished IsNetworkAvailable - false", EventLogEntryType.Warning)
        Return False
    End Function

End Module

