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
            // Fill bitmap from Doom screen/framebuffer.
            // Look at SilkVideo.Render(...) and replace the OpenGL texture upload
            // with Bitmap.LockBits(...) + copy pixels.

            if (doom == null)
                throw new ArgumentNullException("doom");

            int width = bitmap.Width;
            int height = bitmap.Height;

            byte[] screenData = renderer.screen.Data;
            int pixelCount = screenData.Length;
            int requiredBytes = pixelCount * 4; // 4 bytes per pixel for Format32bppArgb

            if (frameBuffer == null || frameBuffer.Length != requiredBytes)
                frameBuffer = new byte[requiredBytes];

            // Render Doom's software framebuffer into our 32-bit byte buffer.
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
                    Marshal.Copy(frameBuffer, 0, data.Scan0, requiredBytes);
                }
                else
                {
                    // Bitmap stride can include padding, so copy row by row.
                    for (int y = 0; y < height; y++)
                    {
                        IntPtr dst = new IntPtr(data.Scan0.ToInt32() + y * dstStride);
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
