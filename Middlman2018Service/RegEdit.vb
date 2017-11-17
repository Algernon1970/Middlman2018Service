Imports Microsoft.Win32
Module RegEdit
    Dim reg3264 As RegistryView

    Public Function ReadReg(ByVal reg As RegInfo) As RegInfo

        Dim rKey As RegistryKey

        Dim reg3264 As RegistryView = RegistryChooser()

        If reg.hive.ToLower.StartsWith("hklm") Then
            rKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, reg3264)
        ElseIf reg.hive.ToLower.StartsWith("hkcu") Then
            rKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, reg3264)
        Else
            reg.returnMessage = "Invalid hive. Needs to be hklm or hkcu"
            Return reg
        End If

        rKey = rKey.OpenSubKey(reg.path)
        reg.type = rKey.GetValueKind(reg.name)
        reg.value = rKey.GetValue(reg.name)

        Return reg
    End Function

    Public Function WriteReg(ByRef reg As RegInfo) As RegInfo
        Dim reg3264 As RegistryView = RegistryChooser()
        Dim rKey As RegistryKey

        If reg.hive.ToLower.StartsWith("hklm") Then
            rKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, reg3264)
        ElseIf reg.hive.ToLower.StartsWith("hkcu") Then
            rKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, reg3264)
        Else
            reg.returnMessage = "Invalid hive. Needs to be hklm or hkcu"
            Return reg
        End If
        rKey = OpenCreateSubKey(rKey, reg.path)
        rKey.SetValue(reg.name, reg.value, reg.type)
        reg.returnMessage = "OK"
        Return reg
    End Function

    Public Function DeleteRegValue(ByRef reg As RegInfo) As RegInfo
        Dim reg3264 As RegistryView = RegistryChooser()
        Dim rKey As RegistryKey

        If reg.hive.ToLower.StartsWith("hklm") Then
            rKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, reg3264)
        ElseIf reg.hive.ToLower.StartsWith("hkcu") Then
            rKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, reg3264)
        Else
            reg.returnMessage = "Invalid hive. Needs to be hklm or hkcu"
            Return reg
        End If
        rKey = rKey.OpenSubKey(reg.path, True)
        If IsNothing(rKey) Then
            reg.returnMessage = "Cannot open path"
            Return reg
        Else
            rKey.DeleteValue(reg.name)
            reg.returnMessage = "OK"
            Return reg
        End If
    End Function

    Public Function DeleteRegKey(ByRef reg As RegInfo) As RegInfo
        Dim reg3264 As RegistryView = RegistryChooser()
        Dim rKey As RegistryKey

        If reg.hive.ToLower.StartsWith("hklm") Then
            rKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, reg3264)
        ElseIf reg.hive.ToLower.StartsWith("hkcu") Then
            rKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, reg3264)
        Else
            reg.returnMessage = "Invalid hive. Needs to be hklm or hkcu"
            Return reg
        End If
        rKey = rKey.OpenSubKey(reg.path, True)
        If IsNothing(rKey) Then
            reg.returnMessage = "Cannot open path"
            Return reg
        Else
            rKey.DeleteSubKey(reg.name)
            reg.returnMessage = "OK"
            Return reg
        End If
    End Function

    Private Function OpenCreateSubKey(ByRef baseKey As RegistryKey, path As String) As RegistryKey
        Dim regKey As RegistryKey
        For Each pathPart As String In path.Split("\")
            regKey = baseKey.OpenSubKey(pathPart, True)
            If IsNothing(regKey) Then
                regKey = baseKey.CreateSubKey(pathPart, True)
            End If
            baseKey = regKey
        Next
        Return baseKey
    End Function

    Private Function RegistryChooser() As RegistryView
        If Environment.Is64BitOperatingSystem Then
            Return RegistryView.Registry64
        Else
            Return RegistryView.Registry32
        End If
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

    Public Function ParseValueObject(ByVal value As String, ByRef reg As RegInfo) As RegInfo
        Dim valuebits As String() = value.Split(":")
        If valuebits.Count <> 2 Then
            reg.returnMessage = "Invalid format for ValueObject " & value
        End If
        Select Case valuebits(1)
            Case "Binary"
                reg.type = RegistryValueKind.Binary
                reg.value = System.Text.Encoding.Unicode.GetBytes(value)
            Case "DWord"
                reg.type = RegistryValueKind.DWord
                reg.value = Integer.Parse(value)
            Case "ExpandString"
                reg.type = RegistryValueKind.ExpandString
                reg.value = value
            Case "QWord"
                reg.type = RegistryValueKind.QWord
                reg.value = Int64.Parse(value)
            Case "String"
                reg.type = RegistryValueKind.String
                reg.value = value
        End Select

        Return reg
    End Function

End Module

Public Structure RegInfo
    Public hive As String
    Public path As String
    Public name As String
    Public value As Object
    Public type As RegistryValueKind
    Public returnMessage As String
End Structure
