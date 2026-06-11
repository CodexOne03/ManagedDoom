using ManagedDoom;
using ManagedDoom.Silk;
using ManagedDoom.UserInput;
using System.Runtime.InteropServices;
using static ManagedDoom.CommandLineArgs;

namespace DesktopDoom
{
    internal static class Program
    {
        private static Doom doom;
        private static WinFormsVideo video;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {/*
            string path = Directory.GetCurrentDirectory() + @"\img.bmp";
            Wallpaper.SetDesktopBackground(path);
            return;*/
            Start();
            do
            {
                if (doom == null) return;

                doom.Update();
                video.Render(doom, Fixed.One);
                video.Paint();
                Thread.Sleep(1000 / 35);
            }
            while (true);
        }

        private static void Start()
        {
            var args = new CommandLineArgs(new[]
            {
            "-iwad", @"C:\DoomHost\doom1.WAD",
            "-nosound" // start here; add audio later
        });

            //var config = /* load config same as SilkConfigUtilities */;
            var config = new Config();
            var content = new GameContent(args);

            video = new WinFormsVideo(config, content);

            doom = new Doom(args, config, content, video, null, null, new NullUserInput());
        }
    }
}