using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Net.Sockets;

namespace AVIStreamCLI
{
    class FFMpegTest
    {

        private static Bitmap makeMsgBitmap(string message, int height, int width)
        {
            float em = 25;
            Bitmap ret = new Bitmap(height, width, PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(ret);
            Brush black = new SolidBrush(Color.Black);
            Brush white = new SolidBrush(Color.White);
            Font f = new Font(FontFamily.GenericMonospace, 25);

            g.FillRectangle(black, 0, 0, width, height);
            g.DrawString(message, f, white, 10, height / 2 - em / 2);

            return ret;
        }


        public unsafe void testEncode()
        {
            Bitmap testImage = makeMsgBitmap("test", 640, 480);
            BitmapData data = testImage.LockBits(new Rectangle(0, 0, 640, 480), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int len = 640 * 480 * 3;
            byte[] buf = new byte[len];
            Marshal.Copy(data.Scan0, buf, 0, len);
            testImage.UnlockBits(data);

            Socket ffmpeg = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                ffmpeg.Connect("arael", 58768);
                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ms);
                bw.Write((int)640);
                bw.Write((int)480);
                bw.Write((float)15);
                bw.Write((byte)1);
                ffmpeg.Send(ms.GetBuffer(), (int)ms.Length, SocketFlags.None);
                while (true)
                {
                    ffmpeg.Send(buf, (int)len, SocketFlags.None);
                }
            }
            catch
            {
                ffmpeg.Close();
                Trace.TraceInformation("Exception thrown, assuming we're done.");
            }
        }
    }
}
