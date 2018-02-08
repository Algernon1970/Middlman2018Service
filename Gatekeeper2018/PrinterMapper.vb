

Module PrinterMapper
    Declare Function AddPrinterConnection Lib "winspool.drv" Alias "AddPrinterConnectionA" (ByVal pName As String) As Integer
    Declare Function DeletePrinterConnection Lib "winspool.drv" Alias "DeletePrinterConnectionA" (ByVal pName As String) As Long
    Declare Function _SetDefaultPrinter Lib "winspool.drv" Alias "SetDefaultPrinterA" (ByVal pszPrinter As String) As Boolean
    Declare Function GetDefaultPrinter Lib "winspool.drv" Alias "GetDefaultPrinterA" (ByVal pszBuffer() As String, ByVal pcchBuffer As Integer) As Boolean

    Public Function MapPrinter(ByVal connectionString As String) As String
        Dim ret As String = "KO"
        For retries = 0 To 5
            If AddPrinterConnection(connectionString) <> 0 Then
                ret = "OK"
                Exit For
            End If
            Threading.Thread.Sleep(1000)
            ret = Err.LastDllError.ToString
        Next
        Return ret
    End Function

    Public Sub SetDefaultPrinter(ByVal connectionString As String)
        _SetDefaultPrinter(connectionString)
    End Sub
End Module
