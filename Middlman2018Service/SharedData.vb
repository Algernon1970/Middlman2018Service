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
        Return elog
    End Function

    Public Function AmIOnDomain() As String
        Try
            ComputerTableAdapter.Connection.Open()
        Catch ex As Exception
            Return "Offline"
        End Try

        Dim serverv As String = ComputerTableAdapter.Connection.ServerVersion
        ComputerTableAdapter.Connection.Close()
        Return serverv
    End Function

End Module

