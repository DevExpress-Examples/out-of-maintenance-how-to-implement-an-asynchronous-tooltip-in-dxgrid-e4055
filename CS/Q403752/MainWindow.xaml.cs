using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Bars;
using System.Windows.Threading;

namespace Q403752 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            DataContext = new VM();
            InitializeComponent();
        }
    }
    public class VM : DependencyObject {
        public TestDataList Source { get; private set; }
        public ICommand GetToolTipCommand { get; private set; }
        int i = 0;
        BackgroundWorker w;
        public VM() {
            Source = TestDataList.Create();
            GetToolTipCommand = new CommandBase(GetToolTip);
        }
        void GetToolTip(object p) {
            GetToolTipEventArgs e = p as GetToolTipEventArgs;
            if(e.CurrentToolTip != e.DefaultToolTip) return;
            w = new BackgroundWorker();
            w.DoWork += new DoWorkEventHandler(w_DoWork);
            w.RunWorkerAsync(e);
        }
        void w_DoWork(object sender, DoWorkEventArgs e) {
            i++;
            Thread.Sleep(TimeSpan.FromSeconds(1));
            GetToolTipEventArgs e1 = e.Argument as GetToolTipEventArgs;
            Dispatcher.BeginInvoke(new Action<GetToolTipEventArgs>((args) => {
                if(args == null || args.CellData == null || args.CellData.RowData == null) return;
                string res1 = "ID: " + ((TestDataItem)args.CellData.RowData.Row).ID.ToString() +", " + "Value: " + ((TestDataItem)args.CellData.RowData.Row).Value;
                args.SetToolTip(res1);
            }), new object[] { e1 }
            );
        }
    }
    public class CommandBase : ICommand {
        public bool CanExecute(object parameter) {
            return true;
        }
        Action<object> ExecuteCore;
        public CommandBase(Action<object> execute) {
            ExecuteCore = execute;
        }
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter) {
            ExecuteCore(parameter);
        }
    }
    public class TestDataList : List<TestDataItem> {
        public static TestDataList Create() {
            TestDataList res = new TestDataList();
            for(int i = 0; i < 10; i++) {
                TestDataItem item = new TestDataItem();
                item.ID = i;
                item.Value = "A";
                res.Add(item);
            }
            for(int i = 0; i < 10; i++) {
                TestDataItem item = new TestDataItem();
                item.ID = i;
                item.Value = "B";
                res.Add(item);
            }
            return res;
        }
    }
    public class TestDataItem {
        public int ID { get; set; }
        public string Value { get; set; }
    }


    public class GetToolTipEventArgs : EventArgs {
        AsyncToolTip Owner;
        public string CurrentToolTip { get; private set; }
        public string DefaultToolTip { get; private set; }
        public GridCellData CellData { get; private set; }
        public GetToolTipEventArgs(AsyncToolTip owner, GridCellData d) {
            Owner = owner;
            CellData = d;
            DefaultToolTip = Owner.DefaultToolTip;
            CurrentToolTip = DefaultToolTip;
        }
        public void SetToolTip(string str) {
            CurrentToolTip = str;
            if(Owner.DataContext != CellData) return;
            Owner.Text = CurrentToolTip;
            Owner.UpdateLayout();
            //((ToolTip)Owner.Parent).IsOpen = false;
            //((ToolTip)Owner.Parent).IsOpen = true;
        }
    }
    public delegate void GetToolTipEventHandler(object sender, GetToolTipEventArgs e);
    public class AsyncToolTip : TextBlock {
        public static readonly DependencyProperty DefaultToolTipProperty =
            DependencyProperty.Register("DefaultToolTip", typeof(string), typeof(AsyncToolTip),
            new PropertyMetadata(string.Empty));
        public string DefaultToolTip {
            get { return (string)GetValue(DefaultToolTipProperty); }
            set { SetValue(DefaultToolTipProperty, value); }
        }
        public event GetToolTipEventHandler GetToolTip;


        public ICommand GetToolTipCommand {
            get { return (ICommand)GetValue(GetToolTipCommandProperty); }
            set { SetValue(GetToolTipCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GetToolTipCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GetToolTipCommandProperty =
            DependencyProperty.Register("GetToolTipCommand", typeof(ICommand), typeof(AsyncToolTip), new PropertyMetadata(null));

        
        public Dictionary<object, GetToolTipEventArgs> Strings;
        public AsyncToolTip() {
            Strings = new Dictionary<object, GetToolTipEventArgs>();
        }
        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);
            Text = DefaultToolTip;
            Loaded += new RoutedEventHandler(AsyncToolTip_Loaded);
        }
        void AsyncToolTip_Loaded(object sender, RoutedEventArgs e) {
            GridCellData d = (GridCellData)DataContext;
            if(d == null) return;
            GetToolTipEventArgs args;
            if(!Strings.ContainsKey(d.RowData.Row)) {
                args = new GetToolTipEventArgs(this, d);
                Strings.Add(d.RowData.Row, args);
                Text = DefaultToolTip;
            }
            args = Strings[d.RowData.Row];
            Text = args.CurrentToolTip;
            if(GetToolTip != null) GetToolTip(this, args);
            if(GetToolTipCommand != null && GetToolTipCommand.CanExecute(args)) GetToolTipCommand.Execute(args);
        }
    }
}
