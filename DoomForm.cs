using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

public class DoomHostControl : Form
{
    private Process doomProcess;
    private IntPtr doomWindow = IntPtr.Zero;

    private const int GWL_STYLE = -16;
    private const int WS_CHILD = 0x40000000;
    private const int WS_VISIBLE = 0x10000000;
    private const int WS_CAPTION = 0x00C00000;
    private const int WS_THICKFRAME = 0x00040000;
    private const int WS_MINIMIZE = 0x20000000;
    private const int WS_MAXIMIZE = 0x01000000;
    private const int WS_SYSMENU = 0x00080000;

    [DllImport("user32.dll")]
    private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool MoveWindow(
        IntPtr hWnd,
        int X,
        int Y,
        int nWidth,
        int nHeight,
        bool bRepaint);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_SHOW = 5;

    public DoomHostControl()
    {
        BackColor = System.Drawing.Color.Black;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        WindowState = FormWindowState.Normal;

        Resize += DoomHostControl_Resize;
    }

    public void StartDoom(string exePath, string wadPath)
    {
        if (doomProcess != null && !doomProcess.HasExited)
            return;

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = "-iwad \"" + wadPath + "\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            WorkingDirectory = System.IO.Path.GetDirectoryName(exePath)
        };

        doomProcess = Process.Start(psi);

        // Wait for the native window to exist.
        int tries = 0;
        while (tries < 100)
        {
            doomProcess.Refresh();

            if (doomProcess.MainWindowHandle != IntPtr.Zero)
            {
                doomWindow = doomProcess.MainWindowHandle;
                break;
            }

            Thread.Sleep(100);
            tries++;
        }

        if (doomWindow == IntPtr.Zero)
            throw new InvalidOperationException("Could not find the Doom window.");

        EmbedWindow();
    }

    private void EmbedWindow()
    {
        SetParent(doomWindow, this.Handle);

        int style = GetWindowLong(doomWindow, GWL_STYLE);

        style = style | WS_CHILD | WS_VISIBLE;
        style = style & ~WS_CAPTION;
        style = style & ~WS_THICKFRAME;
        style = style & ~WS_MINIMIZE;
        style = style & ~WS_MAXIMIZE;
        style = style & ~WS_SYSMENU;

        SetWindowLong(doomWindow, GWL_STYLE, style);

        ShowWindow(doomWindow, SW_SHOW);
        ResizeDoomWindow();
    }

    private void DoomHostControl_Resize(object sender, EventArgs e)
    {
        ResizeDoomWindow();
    }

    private void ResizeDoomWindow()
    {
        if (doomWindow != IntPtr.Zero)
        {
            MoveWindow(
                doomWindow,
                0,
                0,
                this.ClientSize.Width,
                this.ClientSize.Height,
                true);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                if (doomProcess != null && !doomProcess.HasExited)
                    doomProcess.Kill();
            }
            catch
            {
            }
        }

        base.Dispose(disposing);
    }
}