Imports System
Imports System.DirectoryServices
Imports System.Security.Principal
Imports System.Printing
Imports System.Runtime.InteropServices
Imports System.IO
Imports System.Drawing.Printing

Module Printers
    Declare Function AddPrinterConnection Lib "winspool.drv" Alias "AddPrinterConnectionA" (ByVal pName As String) As Long
    Declare Function DeletePrinterConnection Lib "winspool.drv" Alias "DeletePrinterConnectionA" (ByVal pName As String) As Long
    Declare Function SetDefaultPrinter Lib "winspool.drv" Alias "SetDefaultPrinterA" (ByVal pszPrinter As String) As Boolean
    Declare Function GetDefaultPrinter Lib "winspool.drv" Alias "GetDefaultPrinterA" (ByVal pszBuffer() As String, ByVal pcchBuffer As Integer) As Boolean

    Public Sub AddPrinter(ByRef printer As PrinterInfo)
        AddPrinterConnection(printer.connection)
        If printer.isDefault Then
            SetDefaultPrinter(printer.connection)
        End If
    End Sub

    Public Sub DelPrinter(ByRef printer As PrinterInfo)
        DeletePrinterConnection(printer.connection)
    End Sub

    Public Sub DeleteAllNetworkPrinters()
        For Each printerConnection As String In PrinterSettings.InstalledPrinters
            If printerConnection.StartsWith("\\") Then
                DeletePrinterConnection(printerConnection)
            End If
        Next
    End Sub

    Public Sub SetLocalDefault()
        For Each printerConnection As String In PrinterSettings.InstalledPrinters
            If Not printerConnection.StartsWith("\\") Then
                SetDefaultPrinter(printerConnection)
            End If
        Next
    End Sub
End Module

Public Class PrinterInfo
    Public name As String
    Public number As Integer
    Public connection As String
    Public isDefault As Boolean
    Public isSelectable As Boolean
    Public isSelected As Boolean

    Public Overrides Function toString() As String
        If isDefault Then
            Return name & " (Default)"
        End If
        Return name
    End Function

    Public Sub New(ByVal id As Integer, named As String)
        name = named
        number = id
    End Sub

    Public Sub New()
        name = "dummy"
        number = -1
    End Sub
End Class
