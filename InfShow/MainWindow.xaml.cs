using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Windows.Controls;

namespace InfShow
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 剪贴板内容改变时API函数向windows发送的消息
        /// </summary>
        const int WM_CLIPBOARDUPDATE = 0x031D;

        /// <summary>
        /// windows用于监视剪贴板的API函数
        /// </summary>
        /// <param name="hwnd">要监视剪贴板的窗口的句柄</param>
        /// <returns>成功则返回true</returns>
        [DllImport("user32.dll")]//引用dll,确保API可用
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);

        /// <summary>
        /// 取消对剪贴板的监视
        /// </summary>
        /// <param name="hwnd">监视剪贴板的窗口的句柄</param>
        /// <returns>成功则返回true</returns>
        [DllImport("user32.dll")]//引用dll,确保API可用
        public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        Action ClipboardChangedHandle;
        ScriptMgr _mgr = null;
        ScriptMgr Mgr
        {
            get
            {
                if(_mgr==null)
                {
                    
                     _mgr = new ScriptMgr();
                    _mgr.Load();
                    scripts.Items.Clear();
                    foreach (string n in _mgr.GetScriptsName())
                    {
                        MenuItem it = new MenuItem() { Header = n };
                        it.IsCheckable = true;
                        it.IsChecked = (n == ScriptName);
                        it.Click += It_Click;
                        scripts.Items.Add(it);
                    }
                }
                return _mgr;
            }
        }

        private void It_Click(object sender, RoutedEventArgs e)
        {
            MenuItem it = sender as MenuItem;
            if(it!=null && it.Header as string !=null)
            {
                ScriptName = it.Header as string;
                foreach(MenuItem tmp in scripts.Items)
                {
                    tmp.IsChecked = (tmp == it);
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            Left = 0;
            Top = 0;
            Height = 60;
            Width = 250;
            ClipboardChangedHandle = new Action(this.OnClipboardChanged);
            Mgr.GetScriptsName();
            
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            this.win_SourceInitialized(this, e);

            // HTodo  ：添加剪贴板监视 
            System.IntPtr handle = (new System.Windows.Interop.WindowInteropHelper(this)).Handle;

            AddClipboardFormatListener(handle);

        }

        /// <summary> 添加监视消息 </summary>
        void win_SourceInitialized(object sender, EventArgs e)
        {

            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;

            if (hwndSource != null)
            {
                hwndSource.AddHook(new HwndSourceHook(WndProc));
            }

        }

        /// <summary> 剪贴板内容改变 </summary>
        string ScriptName = "Convert1016";
        void OnClipboardChanged()
        {
            // HTodo  ：复制的文件路径 
            string ret = "";
            try
            {
                ret = Mgr.RunScript(ScriptName);
            }
            catch(Exception ee)
            {
                ret = "";
            }
            if (!string.IsNullOrEmpty(ret))
                lb.Content = ret;


        }

        protected virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_CLIPBOARDUPDATE:
                    {

                        if (ClipboardChangedHandle != null)
                        {
                            // HTodo  ：触发剪贴板变化事件 
                            ClipboardChangedHandle.Invoke();
                        }

                    }
                    break;
            }

            return IntPtr.Zero;
        }

        private void delMItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            _mgr = null;
            Mgr.GetScriptsName();
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
            
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("没有什么好说的,有事敲门：wu.daokui@zte.com.cn", "wu.daokui@zte.com.cn", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
     
    
}
