using ObsInterop;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using xObsAsyncImageSource.Helper;
using static xObsAsyncImageSource.AsyncImageSource;

namespace xObsAsyncImageSource.Wrapper
{
    public static class Proxy
    {
        private unsafe static string? CurrentFeature { get; set; }
        private unsafe static ConcurrentDictionary<string, GCHandle> SettingsList { get; set; } = new();
        private unsafe static ConcurrentDictionary<string, GCHandle> ImageSourceList { get; set; } = new();

        public static void MessageBox(string msg = "Hello, World!", int x = 100, int y = 100)
        {
            //uint MB_TOPMOST = 0x00040000;
            //_ = Win32.MessageBox(IntPtr.Zero, msg, "Hint", MB_TOPMOST);

            uint WS_EX_LAYERED = 0x00080000;
            uint WS_EX_TRANSPARENT = 0x00000020;
            uint WS_POPUP = 0x80000000;
            uint MB_TOPMOST = 0x00040000;

            // 创建一个透明窗口
            IntPtr hWnd = Win32.CreateWindowEx(
                WS_EX_LAYERED | WS_EX_TRANSPARENT, // 扩展样式：透明
                "STATIC",                          // 窗口类名
                "",                                // 窗口标题
                WS_POPUP,                          // 样式：弹出窗口
                x, y, 1, 1,                        // 窗口位置和大小
                IntPtr.Zero,                       // 父窗口句柄
                IntPtr.Zero,                       // 菜单句柄
                IntPtr.Zero,                       // 模块句柄
                IntPtr.Zero                        // 参数
            );

            // 显示MessageBox，使用透明窗口作为父窗口
            _ = Win32.MessageBox(hWnd, msg, "Hint", MB_TOPMOST);

            // 销毁透明窗口
            Win32.DestroyWindow(hWnd);
        }
        public static void MessageBoxTask(string msg = "Hello, World!")
        {
            Task.Run(() => { MessageBox(msg); });
        }

        public unsafe static sbyte* CreatePinnedObject(byte[] value)
        {
            GCHandle handle = GCHandle.Alloc(value, GCHandleType.Pinned); // 使用 GCHandle 来固定字节数组
            return (sbyte*)handle.AddrOfPinnedObject();                           // 获取 sbyte* 指针
        }
        public unsafe static string Bytes2String(sbyte* ptr)
        {
            return $"{Marshal.PtrToStringUTF8((IntPtr)ptr)}";
        }
        public unsafe static byte[] String2Bytes(string str)
        {
            if (str == null) { throw new ArgumentNullException(nameof(str)); }
            return Encoding.UTF8.GetBytes(str);
        }

        public unsafe static void SetCurrentFeature(string feature)
        {
            CurrentFeature = feature;
        }

        public unsafe static void SetSettings(string feature, obs_data* settings)
        {
            if (SettingsList.TryAdd(feature, new()))
            {
                //MessageBoxTask($"add {feature}");
            }

            if (settings == null) { throw new NotImplementedException(); }
            if (SettingsList[feature].IsAllocated) { SettingsList[feature].Free(); }
            SettingsList[feature] = GCHandle.Alloc(new IntPtr(settings), GCHandleType.Pinned);
        }
        public unsafe static obs_data* GetSettings()
        {
            var feature = CurrentFeature ?? throw new NotImplementedException();

            if (SettingsList.ContainsKey(feature))
            {
                if (SettingsList[feature].IsAllocated)
                {
                    var ptr = (IntPtr)SettingsList[feature].Target!;
                    return (obs_data*)ptr;
                }
            }

            throw new NotImplementedException();
        }

        public unsafe static void SetContext(string feature, image_source* imagesource)
        {
            ImageSourceList.TryAdd(feature, new());

            if (imagesource == null) { throw new NotImplementedException(); }
            if (ImageSourceList[feature].IsAllocated) { ImageSourceList[feature].Free(); }
            ImageSourceList[feature] = GCHandle.Alloc(new IntPtr(imagesource), GCHandleType.Pinned);
        }
        public unsafe static image_source* GetContext()
        {
            var feature = CurrentFeature ?? throw new NotImplementedException();

            if (ImageSourceList.ContainsKey(feature))
            {
                if (ImageSourceList[feature].IsAllocated)
                {
                    var ptr = (IntPtr)ImageSourceList[feature].Target!;
                    return (image_source*)ptr;
                }
            }

            throw new NotImplementedException();
        }

        public static void TryLogin(string feature, string? ip, int? port, Action<string>? act) => _ = ClientManager.Login(feature, ip, port, act);
        public static void TryLogout(string feature) => _ = ClientManager.Logout(feature);
        public static void TrySendMessage(string feature, string message) => _ = ClientManager.SendMessage(feature, message);
    }
}
