using H3Engine.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.FileSystem
{
    public class ImageData
    {
        private byte[] rawData;

        private byte[] pngData = null;

        private MemoryStream pngStream = null;

        private int dataIndex = 0;

        public int Width
        {
            get; set;
        }

        public int Height
        {
            get; set;
        }

        public ImageData(int width, int height)
        {
            this.Width = width;
            this.Height = height;

            this.rawData = new byte[width * height * 4];
            this.dataIndex = 0;
        }

        public void WriteData(byte red, byte green, byte blue, byte alpha)
        {
            this.rawData[this.dataIndex++] = red;
            this.rawData[this.dataIndex++] = green;
            this.rawData[this.dataIndex++] = blue;
            this.rawData[this.dataIndex++] = alpha;
        }

        public void WriteColor(Color color)
        {
            this.rawData[this.dataIndex++] = color.R;
            this.rawData[this.dataIndex++] = color.G;
            this.rawData[this.dataIndex++] = color.B;
            this.rawData[this.dataIndex++] = color.A;
        }

        /// <summary>
        /// This should not be used any more, it's replaced by GetPNGData()
        /// </summary>
        /// <param name="outputStream"></param>
        public void SaveAsPNGStream(Stream outputStream)
        {
            if (outputStream == null)
            {
                return;
            }

            unsafe
            {
                fixed (byte* ptr = rawData)
                {
                    using (Bitmap image = new Bitmap(this.Width, this.Height, this.Width * 4, PixelFormat.Format32bppRgb, new IntPtr(ptr)))
                    {
                        image.Save(outputStream, ImageFormat.Png);
                    }
                }
            }
        }
        
        public byte[] GetPNGData()
        {
            if (pngData == null)
            {
                using (Stream outputStream = new MemoryStream())
                {
                    this.SaveAsPNGStream(outputStream);
                    pngData = StreamHelper.ReadToEnd(outputStream);
                }
            }

            return pngData;
        }
    }
}
