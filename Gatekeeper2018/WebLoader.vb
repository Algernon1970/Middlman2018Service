Imports System.Net

Module WebLoader

    Public Function Request(ByVal url As String) As String
        Dim browser As New WebClient
        browser.Encoding = System.Text.Encoding.UTF8
        If Not browser.Headers.ToString.Contains("ASHBYSCHOOL") Then
            browser.Headers.Add("x-ashbyauth", "ASHBYSCHOOL")
        End If
        Dim ret As String = ""
        Try
            ret = browser.DownloadString(url)
            'Log(String.Format("Request: {0}", url), EventLogEntryType.Information)
        Catch ex As Exception
            Log(String.Format("Request ERROR: {0} - {1}", url, ex.Message), EventLogEntryType.Error)
        End Try
        Return ret
    End Function
End Module
