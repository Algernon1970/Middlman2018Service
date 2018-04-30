Option Strict Off
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Drawing.Printing
Imports System.Runtime.CompilerServices
Imports System.Threading

Public Class PrinterChooser
    Public PrinterItems As ObservableCollection(Of PrinterItem)

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        'Position
        Dim workArea As Rect = System.Windows.SystemParameters.WorkArea
        Me.Left = workArea.Right - Me.Width
        Me.Top = workArea.Bottom - Me.Height

        'Display
        Dim plist As String = WebLoader.Request(WEB_URL & WEB_GetAllPrinters)
        PrinterItems = New ObservableCollection(Of PrinterItem)
        PListBox.DataContext = PrinterItems
        PListBox.ItemsSource = PrinterItems
        For Each printer As String In plist.Split(","c)
            If printer IsNot "" Then
                PrinterItems.Add(New PrinterItem(printer, SessionSet, AllocateSet))
            End If
        Next

        'Mark Printers

        MarkSessionPrinters()
        MarkAssginedPrinters()
        ShowPrivatePrinters()
        MarkDefaultPrinter()

    End Sub

    Public Sub ShowPrivatePrinters()
        Dim pname As String = "No Printer"
        Dim found As Boolean = False
        Dim plistString As String = WebLoader.Request(String.Format("{0}{1}", WEB_URL, WEB_GetPrinterList))
        Dim pnumbers As String() = plistString.Split(","c)
        For Each pid As String In pnumbers
            If pid.Contains("*") Then
                pid = pid.Replace("*", "")
            End If
            pname = WebLoader.Request(String.Format("{0}{1}{2}", WEB_URL, WEB_GetPrinterNameByID, pid))
            For Each item As PrinterItem In PrinterItems
                If item.Name.Equals(pname) Then
                    found = True
                End If
            Next
            If Not found Then
                If Not pname.StartsWith("Failed") Then
                    Dim newitem As New PrinterItem(pname, MappedPrinter, MappedPrinter)
                    newitem.AColour = "#FFCD2A2A"
                    newitem.SColour = "#FFCD2A2A"
                    newitem.Vis = Visibility.Visible
                    PrinterItems.Add(newitem)
                End If

            End If
        Next

    End Sub

    Public Sub MarkAssginedPrinters()
        Dim pname As String = "NOPRINTER"
        Dim foundItem As PrinterItem = New PrinterItem("dummy", "dummy", "dummy")
        Dim plistString As String = WebLoader.Request(String.Format("{0}{1}", WEB_URL, WEB_GetPrinterList))
        Dim pnumbers As String() = plistString.Split(","c)
        For Each pid As String In pnumbers
            If pid.Contains("*") Then
                pid = pid.Replace("*", "")
            End If
            pname = WebLoader.Request(String.Format("{0}{1}{2}", WEB_URL, WEB_GetPrinterNameByID, pid))
            If Not pname.StartsWith("No Print") Then
                For Each item As PrinterItem In PrinterItems
                    If item.Name.Equals(pname) Then
                        foundItem = item
                        pname = "No Printer"
                    End If
                Next
                foundItem.SelectAssign()
            End If
        Next
    End Sub

    Public Sub MarkSessionPrinters()
        Dim pname As String = "No Printer"
        Dim foundItem As PrinterItem = New PrinterItem("dummy", "dummy", "dummy")
        For Each installedPrinter In PrinterSettings.InstalledPrinters
            If installedPrinter.ToString.StartsWith("\\") Then
                pname = WebLoader.Request(String.Format("{0}{1}{2}", WEB_URL, WEB_GetPrinterNameByConnection, installedPrinter.ToString))
            End If
            If Not pname.StartsWith("No Print") Then
                For Each item As PrinterItem In PrinterItems
                    If item.Name.Equals(pname) Then
                        foundItem = item
                        pname = "No Printer"
                    End If
                Next
                foundItem.SelectSession()
            End If
        Next
    End Sub

    Public Sub MarkDefaultPrinter()
        Dim defPrinter As PrinterSettings = New PrinterSettings
        Dim defPrinterName As String = WebLoader.Request(String.Format("{0}{1}{2}", WEB_URL, WEB_GetPrinterNameByConnection, defPrinter.PrinterName))
        Dim defPrinterObject As PrinterItem = New PrinterItem("Dummy", "dummy", "dummy")
        For Each item As PrinterItem In PrinterItems
            If item.Name.Equals(defPrinterName) Then
                defPrinterObject = item
            End If
        Next
        defPrinterObject.SetDefault = True
    End Sub

    ''' <summary>
    ''' Find which button was pressed, and call the relevent methods to add or remove 
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim pi As PrinterItem = sender.datacontext
        If pi.AllocatedButton.Equals(MappedPrinter) Then Return
        If sender.content.Equals(SessionSet) Or sender.content.Equals(SessionUnSet) Then
            If pi.SessionButton.Equals(SessionSet) Then
                'Add Printer This Session
                AddPrinterSession(pi)
            Else
                'Remove Printer This Session
                RemovePrinterSession(pi)
            End If
        Else
            If pi.AllocatedButton.Equals(AllocateSet) Then
                'Add Printer Always
                AddPrinterAlways(pi)
                AddPrinterSession(pi)
            Else
                'Remove Printer Always
                RemovePrinterAlways(pi)
                RemovePrinterSession(pi)
            End If

        End If
    End Sub

    Private Sub Default_Clicked(sender As Object, e As RoutedEventArgs)
        Dim cid As String = WebLoader.Request(String.Format("{0}{1}", WEB_URL, WEB_GetComputerID))
        WebLoader.Request(String.Format("{0}{1}{2}", WEB_URL, WEB_RemoveDefaultByComputer, cid))
        If sender.ischecked Then
            'remove other checkbox
            For Each pi As PrinterItem In PrinterItems
                If Not pi.Name.Equals(sender.datacontext.name) Then
                    pi.SetDefault = False
                End If
            Next
            Dim selectedPrinter As PrinterItem = sender.datacontext
            Dim pid As String = WebLoader.Request(String.Format("{0}{1}{2}", WEB_URL, Web_GetPID, selectedPrinter.Name))
            WebLoader.Request(String.Format("{0}{1}{2},{3},{4}", WEB_URL, WEB_UpdateDefaultPrinter, cid, pid, "True"))
            Dim cs As String = WebLoader.Request(String.Format("{0}{1}{2}", WEB_URL, WEB_GetPrinterConnection, pid))
            SetDefaultPrinter(cs)
        End If
    End Sub

    Private Sub RemovePrinterAlways(pi As PrinterItem)
        pi.AllocatedButton = AllocateSet
        pi.AColour = SetColour
        pi.SetDefault = False
        'Unallocate and Unassign Printer
        Dim pid As String = WebLoader.Request(String.Format("{0}{1}{2}", WEB_URL, Web_GetPID, pi.Name))
        Dim cid As String = WebLoader.Request(String.Format("{0}{1}", WEB_URL, WEB_GetComputerID))
        WebLoader.Request(String.Format("{0}{1}{2},{3}", WEB_URL, WEB_DeletePrinter, cid, pid))
    End Sub

    Private Sub AddPrinterAlways(pi As PrinterItem)
        pi.AllocatedButton = AllocateUnSet
        pi.AColour = UnSetColour
        pi.Vis = Visibility.Visible
        AllocatePrinter(pi.Name)
    End Sub

    Private Sub RemovePrinterSession(pi As PrinterItem)
        pi.SessionButton = SessionSet
        pi.SColour = SetColour
        pi.Vis = Visibility.Hidden
        'UnallocatePrinter
        Dim cs As String = WebLoader.Request(String.Format("{0}{1}{2}", WEB_URL, WEB_GetPrinterConnection, WebLoader.Request(String.Format("{0}{1}'{2}'", WEB_URL, Web_GetPID, pi.Name))))
        UnMapPrinter(cs)
    End Sub

    Private Sub AddPrinterSession(pi As PrinterItem)
        pi.SessionButton = SessionUnSet
        pi.SColour = UnSetColour
        AddPrinterByID(WebLoader.Request(String.Format("{0}{1}'{2}'", WEB_URL, Web_GetPID, pi.Name)))
    End Sub

    ''' <summary>
    ''' Find printer by name and set it ALLOCATED
    ''' </summary>
    ''' <param name="printerName">Name of the Printer</param>
    ''' <param name="status">True = map, False = unmap</param>
    Public Sub SetAllocatedPrinter(ByVal printerName As String, ByVal status As Boolean)
        For Each item As PrinterItem In PrinterItems
            If item.Name.Equals(printerName) Then
                If status = True Then
                    AddPrinterAlways(item)
                Else
                    RemovePrinterAlways(item)
                End If
            End If
        Next
    End Sub

    ''' <summary>
    ''' Find printer by name and set it SESSION
    ''' </summary>
    ''' <param name="printerName">Name of the Printer</param>
    ''' <param name="status">True = map, False = unmap</param>
    Public Sub SetSessionPrinter(ByVal printerName As String, ByVal status As Boolean)
        For Each item As PrinterItem In PrinterItems
            If item.Name.Equals(printerName) Then
                If status = True Then
                    AddPrinterSession(item)
                Else
                    RemovePrinterSession(item)
                End If
            End If
        Next
    End Sub

    ''' <summary>
    ''' Install printer
    ''' </summary>
    ''' <param name="printerID"></param>
    Private Sub AddPrinterByID(ByVal printerID As String)
        'get connection string
        Dim cs As String = WebLoader.Request(String.Format("{0}{1}{2}", WEB_URL, WEB_GetPrinterConnection, printerID))
        Dim CallBackMethod As WaitCallback = New WaitCallback(AddressOf MapPrinterThread)
        ThreadPool.QueueUserWorkItem(CallBackMethod, cs)
    End Sub

    Shared Sub MapPrinterThread(ByVal state As Object)
        Dim cs As String = CType(state, String)
        MapPrinter(cs)
    End Sub

    ''' <summary>
    ''' Write mapping to Database, then install printer
    ''' </summary>
    ''' <param name="printerName"></param>
    Private Sub AllocatePrinter(ByVal printerName As String)
        'get printerID from printerName
        Dim pid As String = WebLoader.Request(String.Format("{0}{1}'{2}'", WEB_URL, Web_GetPID, printerName))
        Dim cid As String = WebLoader.Request(String.Format("{0}{1}", WEB_URL, WEB_GetComputerID))
        Dim def As String = IsDefault(printerName)

        If Not pid.StartsWith("No") Then
            'WriteToDB
            WebLoader.Request(String.Format("{0}{1}{2},{3},{4}", WEB_URL, WEB_AddPrinter, cid, pid, def))
            'Install Printer
            AddPrinterByID(pid)
        End If

    End Sub

    Private Function IsDefault(ByRef printername As String) As String
        Dim foundItem As PrinterItem = New PrinterItem("No Printer", "dummy", "dummy")
        For Each item As PrinterItem In PrinterItems
            If item.Name.Equals(printername) Then
                foundItem = item
            End If
        Next
        Return If(foundItem.SetDefault = True, "True", "False")
    End Function
