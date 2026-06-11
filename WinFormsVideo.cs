using ManagedDoom;
using ManagedDoom.Video;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace DesktopDoom
{
    public sealed class WinFormsVideo : IVideo, IDisposable
    {
        private readonly Bitmap bitmap;
        private readonly Renderer renderer;
        private byte[] frameBuffer;

        public int MaxWindowSize => ThreeDRenderer.MaxScreenSize;

        public int WindowSize
        {
            get
            {
                return 7;
            }

            set
            {
            }
        }

        public bool DisplayMessage
        {
            get
            {
                return true;
            }

            set
            {
            }
        }

        public int MaxGammaCorrectionLevel => 10;

        public int GammaCorrectionLevel
        {
            get
            {
                return 2;
            }

            set
            {
            }
        }

        public int WipeBandCount => 321;
        public int WipeHeight => 200;

        public WinFormsVideo(Config config, GameContent content)
        {
            bitmap = new Bitmap(config.video_screenwidth, config.video_screenheight, PixelFormat.Format32bppArgb);
            this.renderer = new Renderer(config, content);
        }

        public void Render(Doom doom, Fixed frameFrac)
        {
            if (doom == null)
                throw new ArgumentNullException(nameof(doom));

            int width = bitmap.Width;
            int height = bitmap.Height;

            int dstBytes = width * height * 4;

            if (frameBuffer == null || frameBuffer.Length != dstBytes)
                frameBuffer = new byte[dstBytes];

            // This must render exactly width * height pixels into frameBuffer.
            // For Format32bppArgb, bytes should be: B, G, R, A.
            renderer.Render(doom, frameBuffer, frameFrac);

            BitmapData data = null;

            try
            {
                Rectangle rect = new Rectangle(0, 0, width, height);

                data = bitmap.LockBits(
                    rect,
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb);

                int srcStride = width * 4;
                int dstStride = data.Stride;

                if (dstStride == srcStride)
                {
                    Marshal.Copy(frameBuffer, 0, data.Scan0, dstBytes);
                }
                else
                {
                    for (int y = 0; y < height; y++)
                    {
                        IntPtr dst = IntPtr.Add(data.Scan0, y * dstStride);
                        int srcOffset = y * srcStride;

                        Marshal.Copy(frameBuffer, srcOffset, dst, srcStride);
                    }
                }
            }
            finally
            {
                if (data != null)
                    bitmap.UnlockBits(data);
            }
        }

        public void Paint()
        {
            string path = Directory.GetCurrentDirectory() + @"\img.bmp";
            bitmap.Save(path);
            Wallpaper.SetDesktopBackground(path);
        }

        public void Resize(int width, int height)
        {
        }

        public void Dispose()
        {
            bitmap.Dispose();
        }

        void IVideo.InitializeWipe()
        {
            
        }

        bool IVideo.HasFocus()
        {
            return true;
        }
    }
}
