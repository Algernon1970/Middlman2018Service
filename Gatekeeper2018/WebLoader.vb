Imports System.Net

Module WebLoader
    Dim browser As New WebClient
    Public Function Request(ByVal url As String) As String
        browser.Encoding = System.Text.Encoding.Unicode
        If Not browser.Headers.ToString.Contains("ASHBYSCHOOL") Then
            browser.Headers.Add("x-ashbyauth", "ASHBYSCHOOL")
        End If
        Return browser.DownloadString(url)
    End Function
End Module
