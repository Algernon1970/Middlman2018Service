Module SharedData
    Public ComputerTableAdapter As New ZuulDataSetTableAdapters.Tbl_ComputerTableAdapter
    Public PLinkTableAdapter As New ZuulDataSetTableAdapters.Lnk_ComputerPrinterTableAdapter
    Public PrinterTableAdapter As New ZuulDataSetTableAdapters.Tbl_PrinterTableAdapter

    Public online As String = "Needs to be coded"
    Public currentUser As String = "Noone"
    Public currentUserSid As String = "0"

End Module

