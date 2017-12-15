Imports System.Runtime.InteropServices

Module Module1
    Declare Function AddPrinterConnection Lib "winspool.drv" Alias "AddPrinterConnectionA" (ByVal pName As String) As Integer
    Declare Function DeletePrinterConnection Lib "winspool.drv" Alias "DeletePrinterConnectionA" (ByVal pName As String) As Long
    Declare Function SetDefaultPrinter Lib "winspool.drv" Alias "SetDefaultPrinterA" (ByVal pszPrinter As String) As Boolean
    Declare Function GetDefaultPrinter Lib "winspool.drv" Alias "GetDefaultPrinterA" (ByVal pszBuffer() As String, ByVal pcchBuffer As Integer) As Boolean

    <DllImport("user32.dll", SetLastError:=True)> Private Function LockWorkStation() As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    Dim retryCounter As Integer = 5

    Sub Main()
        For Each arg As String In My.Application.CommandLineArgs
            If arg.Equals("LOCK") Then
                LockWorkStation()
            End If

            If arg.Equals("GPUPDATE") Then
                Try
                    Dim refresher As Process = New Process()
                    refresher.StartInfo.Arguments = "/target:user /wait:-1"
                    refresher.StartInfo.FileName = "c:\windows\system32\gpupdate.exe"
                    refresher.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
                    refresher.Start()
                    refresher.WaitForExit()
                Catch ex As Exception

                End Try
            End If

            If arg.Equals("PRINTER") Then
                Dim pinfo As New PrinterInfo
                Dim parg() As String = arg.Substring(10).Split(","c)
                pinfo.name = parg(0)
                pinfo.connection = parg(1)
                If parg(2).Equals("DEFAULT") Then
                    pinfo.isDefault = True
                Else
                    pinfo.isDefault = False
                End If
                AddPrinter(pinfo)
            End If
        Next

    End Sub

    Public Sub AddPrinter(ByRef printer As printerInfo)
        Dim ret As Integer = 0
        ret = AddPrinterConnection(printer.connection)
        If ret = 0 Then
            Threading.Thread.Sleep(10000)
            retrycounter = retrycounter - 1
            If retrycounter > 0 Then
                AddPrinter(printer)
            End If
        Else
            retrycounter = 5
        End If
        If printer.isDefault Then
            SetDefaultPrinter(printer.connection)
        End If

    End Sub

End Module

Public Structure PrinterInfo
    Dim name As String
    Dim connection As String
    Dim isDefault As Boolean
End Structure
