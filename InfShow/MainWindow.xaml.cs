using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        public MainWindow()
        {
            InitializeComponent();
            Left = 0;
            Top = 0;
            Height = 60;
            Width = 200;
            ClipboardChangedHandle = new Action(this.OnClipboardChanged);
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
        Regex r10 = new Regex("^[0-9]+$");
        Regex r16 = new Regex("^(0x|0X)?[0-9a-fA-F]+$");
        /// <summary> 剪贴板内容改变 </summary>

        void OnClipboardChanged()
        {
            // HTodo  ：复制的文件路径 
            string ret = "";
            try
            {
                string text = System.Windows.Clipboard.GetText();
                if (!string.IsNullOrEmpty(text))
                {
                    text = text.Trim();
                    if (r10.IsMatch(text))
                    {
                        long i = long.Parse(text);
                        long i16 = long.Parse(text, System.Globalization.NumberStyles.HexNumber);
                        ret = string.Format("{0} <-> 0x{0:X}\r\n{1} <-> 0x{1:X}", i, i16);
                    }
                    else if (r16.IsMatch(text))
                    {
                        if (text.StartsWith("0x") || text.StartsWith("0X")) text = text.Substring(2);
                        long i16 = long.Parse(text, System.Globalization.NumberStyles.HexNumber);
                        ret = string.Format("{0} <-> 0x{0:X} ", i16);
                    }

                }
            }
            catch
            {
                ret = "";
            }
            if (ret != "")
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
    }
     
    
}
