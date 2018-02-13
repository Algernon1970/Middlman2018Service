Imports System.DirectoryServices.AccountManagement

Module AD
    Private adUser As String = "ASBromcomADSyncer"
    Private adPass As String = "THw8DCzcPMPwqNC5zBUF"

    Public Function CheckGroup(ByVal grpName As String) As Boolean
        Using ctx As PrincipalContext = GetConnection("as.internal", "OU=Security Groups,OU=AS Groups,OU=Ashby School,DC=as,DC=internal")
            Using gtx As GroupPrincipal = GetGroupPrincipalbyName(ctx, grpName)
                For Each member In gtx.GetMembers
                    Dim name As String = member.Name
                    If name.Equals(My.Computer.Name) Then
                        Return True
                    End If
                Next
            End Using
        End Using
        Return False
    End Function

    Private Function GetConnection(ByRef domain As String, ByRef OU As String) As PrincipalContext
        Return New PrincipalContext(ContextType.Domain, domain, OU, adUser, adPass)
    End Function

    Private Function GetGroupPrincipalbyName(ByRef ctx As PrincipalContext, group As String) As GroupPrincipal
        Try
            Dim usr As GroupPrincipal = GroupPrincipal.FindByIdentity(ctx, group)
            Return usr
        Catch ex As Exception
            Throw
        End Try
    End Function
End Module
