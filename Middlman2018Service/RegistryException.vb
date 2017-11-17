Public Class RegistryException
    Inherits System.ApplicationException

    Public errorSource As String
    Public Sub New(ByVal _source As String, ByVal message As String)
        MyBase.New(message)
        Source = _source
    End Sub

End Class
