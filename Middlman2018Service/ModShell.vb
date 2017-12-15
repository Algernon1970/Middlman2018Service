Imports System.Runtime.InteropServices

Public Module ModShell

    <StructLayout(LayoutKind.Sequential)>
    Public Structure STARTUPINFO
        Public cb As Integer
        Public lpReserved As String
        Public lpDesktop As String
        Public lpTitle As String
        Public dwX As Integer
        Public dwY As Integer
        Public dwXSize As Integer
        Public dwYSize As Integer
        Public dwXCountChars As Integer
        Public dwYCountChars As Integer
        Public dwFillAttribute As Integer
        Public dwFlags As Integer
        Public wShowWindow As Short
        Public cbReserved2 As Short
        Public lpReserved2 As Integer
        Public hStdInput As Integer
        Public hStdOutput As Integer
        Public hStdError As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure PROCESS_INFORMATION
        Public hProcess As IntPtr
        Public hThread As IntPtr
        Public dwProcessId As Integer
        Public dwThreadId As Integer
    End Structure

    Public Declare Unicode Function CreateProcessWithLogonW Lib "Advapi32" (ByVal lpUsername As String, ByVal lpDomain As String, ByVal lpPassword As String, ByVal dwLogonFlags As Int32, ByVal lpApplicationName As String, ByVal lpCommandLine As String, ByVal dwCreationFlags As Int32, ByVal lpEnvironment As IntPtr, ByVal lpCurrentDirectory As String, ByRef si As STARTUPINFO, ByRef pi As PROCESS_INFORMATION) As Integer
    Public Declare Function CloseHandle Lib "kernel32" (ByVal hObject As IntPtr) As Integer

    Public Const LOGON_WITH_PROFILE As Int32 = &H1

    Public Const NORMAL_PRIORITY_CLASS As Int32 = &H20&

    Public Const STARTF_USESHOWWINDOW As Int32 = &H1
    Public Const SW_HIDE As Int16 = 0
    Public Const SW_SHOW As Int16 = 5

    Public Function Shell(ByVal strCmdLine As String, ByVal strCurrentDirectory As String) As Boolean

        Dim pi As PROCESS_INFORMATION
        Dim si As New STARTUPINFO

        si.cb = Marshal.SizeOf(si)
        si.dwFlags = STARTF_USESHOWWINDOW
        si.wShowWindow = SW_SHOW

        Dim result As Integer = CreateProcessWithLogonW("username", "as", "password", 0, vbNullString, strCmdLine, NORMAL_PRIORITY_CLASS, IntPtr.Zero, strCurrentDirectory, si, pi)

        If result <> 0 Then
            Call CloseHandle(pi.hThread)
            Call CloseHandle(pi.hProcess)
        Else
            Return False
        End If

        Return True

    End Function

End Module
