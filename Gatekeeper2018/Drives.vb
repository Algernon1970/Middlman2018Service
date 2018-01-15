Imports System.Runtime.InteropServices

Module Drives

    Public Declare Function WNetAddConnection2 Lib "mpr.dll" Alias "WNetAddConnection2A" _
(ByRef lpNetResource As NETRESOURCE, ByVal lpPassword As String,
ByVal lpUserName As String, ByVal dwFlags As Integer) As Integer

    Public Declare Function WNetCancelConnection2 Lib "mpr" Alias "WNetCancelConnection2A" _
(ByVal lpName As String, ByVal dwFlags As Integer, ByVal fForce As Integer) As Integer

    <StructLayout(LayoutKind.Sequential)>
    Public Structure NETRESOURCE
        Public dwScope As Integer
        Public dwType As Integer
        Public dwDisplayType As Integer
        Public dwUsage As Integer
        Public lpLocalName As String
        Public lpRemoteName As String
        Public lpComment As String
        Public lpProvider As String
    End Structure

    Public Const ForceDisconnect As Integer = 1
    Public Const RESOURCETYPE_DISK As Long = &H1

    Public Function MapDrive(ByVal DriveLetter As String, ByVal UNCPath As String, user As String, pass As String) As Boolean

        Dim nr As NETRESOURCE

        nr = New NETRESOURCE
        nr.lpRemoteName = UNCPath
        nr.lpLocalName = DriveLetter & ":"
        nr.dwType = RESOURCETYPE_DISK

        Dim result As Integer
        result = WNetAddConnection2(nr, user, pass, 0)

        If result = 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function UnMapDrive(ByVal DriveLetter As String) As Boolean
        Dim rc As Integer
        rc = WNetCancelConnection2(DriveLetter & ":", 0, ForceDisconnect)

        If rc = 0 Then
            Return True
        Else
            Return False
        End If

    End Function


End Module
