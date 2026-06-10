using ManagedDoom;
using ManagedDoom.Silk;
using static ManagedDoom.CommandLineArgs;

namespace DesktopDoom
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {/*
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());*/
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            IntPtr desktopHost = DesktopWorkerW.GetDesktopHost();

            if (desktopHost == IntPtr.Zero)
            {
                MessageBox.Show("Could not find Progman or WorkerW desktop host.");
                return;
            }

            DoomHostControl form = new DoomHostControl();
            form.Dock = DockStyle.Fill;

            form.StartDoom(
                @"C:\DoomHost\managed-doom.exe",
                @"C:\DoomHost\doom1.wad");

            // Attach our form behind the desktop icons
            IntPtr formHandle = form.Handle;

            DesktopWorkerW.SetParent(formHandle, desktopHost);

            form.Bounds = Screen.PrimaryScreen.Bounds;

            Application.Run(form);
        }
    }
}