Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.ComponentModel
Imports System.Threading
Imports System.Diagnostics
Imports DevExpress.Xpf.Grid
Imports System.Windows.Threading

Namespace Q403752

    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Public Partial Class MainWindow
        Inherits Window

        Public Sub New()
            DataContext = New VM()
            Me.InitializeComponent()
        End Sub
    End Class

    Public Class VM
        Inherits DependencyObject

        Private _Source As TestDataList, _GetToolTipCommand As ICommand

        Public Property Source As TestDataList
            Get
                Return _Source
            End Get

            Private Set(ByVal value As TestDataList)
                _Source = value
            End Set
        End Property

        Public Property GetToolTipCommand As ICommand
            Get
                Return _GetToolTipCommand
            End Get

            Private Set(ByVal value As ICommand)
                _GetToolTipCommand = value
            End Set
        End Property

        Private i As Integer = 0

        Private w As BackgroundWorker

        Public Sub New()
            Source = TestDataList.Create()
            GetToolTipCommand = New CommandBase(AddressOf GetToolTip)
        End Sub

        Private Sub GetToolTip(ByVal p As Object)
            Dim e As GetToolTipEventArgs = TryCast(p, GetToolTipEventArgs)
            If Not Equals(e.CurrentToolTip, e.DefaultToolTip) Then Return
            w = New BackgroundWorker()
            AddHandler w.DoWork, New DoWorkEventHandler(AddressOf w_DoWork)
            w.RunWorkerAsync(e)
        End Sub

        Private Sub w_DoWork(ByVal sender As Object, ByVal e As DoWorkEventArgs)
            i += 1
            Thread.Sleep(TimeSpan.FromSeconds(1))
            Dim e1 As GetToolTipEventArgs = TryCast(e.Argument, GetToolTipEventArgs)
            Dispatcher.BeginInvoke(New Action(Of GetToolTipEventArgs)(Sub(args)
                If args Is Nothing OrElse args.CellData Is Nothing OrElse args.CellData.RowData Is Nothing Then Return
                Dim res1 As String = "ID: " & CType(args.CellData.RowData.Row, TestDataItem).ID.ToString() & ", " & "Value: " & CType(args.CellData.RowData.Row, TestDataItem).Value
                args.SetToolTip(res1)
            End Sub), New Object() {e1})
        End Sub
    End Class

    Public Class CommandBase
        Implements ICommand

        Public Function CanExecute(ByVal parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function

        Private ExecuteCore As Action(Of Object)

        Public Sub New(ByVal execute As Action(Of Object))
            ExecuteCore = execute
        End Sub

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Public Sub Execute(ByVal parameter As Object) Implements ICommand.Execute
            ExecuteCore(parameter)
        End Sub
    End Class

    Public Class TestDataList
        Inherits List(Of TestDataItem)

        Public Shared Function Create() As TestDataList
            Dim res As TestDataList = New TestDataList()
            For i As Integer = 0 To 10 - 1
                Dim item As TestDataItem = New TestDataItem()
                item.ID = i
                item.Value = "A"
                res.Add(item)
            Next

            For i As Integer = 0 To 10 - 1
                Dim item As TestDataItem = New TestDataItem()
                item.ID = i
                item.Value = "B"
                res.Add(item)
            Next

            Return res
        End Function
    End Class

    Public Class TestDataItem

        Public Property ID As Integer

        Public Property Value As String
    End Class

    Public Class GetToolTipEventArgs
        Inherits EventArgs

        Private _CurrentToolTip As String, _DefaultToolTip As String, _CellData As GridCellData

        Private Owner As AsyncToolTip

        Public Property CurrentToolTip As String
            Get
                Return _CurrentToolTip
            End Get

            Private Set(ByVal value As String)
                _CurrentToolTip = value
            End Set
        End Property

        Public Property DefaultToolTip As String
            Get
                Return _DefaultToolTip
            End Get

            Private Set(ByVal value As String)
                _DefaultToolTip = value
            End Set
        End Property

        Public Property CellData As GridCellData
            Get
                Return _CellData
            End Get

            Private Set(ByVal value As GridCellData)
                _CellData = value
            End Set
        End Property

        Public Sub New(ByVal owner As AsyncToolTip, ByVal d As GridCellData)
            Me.Owner = owner
            CellData = d
            DefaultToolTip = Me.Owner.DefaultToolTip
            CurrentToolTip = DefaultToolTip
        End Sub

        Public Sub SetToolTip(ByVal str As String)
            CurrentToolTip = str
            If Owner.DataContext IsNot CellData Then Return
            Owner.Text = CurrentToolTip
            Owner.UpdateLayout()
        '((ToolTip)Owner.Parent).IsOpen = false;
        '((ToolTip)Owner.Parent).IsOpen = true;
        End Sub
    End Class

    Public Delegate Sub GetToolTipEventHandler(ByVal sender As Object, ByVal e As GetToolTipEventArgs)

    Public Class AsyncToolTip
        Inherits TextBlock

        Public Shared ReadOnly DefaultToolTipProperty As DependencyProperty = DependencyProperty.Register("DefaultToolTip", GetType(String), GetType(AsyncToolTip), New PropertyMetadata(String.Empty))

        Public Property DefaultToolTip As String
            Get
                Return CStr(GetValue(DefaultToolTipProperty))
            End Get

            Set(ByVal value As String)
                SetValue(DefaultToolTipProperty, value)
            End Set
        End Property

        Public Event GetToolTip As GetToolTipEventHandler

        Public Property GetToolTipCommand As ICommand
            Get
                Return CType(GetValue(GetToolTipCommandProperty), ICommand)
            End Get

            Set(ByVal value As ICommand)
                SetValue(GetToolTipCommandProperty, value)
            End Set
        End Property

        ' Using a DependencyProperty as the backing store for GetToolTipCommand.  This enables animation, styling, binding, etc...
        Public Shared ReadOnly GetToolTipCommandProperty As DependencyProperty = DependencyProperty.Register("GetToolTipCommand", GetType(ICommand), GetType(AsyncToolTip), New PropertyMetadata(CType(Nothing, PropertyChangedCallback)))

        Public Strings As Dictionary(Of Object, GetToolTipEventArgs)

        Public Sub New()
            Strings = New Dictionary(Of Object, GetToolTipEventArgs)()
        End Sub

        Protected Overrides Sub OnInitialized(ByVal e As EventArgs)
            MyBase.OnInitialized(e)
            Text = DefaultToolTip
            AddHandler Loaded, New RoutedEventHandler(AddressOf AsyncToolTip_Loaded)
        End Sub

        Private Sub AsyncToolTip_Loaded(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Dim d As GridCellData = CType(DataContext, GridCellData)
            If d Is Nothing Then Return
            Dim args As GetToolTipEventArgs
            If Not Strings.ContainsKey(d.RowData.Row) Then
                args = New GetToolTipEventArgs(Me, d)
                Strings.Add(d.RowData.Row, args)
                Text = DefaultToolTip
            End If

            args = Strings(d.RowData.Row)
            Text = args.CurrentToolTip
            RaiseEvent GetToolTip(Me, args)
            If GetToolTipCommand IsNot Nothing AndAlso GetToolTipCommand.CanExecute(args) Then GetToolTipCommand.Execute(args)
        End Sub
    End Class
End Namespace
