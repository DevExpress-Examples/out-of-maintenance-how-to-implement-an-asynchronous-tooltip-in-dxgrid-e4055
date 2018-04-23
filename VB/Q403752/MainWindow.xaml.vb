Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Shapes
Imports System.ComponentModel
Imports System.Threading
Imports System.Diagnostics
Imports DevExpress.Xpf.Grid
Imports DevExpress.Xpf.Bars
Imports System.Windows.Threading

Namespace Q403752
    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Partial Public Class MainWindow
        Inherits Window
        Public Sub New()
            DataContext = New VM()
            InitializeComponent()
        End Sub
    End Class
    Public Class VM
        Inherits DependencyObject
        Private privateSource As TestDataList
        Public Property Source() As TestDataList
            Get
                Return privateSource
            End Get
            Private Set(ByVal value As TestDataList)
                privateSource = value
            End Set
        End Property
        Private privateGetToolTipCommand As ICommand
        Public Property GetToolTipCommand() As ICommand
            Get
                Return privateGetToolTipCommand
            End Get
            Private Set(ByVal value As ICommand)
                privateGetToolTipCommand = value
            End Set
        End Property
        Private i As Integer = 0
        Private w As BackgroundWorker
        Public Sub New()
            Source = TestDataList.Create()
            GetToolTipCommand = New CommandBase(AddressOf GetToolTip)
        End Sub
        Public Function GetToolTip(ByVal p As Object) As Object
            Dim e As GetToolTipEventArgs = TryCast(p, GetToolTipEventArgs)
            If e.CurrentToolTip <> e.DefaultToolTip Then
                Return Nothing
            End If
            w = New BackgroundWorker()
            AddHandler w.DoWork, AddressOf w_DoWork
            w.RunWorkerAsync(e)
            Return Nothing
        End Function
        Private Sub w_DoWork(ByVal sender As Object, ByVal e As DoWorkEventArgs)
            i += 1
            Thread.Sleep(TimeSpan.FromSeconds(1))
            Dim e1 As GetToolTipEventArgs = TryCast(e.Argument, GetToolTipEventArgs)
            Dispatcher.BeginInvoke(New Action(Of GetToolTipEventArgs)(Function(args) AnonymousMethod1(args)), New Object() {e1})
        End Sub

        'INSTANT VB TODO TASK: The return type of this anonymous method could not be determined by Instant VB:
        Private Function AnonymousMethod1(ByVal args As Object) As Object
            If args Is Nothing OrElse args.CellData Is Nothing OrElse args.CellData.RowData Is Nothing Then
                Return Nothing
            End If
            Dim res1 As String = "ID: " & (CType(args.CellData.RowData.Row, TestDataItem)).ID.ToString() & ", " & "Value: " & (CType(args.CellData.RowData.Row, TestDataItem)).Value
            args.SetToolTip(res1)
            Return Nothing
        End Function
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
            Dim res As New TestDataList()
            For i As Integer = 0 To 9
                Dim item As New TestDataItem()
                item.ID = i
                item.Value = "A"
                res.Add(item)
            Next i
            For i As Integer = 0 To 9
                Dim item As New TestDataItem()
                item.ID = i
                item.Value = "B"
                res.Add(item)
            Next i
            Return res
        End Function
    End Class
    Public Class TestDataItem
        Private privateID As Integer
        Public Property ID() As Integer
            Get
                Return privateID
            End Get
            Set(ByVal value As Integer)
                privateID = value
            End Set
        End Property
        Private privateValue As String
        Public Property Value() As String
            Get
                Return privateValue
            End Get
            Set(ByVal value As String)
                privateValue = value
            End Set
        End Property
    End Class


    Public Class GetToolTipEventArgs
        Inherits EventArgs
        Private Owner As AsyncToolTip
        Private privateCurrentToolTip As String
        Public Property CurrentToolTip() As String
            Get
                Return privateCurrentToolTip
            End Get
            Private Set(ByVal value As String)
                privateCurrentToolTip = value
            End Set
        End Property
        Private privateDefaultToolTip As String
        Public Property DefaultToolTip() As String
            Get
                Return privateDefaultToolTip
            End Get
            Private Set(ByVal value As String)
                privateDefaultToolTip = value
            End Set
        End Property
        Private privateCellData As GridCellData
        Public Property CellData() As GridCellData
            Get
                Return privateCellData
            End Get
            Private Set(ByVal value As GridCellData)
                privateCellData = value
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
            If Owner.DataContext IsNot CellData Then
                Return
            End If
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
        Public Property DefaultToolTip() As String
            Get
                Return CStr(GetValue(DefaultToolTipProperty))
            End Get
            Set(ByVal value As String)
                SetValue(DefaultToolTipProperty, value)
            End Set
        End Property
        Public Event GetToolTip As GetToolTipEventHandler


        Public Property GetToolTipCommand() As ICommand
            Get
                Return CType(GetValue(GetToolTipCommandProperty), ICommand)
            End Get
            Set(ByVal value As ICommand)
                SetValue(GetToolTipCommandProperty, value)
            End Set
        End Property

        ' Using a DependencyProperty as the backing store for GetToolTipCommand.  This enables animation, styling, binding, etc...
        Public Shared ReadOnly GetToolTipCommandProperty As DependencyProperty = DependencyProperty.Register("GetToolTipCommand", GetType(ICommand), GetType(AsyncToolTip), New PropertyMetadata(Nothing))


        Public Strings As Dictionary(Of Object, GetToolTipEventArgs)
        Public Sub New()
            Strings = New Dictionary(Of Object, GetToolTipEventArgs)()
        End Sub
        Protected Overrides Sub OnInitialized(ByVal e As EventArgs)
            MyBase.OnInitialized(e)
            Text = DefaultToolTip
            AddHandler Loaded, AddressOf AsyncToolTip_Loaded
        End Sub
        Private Sub AsyncToolTip_Loaded(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Dim d As GridCellData = CType(DataContext, GridCellData)
            If d Is Nothing Then
                Return
            End If
            Dim args As GetToolTipEventArgs
            If (Not Strings.ContainsKey(d.RowData.Row)) Then
                args = New GetToolTipEventArgs(Me, d)
                Strings.Add(d.RowData.Row, args)
                Text = DefaultToolTip
            End If
            args = Strings(d.RowData.Row)
            Text = args.CurrentToolTip
            RaiseEvent GetToolTip(Me, args)
            If GetToolTipCommand IsNot Nothing AndAlso GetToolTipCommand.CanExecute(args) Then
                GetToolTipCommand.Execute(args)
            End If
        End Sub
    End Class
End Namespace
