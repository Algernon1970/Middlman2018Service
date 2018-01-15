Imports System.Net

Module WebLoader
    Dim browser As New WebClient
    Public Function Request(ByVal url As String) As String
        browser.Encoding = System.Text.Encoding.Unicode
        If Not browser.Headers.ToString.Contains("ASHBYSCHOOL") Then
            browser.Headers.Add("x-ashbyauth", "ASHBYSCHOOL")
        End If
        Dim ret As String = ""
        Try
            ret = browser.DownloadString(url)
        Catch ex As Exception

        End Try
        Return ret
    End Function
End Module
