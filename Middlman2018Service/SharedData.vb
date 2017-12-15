﻿Module SharedData
    Public ComputerTableAdapter As New ZuulDataSetTableAdapters.Tbl_ComputerTableAdapter
    Public PrinterLinkTableAdapter As New ZuulDataSetTableAdapters.Lnk_ComputerPrinterTableAdapter
    Public PrinterTableAdapter As New ZuulDataSetTableAdapters.Tbl_PrinterTableAdapter
    Public PersonLinkTableAdapter As New ZuulDataSetTableAdapters.Lnk_ComputerPersonTableAdapter
    Public PersonTableAdapter As New ZuulDataSetTableAdapters.Tbl_PersonTableAdapter

    Public online As String = "Needs to be coded"
    Public currentUser As String = "Noone"
    Public currentUserSid As String = "0"

    Public Function AmIOnDomain() As String
        Try
            ComputerTableAdapter.Connection.Open()
        Catch ex As Exception
            Return "Offline"
        End Try

        Dim serverv As String = ComputerTableAdapter.Connection.ServerVersion
        ComputerTableAdapter.Connection.Close()
        Return serverv
    End Function

End Module

