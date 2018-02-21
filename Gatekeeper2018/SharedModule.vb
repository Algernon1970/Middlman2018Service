Module SharedModule
    Public eLog As EventLog = Nothing
    Public Function GetLogger() As EventLog
        If eLog Is Nothing Then
            eLog = New EventLog
        End If
        Try
            If Not EventLog.SourceExists("GK2018") Then
                EventLog.CreateEventSource("GK2018", "GK2018")
            End If
        Catch ex As Exception
            Return Nothing
        End Try
        eLog.Source = "GK2018"
        Return eLog
    End Function

    Public Sub Log(ByVal message As String, ByVal type As EventLogEntryType)
        If eLog Is Nothing Then
            eLog = GetLogger()
            If eLog Is Nothing Then
                Return
            Else
            End If
        End If
        eLog.WriteEntry(message, type)
    End Sub
End Module
