Imports Microsoft.Win32
Module RegEdit

    Public Function ReadReg(ByVal path As String) As RegInfo
        Dim reg As RegInfo = ParseRegPath(path)
        Dim rKey As RegistryKey

        Dim reg3264 As RegistryView
        If Environment.Is64BitOperatingSystem Then
            reg3264 = RegistryView.Registry64
        Else
            reg3264 = RegistryView.Registry32
        End If

        If reg.hive.ToLower.StartsWith("hklm") Then
            rKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, reg3264)
        ElseIf reg.hive.ToLower.StartsWith("hkcu") Then
            rKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, reg3264)
        Else
            Return Nothing
        End If

        rKey = rKey.OpenSubKey(reg.path)
        reg.type = rKey.GetValueKind(reg.name)
        reg.value = rKey.GetValue(reg.name)

        Return reg
    End Function

    Public Function ParseRegPath(ByVal path As String) As RegInfo
        Dim reg As New RegInfo
        Dim firstSlashPos As Integer = path.IndexOf("\")
        Dim lastSlashPos As Integer = path.LastIndexOf("\")
        reg.hive = path.Split("\")(0)
        reg.name = path.Substring(lastSlashPos + 1)
        reg.path = path.Substring(firstSlashPos + 1, (lastSlashPos - firstSlashPos))
        Return reg
    End Function

End Module

Public Structure RegInfo
    Public hive As String
    Public path As String
    Public name As String
    Public value As Object
    Public type As RegistryValueKind
End Structure
