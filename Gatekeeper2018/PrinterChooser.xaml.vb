Option Strict Off
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Runtime.CompilerServices

Public Class PrinterChooser
    Public PrinterItems As ObservableCollection(Of PrinterItem)
    Private Const SessionSet As String = "Add this time"
    Private Const SessionUnSet As String = "Remove this time"
    Private Const AllocateSet As String = "Always add"
    Private Const AllocateUnSet As String = "Never add"
    Private Const SetColour As String = "#FFFFFFFF"
    Private Const UnSetColour As String = "#FFFF00FF"

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

    End Sub

    Public Sub DisplayList(ByVal pString As String)
        PrinterItems = New ObservableCollection(Of PrinterItem)
        PListBox.DataContext = PrinterItems
        PListBox.ItemsSource = PrinterItems
        For Each printer As String In pString.Split(","c)
            If printer IsNot "" Then
                PrinterItems.Add(New PrinterItem(printer, SessionSet, AllocateSet))
            End If
        Next
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim pi As PrinterItem = sender.datacontext
        If sender.content.Equals(SessionSet) Or sender.content.Equals(SessionUnSet) Then
            If pi.SessionButton.Equals(SessionSet) Then
                pi.SessionButton = SessionUnSet
                pi.SColour = UnSetColour
            Else
                pi.SessionButton = SessionSet
                pi.SColour = SetColour
            End If
        Else
            If pi.AllocatedButton.Equals(AllocateSet) Then
                pi.AllocatedButton = AllocateUnSet
                pi.AColour = UnSetColour
            Else
                pi.AllocatedButton = AllocateSet
                pi.AColour = SetColour
            End If

        End If
    End Sub

    Private Sub AddPrinter(ByVal printerName As String)
        'get connection string
        Dim cs As String = "Get CS from printerName"
        MapPrinter(cs)
    End Sub

    Private Sub AllocatePrinter(ByVal printerName As String)
        'get printerID from printerName
        AddPrinter(printerName)
    End Sub
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

    Private Const SetColour As String = "#FFFFFFFF"
    Private Const UnSetColour As String = "#FFFF00FF"

    Public Sub New(ByVal n As String, sButton As String, aButton As String)
        _name = n
        _session = sButton
        _allocate = aButton
        _acolour = SetColour
        _scolour = SetColour
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

    Public Overrides Function ToString() As String
        Return _name
    End Function
End Class


