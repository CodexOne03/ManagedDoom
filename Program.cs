using ManagedDoom;
using ManagedDoom.Silk;
using ManagedDoom.UserInput;
using System.Runtime.InteropServices;
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
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form = new Form1();
            form.Start(@"C:\DoomHost\doom1.WAD");
            Application.Run(form);
        }
    }
}