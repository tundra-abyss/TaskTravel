using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TaskTravel
{
    static class Win32
    {
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        const uint WM_GETICON = 0x007F;
        const int ICON_SMALL = 0;
        const int ICON_BIG = 1;
        const int ICON_SMALL2 = 2;

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex);

        const int GCLP_HICON = -14;

        public static IEnumerable<IntPtr> EnumTopLevelWindows()
        {
            var list = new List<IntPtr>();
            EnumWindows((h, l) => { list.Add(h); return true; }, IntPtr.Zero);
            return list;
        }

        public static bool IsVisibleAppWindow(IntPtr hWnd)
        {
            if (!IsWindowVisible(hWnd)) return false;
            var sb = new StringBuilder(256);
            GetWindowText(hWnd, sb, sb.Capacity);
            if (string.IsNullOrWhiteSpace(sb.ToString())) return false;
            return true;
        }

        public static Icon GetSmallWindowIcon(IntPtr hWnd)
        {
            IntPtr hIcon = SendMessage(hWnd, WM_GETICON, (IntPtr)ICON_SMALL2, IntPtr.Zero);
            if (hIcon == IntPtr.Zero) hIcon = SendMessage(hWnd, WM_GETICON, (IntPtr)ICON_SMALL, IntPtr.Zero);
            if (hIcon == IntPtr.Zero) hIcon = SendMessage(hWnd, WM_GETICON, (IntPtr)ICON_BIG, IntPtr.Zero);
            if (hIcon == IntPtr.Zero) hIcon = GetClassLongPtr(hWnd, GCLP_HICON);

            if (hIcon != IntPtr.Zero)
            {
                try { return Icon.FromHandle(hIcon); }
                catch { /* ignore */ }
            }

            GetWindowThreadProcessId(hWnd, out int pid);
            try
            {
                var proc = Process.GetProcessById(pid);
                var path = proc.MainModule.FileName;
                var ico = Icon.ExtractAssociatedIcon(path);
                return ico;
            }
            catch
            {
                return null;
            }
        }

        public static void RestoreAndActivateWindow(IntPtr hWnd)
        {
            const int SW_RESTORE = 9;
            ShowWindow(hWnd, SW_RESTORE);
            SetForegroundWindow(hWnd);
        }

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        [DllImport("user32.dll")]
        public static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        [DllImport("user32.dll")]
        static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        public static void EnableBlur(IntPtr hwnd)
        {
            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
            var accentStructSize = Marshal.SizeOf(accent);
            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(hwnd, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
            ACCENT_ENABLE_HOSTBACKDROP = 5,
        }

        [StructLayout(LayoutKind.Sequential)]
        struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        enum WindowCompositionAttribute
        {
            WCA_ACCENT_POLICY = 19
        }

        [StructLayout(LayoutKind.Sequential)]
        struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }
    }
}
