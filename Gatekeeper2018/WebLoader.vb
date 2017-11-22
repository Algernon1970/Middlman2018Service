Imports System.Net

Module WebLoader
    Dim browser As New WebClient
    Public Function Request(ByVal url As String) As String
        browser.Encoding = System.Text.Encoding.Unicode
        browser.Headers.Add("x-ashbyauth", "ASHBYSCHOOL")
        Return browser.DownloadString(url)
    End Function
End Module
