using ManagedDoom;
using ManagedDoom.Audio;
using ManagedDoom.UserInput;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DesktopDoom
{
    public partial class Form1 : Form
    {
        private Doom doom;
        private WinFormsVideo video;
        private System.Windows.Forms.Timer timer;

        public Form1()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.Black;

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000 / 35;
            timer.Tick += new EventHandler(OnTick);
        }

        public void Start(string wadPath)
        {
            if (doom != null)
                return;

            string[] argv = new string[]
            {
                "-iwad",
                wadPath,
                "-nosound",
                "-nomusic"
            };

            CommandLineArgs args = new CommandLineArgs(argv);

            Config config = new Config(ConfigUtilities.GetConfigPath());

            GameContent content = new GameContent(args);

            video = new WinFormsVideo(this, config, content);

            doom = new Doom(
                args,
                config,
                content,
                video,
                new NullSound(),
                new NullMusic(),
                new NullUserInput());

            timer.Start();
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (doom == null || video == null)
                return;

            doom.Update();

            // Demo/title loop rendering.
            video.Render(doom, Fixed.One);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (video != null)
                video.Paint(e.Graphics, this.ClientRectangle);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }

            if (video != null)
            {
                video.Dispose();
                video = null;
            }
        }
    }
}