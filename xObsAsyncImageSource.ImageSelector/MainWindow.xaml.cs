﻿using CommunityToolkit.Mvvm.Messaging;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using xObsAsyncImageSource.Helper;
using xObsAsyncImageSource.ImageSelector.Extensions;
using xObsAsyncImageSource.ImageSelector.Messages;

namespace xObsAsyncImageSource.ImageSelector
{
    // Borderless
    public partial class MainWindow
    {
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var handle = new WindowInteropHelper(this).Handle;
            if (handle != IntPtr.Zero)
            {
                var style = Win32.GetWindowLong(handle, (int)Win32.GetWindowLongIndex.GWL_STYLE);
                style |= (int)Win32.WindowStyles.WS_CAPTION;
                Win32.SetWindowLong(handle, (int)Win32.GetWindowLongIndex.GWL_STYLE, style);
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(this.WindowProc));
            }
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            if (handle != IntPtr.Zero)
            {
                HwndSource.FromHwnd(handle).RemoveHook(this.WindowProc);
            }
            base.OnClosing(e);
        }

        private IntPtr WindowProc(IntPtr handle, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == (int)Win32.WindowMessages.WM_NCHITTEST)
            {
                if (this.OnNcHitTest(handle, wParam, lParam) is nint result)
                {
                    handled = true;
                    return result;
                }
            }
            if (msg == (int)Win32.WindowMessages.WM_SIZE)
            {
                //this.LayoutRoot.Margin = new Thickness(0);
            }
            if (msg == (int)Win32.WindowMessages.WM_DPICHANGED)
            {
                //handled = true;
            }
            return IntPtr.Zero;
        }
        private IntPtr? OnNcHitTest(IntPtr handle, IntPtr wParam, IntPtr lParam)
        {
            var screenPoint = new Point((int)lParam & 0xFFFF, ((int)lParam >> 16) & 0xFFFF);
            var clientPoint = this.PointFromScreen(screenPoint);
            //if (this.GetBorderHitTest(clientPoint) is Win32.HitTestResult borderHitTest)
            //{
            //    return (IntPtr)borderHitTest;
            //}
            clientPoint.Y -= this.BorderThickness.Top;// 边框补正
            clientPoint.X -= this.BorderThickness.Left;
            if (this.GetChromeHitTest(clientPoint) is Win32.HitTestResult chromeHitTest)
            {
                return (IntPtr)chromeHitTest;
            }
            return null;
        }
        private Win32.HitTestResult? GetBorderHitTest(Point point)
        {
            if (this.WindowState != WindowState.Normal) return null;
            if (this.ResizeMode == ResizeMode.NoResize) return null;

            var 边距 = (Math.Max(this.BorderThickness.Left * 2, 4));//MainWindow.BorderThickness
            var top = (point.Y <= 边距);
            var bottom = (point.Y >= this.Height - 边距);
            var left = (point.X <= 边距);
            var right = (point.X >= this.Width - 边距);

            if (top && left) return Win32.HitTestResult.HTTOPLEFT;
            if (top && right) return Win32.HitTestResult.HTTOPRIGHT;
            if (top) return Win32.HitTestResult.HTTOP;

            if (bottom && left) return Win32.HitTestResult.HTBOTTOMLEFT;
            if (bottom && right) return Win32.HitTestResult.HTBOTTOMRIGHT;
            if (bottom) return Win32.HitTestResult.HTBOTTOM;

            if (left) return Win32.HitTestResult.HTLEFT;
            if (right) return Win32.HitTestResult.HTRIGHT;

            return null;
        }
        private Win32.HitTestResult? GetChromeHitTest(Point point)
        {
            if (this.Chrome.Visibility is Visibility.Collapsed) { return null; }

            if (VisualTreeHelper.HitTest(this.Chrome, point) is HitTestResult result)
            {
                var button = result.VisualHit.FindVisualAncestor<Button>();
                var checkbox = result.VisualHit.FindVisualAncestor<CheckBox>();
                if ((button == null || !button.IsHitTestVisible) && (checkbox == null || !checkbox.IsHitTestVisible))
                {
                    return Win32.HitTestResult.HTCAPTION;
                }
            }

            return null;
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 关闭
            WeakReferenceMessenger.Default.Register<WindowCloseMessage>(this, (r, m) =>
            {
                Environment.Exit(0);
            });
        }
    }
}