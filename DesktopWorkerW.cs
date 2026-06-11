using System;
using System.Runtime.InteropServices;

namespace DesktopDoom
{
    public static class DesktopWorkerW
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(
            IntPtr hwndParent,
            IntPtr hwndChildAfter,
            string lpszClass,
            string? lpszWindow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            uint Msg,
            IntPtr wParam,
            IntPtr lParam,
            uint fuFlags,
            uint uTimeout,
            out IntPtr lpdwResult);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        private const uint WM_SPAWN_WORKER = 0x052C;
        private const uint SMTO_NORMAL = 0x0000; private static IntPtr FindAnyEmptyWorkerW()
        {
            IntPtr result = IntPtr.Zero;

            EnumWindows((top, _) =>
            {
                if (GetWindowClass(top) == "WorkerW" &&
                    FindWindowEx(top, IntPtr.Zero, "SHELLDLL_DefView", null) == IntPtr.Zero)
                {
                    result = top;
                }

                return true;
            }, IntPtr.Zero);

            return result;
        }

        public static IntPtr GetDesktopHost()
        {
            IntPtr progman = FindWindow("Progman", null);
            if (progman == IntPtr.Zero)
                return IntPtr.Zero;

            SendMessageTimeout(
                progman,
                0x052C,
                IntPtr.Zero,
                IntPtr.Zero,
                0,
                1000,
                out _);

            // First try the classic layout:
            // WorkerW -> SHELLDLL_DefView, followed by empty WorkerW.
            IntPtr classicWorker = FindWorkerWBehindIcons();
            if (classicWorker != IntPtr.Zero)
                return classicWorker;

            // Your layout:
            // Progman -> SHELLDLL_DefView, many empty WorkerWs.
            IntPtr emptyWorker = FindAnyEmptyWorkerW();
            if (emptyWorker != IntPtr.Zero)
                return emptyWorker;

            // Do not blindly fall back to Progman.
            return IntPtr.Zero;
        }
        private static IntPtr FindWorkerWBehindIcons()
        {
            IntPtr result = IntPtr.Zero;

            EnumWindows((top, _) =>
            {
                IntPtr shellView = FindWindowEx(
                    top,
                    IntPtr.Zero,
                    "SHELLDLL_DefView",
                    null);

                if (shellView != IntPtr.Zero)
                {
                    IntPtr worker = FindWindowEx(
                        IntPtr.Zero,
                        top,
                        "WorkerW",
                        null);

                    if (worker != IntPtr.Zero)
                    {
                        result = worker;
                        return false;
                    }
                }

                return true;
            }, IntPtr.Zero);

            return result;
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

        private static string GetWindowClass(IntPtr hWnd)
        {
            var sb = new System.Text.StringBuilder(256);
            GetClassName(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        public static void DumpDesktopWindows()
        {
            EnumWindows((top, _) =>
            {
                string cls = GetWindowClass(top);

                if (cls == "Progman" || cls == "WorkerW")
                {
                    IntPtr shell = FindWindowEx(top, IntPtr.Zero, "SHELLDLL_DefView", null);
                    IntPtr list = shell == IntPtr.Zero
                        ? IntPtr.Zero
                        : FindWindowEx(shell, IntPtr.Zero, "SysListView32", null);

                    System.Diagnostics.Debug.WriteLine(
                        $"{top} {cls} ShellView={shell} ListView={list}");
                }

                return true;
            }, IntPtr.Zero);
        }
    }
}