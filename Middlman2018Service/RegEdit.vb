Imports Microsoft.Win32
Module RegEdit
    Dim reg3264 As RegistryView

    Public Function ReadReg(ByVal reg As RegInfo) As RegInfo
        Try
            Dim rKey As RegistryKey

            Dim reg3264 As RegistryView = RegistryChooser()

            If reg.hive.ToLower.StartsWith("hklm") Then
                rKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, reg3264)
            ElseIf reg.hive.ToLower.StartsWith("hkcu") Then
                rKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, reg3264)
            Else
                Throw New RegistryException(System.Reflection.MethodInfo.GetCurrentMethod.Name.ToString, "Invalid Hive")
            End If
            rKey = rKey.OpenSubKey(reg.path)
            reg.type = rKey.GetValueKind(reg.name)
            reg.value = rKey.GetValue(reg.name)
        Catch ex As Exception
            reg.returnMessage = "Error ReadReg - " & ex.Message
            Throw New RegistryException(System.Reflection.MethodInfo.GetCurrentMethod.Name.ToString, ex.Message)
        End Try

        Return reg
    End Function

    Public Function WriteReg(ByRef reg As RegInfo) As RegInfo
        Try
            Dim reg3264 As RegistryView = RegistryChooser()
            Dim rKey As RegistryKey

            If reg.hive.ToLower.StartsWith("hklm") Then
                rKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, reg3264)
            ElseIf reg.hive.ToLower.StartsWith("hkcu") Then
                rKey = RegistryKey.OpenBaseKey(RegistryHive.Users, reg3264)
                reg.path = SharedData.currentUserSid & "\" & reg.path
            Else
                Throw New RegistryException(System.Reflection.MethodInfo.GetCurrentMethod.Name.ToString, "Invalid Hive")
            End If
            rKey = OpenCreateSubKey(rKey, reg.path)
            rKey.SetValue(reg.name, reg.value, reg.type)
            reg.returnMessage = "OK"
        Catch ex As Exception
            Throw New RegistryException(System.Reflection.MethodInfo.GetCurrentMethod.Name.ToString, ex.Message)
        End Try

        Return reg
    End Function

    Public Function DeleteRegValue(ByRef reg As RegInfo) As RegInfo
        Try
            Dim reg3264 As RegistryView = RegistryChooser()
            Dim rKey As RegistryKey

            If reg.hive.ToLower.StartsWith("hklm") Then
                rKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, reg3264)

            ElseIf reg.hive.ToLower.StartsWith("hkcu") Then
                rKey = RegistryKey.OpenBaseKey(RegistryHive.Users, reg3264)
                reg.path = SharedData.currentUserSid & "\" & reg.path
            Else
                Throw New RegistryException(System.Reflection.MethodInfo.GetCurrentMethod.Name.ToString, "Invalid Hive")
            End If
            rKey = rKey.OpenSubKey(reg.path, True)
            If IsNothing(rKey) Then
                Throw New RegistryException(System.Reflection.MethodInfo.GetCurrentMethod.Name.ToString, "Cannot open path")
            Else
                rKey.DeleteValue(reg.name)
                reg.returnMessage = "OK"
                Return reg
            End If
        Catch ex As Exception
            Throw New RegistryException(System.Reflection.MethodInfo.GetCurrentMethod.Name.ToString, ex.Message)
        End Try

    End Function

    Public Function DeleteRegKey(ByRef reg As RegInfo) As RegInfo
        Try
            Dim reg3264 As RegistryView = RegistryChooser()
            Dim rKey As RegistryKey

            If reg.hive.ToLower.StartsWith("hklm") Then
                rKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, reg3264)
            ElseIf reg.hive.ToLower.StartsWith("hkcu") Then
                rKey = RegistryKey.OpenBaseKey(RegistryHive.Users, reg3264)
                reg.path = SharedData.currentUserSid & "\" & reg.path
            Else
                Throw New RegistryException(System.Reflection.MethodInfo.GetCurrentMethod.Name.ToString, "Invalid Hive")
            End If
            rKey = rKey.OpenSubKey(reg.path, True)
            If IsNothing(rKey) Then
                Throw New RegistryException(System.Reflection.MethodInfo.GetCurrentMethod.Name.ToString, "Cannot open path")
            Else
                rKey.DeleteSubKey(reg.name)
                reg.returnMessage = "OK"
                Return reg
            End If
        Catch ex As Exception
            Throw New RegistryException(System.Reflection.MethodInfo.GetCurrentMethod.Name.ToString, ex.Message)
        End Try

    End Function

    Private Function OpenCreateSubKey(ByRef baseKey As RegistryKey, path As String) As RegistryKey
        Dim regKey As RegistryKey
        For Each pathPart As String In path.Split("\"c)
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
        Try
            Dim firstSlashPos As Integer = path.IndexOf("\")
            Dim lastSlashPos As Integer = path.LastIndexOf("\")
            reg.hive = path.Split("\"c)(0)
            reg.name = path.Substring(lastSlashPos + 1)
            reg.path = path.Substring(firstSlashPos + 1, (lastSlashPos - firstSlashPos))
            reg.returnMessage = "OK"
            Return reg
        Catch ex As Exception
            Throw New RegistryException(System.Reflection.MethodInfo.GetCurrentMethod.Name.ToString, ex.Message)
        End Try

    End Function

    Public Function ParseValueObject(ByVal value As String, ByRef reg As RegInfo) As RegInfo
        Dim valuebits As String() = value.Split("|"c)
        If valuebits.Count <> 2 Then
            Throw New RegistryException(System.Reflection.MethodInfo.GetCurrentMethod.Name.ToString, "Error ParseValueObject - Invalid format for ValueObject " & value)
        End If
        reg.type = RegistryValueKind.String
        reg.value = "Error ParseValueObject - Unknown type"
        reg.returnMessage = "Error ParseValueObject - Unknown type"
        Select Case valuebits(1).ToLower
            Case "binary"
                reg.type = RegistryValueKind.Binary
                reg.value = System.Text.Encoding.Unicode.GetBytes(valuebits(0))
            Case "dword"
                reg.type = RegistryValueKind.DWord
                reg.value = Integer.Parse(valuebits(0))
            Case "expandstring"
                reg.type = RegistryValueKind.ExpandString
                reg.value = valuebits(0)
            Case "qword"
                reg.type = RegistryValueKind.QWord
                reg.value = Int64.Parse(valuebits(0))
            Case "string"
                reg.type = RegistryValueKind.String
                reg.value = valuebits(0)
            Case Else
                Throw New RegistryException(System.Reflection.MethodInfo.GetCurrentMethod.Name.ToString, "Error ParseValueObject - unknown type " & value)
        End Select
        reg.returnMessage = "OK"
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