End Class

Public Class PrinterItem
    Implements INotifyPropertyChanged

    Public Event PropertyChanged As PropertyChangedEventHandler _
        Implements INotifyPropertyChanged.PropertyChanged

    Private Sub NotifyPropertyChanged(<CallerMemberName()> Optional ByVal propertyName As String = Nothing)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub

    Public _name As String
    Public _session As String
    Public _allocate As String
    Public _acolour As String
    Public _scolour As String
    Public _default As Boolean
    Public _allocated As Boolean
    Public _vis As Visibility

    Public Sub New(ByVal n As String, sButton As String, aButton As String)
        _name = n
        _session = sButton
        _allocate = aButton
        _acolour = SetColour
        _scolour = SetColour
        _default = False
        _vis = Visibility.Hidden
    End Sub

    Public Property AColour As String
        Get
            Return _acolour
        End Get
        Set(value As String)
            _acolour = value
            NotifyPropertyChanged()
        End Set
    End Property

    Public Property Vis As String
        Get
            Return _vis
        End Get
        Set(value As String)
            _vis = value
            NotifyPropertyChanged()
        End Set
    End Property

    Public Property SColour As String
        Get
            Return _scolour
        End Get
        Set(value As String)
            _scolour = value
            NotifyPropertyChanged()
        End Set
    End Property

    Public Property Name As String
        Get
            Return _name
        End Get
        Set(value As String)
            _name = value
            NotifyPropertyChanged()
        End Set
    End Property

    Public Property SessionButton As String
        Get
            Return _session
        End Get
        Set(value As String)
            _session = value
            NotifyPropertyChanged()
        End Set
    End Property

    Public Property AllocatedButton As String
        Get
            Return _allocate
        End Get
        Set(value As String)
            _allocate = value
            NotifyPropertyChanged()
        End Set
    End Property

    Public Property SetDefault As Boolean
        Get
            Return _default
        End Get
        Set(value As Boolean)
            _default = value
            NotifyPropertyChanged()
        End Set
    End Property

    Public Overrides Function ToString() As String
        Return _name
    End Function

    Public Sub SelectSession()
        SessionButton = SessionUnSet
        SColour = UnSetColour
    End Sub

    Public Sub UnSelectSession()
        SessionButton = SessionSet
        SColour = SetColour
    End Sub

    Public Sub SelectAssign()
        AllocatedButton = AllocateUnSet
        AColour = UnSetColour
        _allocated = True
        Vis = Visibility.Visible
    End Sub

    Public Sub UnSelectAssign()
        AllocatedButton = AllocateSet
        AColour = SetColour
        _allocated = False
        Vis = Visibility.Hidden
    End Sub

    Public ReadOnly Property isAllocated() As Boolean
        Get
            Return _allocated
        End Get
    End Property

End Class


