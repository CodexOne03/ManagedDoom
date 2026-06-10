using System;
using System.Runtime.InteropServices;

namespace DesktopDoom
{
    public static class DesktopWorkerW
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(
            IntPtr hwndParent,
            IntPtr hwndChildAfter,
            string lpszClass,
            string lpszWindow
        );

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
            out IntPtr lpdwResult
        );

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        private const uint SMTO_NORMAL = 0x0000;
        private const uint WM_SPAWN_WORKER = 0x052C;

        public static IntPtr GetDesktopHost()
        {
            IntPtr progman = FindWindow("Progman", null);

            if (progman == IntPtr.Zero)
                return IntPtr.Zero;

            // Different Windows builds respond better to different parameter combos.
            SendMessageTimeout(
                progman,
                WM_SPAWN_WORKER,
                IntPtr.Zero,
                IntPtr.Zero,
                SMTO_NORMAL,
                1000,
                out _
            );

            SendMessageTimeout(
                progman,
                WM_SPAWN_WORKER,
                new IntPtr(0xD),
                IntPtr.Zero,
                SMTO_NORMAL,
                1000,
                out _
            );

            SendMessageTimeout(
                progman,
                WM_SPAWN_WORKER,
                new IntPtr(0xD),
                new IntPtr(1),
                SMTO_NORMAL,
                1000,
                out _
            );

            IntPtr workerW = IntPtr.Zero;

            EnumWindows((topHandle, topParamHandle) =>
            {
                IntPtr shellView = FindWindowEx(
                    topHandle,
                    IntPtr.Zero,
                    "SHELLDLL_DefView",
                    null
                );

                if (shellView != IntPtr.Zero)
                {
                    // Try to get the WorkerW behind the icon layer
                    workerW = FindWindowEx(
                        IntPtr.Zero,
                        topHandle,
                        "WorkerW",
                        null
                    );
                }

                return true;
            }, IntPtr.Zero);

            // Fallback: use Progman directly.
            // This often works when Explorer does not create a separate WorkerW.
            if (workerW == IntPtr.Zero)
                workerW = progman;

            return workerW;
        }
    }
}
