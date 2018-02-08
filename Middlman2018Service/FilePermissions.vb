Imports System.Security.AccessControl
Imports System.Security.Principal
Imports System.IO

Module FilePermissions

    Public Sub GrantEveroneReadAccess(ByVal fp As String)
        Dim fInfo As New FileInfo(fp)
        Dim fSec As FileSecurity = fInfo.GetAccessControl
        Dim SecID As New SecurityIdentifier(WellKnownSidType.WorldSid, Nothing)
        fSec.AddAccessRule(New FileSystemAccessRule(SecID, FileSystemRights.ReadAndExecute, AccessControlType.Allow))
        fInfo.SetAccessControl(fSec)
    End Sub

End Module
