Option Strict Off
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Runtime.CompilerServices

Public Class PrinterChooser
    Public PrinterItems As ObservableCollection(Of PrinterItem)
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

    End Sub

    Public Sub DisplayList(ByVal pString As String)
        PrinterItems = New ObservableCollection(Of PrinterItem)
        PListBox.DataContext = PrinterItems
        PListBox.ItemsSource = PrinterItems
        For Each printer As String In pString.Split(","c)
            If printer IsNot "" Then
                PrinterItems.Add(New PrinterItem(printer, "Add this time", "Always Add"))
            End If
        Next
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        MsgBox("Woot")
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
    Public Sub New(ByVal n As String, sButton As String, aButton As String)
        _name = n
        _session = sButton

        _allocate = aButton
    End Sub

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


